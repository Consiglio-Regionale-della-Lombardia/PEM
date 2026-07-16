/*
 * Copyright (C) 2019 Consiglio Regionale della Lombardia
 * SPDX-License-Identifier: AGPL-3.0-or-later
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PortaleRegione.API.Controllers;
using PortaleRegione.Common;
using PortaleRegione.Contracts;
using PortaleRegione.Crypto;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;
using PortaleRegione.SDK.EDMA.Models;
using PortaleRegione.SDK.EDMA.Persistance;

namespace PortaleRegione.BAL
{
    /// <summary>
    ///     Orchestratore della protocollazione di un atto DASI verso EDMA.
    ///     Espone una singola entry point (<see cref="ProtocollaAttoAsync"/>)
    ///     che esegue in sequenza i cinque step previsti dalla specifica ARIA:
    ///     creazione Pratica, creazione DocumentoFile (pdf principale),
    ///     fascicolazione del documento nella pratica e protocollazione
    ///     applicativa con segnatura in arrivo.
    ///
    ///     Gli id restituiti dai singoli servizi EDMA vengono persistiti man
    ///     mano sulla tabella ATTI_DASI; in caso di errore parziale, una nuova
    ///     chiamata riparte dal primo step incompleto senza ripetere quelli
    ///     gia' andati a buon fine (idempotenza).
    /// </summary>
    public class DASIProtocollazioneService : BaseLogic
    {
        public DASIProtocollazioneService(IUnitOfWork unitOfWork,
            DASILogic dasiLogic,
            AttiFirmeLogic attiFirmeLogic)
        {
            _unitOfWork = unitOfWork;
            _logicDasi = dasiLogic;
            _logicAttiFirme = attiFirmeLogic;
        }

        /// <summary>
        ///     Esegue il flusso completo di protocollazione per l'atto indicato.
        ///     Viene invocato dal click "Protocolla" della segreteria, una
        ///     volta confermato il modale.
        /// </summary>
        public async Task<EdmaProtocollazioneEsitoDto> ProtocollaAttoAsync(Guid uidAtto, PersonaDto currentUser)
        {
            var atto = await _unitOfWork.DASI.Get(uidAtto);
            if (atto == null)
                return Errore("Atto non trovato.");

            if (atto.IDStato != (int)StatiAttoEnum.PRESENTATO
                && atto.IDStato != (int)StatiAttoEnum.IN_TRATTAZIONE
                && atto.IDStato != (int)StatiAttoEnum.COMPLETATO)
                return Errore("L'atto non e' in uno stato compatibile con la protocollazione.");

            // Idempotenza: se la segnatura e' gia' presente non rifacciamo nulla.
            if (!string.IsNullOrEmpty(atto.EDMA_Segnatura))
                return EsitoDaAtto(atto, "Atto gia' protocollato.");

            var client = CreateClient();

            try
            {
                // STEP 1 - creazione Pratica nel sotto-fascicolo titolario.
                if (string.IsNullOrEmpty(atto.EDMA_IdPratica))
                {
                    var subfasc = GetSottofascicoloPerTipo(atto.Tipo);
                    if (string.IsNullOrEmpty(subfasc.IdEdma))
                        return await SalvaErrore(atto,
                            $"Sotto-fascicolo non configurato per il tipo {Utility.GetText_Tipo(atto.Tipo)}.",
                            step: "creaPratica");

                    var pratica = BuildPratica(atto, subfasc.CodProcedimento);
                    var resp = await client.CreaInserisciPraticaAsync(
                        subfasc.IdEdma,
                        100127, // metamodulo SottoFascicolo (il padre della pratica)
                        pratica);

                    if (!resp.Success || resp.Data == null || string.IsNullOrEmpty(resp.Data.IdPratica))
                        return await SalvaErrore(atto,
                            $"Errore creazione pratica: {resp.Message ?? "id assente"}",
                            step: "creaPratica", dettaglio: resp.RawResponse);

                    atto.EDMA_IdPratica = resp.Data.IdPratica;
                    atto.EDMA_NumeroPratica = resp.Data.NumeroPratica;
                    await _unitOfWork.CompleteAsync();
                }

                // STEP 2 - creazione DocumentoFile con il pdf dell'atto.
                if (string.IsNullOrEmpty(atto.EDMA_IdDocumento))
                {
                    var pdfBytes = await _logicDasi.PDFIstantaneo(atto, currentUser);
                    if (pdfBytes == null || pdfBytes.Length == 0)
                        return await SalvaErrore(atto, "Impossibile generare il pdf dell'atto.",
                            step: "creaDocumento");

                    var nomeFile = GetNomeFileAtto(atto);
                    var documento = BuildDocumentoBase(atto);
                    var resp = await client.CreaDocumentoAsync(documento, nomeFile, pdfBytes, "pdf");

                    if (!resp.Success || resp.Data == null || string.IsNullOrEmpty(resp.Data.IdDocumento))
                        return await SalvaErrore(atto,
                            $"Errore creazione documento: {resp.Message ?? "id assente"}",
                            step: "creaDocumento", dettaglio: resp.RawResponse);

                    atto.EDMA_IdDocumento = resp.Data.IdDocumento;
                    await _unitOfWork.CompleteAsync();
                }

                // STEP 3 - allegato come figlio del documento principale.
                // Se l'atto ha un PATH_AllegatoGenerico valorizzato, lo
                // carichiamo in EDMA come DocumentoFile figlio del pdf
                // principale: cosi' finisce nella pratica e viene incluso
                // automaticamente nel pacchetto del protocollo.
                if (!string.IsNullOrEmpty(atto.PATH_AllegatoGenerico)
                    && string.IsNullOrEmpty(atto.EDMA_IdAllegatoGenerico))
                {
                    var pathAllegato = System.IO.Path.Combine(
                        AppSettingsConfiguration.PercorsoCompatibilitaDocumenti,
                        System.IO.Path.GetFileName(atto.PATH_AllegatoGenerico));

                    if (!System.IO.File.Exists(pathAllegato))
                    {
                        // L'allegato e' indicato in DB ma il file non esiste piu'
                        // sul filesystem: lo segnaliamo nel log e proseguiamo,
                        // perche' bloccare la protocollazione su questo sarebbe
                        // troppo restrittivo (il pdf principale lo contiene
                        // gia' embedded).
                        Log.Debug($"EDMA allegato non trovato su disco per atto {uidAtto}: {pathAllegato}");
                    }
                    else
                    {
                        var fileBytes = System.IO.File.ReadAllBytes(pathAllegato);
                        var nomeAllegato = System.IO.Path.GetFileName(atto.PATH_AllegatoGenerico);
                        var estensione = System.IO.Path.GetExtension(atto.PATH_AllegatoGenerico)
                            ?.TrimStart('.').ToLowerInvariant() ?? "bin";

                        var figlio = new DocumentoBase
                        {
                            Oggetto = $"Allegato {GetNomeAtto(atto)}",
                            CodAutore = AppSettingsConfiguration.EDMA_CodAutore,
                            Metamodulo = 200031,
                            MetaDocumento = new MetaDocumento
                            {
                                Codice = AppSettingsConfiguration.EDMA_CodiceMetadocumento_Allegato
                            }
                        };

                        var resp = await client.CreaInserisciDocumentoFiglioAsync(
                            atto.EDMA_IdDocumento,
                            AppSettingsConfiguration.EDMA_CodiceMetadocumento_Atto,
                            figlio,
                            nomeAllegato,
                            fileBytes,
                            estensione);

                        if (!resp.Success || resp.Data == null || string.IsNullOrEmpty(resp.Data.IdDocumento))
                            return await SalvaErrore(atto,
                                $"Errore creazione allegato: {resp.Message ?? "id assente"}",
                                step: "creaAllegato", dettaglio: resp.RawResponse);

                        atto.EDMA_IdAllegatoGenerico = resp.Data.IdDocumento;
                        await _unitOfWork.CompleteAsync();
                    }
                }

                // STEP 4 - fascicolazione del pdf principale nella pratica.
                // L'esito boolean non blocca il flusso: alcuni provider EDMA
                // tornano false se l'associazione esiste gia' (in caso di
                // ripresa idempotente).
                {
                    var resp = await client.AssociaDocumentiAsync(
                        atto.EDMA_IdPratica,
                        atto.EDMA_IdDocumento,
                        200031); // metamodulo DocumentoFile
                    if (!resp.Success)
                        Log.Debug($"EDMA associaDocumenti atto {uidAtto}: {resp.Message}");
                    else if (!resp.Data)
                        Log.Debug($"EDMA associaDocumenti atto {uidAtto}: risposta false (probabilmente gia' associato)");
                }

                // STEP 5 - protocollazione applicativa, con segnatura in arrivo.
                {
                    var firmatari = await GetDescrizioneFirmatari(atto);
                    var parametri = BuildParametriProtocollazione(atto, firmatari);
                    var resp = await client.ProtocollazioneApplicativaAsync(atto.EDMA_IdDocumento, parametri);

                    if (!resp.Success || resp.Data == null || string.IsNullOrEmpty(resp.Data.Segnatura))
                        return await SalvaErrore(atto,
                            $"Errore protocollazione: {resp.Message ?? "segnatura non restituita da EDMA"}",
                            step: "protocollazioneApplicativa", dettaglio: resp.RawResponse);

                    atto.EDMA_IdProtocollo = resp.Data.IdScheda;
                    atto.EDMA_Segnatura = resp.Data.Segnatura ?? string.Empty;
                    // La segreteria gestisce come "Protocollo" il numero pratica
                    // EDMA; la segnatura (EDMA_Segnatura) va invece riportata sul PDF.
                    if (!string.IsNullOrEmpty(atto.EDMA_NumeroPratica))
                        atto.Protocollo = atto.EDMA_NumeroPratica;
                    atto.Inviato_Al_Protocollo = true;
                    atto.DataInvioAlProtocollo = DateTime.Now;
                }

                // #1663 - EDMA ha gia' ricevuto l'originale: da qui in avanti
                // ogni altro flusso deve vedere solo l'offuscato.
                ConsolidaTestoOffuscato(atto);
                await RicertificaTestoOffuscato(atto, currentUser);

                atto.EDMA_UltimoErrore = null;
                atto.EDMA_DataUltimoTentativo = DateTime.Now;
                await _unitOfWork.CompleteAsync();

                return EsitoDaAtto(atto, "Protocollazione completata.");
            }
            catch (Exception ex)
            {
                Log.Error($"Protocollazione EDMA atto {uidAtto}", ex);
                return await SalvaErrore(atto, ex.Message,
                    step: "eccezione", dettaglio: ex.ToString());
            }
        }

        // ----------------------------------------------------------------
        // Consolidamento del testo offuscato
        // ----------------------------------------------------------------

        /// <summary>
        ///     Sostituisce il testo dell'atto con l'offuscato, dove presente.
        ///     Non reversibile: l'originale resta solo a protocollo e in audit.
        ///     I campi offuscati non vengono azzerati.
        /// </summary>
        private static void ConsolidaTestoOffuscato(ATTI_DASI atto)
        {
            var oggetto = atto.OggettoView();
            if (HaTesto(oggetto))
                atto.Oggetto = oggetto;

            if (HaTesto(atto.Premesse_Modificato))
                atto.Premesse = atto.Premesse_Modificato;

            if (HaTesto(atto.Richiesta_Modificata))
                atto.Richiesta = atto.Richiesta_Modificata;
        }

        // Premesse e richiesta sono html: il markup lasciato da un editor
        // svuotato (<p><br></p>, &nbsp;) non e' testo valido.
        private static bool HaTesto(string testo)
        {
            if (string.IsNullOrWhiteSpace(testo))
                return false;

            return !string.IsNullOrWhiteSpace(Utility.StripHTML(testo));
        }

        /// <summary>
        ///     Riallinea il corpo certificato, che nei pdf senza vista privacy
        ///     ha la precedenza sui campi (cfr. BaseLogic.GetBody). Cambia solo
        ///     il testo, le firme restano valide.
        /// </summary>
        private async Task RicertificaTestoOffuscato(ATTI_DASI atto, PersonaDto currentUser)
        {
            if (string.IsNullOrEmpty(atto.Atto_Certificato))
                return;

            var body = await _logicDasi.GetBodyDASI(atto.UIDAtto, currentUser, TemplateTypeEnum.FIRMA);
            atto.Atto_Certificato = CryptoHelper.EncryptString(body, BALHelper.Decrypt(atto.Hash));
        }

        // ----------------------------------------------------------------
        // Costruzione payload e helper
        // ----------------------------------------------------------------

        private static EdmaApiService CreateClient()
        {
            return new EdmaApiService(
                AppSettingsConfiguration.EDMA_Url,
                AppSettingsConfiguration.EDMA_Username,
                AppSettingsConfiguration.EDMA_Password,
                AppSettingsConfiguration.EDMA_NoSession,
                (msg, ex) =>
                {
                    if (ex != null) Log.Error("EDMA SDK: " + msg, ex);
                    else Log.Debug("EDMA SDK: " + msg);
                });
        }

        // Restituisce, per tipo atto, l'id EDMA del SottoFascicolo titolario
        // (tag <padre><id>) e il suo codProcedimento (tag <codProcedimento> del
        // documento). Entrambi sono valori specifici dell'ambiente forniti da ARIA.
        private (string IdEdma, string CodProcedimento) GetSottofascicoloPerTipo(int tipo)
        {
            switch ((TipoAttoEnum)tipo)
            {
                case TipoAttoEnum.ITL:
                    return (AppSettingsConfiguration.EDMA_Sottofascicolo_ITL_IdEdma,
                        AppSettingsConfiguration.EDMA_Sottofascicolo_ITL_CodProcedimento);
                case TipoAttoEnum.ITR:
                    return (AppSettingsConfiguration.EDMA_Sottofascicolo_ITR_IdEdma,
                        AppSettingsConfiguration.EDMA_Sottofascicolo_ITR_CodProcedimento);
                case TipoAttoEnum.MOZ:
                    return (AppSettingsConfiguration.EDMA_Sottofascicolo_MOZ_IdEdma,
                        AppSettingsConfiguration.EDMA_Sottofascicolo_MOZ_CodProcedimento);
                case TipoAttoEnum.ODG:
                    return (AppSettingsConfiguration.EDMA_Sottofascicolo_ODG_IdEdma,
                        AppSettingsConfiguration.EDMA_Sottofascicolo_ODG_CodProcedimento);
                case TipoAttoEnum.IQT:
                    return (AppSettingsConfiguration.EDMA_Sottofascicolo_IQT_IdEdma,
                        AppSettingsConfiguration.EDMA_Sottofascicolo_IQT_CodProcedimento);
                case TipoAttoEnum.RIS:
                    return (AppSettingsConfiguration.EDMA_Sottofascicolo_RIS_IdEdma,
                        AppSettingsConfiguration.EDMA_Sottofascicolo_RIS_CodProcedimento);
                default:
                    return (null, null);
            }
        }

        private SDK.EDMA.Models.FascicoloPratica BuildPratica(ATTI_DASI atto, string codProcedimentoSottofascicolo)
        {
            var nomeAtto = GetNomeAtto(atto);
            var titolo = $"{nomeAtto} – \"{atto.Oggetto}\"";

            // Niente attributi in creazione: il titolario e' gia' dato dal
            // SottoFascicolo padre (cfr. SIS.EDMA - Fascicolazione v5,
            // "Caricare un Metadocumento", in Documentazione/EDMA).
            return new SDK.EDMA.Models.FascicoloPratica
            {
                Titolo = titolo,
                // codProcedimento del SottoFascicolo padre (NON dell'atto):
                // valore per ambiente fornito da ARIA, configurato per tipo.
                CodProcedimento = codProcedimentoSottofascicolo,
                DataApertura = DateTime.Now.Date,
                AnniConservazione = AppSettingsConfiguration.EDMA_AnniConservazione_Pratica,
                DataChiusura = ParseDataChiusura(AppSettingsConfiguration.EDMA_DataChiusura_Pratica),
                ResponsabileCodPersona = AppSettingsConfiguration.EDMA_Responsabile_CodPersona,
                ReferenteCodPersona = AppSettingsConfiguration.EDMA_Istruttore_CodPersona,
                MetadocumentoCodice = AppSettingsConfiguration.EDMA_CodiceMetadocumento_Pratica,
                // <procedimento> = id del Fascicolo principale (FascicoloProcedimento)
                // e <livelloAppartenenza> = id del livello del fascicolo: entrambi
                // obbligatori in creaInserisciDocumento e specifici dell'ambiente
                // (forniti da ARIA), comuni a tutte le tipologie di atto.
                ProcedimentoIdEdma = AppSettingsConfiguration.EDMA_FascicoloPrincipale_IdEdma,
                LivelloAppartenenzaId = AppSettingsConfiguration.EDMA_LivelloAppartenenza_IdEdma,
                SedeSoggetto = new SedeSoggetto
                {
                    Descrizione = AppSettingsConfiguration.EDMA_DescrizioneSoggettoPratica,
                    CodFiscale = AppSettingsConfiguration.EDMA_CF_Consiglio
                }
            };
        }

        private DocumentoBase BuildDocumentoBase(ATTI_DASI atto)
        {
            var oggetto = $"{GetNomeAtto(atto)} – {atto.Oggetto}";
            return new DocumentoBase
            {
                Oggetto = oggetto,
                CodAutore = AppSettingsConfiguration.EDMA_CodAutore,
                Metamodulo = 200031, // DocumentoFile
                MetaDocumento = new MetaDocumento
                {
                    Codice = AppSettingsConfiguration.EDMA_CodiceMetadocumento_Atto
                }
            };
        }

        private ParametriProtocollazioneApplicativa BuildParametriProtocollazione(ATTI_DASI atto, string descrizioneFirmatari)
        {
            var parametri = new ParametriProtocollazioneApplicativa
            {
                CodiceStrutturaProtocollante = AppSettingsConfiguration.EDMA_StrutturaProtocollante,
                FlagRiscontro = AppSettingsConfiguration.EDMA_FlagRiscontro,
                TipoDocumento = AppSettingsConfiguration.EDMA_TipoDocumento,
                MezzoSpedizione = AppSettingsConfiguration.EDMA_MezzoSpedizione,
                Oggetto = $"{GetNomeAtto(atto)} – {atto.Oggetto}",
                Mittente = new MittenteEsterno
                {
                    Manuale = true,
                    Descrizione = string.IsNullOrEmpty(descrizioneFirmatari)
                        ? "Consiglieri firmatari"
                        : descrizioneFirmatari
                }
            };

            // Destinatario per competenza: la UO indicata in configurazione.
            if (!string.IsNullOrEmpty(AppSettingsConfiguration.EDMA_CodiceEnteCompetente_Destinatario_Competenza))
                parametri.DestinatariCompetenza.Add(new DestinatarioInterno
                {
                    CodiceEc = AppSettingsConfiguration.EDMA_CodiceEnteCompetente_Destinatario_Competenza,
                    Tipologia = 2
                });

            // Eventuali destinatari per conoscenza, in formato CSV in config.
            var perConoscenza = AppSettingsConfiguration.EDMA_CodiceEnteCompetente_Destinatari_PerConoscenza ?? string.Empty;
            foreach (var codice in perConoscenza.Split(',')
                         .Select(s => s.Trim())
                         .Where(s => !string.IsNullOrEmpty(s)))
                parametri.DestinatariConoscenza.Add(new DestinatarioInterno
                {
                    CodiceEc = codice,
                    Tipologia = 1
                });

            return parametri;
        }

        private async Task<string> GetDescrizioneFirmatari(ATTI_DASI atto)
        {
            try
            {
                var firme = await _logicAttiFirme.GetFirme(atto, FirmeTipoEnum.ATTIVI);
                if (firme == null) return string.Empty;

                var nomi = firme
                    .Where(f => string.IsNullOrEmpty(f.Data_ritirofirma))
                    // f.FirmaCert e' gia' decifrato da GetFirme (BALHelper.Decrypt):
                    // contiene il display name "Cognome Nome (SIGLA)". Non va
                    // decifrato di nuovo, altrimenti torna "Valore Corrotto".
                    .Select(f => RimuoviGruppoDaFirmatario(f.FirmaCert))
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct()
                    .ToList();

                return string.Join(", ", nomi);
            }
            catch (Exception ex)
            {
                Log.Debug($"Lettura firmatari atto {atto.UIDAtto}: " + ex.Message);
                return string.Empty;
            }
        }

        // Il DisplayName_GruppoCode usato lato firma e' "Cognome Nome (SIGLA)";
        // per la scheda protocollo serve solo nome e cognome.
        private static string RimuoviGruppoDaFirmatario(string nominativo)
        {
            if (string.IsNullOrEmpty(nominativo)) return nominativo;
            var pulito = Regex.Replace(nominativo, @"\s*\([^)]*\)\s*$", string.Empty);
            return pulito.Trim();
        }

        private static DateTime ParseDataChiusura(string raw)
        {
            if (DateTime.TryParseExact(raw ?? string.Empty, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var dt))
                return dt;
            return new DateTime(2099, 9, 9);
        }

        private string GetNomeAtto(ATTI_DASI atto)
        {
            var tipo = Utility.GetText_Tipo(atto.Tipo);
            var numero = GetNome(atto.NAtto, atto.Progressivo);
            return $"{tipo} {numero}".Trim();
        }

        private string GetNomeFileAtto(ATTI_DASI atto)
        {
            var tipo = Utility.GetText_Tipo(atto.Tipo) ?? "ATTO";
            var numero = GetNome(atto.NAtto, atto.Progressivo);
            return $"{tipo}_{numero}.pdf".Replace(' ', '_');
        }

        // ----------------------------------------------------------------
        // Esito
        // ----------------------------------------------------------------

        private static EdmaProtocollazioneEsitoDto Errore(string messaggio)
        {
            return new EdmaProtocollazioneEsitoDto
            {
                Success = false,
                Messaggio = messaggio
            };
        }

        private async Task<EdmaProtocollazioneEsitoDto> SalvaErrore(ATTI_DASI atto, string messaggio,
            string step = null, string dettaglio = null)
        {
            // L'ultimo errore in DB include anche lo step e il dettaglio se
            // disponibili, cosi' anche guardando direttamente la tabella
            // ATTI_DASI si capisce dove il flusso si e' fermato e perche'.
            var ultimoErrore = string.IsNullOrEmpty(step) ? messaggio : $"[{step}] {messaggio}";
            if (!string.IsNullOrEmpty(dettaglio))
                ultimoErrore += Environment.NewLine + dettaglio;

            if (atto != null)
            {
                atto.EDMA_TentativiInvio = atto.EDMA_TentativiInvio + 1;
                atto.EDMA_UltimoErrore = ultimoErrore;
                atto.EDMA_DataUltimoTentativo = DateTime.Now;
                try
                {
                    await _unitOfWork.CompleteAsync();
                }
                catch (Exception ex)
                {
                    Log.Error("Salvataggio errore EDMA su ATTI_DASI", ex);
                }
            }

            return new EdmaProtocollazioneEsitoDto
            {
                Success = false,
                Messaggio = messaggio,
                StepFallito = step,
                Dettaglio = dettaglio,
                IdPratica = atto?.EDMA_IdPratica,
                NumeroPratica = atto?.EDMA_NumeroPratica,
                IdDocumento = atto?.EDMA_IdDocumento,
                IdProtocollo = atto?.EDMA_IdProtocollo,
                Segnatura = atto?.EDMA_Segnatura,
                TentativiInvio = atto?.EDMA_TentativiInvio ?? 0,
                UltimoErrore = atto?.EDMA_UltimoErrore
            };
        }

        private static EdmaProtocollazioneEsitoDto EsitoDaAtto(ATTI_DASI atto, string messaggio)
        {
            return new EdmaProtocollazioneEsitoDto
            {
                Success = true,
                Messaggio = messaggio,
                IdPratica = atto.EDMA_IdPratica,
                NumeroPratica = atto.EDMA_NumeroPratica,
                IdDocumento = atto.EDMA_IdDocumento,
                IdProtocollo = atto.EDMA_IdProtocollo,
                Segnatura = atto.EDMA_Segnatura,
                DataInvioAlProtocollo = atto.DataInvioAlProtocollo,
                TentativiInvio = atto.EDMA_TentativiInvio,
                UltimoErrore = atto.EDMA_UltimoErrore
            };
        }
    }
}
