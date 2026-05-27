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

                    var pratica = BuildPratica(atto, subfasc.Titolario);
                    var resp = await client.CreaInserisciPraticaAsync(
                        subfasc.IdEdma,
                        AppSettingsConfiguration.EDMA_CodiceMetadocumento_Pratica,
                        100128, // metamodulo FascicoloProcedimento (padre del sotto-fascicolo)
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
                            Oggetto = TruncaPerSicurezza($"Allegato {GetNomeAtto(atto)}", 1000),
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

                    if (!resp.Success || resp.Data == null)
                        return await SalvaErrore(atto,
                            $"Errore protocollazione: {resp.Message ?? "esito vuoto"}",
                            step: "protocollazioneApplicativa", dettaglio: resp.RawResponse);

                    atto.EDMA_IdProtocollo = resp.Data.IdScheda;
                    atto.EDMA_Segnatura = resp.Data.Segnatura ?? string.Empty;
                    if (!string.IsNullOrEmpty(atto.EDMA_Segnatura))
                        atto.Protocollo = atto.EDMA_Segnatura;
                    atto.Inviato_Al_Protocollo = true;
                    atto.DataInvioAlProtocollo = DateTime.Now;
                }

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

        private (string IdEdma, string Titolario) GetSottofascicoloPerTipo(int tipo)
        {
            switch ((TipoAttoEnum)tipo)
            {
                case TipoAttoEnum.ITL:
                    return (AppSettingsConfiguration.EDMA_Sottofascicolo_ITL_IdEdma,
                        AppSettingsConfiguration.EDMA_Sottofascicolo_ITL_Titolario);
                case TipoAttoEnum.ITR:
                    return (AppSettingsConfiguration.EDMA_Sottofascicolo_ITR_IdEdma,
                        AppSettingsConfiguration.EDMA_Sottofascicolo_ITR_Titolario);
                case TipoAttoEnum.MOZ:
                    return (AppSettingsConfiguration.EDMA_Sottofascicolo_MOZ_IdEdma,
                        AppSettingsConfiguration.EDMA_Sottofascicolo_MOZ_Titolario);
                case TipoAttoEnum.ODG:
                    return (AppSettingsConfiguration.EDMA_Sottofascicolo_ODG_IdEdma,
                        AppSettingsConfiguration.EDMA_Sottofascicolo_ODG_Titolario);
                case TipoAttoEnum.IQT:
                    return (AppSettingsConfiguration.EDMA_Sottofascicolo_IQT_IdEdma,
                        AppSettingsConfiguration.EDMA_Sottofascicolo_IQT_Titolario);
                case TipoAttoEnum.RIS:
                    return (AppSettingsConfiguration.EDMA_Sottofascicolo_RIS_IdEdma,
                        AppSettingsConfiguration.EDMA_Sottofascicolo_RIS_Titolario);
                default:
                    return (null, null);
            }
        }

        private SDK.EDMA.Models.FascicoloPratica BuildPratica(ATTI_DASI atto, string titolario)
        {
            var nomeAtto = GetNomeAtto(atto);
            var titolo = $"{nomeAtto} – \"{atto.Oggetto}\"";

            var pratica = new SDK.EDMA.Models.FascicoloPratica
            {
                Titolo = TruncaPerSicurezza(titolo, 1000),
                CodProcedimento = atto.Etichetta ?? nomeAtto,
                DataApertura = DateTime.Now.Date,
                AnniConservazione = AppSettingsConfiguration.EDMA_AnniConservazione_Pratica,
                DataChiusura = ParseDataChiusura(AppSettingsConfiguration.EDMA_DataChiusura_Pratica),
                ResponsabileCodPersona = AppSettingsConfiguration.EDMA_Responsabile_CodPersona,
                ReferenteCodPersona = AppSettingsConfiguration.EDMA_Istruttore_CodPersona,
                MetadocumentoCodice = AppSettingsConfiguration.EDMA_CodiceMetadocumento_Pratica,
                SedeSoggetto = new SedeSoggetto
                {
                    Descrizione = AppSettingsConfiguration.EDMA_DescrizioneSoggettoPratica,
                    CodFiscale = AppSettingsConfiguration.EDMA_CF_Consiglio,
                    Pariva = AppSettingsConfiguration.EDMA_CF_Consiglio
                }
            };

            if (!string.IsNullOrEmpty(titolario))
                pratica.Attributi["titolario"] = titolario;

            return pratica;
        }

        private DocumentoBase BuildDocumentoBase(ATTI_DASI atto)
        {
            var oggetto = $"{GetNomeAtto(atto)} – {atto.Oggetto}";
            return new DocumentoBase
            {
                Oggetto = TruncaPerSicurezza(oggetto, 1000),
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
                    .Select(f => RimuoviGruppoDaFirmatario(
                        CryptoHelper.DecryptString(f.FirmaCert, AppSettingsConfiguration.masterKey)))
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

        private static string TruncaPerSicurezza(string testo, int max)
        {
            if (string.IsNullOrEmpty(testo)) return testo;
            return testo.Length <= max ? testo : testo.Substring(0, max);
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
                atto.EDMA_UltimoErrore = TruncaPerSicurezza(ultimoErrore, 4000);
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
