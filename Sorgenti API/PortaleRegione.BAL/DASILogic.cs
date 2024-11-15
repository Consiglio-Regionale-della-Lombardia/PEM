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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using HtmlToOpenXml;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PortaleRegione.BAL;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.GestioneStampe;
using PortaleRegione.Logger;
using Color = System.Drawing.Color;
using Utility = PortaleRegione.Common.Utility;

namespace PortaleRegione.API.Controllers
{
    public class DASILogic : BaseLogic
    {
        internal AdminLogic _logicAdmin;

        public DASILogic(IUnitOfWork unitOfWork, PersoneLogic logicPersona, AttiFirmeLogic logicAttiFirme,
            SeduteLogic logicSedute, AttiLogic logicAtti, UtilsLogic logicUtil, AdminLogic logicAdmin)
        {
            _logicAdmin = logicAdmin;
            _unitOfWork = unitOfWork;
            _logicPersona = logicPersona;
            _logicAttiFirme = logicAttiFirme;
            _logicSedute = logicSedute;
            _logicAtti = logicAtti;
            _logicUtil = logicUtil;

            GetUsersInDb();
            GetGroupsInDb();
        }

        public async Task<ATTI_DASI> Salva(AttoDASIDto attoDto, PersonaDto currentUser)
        {
            if (attoDto.UIDPersonaProponente == Guid.Empty)
                throw new InvalidOperationException("Indicare un proponente");

            var result = new ATTI_DASI();
            if (attoDto.UIDAtto == Guid.Empty)
            {
                //Nuovo inserimento
                result.Tipo = attoDto.Tipo;
                if (attoDto.Tipo == (int)TipoAttoEnum.MOZ)
                {
                    result.TipoMOZ = attoDto.TipoMOZ;
                    result.UID_MOZ_Abbinata = result.TipoMOZ == (int)TipoMOZEnum.ABBINATA
                        ? result.UID_MOZ_Abbinata
                        : null;
                }

                if (attoDto.Tipo == (int)TipoAttoEnum.ODG)
                {
                    if (!attoDto.UID_Atto_ODG.HasValue || attoDto.UID_Atto_ODG == Guid.Empty)
                        throw new InvalidOperationException(
                            "Seleziona un atto a cui iscrivere l'ordine del giorno");

                    result.UID_Atto_ODG = attoDto.UID_Atto_ODG;
                    var attoPEM = await _unitOfWork.Atti.Get(result.UID_Atto_ODG.Value);
                    var seduta = await _unitOfWork.Sedute.Get(attoPEM.UIDSeduta.Value);
                    result.UIDSeduta = seduta.UIDSeduta;
                    result.DataRichiestaIscrizioneSeduta = BALHelper.EncryptString(
                        seduta.Data_seduta.ToString("dd/MM/yyyy"),
                        AppSettingsConfiguration.masterKey);
                    result.UIDPersonaRichiestaIscrizione = currentUser.UID_persona;

                    result.Non_Passaggio_In_Esame = attoDto.Non_Passaggio_In_Esame;
                }

                var legislatura = await _unitOfWork.Legislature.Legislatura_Attiva();
                result.Legislatura = legislatura;
                var progressivo =
                    await _unitOfWork.DASI.GetProgressivo((TipoAttoEnum)attoDto.Tipo, currentUser.Gruppo.id_gruppo,
                        legislatura);
                result.Progressivo = progressivo;

                if (currentUser.IsSegreteriaAssemblea
                    || currentUser.IsPresidente)
                    result.IDStato = (int)StatiAttoEnum.BOZZA;
                else
                    result.IDStato = currentUser.Gruppo.abilita_em_privati
                        ? (int)StatiAttoEnum.BOZZA_RISERVATA
                        : (int)StatiAttoEnum.BOZZA;

                if (currentUser.IsConsigliereRegionale ||
                    currentUser.IsAssessore)
                    result.UIDPersonaProponente = currentUser.UID_persona;
                else
                    result.UIDPersonaProponente = attoDto.UIDPersonaProponente;

                result.UIDPersonaCreazione = currentUser.UID_persona;
                result.DataCreazione = DateTime.Now;
                result.idRuoloCreazione = (int)currentUser.CurrentRole;
                if (!currentUser.IsSegreteriaAssemblea
                    && !currentUser.IsPresidente)
                {
                    result.id_gruppo = currentUser.Gruppo.id_gruppo;
                }
                else
                {
                    var proponente = await _logicPersona.GetPersona(result.UIDPersonaProponente.Value);
                    result.id_gruppo = proponente.Gruppo.id_gruppo;
                }

                result.UIDAtto = Guid.NewGuid();
                attoDto.UIDAtto = result.UIDAtto;
                result.UID_QRCode = Guid.NewGuid();
                result.Oggetto = attoDto.Oggetto;
                result.Premesse = attoDto.Premesse;
                result.Richiesta = attoDto.Richiesta;
                result.IDTipo_Risposta = attoDto.IDTipo_Risposta;

                if (attoDto.DocAllegatoGenerico_Stream.Length > 0)
                {
                    await Salva_Documento(new SalvaDocumentoRequest
                    {
                        Tipo = (int)TipoDocumentoEnum.TESTO_ALLEGATO,
                        Contenuto = attoDto.DocAllegatoGenerico_Stream,
                        UIDAtto = result.UIDAtto
                    }, currentUser);
                }

                _unitOfWork.DASI.Add(result);

                await _unitOfWork.CompleteAsync();
                await GestioneCommissioni(attoDto);
                return result;
            }

            //Modifica
            var attoInDb = await _unitOfWork.DASI.Get(attoDto.UIDAtto);
            if (attoInDb == null)
                throw new InvalidOperationException("Atto non trovato");

            if (attoDto.Tipo == (int)TipoAttoEnum.MOZ)
            {
                attoInDb.TipoMOZ = attoDto.TipoMOZ;
                attoInDb.UID_MOZ_Abbinata =
                    attoInDb.TipoMOZ == (int)TipoMOZEnum.ABBINATA ? attoDto.UID_MOZ_Abbinata : null;
            }

            if (attoDto.Tipo == (int)TipoAttoEnum.ODG)
            {
                if (!attoDto.UID_Atto_ODG.HasValue || attoDto.UID_Atto_ODG == Guid.Empty)
                    throw new InvalidOperationException(
                        "Seleziona un atto a cui iscrivere l'ordine del giorno");

                attoInDb.UID_Atto_ODG = attoDto.UID_Atto_ODG;
                var attoPEM = await _unitOfWork.Atti.Get(attoInDb.UID_Atto_ODG.Value);
                var seduta = await _unitOfWork.Sedute.Get(attoPEM.UIDSeduta.Value);
                attoInDb.UIDSeduta = seduta.UIDSeduta;
                attoInDb.DataRichiestaIscrizioneSeduta = BALHelper.EncryptString(
                    seduta.Data_seduta.ToString("dd/MM/yyyy"),
                    AppSettingsConfiguration.masterKey);
                attoInDb.UIDPersonaRichiestaIscrizione = currentUser.UID_persona;
                attoInDb.Non_Passaggio_In_Esame = attoDto.Non_Passaggio_In_Esame;
            }

            attoInDb.UIDPersonaModifica = currentUser.UID_persona;
            attoInDb.DataModifica = DateTime.Now;
            attoInDb.Oggetto = attoDto.Oggetto;
            attoInDb.Premesse = attoDto.Premesse;
            attoInDb.Richiesta = attoDto.Richiesta;
            attoInDb.IDTipo_Risposta = attoDto.IDTipo_Risposta;
            
            if (attoDto.DocAllegatoGenerico_Stream.Length > 0)
            {
                await Salva_Documento(new SalvaDocumentoRequest
                {
                    Tipo = (int)TipoDocumentoEnum.TESTO_ALLEGATO,
                    Contenuto = attoDto.DocAllegatoGenerico_Stream,
                    UIDAtto = attoInDb.UIDAtto
                }, currentUser);
            }

            await _unitOfWork.CompleteAsync();
            await GestioneCommissioni(attoDto, true);

            if (!string.IsNullOrEmpty(attoInDb.Atto_Certificato))
            {
                // Matteo Cattapan #527 - Avviso modifica bozza 
                // Quando un atto in bozza è già stato firmato da altri consiglieri e viene modificato dal proponente,
                // il sistema deve invalidare le firme ed inviare una notifica ad ogni firmatario
                var firme = await _logicAttiFirme.GetFirme(attoInDb, FirmeTipoEnum.TUTTE);
                if (firme.Count(i => i.UID_persona != currentUser.UID_persona) > 0)
                {
                    var firmatari = new List<string>();
                    foreach (var firma in firme.Where(i => i.UID_persona != currentUser.UID_persona))
                    {
                        var firmatario = await _logicPersona.GetPersona(firma.UID_persona);
                        firmatari.Add(firmatario.email);
                    }

                    if (firmatari.Count > 0)
                    {
                        try
                        {
                            var nome_atto = $"{Utility.GetText_Tipo(attoDto.Tipo)} {attoDto.NAtto}";
                            var mailModel = new MailModel
                            {
                                DA = currentUser.email,
                                A = firmatari.Aggregate((i, j) => i + ";" + j),
                                OGGETTO =
                                    $"Atto {nome_atto} modificato dal consigliere proponente",
                                MESSAGGIO =
                                    $"{currentUser.DisplayName_GruppoCode} ha modificato l'atto {nome_atto} con oggetto {attoInDb.Oggetto}. <br> Pertanto il sistema ha invalidato tutte le firme apposte. <br> Contatta il proponente per firmare nuovamente l’atto. {GetBodyFooterMail()}"
                            };
                            await _logicUtil.InvioMail(mailModel);
                        }
                        catch (Exception e)
                        {
                            Log.Error("Invio mail", e);
                        }

                        await _logicAttiFirme.RimuoviFirme(attoInDb);
                        await _unitOfWork.CompleteAsync();
                    }
                }

                //re-crypt del testo certificato
                var body = await GetBodyDASI(attoInDb, null, currentUser,
                    TemplateTypeEnum.FIRMA);
                var body_encrypt = BALHelper.EncryptString(body, BALHelper.Decrypt(attoInDb.Hash));

                attoInDb.Atto_Certificato = body_encrypt;
                await _unitOfWork.CompleteAsync();
            }

            return attoInDb;
        }

        public async Task Salva_InformazioniGenerali(AttoDASI_InformazioniGeneraliDto request, PersonaDto persona)
        {
            var attoInDb = await _unitOfWork.DASI.Get(request.UIDAtto);
            if (attoInDb == null)
                throw new InvalidOperationException("Atto non trovato");

            attoInDb.UIDPersonaModifica = persona.UID_persona;
            attoInDb.DataModifica = DateTime.Now;
            attoInDb.Oggetto = request.Oggetto;
            attoInDb.IDTipo_Risposta = request.RispostaRichiesta;
            attoInDb.AreaPolitica = request.AreaPolitica;
            attoInDb.IDStato = request.Stato;
            attoInDb.CodiceMateria = request.CodiceMateria;
            attoInDb.Protocollo = request.Protocollo;
            if (request.DataAnnunzio > DateTime.MinValue)
                attoInDb.DataAnnunzio = request.DataAnnunzio;
            attoInDb.Pubblicato = request.Pubblicato;
            attoInDb.Sollecito = request.Sollecito;

            if (attoInDb.Tipo == (int)TipoAttoEnum.RIS)
            {
                attoInDb.UIDPersonaRelatore1 = request.UIDPersonaRelatore1;
                attoInDb.UIDPersonaRelatore2 = request.UIDPersonaRelatore2;
                attoInDb.UIDPersonaRelatoreMinoranza = request.UIDPersonaRelatoreMinoranza;
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task Salva_Privacy(AttoDASIDto request, PersonaDto persona)
        {
            var attoInDb = await _unitOfWork.DASI.Get(request.UIDAtto);
            if (attoInDb == null)
                throw new InvalidOperationException("Atto non trovato");

            attoInDb.UIDPersonaModifica = persona.UID_persona;
            attoInDb.DataModifica = DateTime.Now;
            
            attoInDb.Privacy_Dati_Personali_Giudiziari = request.Privacy_Dati_Personali_Giudiziari;
            attoInDb.Privacy_Dati_Personali_Semplici = request.Privacy_Dati_Personali_Semplici;
            attoInDb.Privacy_Dati_Personali_Sensibili = request.Privacy_Dati_Personali_Sensibili;

            attoInDb.Privacy_Divieto_Pubblicazione_Altri = request.Privacy_Divieto_Pubblicazione_Altri;
            attoInDb.Privacy_Divieto_Pubblicazione_Salute = request.Privacy_Divieto_Pubblicazione_Salute;
            attoInDb.Privacy_Divieto_Pubblicazione_Vita_Sessuale = request.Privacy_Divieto_Pubblicazione_Vita_Sessuale;

            // #998
            if (request.Privacy_Dati_Personali_Giudiziari
                || request.Privacy_Dati_Personali_Semplici
                || request.Privacy_Dati_Personali_Sensibili
                || request.Privacy_Divieto_Pubblicazione_Vita_Sessuale
                || request.Privacy_Divieto_Pubblicazione_Altri
                || request.Privacy_Divieto_Pubblicazione_Salute)
            {
                attoInDb.Privacy = true;
            }
            else
            {
                attoInDb.Privacy = false;
            }

            if (request.Privacy_Divieto_Pubblicazione_Vita_Sessuale
                || request.Privacy_Divieto_Pubblicazione_Altri
                || request.Privacy_Divieto_Pubblicazione_Salute)
            {
                attoInDb.Privacy_Divieto_Pubblicazione = true;
            }
            else
            {
                attoInDb.Privacy_Divieto_Pubblicazione = false;
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task Salva_NuovoAbbinamento(AttiAbbinamentoDto request)
        {
            var attoInDb = await _unitOfWork.DASI.Get(request.UidAbbinamento);
            if (attoInDb == null)
                throw new InvalidOperationException("Atto non trovato");

            _unitOfWork.DASI.AggiungiAbbinamento(request.UidAbbinamento, request.UidAttoAbbinato);

            await _unitOfWork.CompleteAsync();
        }

        public async Task Salva_NuovaRisposta(AttiRisposteDto request)
        {
            var attoInDb = await _unitOfWork.DASI.Get(request.UIDAtto);
            if (attoInDb == null)
                throw new InvalidOperationException("Atto non trovato");

            var risposta = new ATTI_RISPOSTE
            {
                UIDAtto = request.UIDAtto,
                DescrizioneOrgano = request.DescrizioneOrgano,
                Tipo = request.Tipo,
                IdOrgano = request.IdOrgano,
                TipoOrgano = request.TipoOrgano,
                UIDRispostaAssociata = request.Uid
            };

            _unitOfWork.DASI.AggiungiRisposta(risposta);

            await _unitOfWork.CompleteAsync();
        }

        public async Task Salva_RimuoviAbbinamento(AttiAbbinamentoDto request)
        {
            var attoInDb = await _unitOfWork.DASI.Get(request.UidAbbinamento);
            if (attoInDb == null)
                throw new InvalidOperationException("Atto non trovato");

            var abbinamentoInDb =
                await _unitOfWork.DASI.GetAbbinamento(request.UidAbbinamento, request.UidAttoAbbinato);

            _unitOfWork.DASI.RimuoviAbbinamento(abbinamentoInDb);
            await _unitOfWork.CompleteAsync();
        }

        public async Task Salva_RimuoviRisposta(AttiRisposteDto request)
        {
            var rispostaInDb = await _unitOfWork.DASI.GetRisposta(request.Uid);
            if (rispostaInDb == null)
            {
                throw new InvalidOperationException("Risposta non trovata");
            }

            var risposteAtto = await _unitOfWork.DASI.GetRisposte(rispostaInDb.UIDAtto);
            var rispostaDto = risposteAtto.FirstOrDefault(risp => risp.Uid == request.Uid);
            if (rispostaDto != null)
            {
                if (rispostaDto.RisposteAssociate.Any())
                {
                    foreach (var rispostaAssociata in rispostaDto.RisposteAssociate)
                    {
                        await Salva_RimuoviRisposta(rispostaAssociata);
                    }
                }
            }

            _unitOfWork.DASI.RimuoviRisposta(rispostaInDb);

            if (rispostaInDb.UIDDocumento.HasValue)
            {
                await Rimuovi_Documento(new AttiDocumentiDto(rispostaInDb.UIDDocumento.Value));
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task Salva_RimuoviNota(NoteDto request)
        {
            var attoInDb = await _unitOfWork.DASI.Get(request.UIDAtto);
            if (attoInDb == null)
                throw new InvalidOperationException("Atto non trovato");

            var notaInDb = await _unitOfWork.DASI.GetNota(request.UIDAtto, request.TipoEnum);
            if (notaInDb == null)
                return;
            _unitOfWork.DASI.RimuoviNota(notaInDb);
            await _unitOfWork.CompleteAsync();
        }

        public async Task Salva_Nota(NoteDto request, PersonaDto currentUser)
        {
            if (request.TipoEnum == 0)
                throw new InvalidOperationException("Tipo nota non impostato");
            if (string.IsNullOrEmpty(request.Nota))
                throw new InvalidOperationException("Non è possibile inserire una nota vuota");
            var attoInDb = await _unitOfWork.DASI.Get(request.UIDAtto);
            if (attoInDb == null)
                throw new InvalidOperationException("Atto non trovato");

            var nota = await _unitOfWork.DASI.GetNota(request.UIDAtto, request.TipoEnum);
            if (nota == null)
            {
                //Aggiungi nota
                _unitOfWork.DASI.AggiungiNota(new ATTI_NOTE
                {
                    Data = DateTime.Now,
                    Nota = request.Nota,
                    Tipo = (int)request.TipoEnum,
                    UIDAtto = request.UIDAtto,
                    UIDPersona = currentUser.UID_persona,
                    Uid = Guid.NewGuid()
                });
                await _unitOfWork.CompleteAsync();
                return;
            }

            //Modifica nota
            nota.Nota = request.Nota;
            nota.UIDPersona = currentUser.UID_persona;
            nota.Data = DateTime.Now;

            await _unitOfWork.CompleteAsync();
        }

        public async Task Salva_DettagliRisposta(AttiRisposteDto request)
        {
            var risposteInDb = await _unitOfWork.DASI.GetRisposta(request.Uid);
            if (risposteInDb == null)
            {
                throw new InvalidOperationException("Risposta non trovata");
            }

            risposteInDb.Data = request.Data;
            risposteInDb.DataTrasmissione = request.DataTrasmissione;
            risposteInDb.DataTrattazione = request.DataTrattazione;
            await _unitOfWork.CompleteAsync();
        }

        public async Task Salva_InformazioniRisposta(AttoDASIDto request)
        {
            var attoInDb = await _unitOfWork.DASI.Get(request.UIDAtto);
            if (attoInDb == null)
            {
                throw new InvalidOperationException("Atto non trovato");
            }

            attoInDb.IDTipo_Risposta_Effettiva = request.IDTipo_Risposta_Effettiva;
            attoInDb.DataSedutaRisposta = request.DataSedutaRisposta;
            attoInDb.DataComunicazioneAssemblea = request.DataComunicazioneAssemblea;

            await _unitOfWork.CompleteAsync();
        }

        public async Task Salva_RimuoviMonitoraggio(AttiRisposteDto request)
        {
            var attoInDb = await _unitOfWork.DASI.Get(request.UIDAtto);
            if (attoInDb == null)
                throw new InvalidOperationException("Atto non trovato");

            var monitoraggioInDb = await _unitOfWork.DASI.GetMonitoraggio(request.Uid);
            _unitOfWork.DASI.RimuoviMonitoraggio(monitoraggioInDb);
            await _unitOfWork.CompleteAsync();
        }

        public async Task Salva_NuovoMonitoraggio(AttiRisposteDto request)
        {
            var attoInDb = await _unitOfWork.DASI.Get(request.UIDAtto);
            if (attoInDb == null)
                throw new InvalidOperationException("Atto non trovato");

            var monitoraggio = new ATTI_MONITORAGGIO
            {
                Uid = Guid.NewGuid(),
                UIDAtto = request.UIDAtto,
                IdOrgano = request.IdOrgano,
                TipoOrgano = request.TipoOrgano,
                DescrizioneOrgano = request.DescrizioneOrgano
            };
            _unitOfWork.DASI.AggiungiMonitoraggio(monitoraggio);

            await _unitOfWork.CompleteAsync();
        }

        public async Task Salva_InformazioniMonitoraggio(AttoDASIDto request)
        {
            var attoInDb = await _unitOfWork.DASI.Get(request.UIDAtto);
            if (attoInDb == null)
                throw new InvalidOperationException("Atto non trovato");

            attoInDb.AltriSoggetti = request.AltriSoggetti;
            attoInDb.AreaTematica = request.AreaTematica;

            await _unitOfWork.CompleteAsync();
        }

        public async Task Salva_InformazioniChiusuraIter(AttoDASIDto request, PersonaDto currentUser)
        {
            var attoInDb = await _unitOfWork.DASI.Get(request.UIDAtto);
            if (attoInDb == null)
                throw new InvalidOperationException("Atto non trovato");

            if (!attoInDb.DCRL.Equals(request.DCRL)
                || !attoInDb.DCR.Equals(request.DCR)
                || !attoInDb.DCCR.Equals(request.DCCR))
            {
                var filtroDCR = new List<FilterItem>
                {
                    new FilterItem
                    {
                        property = nameof(AttoDASIDto.DCRL),
                        value = request.DCRL
                    },
                    new FilterItem
                    {
                        property = nameof(AttoDASIDto.DCR),
                        value = request.DCR.ToString()
                    },
                    new FilterItem
                    {
                        property = nameof(AttoDASIDto.DCCR),
                        value = request.DCCR.ToString()
                    }
                };

                var query = Utility.ParseFilterDasi(filtroDCR);
                var queryFilter = new Filter<ATTI_DASI>();
                queryFilter.ImportStatements(query);
                var res = await _unitOfWork.DASI.GetAll(currentUser, 1, 1, ClientModeEnum.GRUPPI, queryFilter,
                    new QueryExtendedRequest(), new List<SortingInfo>());

                if (res.Any()) throw new InvalidOperationException("DCR/DCCR già presente a sistema.");

                attoInDb.DCRL = request.DCRL;
                attoInDb.DCR = request.DCR;
                attoInDb.DCCR = request.DCCR;
            }

            attoInDb.TipoChiusuraIter = request.TipoChiusuraIter;
            attoInDb.TipoVotazioneIter = request.TipoVotazioneIter;
            attoInDb.DataChiusuraIter = request.DataChiusuraIter;
            attoInDb.Emendato = request.Emendato;

            await _unitOfWork.CompleteAsync();
        }

        private async Task GestioneProponentiCommissioni(AttoDASIDto attoDto, bool isUpdate = false)
        {
            if (isUpdate) await _unitOfWork.DASI.RimuoviCommissioniProponenti(attoDto.UIDAtto);

            if (!string.IsNullOrEmpty(attoDto.CommissioniProponenti_string))
            {
                var commissioniProponenti =
                    JsonConvert.DeserializeObject<List<KeyValueDto>>(attoDto.CommissioniProponenti_string);
                foreach (var commissioneProponente in commissioniProponenti)
                {
                    await _unitOfWork.DASI.AggiungiCommissioneProponente(attoDto.UIDAtto, commissioneProponente);
                }
            }

            await _unitOfWork.CompleteAsync();
        }

        private async Task GestioneCommissioni(AttoDASIDto attoDto, bool isUpdate = false)
        {
            if (isUpdate) await _unitOfWork.DASI.RimuoviCommissioni(attoDto.UIDAtto);

            if (!string.IsNullOrEmpty(attoDto.Commissioni_client))
            {
                var commissioni = attoDto
                    .Commissioni_client
                    .Split(',')
                    .Select(item => Convert.ToInt32(item));
                foreach (var commissione in commissioni)
                    _unitOfWork.DASI.AggiungiCommissione(attoDto.UIDAtto, commissione);
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task<ATTI_DASI> Get(Guid id)
        {
            var attoInDb = await _unitOfWork.DASI.Get(id);
            return attoInDb;
        }

        public async Task<RiepilogoDASIModel> Get(BaseRequest<AttoDASIDto> model, PersonaDto persona, Uri uri)
        {
            var requestStato = GetResponseStatusFromFilters(model.filtro);
            var requestTipo = GetResponseTypeFromFilters(model.filtro);

            var queryExtended = CreateQueryExtendedRequest(model);
            
            model.param.TryGetValue("CLIENT_MODE", out var CLIENT_MODE); // per trattazione aula
            model.param.TryGetValue("RequireMySign", out var RequireMySign); // #539
            if (RequireMySign == null)
                RequireMySign = false;

            await AddRequireMySignData(queryExtended, persona, Convert.ToBoolean(RequireMySign));

            var queryFilter = new Filter<ATTI_DASI>();
            queryFilter.ImportStatements(model.filtro);
            var atti_in_db = await _unitOfWork.DASI.GetAll(persona, model.page, model.size, GetClientMode(CLIENT_MODE),
                queryFilter, queryExtended, model.dettagliOrdinamento);

            if (!atti_in_db.Any())
            {
                await RestoreFiltersIfEmptyResult(model, queryFilter, persona,
                    GetClientMode(model.param["CLIENT_MODE"]), queryExtended);
                return new RiepilogoDASIModel
                {
                    Stato = requestStato,
                    Tipo = requestTipo,
                    Data = new BaseResponse<AttoDASIDto>(model.page, model.size, new List<AttoDASIDto>(), model.filtro,
                        0, uri),
                    CountBarData = await GetResponseCountBar(persona, GetClientMode(CLIENT_MODE),
                        queryFilter, queryExtended)
                };
            }

            var result = await GetAttoDtos(atti_in_db, persona);
            var totaleAtti = await _unitOfWork.DASI.Count(persona, GetClientMode(CLIENT_MODE),
                queryFilter, queryExtended);

            queryFilter.ImportStatements(model.filtro);
            return new RiepilogoDASIModel
            {
                Stato = requestStato,
                Tipo = requestTipo,
                Data = new BaseResponse<AttoDASIDto>(model.page, model.size, result, model.filtro, totaleAtti, uri),
                CountBarData = await GetResponseCountBar(persona, GetClientMode(CLIENT_MODE),
                    queryFilter, queryExtended),
                CommissioniAttive = persona.IsSegreteriaAssemblea ? await GetCommissioniAttive() : null
            };
        }

        private QueryExtendedRequest CreateQueryExtendedRequest(BaseRequest<AttoDASIDto> model)
        {
            var queryExtended = new QueryExtendedRequest();

            ExtractAndAddFilters(model, nameof(AttoDASIDto.UIDPersonaProponente), queryExtended.Proponenti, Guid.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.AreaPolitica), queryExtended.AreaPolitica, int.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.id_gruppo), queryExtended.GruppiProponenti, int.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.id_gruppo_firmatari), queryExtended.GruppiFirmatari,
                int.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.Firme), queryExtended.Firmatari, Guid.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.Abbinamenti), queryExtended.Provvedimenti, Guid.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.DataTrasmissione), queryExtended.DataTrasmissione,
                DateTime.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.DataRisposta), queryExtended.DataRisposta,
                DateTime.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.UIDSeduta), queryExtended.DataSeduta,
                DateTime.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.DataComunicazioneAssemblea),
                queryExtended.DataComunicazioneAssemblea,
                DateTime.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.DataAnnunzio), queryExtended.DataAnnunzio,
                DateTime.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.DataChiusuraIter), queryExtended.DataChiusuraIter,
                DateTime.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.DataTrattazione), queryExtended.DataTrattazione,
                DateTime.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.Timestamp), queryExtended.DataPresentazione,
                DateTime.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.Organi), queryExtended.Organi, int.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.IDStato), queryExtended.Stati, int.Parse, queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.Tipo), queryExtended.Tipi, int.Parse, queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.IDTipo_Risposta), queryExtended.TipiRispostaRichiesta,
                int.Parse, queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.TipoChiusuraIter), queryExtended.TipiChiusura, int.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.TipoVotazioneIter), queryExtended.TipiVotazione, int.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.Documenti), queryExtended.TipiDocumento, int.Parse,
                queryExtended);
            ExtractAndAddFilters(model, nameof(AttoDASIDto.Risposte), queryExtended.Risposte, int.Parse,
                queryExtended);

            return queryExtended;
        }

        private void ExtractAndAddFilters<T>(BaseRequest<AttoDASIDto> model, string propertyId, List<T> targetList,
            Func<string, T> convertFunc, QueryExtendedRequest query)
        {
            if (model.filtro.Any(statement => statement.PropertyId == propertyId
                                              && propertyId == nameof(AttoDASIDto.DataTrasmissione)
                                              && statement.Operation == Operation.IsNull))
            {
                query.DataTrasmissioneIsNull = true;
                var statementTrasmissione = model.filtro
                    .Where(statement => statement.PropertyId == propertyId && statement.Value == null).ToList();
                foreach (var statement in statementTrasmissione) model.filtro.Remove(statement);

                return;
            }

            if (model.filtro.Any(statement => statement.PropertyId == propertyId
                                              && propertyId == nameof(AttoDASIDto.DataTrattazione)
                                              && statement.Operation == Operation.IsNull))
            {
                query.DataTrattazioneIsNull = true;
                var statementTrattazione = model.filtro
                    .Where(statement => statement.PropertyId == propertyId && statement.Value == null).ToList();
                foreach (var statement in statementTrattazione) model.filtro.Remove(statement);

                return;
            }

            if (model.filtro.Any(statement => statement.PropertyId == propertyId
                                              && propertyId == nameof(AttoDASIDto.DataRisposta)
                                              && statement.Operation == Operation.IsNull))
            {
                query.DataRispostaIsNull = true;
                var statementRisposta = model.filtro
                    .Where(statement => statement.PropertyId == propertyId && statement.Value == null).ToList();
                foreach (var statement in statementRisposta) model.filtro.Remove(statement);

                return;
            }

            if (model.filtro.Any(statement => statement.PropertyId == propertyId
                                              && propertyId == nameof(AttoDASIDto.DataAnnunzio)
                                              && statement.Operation == Operation.IsNull))
            {
                query.DataAnnunzioIsNull = true;
                var statementAnnunzio = model.filtro
                    .Where(statement => statement.PropertyId == propertyId && statement.Value == null).ToList();
                foreach (var statement in statementAnnunzio) model.filtro.Remove(statement);

                return;
            }

            if (model.filtro.Any(statement => statement.PropertyId == propertyId
                                              && propertyId == nameof(AttoDASIDto.DataComunicazioneAssemblea)
                                              && statement.Operation == Operation.IsNull))
            {
                query.DataComunicazioneAssembleaIsNull = true;
                var statementDataComunicazioneAssemblea = model.filtro
                    .Where(statement => statement.PropertyId == propertyId && statement.Value == null).ToList();
                foreach (var statement in statementDataComunicazioneAssemblea) model.filtro.Remove(statement);

                return;
            }

            if (model.filtro.Any(statement => statement.PropertyId == propertyId
                                              && propertyId == nameof(AttoDASIDto.DataChiusuraIter)
                                              && statement.Operation == Operation.IsNull))
            {
                query.DataChiusuraIterIsNull = true;
                var statementDataChiusuraIter = model.filtro
                    .Where(statement => statement.PropertyId == propertyId && statement.Value == null).ToList();
                foreach (var statement in statementDataChiusuraIter) model.filtro.Remove(statement);

                return;
            }

            if (model.filtro.Any(statement => statement.PropertyId == propertyId
                                              && propertyId == nameof(AttoDASIDto.Timestamp)
                                              && statement.Operation == Operation.IsNull))
            {
                query.DataPresentazioneIsNull = true;
                var statementDataChiusuraIter = model.filtro
                    .Where(statement => statement.PropertyId == propertyId && statement.Value == null).ToList();
                foreach (var statement in statementDataChiusuraIter) model.filtro.Remove(statement);

                return;
            }

            if (model.filtro.Any(statement => statement.PropertyId == propertyId
                                              && propertyId == nameof(AttoDASIDto.Risposte)
                                              && statement.Operation == Operation.IsNull))
            {
                query.RispostaMancante = true;
                var statementRisposta = model.filtro
                    .Where(statement => statement.PropertyId == propertyId && statement.Value == null).ToList();
                foreach (var statement in statementRisposta) model.filtro.Remove(statement);

                return;
            }

            if (model.filtro.Any(statement => statement.PropertyId == propertyId
                                              && propertyId == nameof(AttoDASIDto.Organi)
                                              && statement.Operation == Operation.IsNull))
            {
                query.OrganiIsNull = true;
                var statementOrgani = model.filtro
                    .Where(statement => statement.PropertyId == propertyId && statement.Value == null).ToList();
                foreach (var statement in statementOrgani) model.filtro.Remove(statement);

                return;
            }

            if (model.filtro.Any(statement => statement.PropertyId == propertyId
                                              && propertyId == nameof(AttoDASIDto.UIDSeduta)))
            {
                var statementUIDSeduta = model.filtro
                    .Where(statement => statement.PropertyId == propertyId).ToList();

                foreach (var statement in statementUIDSeduta)
                {
                    if (Guid.TryParse(statement.Value.ToString(), out var guid))
                    {
                        return;
                    }
                }
            }

            if (model.filtro.Any(statement => (statement.PropertyId == propertyId
                                               && statement.Operation == Operation.IsNull) ||
                                              statement.Operation == Operation.IsNullOrWhiteSpace))
                return;

            var statements = model.filtro
                .Where(statement => statement.PropertyId == propertyId
                                    && !statement.Value.Equals("_NOT_")).ToList();
            if (statements.Any())
            {
                targetList.AddRange(statements.Select(statement => convertFunc(statement.Value.ToString())));
                foreach (var statement in statements) model.filtro.Remove(statement);
            }

            if (model.filtro.Any(statement => statement.PropertyId == propertyId && statement.Value.Equals("_NOT_")))
            {
                if (propertyId == nameof(AttoDASIDto.Documenti)) query.DocumentiMancanti = true;

                var statementsNOT = model.filtro.Where(statement =>
                    statement.PropertyId == propertyId && statement.Value.Equals("_NOT_")).ToList();
                foreach (var statement in statementsNOT) model.filtro.Remove(statement);
            }
        }

        private ClientModeEnum GetClientMode(object clientMode)
        {
            return (ClientModeEnum)Convert.ToInt16(clientMode);
        }

        private async Task AddRequireMySignData(QueryExtendedRequest queryExtended, PersonaDto persona,
            bool requireMySign)
        {
            if (requireMySign)
            {
                var notificheDestinatari = await _unitOfWork.Notifiche_Destinatari.Get(persona.UID_persona);
                foreach (var notificaDestinatario in notificheDestinatari)
                {
                    var notifica = await _unitOfWork.Notifiche.Get(notificaDestinatario.UIDNotifica);
                    if (notifica != null && !notifica.Chiuso && !notifica.UIDEM.HasValue &&
                        !queryExtended.AttiDaFirmare.Contains(notifica.UIDAtto))
                        queryExtended.AttiDaFirmare.Add(notifica.UIDAtto);
                }

                var myAttiProponente = await _unitOfWork.DASI.GetAttiProponente(persona.UID_persona);
                queryExtended.AttiDaFirmare.AddRange(myAttiProponente);
            }
        }

        private async Task RestoreFiltersIfEmptyResult(BaseRequest<AttoDASIDto> model, Filter<ATTI_DASI> queryFilter,
            PersonaDto persona, ClientModeEnum clientMode, QueryExtendedRequest queryExtended)
        {
            queryFilter.ImportStatements(model.filtro);
            await GetResponseCountBar(persona, clientMode, queryFilter, queryExtended);

            if (queryExtended.Proponenti.Any())
                model.filtro.AddRange(CreateFilterStatements(nameof(AttoDASIDto.UIDPersonaProponente),
                    queryExtended.Proponenti));
            if (queryExtended.Provvedimenti.Any())
                model.filtro.AddRange(CreateFilterStatements(nameof(AttoDASIDto.Abbinamenti),
                    queryExtended.Provvedimenti));
            if (queryExtended.Stati.Any())
                model.filtro.AddRange(CreateFilterStatements(nameof(AttoDASIDto.IDStato), queryExtended.Stati));
            if (queryExtended.Tipi.Any())
                model.filtro.AddRange(CreateFilterStatements(nameof(AttoDASIDto.Tipo), queryExtended.Tipi));
            if (queryExtended.TipiRispostaRichiesta.Any())
                model.filtro.AddRange(CreateFilterStatements(nameof(AttoDASIDto.IDTipo_Risposta),
                    queryExtended.TipiRispostaRichiesta));
            if (queryExtended.TipiChiusura.Any())
                model.filtro.AddRange(CreateFilterStatements(nameof(AttoDASIDto.TipoChiusuraIter),
                    queryExtended.TipiChiusura));
            if (queryExtended.TipiVotazione.Any())
                model.filtro.AddRange(CreateFilterStatements(nameof(AttoDASIDto.TipoVotazioneIter),
                    queryExtended.TipiVotazione));
            if (queryExtended.TipiDocumento.Any())
                model.filtro.AddRange(
                    CreateFilterStatements(nameof(AttoDASIDto.Documenti), queryExtended.TipiDocumento));
            if (queryExtended.Risposte.Any())
                model.filtro.AddRange(
                    CreateFilterStatements(nameof(AttoDASIDto.Risposte), queryExtended.Risposte));
        }

        private List<FilterStatement<AttoDASIDto>> CreateFilterStatements<T>(string propertyId, List<T> values)
        {
            return values.Select(value => new FilterStatement<AttoDASIDto>
                { PropertyId = propertyId, Value = value.ToString() }).ToList();
        }

        private async Task<List<AttoDASIDto>> GetAttoDtos(List<Guid> atti_in_db, PersonaDto persona)
        {
            var result = new List<AttoDASIDto>();
            foreach (var attoUId in atti_in_db)
            {
                var dto = await GetAttoDto(attoUId, persona);
                result.Add(dto);
            }

            return result;
        }


        private async Task<CountBarData> GetResponseCountBar(PersonaDto persona, ClientModeEnum clientMode,
            Filter<ATTI_DASI> queryFilter, QueryExtendedRequest queryExtended)
        {
            var filtro = PulisciFiltro(queryFilter);
            var result = new CountBarData
            {
                ITL = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.ITL, clientMode, filtro, queryExtended),
                ITR = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.ITR, clientMode, filtro, queryExtended),
                IQT = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.IQT, clientMode, filtro, queryExtended),
                MOZ = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.MOZ, clientMode, filtro, queryExtended),
                ODG = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.ODG, clientMode, filtro, queryExtended),
                RIS = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.RIS, clientMode, filtro, queryExtended),
                TUTTI = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.TUTTI, clientMode, filtro, queryExtended),
                BOZZE = await _unitOfWork
                    .DASI
                    .Count(persona, StatiAttoEnum.BOZZA, clientMode, filtro, queryExtended),
                PRESENTATI = await _unitOfWork
                    .DASI
                    .Count(persona, StatiAttoEnum.PRESENTATO, clientMode, filtro, queryExtended),
                IN_TRATTAZIONE = await _unitOfWork
                    .DASI
                    .Count(persona, StatiAttoEnum.IN_TRATTAZIONE, clientMode, filtro, queryExtended),
                CHIUSO = await _unitOfWork
                    .DASI
                    .Count(persona, StatiAttoEnum.COMPLETATO, clientMode, filtro, queryExtended)
            };

            return result;
        }

        public async Task<List<Guid>> GetSoloIds(BaseRequest<AttoDASIDto> model, PersonaDto persona, Uri uri)
        {
            var queryExtended = CreateQueryExtendedRequest(model);

            model.param.TryGetValue("CLIENT_MODE", out var CLIENT_MODE); // per trattazione aula
            model.param.TryGetValue("RequireMySign", out var RequireMySign); // #539
            if (RequireMySign == null)
                RequireMySign = false;

            await AddRequireMySignData(queryExtended, persona, Convert.ToBoolean(RequireMySign));

            var queryFilter = new Filter<ATTI_DASI>();
            queryFilter.ImportStatements(model.filtro);
            return await _unitOfWork.DASI.GetAll(persona, model.page, model.size, GetClientMode(CLIENT_MODE),
                queryFilter, queryExtended, model.dettagliOrdinamento);
        }

        public async Task<AttoDASIDto> GetAttoDto(Guid attoUid)
        {
            var attoInDb = await _unitOfWork.DASI.Get(attoUid);

            var dto = Mapper.Map<ATTI_DASI, AttoDASIDto>(attoInDb);

            dto.DisplayTipo = Utility.GetText_Tipo(attoInDb.Tipo);

            if (dto.Tipo == (int)TipoAttoEnum.RIS)
            {
                if (!string.IsNullOrEmpty(dto.NAtto))
                {
                    dto.NAtto = GetNome(attoInDb.NAtto, attoInDb.Progressivo);
                    dto.Display = $"{dto.DisplayTipo} {dto.NAtto}";
                    dto.DisplayExtended = $"{Utility.GetText_TipoEstesoDASI(dto.Tipo)} n. {dto.NAtto}";
                }
            }
            else
            {
                dto.NAtto = GetNome(attoInDb.NAtto, attoInDb.Progressivo);

                dto.Display = $"{dto.DisplayTipo} {dto.NAtto}";
                dto.DisplayExtended = $"{Utility.GetText_TipoEstesoDASI(dto.Tipo)} n. {dto.NAtto}";
                dto.Firma_da_ufficio = await _unitOfWork.Atti_Firme.CheckFirmatoDaUfficio(attoUid);
                dto.Firmato_Dal_Proponente =
                    await _unitOfWork.Atti_Firme.CheckFirmato(attoUid, attoInDb.UIDPersonaProponente);
                dto.ConteggioFirme = await _logicAttiFirme.CountFirme(attoUid);

                if (dto.id_gruppo > 0)
                    dto.gruppi_politici =
                        Mapper.Map<View_gruppi_politici_con_giunta, GruppiDto>(
                            await _unitOfWork.Gruppi.Get(attoInDb.id_gruppo));

                if (!string.IsNullOrEmpty(attoInDb.FirmeCartacee))
                    dto.FirmeCartacee = JsonConvert.DeserializeObject<List<KeyValueDto>>(attoInDb.FirmeCartacee);
            }

            dto.DisplayTipoRispostaRichiesta = Utility.GetText_TipoRispostaDASI(dto.IDTipo_Risposta);
            dto.DisplayStato = Utility.GetText_StatoDASI(dto.IDStato);
            dto.DisplayAreaPolitica = Utility.GetText_AreaPolitica(dto.AreaPolitica);
            dto.DisplayTipoMozione = Utility.GetText_TipoMOZDASI(dto.TipoMOZ);

            if (attoInDb.TipoChiusuraIter.HasValue)
                dto.DisplayTipoChiusuraIter = Utility.GetText_ChiusuraIterDASI(attoInDb.TipoChiusuraIter.Value);
            if (attoInDb.TipoVotazioneIter.HasValue)
                dto.DisplayTipoVotazioneIter = Utility.GetText_TipoVotazioneDASI(attoInDb.TipoVotazioneIter.Value);

            if (!string.IsNullOrEmpty(attoInDb.DataPresentazione))
                dto.DataPresentazione = BALHelper.Decrypt(attoInDb.DataPresentazione);
            if (!string.IsNullOrEmpty(attoInDb.DataPresentazione_MOZ))
                dto.DataPresentazione_MOZ = BALHelper.Decrypt(attoInDb.DataPresentazione_MOZ);
            if (!string.IsNullOrEmpty(attoInDb.DataPresentazione_MOZ_URGENTE))
                dto.DataPresentazione_MOZ_URGENTE = BALHelper.Decrypt(attoInDb.DataPresentazione_MOZ_URGENTE);
            if (!string.IsNullOrEmpty(attoInDb.DataPresentazione_MOZ_ABBINATA))
                dto.DataPresentazione_MOZ_ABBINATA = BALHelper.Decrypt(attoInDb.DataPresentazione_MOZ_ABBINATA);
            if (!string.IsNullOrEmpty(attoInDb.DataRichiestaIscrizioneSeduta))
                dto.DataRichiestaIscrizioneSeduta = BALHelper.Decrypt(attoInDb.DataRichiestaIscrizioneSeduta);

            dto.Risposte = await _unitOfWork.DASI.GetRisposte(attoInDb.UIDAtto);
            dto.Monitoraggi = await _unitOfWork.DASI.GetMonitoraggi(attoInDb.UIDAtto);
            dto.Documenti = await _unitOfWork.DASI.GetDocumenti(attoInDb.UIDAtto);
            dto.Abbinamenti = await _unitOfWork.DASI.GetAbbinamenti(attoInDb.UIDAtto);
            dto.Note = await _unitOfWork.DASI.GetNote(attoInDb.UIDAtto);
            if (attoInDb.Tipo == (int)TipoAttoEnum.RIS)
                dto.CommissioniProponenti = await _unitOfWork.DASI.GetCommissioniProponenti(attoInDb.UIDAtto);
            
            if (dto.ConteggioFirme > 1)
            {
                var firme = await _logicAttiFirme.GetFirme(attoInDb, FirmeTipoEnum.ATTIVI);
                dto.Firme = firme
                    .Where(f => f.UID_persona != attoInDb.UIDPersonaProponente)
                    .Select(f => Utility.ConvertiCaratteriSpeciali(f.FirmaCert))
                    .Aggregate((i, j) => i + "<br>" + j);
            }

            dto.PersonaProponente = attoInDb.UIDPersonaProponente != null
                ? Users.First(p => p.UID_persona == attoInDb.UIDPersonaProponente)
                : dto.PersonaCreazione;

            if (attoInDb.UIDSeduta.HasValue)
                dto.Seduta = Mapper.Map<SEDUTE, SeduteDto>(await _unitOfWork.Sedute.Get(attoInDb.UIDSeduta.Value));

            var commissioni = await _unitOfWork.DASI.GetCommissioni(dto.UIDAtto);
            dto.Organi = commissioni
                .Select(Mapper.Map<View_Commissioni_attive, OrganoDto>).ToList();

            return dto;
        }

        public async Task<AttoDASIDto> GetAttoDto(Guid attoUid, PersonaDto persona)
        {
            var attoInDb = await _unitOfWork.DASI.Get(attoUid);
            if (attoInDb == null)
                return null;
            var dto = Mapper.Map<ATTI_DASI, AttoDASIDto>(attoInDb);

            dto.NAtto = GetNome(attoInDb.NAtto, attoInDb.Progressivo);
            dto.DisplayTipo = Utility.GetText_Tipo(attoInDb.Tipo);
            dto.Display = $"{dto.DisplayTipo} {dto.NAtto}";
            dto.DisplayExtended = $"{Utility.GetText_TipoEstesoDASI(dto.Tipo)} n. {dto.NAtto}"; // #1039
            dto.DisplayTipoRispostaRichiesta = Utility.GetText_TipoRispostaDASI(dto.IDTipo_Risposta);
            dto.DisplayStato = Utility.GetText_StatoDASI(dto.IDStato);
            dto.DisplayAreaPolitica = Utility.GetText_AreaPolitica(dto.AreaPolitica);

            try
            {
                if (!string.IsNullOrEmpty(attoInDb.DataPresentazione))
                    dto.DataPresentazione = BALHelper.Decrypt(attoInDb.DataPresentazione);
                if (!string.IsNullOrEmpty(attoInDb.DataPresentazione_MOZ))
                    dto.DataPresentazione_MOZ = BALHelper.Decrypt(attoInDb.DataPresentazione_MOZ);
                if (!string.IsNullOrEmpty(attoInDb.DataPresentazione_MOZ_URGENTE))
                    dto.DataPresentazione_MOZ_URGENTE = BALHelper.Decrypt(attoInDb.DataPresentazione_MOZ_URGENTE);
                if (!string.IsNullOrEmpty(attoInDb.DataPresentazione_MOZ_ABBINATA))
                    dto.DataPresentazione_MOZ_ABBINATA = BALHelper.Decrypt(attoInDb.DataPresentazione_MOZ_ABBINATA);
                if (!string.IsNullOrEmpty(attoInDb.DataRichiestaIscrizioneSeduta))
                    dto.DataRichiestaIscrizioneSeduta = BALHelper.Decrypt(attoInDb.DataRichiestaIscrizioneSeduta);

                if (!string.IsNullOrEmpty(attoInDb.Atto_Certificato))
                    dto.Atto_Certificato = BALHelper.Decrypt(attoInDb.Atto_Certificato, attoInDb.Hash);

                if (persona != null && (persona.CurrentRole == RuoliIntEnum.Consigliere_Regionale ||
                                        persona.CurrentRole == RuoliIntEnum.Assessore_Sottosegretario_Giunta))
                    dto.Firmato_Da_Me = await _unitOfWork.Atti_Firme.CheckFirmato(attoUid, persona.UID_persona);

                dto.Firma_da_ufficio = await _unitOfWork.Atti_Firme.CheckFirmatoDaUfficio(attoUid);
                dto.Firmato_Dal_Proponente =
                    await _unitOfWork.Atti_Firme.CheckFirmato(attoUid, attoInDb.UIDPersonaProponente);

                dto.PersonaCreazione = Users.FirstOrDefault(p => p.UID_persona == attoInDb.UIDPersonaCreazione);
                dto.PersonaProponente = attoInDb.UIDPersonaProponente != null
                    ? Users.First(p => p.UID_persona == attoInDb.UIDPersonaProponente)
                    : dto.PersonaCreazione;

                if (dto.UIDPersonaModifica.HasValue)
                    dto.PersonaModifica =
                        Users.First(p => p.UID_persona == attoInDb.UIDPersonaModifica);
                
                if (!dto.IsRIS())
                {
                    dto.ConteggioFirme = await _logicAttiFirme.CountFirme(attoUid);
                    if (dto.ConteggioFirme > 1)
                    {
                        var firme = await _logicAttiFirme.GetFirme(attoInDb, FirmeTipoEnum.ATTIVI);
                        dto.Firme = firme
                            .Where(f => f.UID_persona != attoInDb.UIDPersonaProponente)
                            .Select(f => Utility.ConvertiCaratteriSpeciali(f.FirmaCert))
                            .Aggregate((i, j) => i + "<br>" + j);
                    }

                    // #1049
                    var firme_dopo_deposito = await _logicAttiFirme.GetFirme(attoInDb, FirmeTipoEnum.DOPO_DEPOSITO);
                    if(firme_dopo_deposito.Any())
                    {
                        dto.Firme_dopo_deposito = firme_dopo_deposito
                            .Select(f => Utility.ConvertiCaratteriSpeciali(f.FirmaCert))
                            .Aggregate((i, j) => i + "<br>" + j);
                    }
                    
                    // #1048
                    var firme_ritirate = await _logicAttiFirme.GetFirme(attoInDb, FirmeTipoEnum.RITIRATI);
                    if(firme_ritirate.Any())
                    {
                        dto.Firme_ritirate = firme_ritirate
                            .Select(f => Utility.ConvertiCaratteriSpeciali(f.FirmaCert))
                            .Aggregate((i, j) => i + "<br>" + j);
                    }

                    dto.gruppi_politici =
                        Mapper.Map<View_gruppi_politici_con_giunta, GruppiDto>(
                            await _unitOfWork.Gruppi.Get(attoInDb.id_gruppo));
                }

                if (!string.IsNullOrEmpty(attoInDb.FirmeCartacee))
                    dto.FirmeCartacee = JsonConvert.DeserializeObject<List<KeyValueDto>>(attoInDb.FirmeCartacee);

                if (persona != null)
                {
                    if (string.IsNullOrEmpty(attoInDb.DataPresentazione))
                        dto.Presentabile = _unitOfWork
                            .DASI
                            .CheckIfPresentabile(dto,
                                persona);

                    dto.Firmabile = await _unitOfWork
                        .Atti_Firme
                        .CheckIfFirmabile(dto,
                            persona);

                    if (!dto.DataRitiro.HasValue)
                        dto.Ritirabile = _unitOfWork
                            .DASI
                            .CheckIfRitirabile(dto,
                                persona);

                    if (string.IsNullOrEmpty(attoInDb.DataPresentazione))
                        dto.Eliminabile = _unitOfWork
                            .DASI
                            .CheckIfEliminabile(dto,
                                persona);

                    dto.Modificabile = _unitOfWork
                        .DASI
                        .CheckIfModificabile(dto,
                            persona);

                    dto.Invito_Abilitato = _unitOfWork
                        .Notifiche
                        .CheckIfNotificabile(dto,
                            persona);
                }

                var commissioni = await _unitOfWork.DASI.GetCommissioni(dto.UIDAtto);
                dto.Organi = commissioni
                    .Select(Mapper.Map<View_Commissioni_attive, OrganoDto>).ToList();

                if (attoInDb.IDStato >= (int)StatiAttoEnum.PRESENTATO
                    && attoInDb.IDStato != (int)StatiAttoEnum.BOZZA_CARTACEA)
                {
                    SEDUTE sedutaInDb = null;

                    if (!dto.UIDSeduta.HasValue)
                        try
                        {
                            if (!string.IsNullOrEmpty(dto.DataRichiestaIscrizioneSeduta))
                                sedutaInDb =
                                    await _logicSedute.GetSeduta(Convert.ToDateTime(dto.DataRichiestaIscrizioneSeduta));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    else
                        sedutaInDb = await _logicSedute.GetSeduta(dto.UIDSeduta.Value);

                    if (sedutaInDb != null)
                    {
                        dto.Seduta = Mapper.Map<SEDUTE, SeduteDto>(sedutaInDb);

                        var presentato_oltre_termini = IsOutdate(dto);
                        dto.PresentatoOltreITermini = presentato_oltre_termini;
                    }
                }

                if (attoInDb.Tipo == (int)TipoAttoEnum.MOZ && attoInDb.TipoMOZ == (int)TipoMOZEnum.ABBINATA &&
                    attoInDb.UID_MOZ_Abbinata.HasValue)
                {
                    var attoAbbinato = await _unitOfWork.DASI.Get(attoInDb.UID_MOZ_Abbinata.Value);
                    dto.MOZ_Abbinata =
                        $"{Utility.GetText_Tipo(attoAbbinato.Tipo)} {GetNome(attoAbbinato.NAtto, attoAbbinato.Progressivo)}";
                }

                if (attoInDb.Tipo == (int)TipoAttoEnum.ODG && attoInDb.UID_Atto_ODG.HasValue)
                {
                    var attoPem = await _unitOfWork.Atti.Get(attoInDb.UID_Atto_ODG.Value);
                    dto.ODG_Atto_PEM = attoPem.IDTipoAtto == (int)TipoAttoEnum.ALTRO
                        ? "Dibattito"
                        : $"{Utility.GetText_Tipo(attoPem.IDTipoAtto)} {attoPem.NAtto}";

                    dto.ODG_Atto_Oggetto_PEM = attoPem.Oggetto;
                }

                dto.DettaglioMozioniAbbinate = await GetDettagioMozioniAbbinate(dto.UIDAtto);

                if (attoInDb.TipoChiusuraIter.HasValue)
                    dto.DisplayTipoChiusuraIter = Utility.GetText_ChiusuraIterDASI(attoInDb.TipoChiusuraIter.Value);
                if (attoInDb.TipoVotazioneIter.HasValue)
                    dto.DisplayTipoVotazioneIter = Utility.GetText_TipoVotazioneDASI(attoInDb.TipoVotazioneIter.Value);

                dto.Risposte = await _unitOfWork.DASI.GetRisposte(attoInDb.UIDAtto);
                dto.Monitoraggi = await _unitOfWork.DASI.GetMonitoraggi(attoInDb.UIDAtto);
                dto.Documenti = await _unitOfWork.DASI.GetDocumenti(attoInDb.UIDAtto);
                dto.Note = await _unitOfWork.DASI.GetNote(attoInDb.UIDAtto);
                dto.Abbinamenti = await _unitOfWork.DASI.GetAbbinamenti(attoInDb.UIDAtto);
                if (attoInDb.Tipo == (int)TipoAttoEnum.RIS)
                {
                    dto.CommissioniProponenti = await _unitOfWork.DASI.GetCommissioniProponenti(attoInDb.UIDAtto);

                    dto.PersonaRelatore1 = Users.FirstOrDefault(p => p.UID_persona == attoInDb.UIDPersonaRelatore1);
                    dto.PersonaRelatore2 = Users.FirstOrDefault(p => p.UID_persona == attoInDb.UIDPersonaRelatore2);
                    dto.PersonaRelatoreMinoranza =
                        Users.FirstOrDefault(p => p.UID_persona == attoInDb.UIDPersonaRelatoreMinoranza);
                }

                return dto;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<string> GetDettagioMozioniAbbinate(Guid uidAtto)
        {
            var sb = new List<string>();
            var atti = await _unitOfWork.DASI.GetAbbinamentiMozione(uidAtto);
            if (!atti.Any()) return string.Empty;

            foreach (var guid in atti)
            {
                var moz_abbinata = await GetAttoDto(guid);
                sb.Add($"{Utility.GetText_Tipo(moz_abbinata.Tipo)} {moz_abbinata.NAtto}");
            }

            return sb.Aggregate((i, j) => i + "<br>" + j);
        }

        private Filter<ATTI_DASI> PulisciFiltro(Filter<ATTI_DASI> filtro)
        {
            var filtro_pulito = new List<FilterStatement<ATTI_DASI>>();
            var filtri_da_rimuovere = filtro.Statements.Where(f => f.PropertyId != nameof(AttoDASIDto.Tipo));
            foreach (var filterStatement in filtri_da_rimuovere)
                filtro_pulito.Add(new FilterStatement<ATTI_DASI>
                {
                    PropertyId = filterStatement.PropertyId,
                    Value = filterStatement.Value,
                    Value2 = filterStatement.Value2,
                    Connector = filterStatement.Connector,
                    Operation = filterStatement.Operation
                });

            var result = new Filter<ATTI_DASI>();
            result.ImportStatements(filtro_pulito);
            return result;
        }

        private TipoAttoEnum GetResponseTypeFromFilters(List<FilterStatement<AttoDASIDto>> modelFiltro)
        {
            if (modelFiltro == null)
                return TipoAttoEnum.TUTTI;
            if (!modelFiltro.Any()) return TipoAttoEnum.TUTTI;

            var typeFilter = modelFiltro
                .FirstOrDefault(item => item.PropertyId == nameof(AttoDASIDto.Tipo));
            if (typeFilter == null)
                return TipoAttoEnum.TUTTI;

            return (TipoAttoEnum)Convert.ToInt16(typeFilter.Value);
        }

        private StatiAttoEnum GetResponseStatusFromFilters(List<FilterStatement<AttoDASIDto>> modelFiltro)
        {
            if (modelFiltro == null)
                return StatiAttoEnum.TUTTI;
            if (!modelFiltro.Any()) return StatiAttoEnum.TUTTI;

            var statusFilter = modelFiltro
                .FirstOrDefault(item => item.PropertyId == nameof(AttoDASIDto.IDStato));
            if (statusFilter == null)
                return StatiAttoEnum.TUTTI;

            return (StatiAttoEnum)Convert.ToInt16(statusFilter.Value);
        }

        public async Task<Dictionary<Guid, string>> Firma(ComandiAzioneModel firmaModel, PersonaDto persona,
            PinDto pin, bool firmaUfficio = false)
        {
            try
            {
                if (!persona.IsConsigliereRegionale && !firmaUfficio)
                    throw new Exception("Ruolo non abilitato alla firma di atti");

                var results = new Dictionary<Guid, string>();
                var counterFirme = 1;

                foreach (var idGuid in firmaModel.Lista)
                {
                    if (counterFirme == Convert.ToInt32(AppSettingsConfiguration.LimiteFirmaMassivo) + 1) break;

                    var attoInDb = await _unitOfWork.DASI.Get(idGuid);
                    var atto = await GetAttoDto(idGuid, persona);
                    var firmabile = await _unitOfWork
                        .Atti_Firme
                        .CheckIfFirmabile(atto,
                            persona, firmaUfficio);
                    var nome_atto = atto.Display;

                    if (!firmabile)
                    {
                        results.Add(idGuid,
                            $"ERROR: Atto {nome_atto} non è più sottoscrivibile");
                        continue;
                    }

                    var firmaCert = string.Empty;
                    var primoFirmatario = false;

                    var timestampFirma = DateTime.Now;
                    var dataFirma = BALHelper.EncryptString(timestampFirma.ToString("dd/MM/yyyy HH:mm:ss"),
                        AppSettingsConfiguration.masterKey);

                    if (firmaUfficio)
                    {
                        firmaCert = BALHelper.EncryptString($"{persona.DisplayName_GruppoCode}",
                            AppSettingsConfiguration.masterKey); // matcat - #615
                        timestampFirma = atto.Timestamp.Value.AddMinutes(-2);
                        dataFirma = BALHelper.EncryptString(timestampFirma.ToString("dd/MM/yyyy HH:mm"),
                            AppSettingsConfiguration.masterKey);
                    }
                    else
                    {
                        //Controllo se l'utente ha già firmato
                        if (atto.Firmato_Da_Me) continue;

                        if (atto.Tipo == (int)TipoAttoEnum.IQT)
                            if (string.IsNullOrEmpty(atto.DataRichiestaIscrizioneSeduta))
                                if (atto.UIDPersonaProponente.Value == persona.UID_persona)
                                {
                                    results.Add(idGuid,
                                        $"ERROR: E' necessario richiedere l'iscrizione dell'atto {nome_atto} in una seduta prima di poterlo firmare.");
                                    continue;
                                }

                        //Controllo la firma del proponente
                        if (!atto.Firmato_Dal_Proponente && atto.UIDPersonaProponente.Value != persona.UID_persona)
                        {
                            results.Add(idGuid, $"ERROR: Il Proponente non ha ancora firmato l'atto {nome_atto}");
                            continue;
                        }

                        var info_codice_carica_gruppo = persona.Gruppo.codice_gruppo;

                        var bodyFirmaCert =
                            $"{persona.DisplayName} ({info_codice_carica_gruppo})";
                        firmaCert = BALHelper.EncryptString(bodyFirmaCert
                            , AppSettingsConfiguration.masterKey);
                    }

                    var countFirme = await _unitOfWork.Atti_Firme.CountFirme(idGuid);
                    if (countFirme == 0)
                    {
                        //Se è la prima firma dell'atto, questo viene cryptato e così certificato e non modificabile
                        attoInDb.Hash = firmaUfficio
                            ? BALHelper.EncryptString(AppSettingsConfiguration.MasterPIN,
                                AppSettingsConfiguration.masterKey)
                            : pin.PIN;
                        attoInDb.UIDPersonaPrimaFirma = persona.UID_persona;
                        attoInDb.DataPrimaFirma = timestampFirma;
                        var body = await GetBodyDASI(attoInDb, new List<AttiFirmeDto>
                            {
                                new AttiFirmeDto
                                {
                                    UIDAtto = idGuid,
                                    UID_persona = persona.UID_persona,
                                    FirmaCert = firmaCert,
                                    Data_firma = dataFirma,
                                    Timestamp = timestampFirma,
                                    ufficio = firmaUfficio
                                }
                            }, persona,
                            TemplateTypeEnum.FIRMA);
                        var body_encrypt = BALHelper.EncryptString(body,
                            firmaUfficio ? AppSettingsConfiguration.MasterPIN : pin.PIN_Decrypt);

                        attoInDb.Atto_Certificato = body_encrypt;
                        primoFirmatario = true;
                    }

                    var destinatario_notifica =
                        await _unitOfWork.Notifiche_Destinatari.ExistDestinatarioNotifica(idGuid, persona.UID_persona,
                            true);

                    var id_gruppo = persona.Gruppo?.id_gruppo ?? 0;
                    var valida = !(id_gruppo != attoInDb.id_gruppo && destinatario_notifica == null && !firmaUfficio);
                    var prioritario = true;
                    if (atto.Tipo == (int)TipoAttoEnum.IQT
                        && atto.UIDPersonaProponente.Value != persona.UID_persona
                        && (atto.IDStato == (int)StatiAttoEnum.BOZZA ||
                            atto.IDStato == (int)StatiAttoEnum.BOZZA_CARTACEA))
                    {
                        var sedutaRichiesta =
                            await _unitOfWork.Sedute.Get(Convert.ToDateTime(atto.DataRichiestaIscrizioneSeduta));
                        if (sedutaRichiesta == null) throw new Exception("Seduta richiesta non valida!");

                        var iqt_in_seduta = await _unitOfWork.DASI.GetAttiBySeduta(sedutaRichiesta.UIDSeduta,
                            TipoAttoEnum.IQT, 0);
                        var iqt_proposte = await _unitOfWork.DASI.GetProposteAtti(
                            BALHelper.EncryptString(atto.DataRichiestaIscrizioneSeduta,
                                AppSettingsConfiguration.masterKey),
                            TipoAttoEnum.IQT, 0);
                        var iqt_da_esaminare = new List<ATTI_DASI>();
                        iqt_da_esaminare.AddRange(iqt_in_seduta.Where(a => !a.TipoChiusuraIter.HasValue));
                        iqt_da_esaminare.AddRange(iqt_proposte);
                        if (iqt_da_esaminare.FindIndex(i => i.UIDAtto == atto.UIDAtto) != -1)
                            iqt_da_esaminare.RemoveAt(iqt_da_esaminare.FindIndex(i => i.UIDAtto == atto.UIDAtto));

                        var iqt_firmatari =
                            await _unitOfWork.Atti_Firme.GetFirmatari(iqt_da_esaminare.Select(i => i.UIDAtto).ToList(),
                                AppSettingsConfiguration.MinimoConsiglieriIQT);

                        if (iqt_firmatari.Any(f => f.UID_persona == persona.UID_persona && f.Prioritario))
                            prioritario = false;
                    }
                    else if (atto.Tipo == (int)TipoAttoEnum.IQT
                             && atto.UIDPersonaProponente.Value != persona.UID_persona
                             && atto.IDStato != (int)StatiAttoEnum.BOZZA
                             && atto.IDStato != (int)StatiAttoEnum.BOZZA_CARTACEA)
                    {
                        // #976
                        prioritario = false;
                    }

                    await _unitOfWork.Atti_Firme.Firma(idGuid, persona.UID_persona, id_gruppo, firmaCert, dataFirma,
                        timestampFirma,
                        firmaUfficio, primoFirmatario, valida, persona.IsCapoGruppo, prioritario, countFirme);

                    if (destinatario_notifica != null)
                    {
                        await _unitOfWork.Notifiche_Destinatari.SetSeen_DestinatarioNotifica(destinatario_notifica,
                            persona.UID_persona);
                    }
                    else if (!valida)
                    {
                        var newNotifica = new NOTIFICHE
                        {
                            UIDNotifica = Guid.NewGuid().ToString(),
                            UIDAtto = atto.UIDAtto,
                            Mittente = persona.UID_persona,
                            RuoloMittente = (int)persona.CurrentRole,
                            IDTipo = (int)TipoNotificaEnum.RICHIESTA,
                            Messaggio = $"Richiesta di sottoscrivere l'atto {nome_atto}",
                            DataCreazione = DateTime.Now,
                            IdGruppo = atto.id_gruppo,
                            SyncGUID = Guid.NewGuid(),
                            Valida = false
                        };

                        var newDestinatario = new NOTIFICHE_DESTINATARI
                        {
                            NOTIFICHE = newNotifica,
                            UIDPersona = atto.UIDPersonaProponente.Value,
                            IdGruppo = atto.id_gruppo,
                            UID = Guid.NewGuid()
                        };

                        _unitOfWork.Notifiche_Destinatari.Add(newDestinatario);
                        await _unitOfWork.CompleteAsync();
                    }

                    if (valida && (atto.IDStato == (int)StatiAttoEnum.PRESENTATO ||
                                   atto.IDStato == (int)StatiAttoEnum.IN_TRATTAZIONE))
                        try
                        {
                            var mailModel = new MailModel
                            {
                                DA = AppSettingsConfiguration.EmailInvioDASI,
                                A = AppSettingsConfiguration.EmailInvioDASI,
                                OGGETTO =
                                    $"Aggiunta firma all'atto {nome_atto}",
                                MESSAGGIO =
                                    $"Il consigliere {persona.DisplayName_GruppoCode} ha firmato l'atto {nome_atto}. {GetBodyFooterMail()}"
                            };
                            await _logicUtil.InvioMail(mailModel);
                        }
                        catch (Exception e)
                        {
                            Log.Error("Invio Mail - Firma", e);
                        }

                    results.Add(idGuid, $"{nome_atto} - {(valida ? "OK" : "?!?")}");
                    counterFirme++;
                }

                return results;
            }
            catch (Exception e)
            {
                Log.Error("Logic - Firma - DASI", e);
                throw e;
            }
        }

        public async Task<Dictionary<Guid, string>> RitiroFirma(ComandiAzioneModel firmaModel,
            PersonaDto persona)
        {
            var results = new Dictionary<Guid, string>();

            foreach (var idGuid in firmaModel.Lista)
            {
                var atto = await _unitOfWork.DASI.Get(idGuid);
                if (atto == null)
                {
                    results.Add(idGuid, "ERROR: NON TROVATO");
                    continue;
                }

                if (atto.IsChiuso)
                    throw new InvalidOperationException(
                        "Non è possibile ritirare la firma.");

                if (atto.DataIscrizioneSeduta.HasValue)
                {
                    if (atto.Tipo == (int)TipoAttoEnum.ITL
                        && atto.IDTipo_Risposta == (int)TipoRispostaEnum.ORALE)
                    {
                        //https://github.com/Consiglio-Regionale-della-Lombardia/PEM/issues/467
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "Per ritirare un atto già iscritto ad una seduta contatta la Segreteria dell’Assemblea.");
                    }
                }

                var richiestaPresente =
                    await _unitOfWork.Notifiche.EsisteRitiroDasi(atto.UIDAtto, persona.UID_persona);
                if (richiestaPresente)
                    throw new InvalidOperationException(
                        "Richiesta di ritiro già inviata al proponente.");

                var dto = await GetAttoDto(idGuid);
                var nome_atto = dto.Display;

                SEDUTE seduta = null;
                if (!string.IsNullOrEmpty(dto.DataRichiestaIscrizioneSeduta))
                    seduta = await _unitOfWork.Sedute.Get(Convert.ToDateTime(dto.DataRichiestaIscrizioneSeduta));

                var firme = await _logicAttiFirme.GetFirme(atto, FirmeTipoEnum.ATTIVI);
                var firmeList = firme.ToList();
                var miaFirma = firmeList.First(f => f.UID_persona == persona.UID_persona);
                firmeList.Remove(miaFirma); // #853 Rimuovo la mia firma dai conteggi per il controllo delle firme.

                var result_check = await ControlloFirmePresentazione(dto, firmeList.Count(f => f.Prioritario), seduta);
                if (!string.IsNullOrEmpty(result_check))
                    switch ((TipoAttoEnum)atto.Tipo)
                    {
                        case TipoAttoEnum.IQT:
                        case TipoAttoEnum.MOZ:
                        {
                            if (atto.TipoMOZ == (int)TipoMOZEnum.URGENTE)
                            {
                                var checkIfFirmatoDaiCapigruppo =
                                    await _unitOfWork.DASI.CheckIfFirmatoDaiCapigruppo(atto.UIDAtto);
                                if (!checkIfFirmatoDaiCapigruppo)
                                {
                                    // Matteo Cattapan #535 - Avviso perdita urgenza di una mozione
                                    // Quando, a seguito del ritiro di una firma necessaria, una mozione perde l’urgenza, deve essere inviato un alert via email
                                    // agli altri firmatari e alla segreteria dell’assemblea
                                    var firmatari = new List<string>();
                                    foreach (var attiFirmeDto in firme)
                                    {
                                        if (attiFirmeDto.UID_persona == persona.UID_persona)
                                            continue;

                                        var firmatario = await _logicPersona.GetPersona(attiFirmeDto.UID_persona);
                                        firmatari.Add(firmatario.email);
                                    }

                                    firmatari.Add(AppSettingsConfiguration.EmailInvioDASI);

                                    try
                                    {
                                        var mailModel = new MailModel
                                        {
                                            DA = AppSettingsConfiguration.EmailInvioDASI,
                                            A = firmatari.Aggregate((i, j) => i + ";" + j),
                                            OGGETTO =
                                                $"Non può essere trattata la mozione {nome_atto} come urgente",
                                            MESSAGGIO =
                                                $"Il consigliere {persona.DisplayName_GruppoCode} ha ritirato la propria firma dall'atto {nome_atto}. Non c’è più il numero necessario di firme per trattare la mozione con urgenza. {GetBodyFooterMail()}"
                                        };
                                        await _logicUtil.InvioMail(mailModel);
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Error("Invio Mail", e);
                                    }
                                }

                                break;
                            }

                            // #748 - Ritiro firma da parte del proponente - Se il proponente ritira la propria firma non invia il messaggio di notifica
                            if (atto.UIDPersonaProponente == persona.UID_persona) break;

                            var _guid = Guid.NewGuid();
                            var sync_guid = Guid.NewGuid();

                            var newNotifica = new NOTIFICHE
                            {
                                UIDAtto = atto.UIDAtto,
                                Mittente = persona.UID_persona,
                                RuoloMittente = (int)persona.CurrentRole,
                                IDTipo = (int)TipoNotificaEnum.RITIRO,
                                Messaggio =
                                    $"Richiesta di ritiro firma dall'atto {nome_atto}. L'atto non avrà più il numero di firme minime richieste e decadrà per mancanza di firme.",
                                DataCreazione = DateTime.Now,
                                IdGruppo = atto.id_gruppo,
                                SyncGUID = sync_guid,
                                UIDNotifica = _guid.ToString()
                            };

                            _unitOfWork.Notifiche.Add(newNotifica);

                            var newDestinatario = new NOTIFICHE_DESTINATARI
                            {
                                UIDNotifica = _guid.ToString(),
                                UIDPersona = atto.UIDPersonaProponente.Value,
                                IdGruppo = atto.id_gruppo,
                                UID = Guid.NewGuid()
                            };

                            _unitOfWork.Notifiche_Destinatari.Add(newDestinatario);

                            await _unitOfWork.CompleteAsync();

                            throw new InvalidOperationException(
                                "INFO: Se ritiri la firma l'atto decadrà in quanto non ci sarà più il numero di firme necessario. La richiesta di ritiro firma è stata inviata al proponente dell'atto.");
                        }
                    }

                if (firme.Count() == 1
                    || !string.IsNullOrEmpty(result_check))
                {
                    if (atto.Tipo == (int)TipoAttoEnum.ITL
                        && atto.IDTipo_Risposta == (int)TipoRispostaEnum.ORALE
                        && atto.DataIscrizioneSeduta.HasValue)
                        throw new InvalidOperationException(
                            "Per ritirare un atto già iscritto ad una seduta contatta la Segreteria dell’Assemblea.");

                    //RITIRA ATTO
                    if (atto.Tipo == (int)TipoAttoEnum.MOZ && !string.IsNullOrEmpty(result_check) && firme.Count() > 1)
                    {
                        if (atto.TipoMOZ == (int)TipoMOZEnum.URGENTE)
                        {
                            atto.TipoMOZ = (int)TipoMOZEnum.ORDINARIA;

                            // #844
                            try
                            {
                                var mailModel = new MailModel
                                {
                                    DA = persona.email,
                                    A = AppSettingsConfiguration.EmailInvioDASI,
                                    OGGETTO = $"Perdita urgenza {nome_atto}",
                                    MESSAGGIO =
                                        $"Il consigliere {persona.DisplayName_GruppoCode} ha ritirato la firma dall'atto {nome_atto} con oggetto \"{atto.Oggetto}\" che non potrà essere trattato con urgenza. {GetBodyFooterMail()}"
                                };
                                await _logicUtil.InvioMail(mailModel);
                            }
                            catch (Exception e)
                            {
                                Log.Error("Invio Mail - Perdita urgenza", e);
                            }
                        }
                    }
                    else
                    {
                        atto.IDStato = (int)StatiAttoEnum.COMPLETATO;
                        atto.TipoChiusuraIter = (int)TipoChiusuraIterEnum.RITIRATO;
                        atto.UIDPersonaRitiro = persona.UID_persona;
                        atto.DataRitiro = DateTime.Now;

                        // #844
                        try
                        {
                            var mailModel = new MailModel
                            {
                                DA = persona.email,
                                A = AppSettingsConfiguration.EmailInvioDASI,
                                OGGETTO = $"Ritiro effettuato da parte di {persona.DisplayName_GruppoCode}",
                                MESSAGGIO =
                                    $"Il consigliere {persona.DisplayName_GruppoCode} ha ritirato l'atto {nome_atto} con oggetto \"{atto.Oggetto}\". {GetBodyFooterMail()}"
                            };
                            await _logicUtil.InvioMail(mailModel);
                        }
                        catch (Exception e)
                        {
                            Log.Error("Invio Mail - Ritiro atto", e);
                        }
                    }
                }

                //RITIRA FIRMA
                var firmeAttive = await _unitOfWork
                    .Atti_Firme
                    .GetFirmatari(atto, FirmeTipoEnum.ATTIVI);
                ATTI_FIRME firma_utente;
                firma_utente = persona.IsSegreteriaAssemblea
                    ? firmeAttive.Single(f => f.ufficio)
                    : firmeAttive.Single(f => f.UID_persona == persona.UID_persona);

                firma_utente.Prioritario = false;
                firma_utente.Data_ritirofirma =
                    BALHelper.EncryptString(DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        AppSettingsConfiguration.masterKey);

                await _unitOfWork.CompleteAsync();
                results.Add(idGuid, $"{nome_atto} - OK");

                //Matteo Cattapan #525 - Cambio di proponente a seguito di ritiro firma primo firmatario
                if (atto.UIDPersonaProponente == persona.UID_persona
                    && string.IsNullOrEmpty(result_check))
                {
                    var firma = firme.First(f => string.IsNullOrEmpty(f.Data_ritirofirma) && f.Prioritario);
                    atto.UIDPersonaProponente = firma.UID_persona;
                    if (firma.id_gruppo != atto.id_gruppo) atto.id_gruppo = firma.id_gruppo;

                    await _unitOfWork.CompleteAsync();
                }

                try
                {
                    var mailModel = new MailModel
                    {
                        DA = persona.email,
                        A = AppSettingsConfiguration.EmailInvioDASI,
                        OGGETTO = $"Ritiro firma effettuato da parte di {persona.DisplayName_GruppoCode}",
                        MESSAGGIO =
                            $"Il consigliere {persona.DisplayName_GruppoCode} ha ritirato la propria firma da {nome_atto} con oggetto \"{atto.Oggetto}\". {GetBodyFooterMail()}"
                    };
                    await _logicUtil.InvioMail(mailModel);
                }
                catch (Exception e)
                {
                    Log.Error("Invio Mail - Ritiro firma", e);
                }
            }

            return results;
        }

        public async Task<Dictionary<Guid, string>> EliminaFirma(ComandiAzioneModel firmaModel,
            PersonaDto persona)
        {
            try
            {
                var results = new Dictionary<Guid, string>();

                foreach (var idGuid in firmaModel.Lista)
                {
                    var atto = await _unitOfWork.DASI.Get(idGuid);
                    if (atto == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    var countFirme = await _unitOfWork.Atti_Firme.CountFirme(idGuid);
                    if (countFirme == 1)
                    {
                        atto.Atto_Certificato = string.Empty;
                        atto.DataPrimaFirma = null;
                        atto.UIDPersonaPrimaFirma = null;
                    }

                    //RITIRA FIRMA
                    var firmeAttive = await _logicAttiFirme.GetFirme(atto, FirmeTipoEnum.ATTIVI);
                    var firma_utente = firmeAttive.Single(f => f.UID_persona == persona.UID_persona);
                    var firma_da_ritirare =
                        await _unitOfWork.Atti_Firme.FindInCache(firma_utente.UIDAtto, firma_utente.UID_persona);
                    _unitOfWork.Atti_Firme.Remove(firma_da_ritirare);

                    results.Add(idGuid, "OK");
                }

                await _unitOfWork.CompleteAsync();
                return results;
            }
            catch (Exception e)
            {
                Log.Error("Logic - EliminaFirma - DASI", e);
                throw e;
            }
        }

        public async Task<Dictionary<Guid, string>> Presenta(ComandiAzioneModel model,
            PersonaDto persona)
        {
            var results = new Dictionary<Guid, string>();
            var counterPresentazioni = 1;
            var legislaturaId = await _unitOfWork.Legislature.Legislatura_Attiva();
            var legislatura = await _unitOfWork.Legislature.Get(legislaturaId);

            var id_gruppo = 0;

            ManagerLogic.BloccaPresentazione = true;

            var attachList = new List<AllegatoMail>();
            foreach (var idGuid in model.Lista)
            {
                if (counterPresentazioni ==
                    Convert.ToInt32(AppSettingsConfiguration.LimitePresentazioneMassivo) + 1) break;

                var atto = await _unitOfWork.DASI.Get(idGuid);
                if (atto == null)
                {
                    results.Add(idGuid, "ERROR: NON TROVATO");
                    continue;
                }

                if (id_gruppo == 0)
                    id_gruppo = atto.id_gruppo;

                var attoDto = await GetAttoDto(idGuid, persona);
                var nome_atto = $"{Utility.GetText_Tipo(attoDto.Tipo)} {attoDto.NAtto}";
                if (atto.IDStato >= (int)StatiAttoEnum.PRESENTATO) continue;

                if (!attoDto.Presentabile)
                {
                    results.Add(idGuid,
                        !attoDto.Firmato_Dal_Proponente
                            ? $"ERROR: {nome_atto} non depositabile. Deposito dell’atto consentito solo al proponente o al capogruppo, dopo la firma del proponente"
                            : $"ERROR: {nome_atto} non depositabile");
                    continue;
                }

                if (atto.Tipo == (int)TipoAttoEnum.IQT
                    && string.IsNullOrEmpty(atto.DataRichiestaIscrizioneSeduta))
                {
                    results.Add(idGuid,
                        $"ERROR: {nome_atto} non depositabile. Data seduta non indicata: scegli prima la data della seduta a cui iscrivere l’IQT.");
                    continue;
                }

                ATTI attoPEM = null;
                SEDUTE seduta = null;
                if (atto.Tipo == (int)TipoAttoEnum.ODG)
                {
                    /*
                     *  Ogni consigliere può depositare, come primo firmatario, fino a {MassimoODG} ODG per atto/argomento e fino alla "data scadenza ODG"
                     *  (poi risultano fuori orario se < {MassimoODG} e comunque sono bloccati se > {MassimoODG}).
                     *
                     *  I capigruppo il giorno della seduta possono presentare fino a {MassimoODG_DuranteSeduta} ODG per atto/argomento, a prescindere da quanti ne hanno presentati prima
                     *  (quindi il quarto è sempre bloccato) e fino a quando non viene attivato il falg BloccoODG
                     */

                    //Atto PEM associato all'ODG
                    attoPEM = await _unitOfWork.Atti.Get(atto.UID_Atto_ODG.Value);
                    //Seduta associata all'atto PEM
                    seduta = await _unitOfWork.Sedute.Get(attoPEM.UIDSeduta.Value);

                    //Blocco inserimento ODG
                    if (attoPEM.BloccoODG)
                    {
                        results.Add(idGuid,
                            $"ERROR: {nome_atto} non depositabile. Non puoi depositare altri ordini del giorno.");
                        continue;
                    }

                    var dataRichiesta = BALHelper.EncryptString(seduta.Data_seduta.ToString("dd/MM/yyyy"),
                        AppSettingsConfiguration.masterKey);
                    atto.DataRichiestaIscrizioneSeduta = dataRichiesta;
                    atto.UIDPersonaRichiestaIscrizione = persona.UID_persona;

                    //Ricava tutti gli ODG iscritti in seduta
                    var odg_in_seduta = await _unitOfWork.DASI.GetAttiBySeduta(atto.UIDSeduta.Value,
                        TipoAttoEnum.ODG, 0);
                    //Ricava tutti gli ODG proposti in seduta
                    var odg_proposte = await _unitOfWork.DASI.GetProposteAtti(atto.DataRichiestaIscrizioneSeduta,
                        TipoAttoEnum.ODG, 0);

                    var atti = new List<ATTI_DASI>();
                    atti.AddRange(odg_in_seduta.Where(a => !a.TipoChiusuraIter.HasValue));
                    atti.AddRange(odg_proposte);

                    //Ricava tutti gli ODG iscritti in seduta

                    //Atti filtrati per consigliere primo firmatario tra gli atti presentati in seduta
                    var my_atti = atti.Where(a => a.UIDPersonaProponente == attoDto.UIDPersonaProponente
                                                  && (a.IDStato == (int)StatiAttoEnum.COMPLETATO
                                                      || a.IDStato == (int)StatiAttoEnum.PRESENTATO
                                                      || a.IDStato == (int)StatiAttoEnum.IN_TRATTAZIONE))
                        .Distinct()
                        .ToList();

                    //Jolly attivo limite impostato {MassimoODG_Jolly}
                    // #840 Funzione Jolly
                    if (attoPEM.Jolly)
                    {
                        if (my_atti.Count + 1 >=
                            AppSettingsConfiguration.MassimoODG_Jolly)
                        {
                            results.Add(idGuid,
                                $"ERROR: {nome_atto} non depositabile. Non puoi depositare altri ordini del giorno per l'atto {Utility.GetText_Tipo(attoPEM.IDTipoAtto)} {attoPEM.NAtto}.");
                            continue;
                        }
                    }
                    else
                    {
                        // https://github.com/Consiglio-Regionale-della-Lombardia/PEM/issues/886
                        var capogruppo = await _unitOfWork.Gruppi.GetCapoGruppo(atto.id_gruppo);
                        var proponente = await _logicPersona.GetPersona(atto.UIDPersonaProponente.Value);
                        if (capogruppo != null)
                            if (capogruppo.id_persona == proponente.id_persona)
                                proponente.IsCapoGruppo = true;
                        var dataOdierna = DateTime.Now;
                        // https://github.com/Consiglio-Regionale-della-Lombardia/PEM/issues/919
                        var dataSedutaPerODG = new DateTime(
                            seduta.Data_seduta.Year,
                            seduta.Data_seduta.Month,
                            seduta.Data_seduta.Day);

                        if (proponente.IsCapoGruppo
                            && dataSedutaPerODG <= dataOdierna)
                        {
                            var atti_dopo_scadenza =
                                my_atti.Where(a => a.Timestamp >= dataSedutaPerODG
                                                   && a.UID_Atto_ODG ==
                                                   attoPEM
                                                       .UIDAtto) // #852 - aggiunto UID_Atto_ODG per avere il conteggio solo del provvedimento selezionato
                                    .ToList();
                            if (atti_dopo_scadenza.Count + 1 > AppSettingsConfiguration.MassimoODG_DuranteSeduta)
                            {
                                results.Add(idGuid,
                                    $"ERROR: {nome_atto} non depositabile. Non puoi depositare altri ordini del giorno per l'atto {Utility.GetText_Tipo(attoPEM.IDTipoAtto)} {attoPEM.NAtto}.");

                                continue;
                            }

                            atto.CapogruppoNeiTermini = true;
                        }
                        else
                        {
                            //Matteo Cattapan #484
                            //Massimo ODG presentabili per provvedimento
                            var group_odg_per_atto = my_atti.GroupBy(dasi => dasi.UID_Atto_ODG)
                                .OrderBy(group => group.Key)
                                .Select(group => Tuple.Create(group.Key, group.Count()));
                            var current_group =
                                group_odg_per_atto.FirstOrDefault(group => group.Item1 == atto.UID_Atto_ODG);
                            var count_odg_per_atto = 0;
                            if (current_group != null) count_odg_per_atto = current_group.Item2;

                            if (count_odg_per_atto + 1 > AppSettingsConfiguration.MassimoODG)
                            {
                                results.Add(idGuid,
                                    $"ERROR: {nome_atto} non depositabile. Non puoi depositare più di {AppSettingsConfiguration.MassimoODG} ordini del giorno per l'atto {Utility.GetText_Tipo(attoPEM.IDTipoAtto)} {attoPEM.NAtto}.");

                                continue;
                            }
                        }
                    }
                }

                //controllo max firme
                SEDUTE sedutaRichiesta = null;
                var count_firme = await _unitOfWork.Atti_Firme.CountFirmePrioritarie(idGuid);
                var controllo_firme = string.Empty;
                if (!string.IsNullOrEmpty(attoDto.DataRichiestaIscrizioneSeduta))
                    sedutaRichiesta =
                        await _unitOfWork.Sedute.Get(Convert.ToDateTime(attoDto.DataRichiestaIscrizioneSeduta));

                controllo_firme = await ControlloFirmePresentazione(attoDto, count_firme, sedutaRichiesta);

                if (!string.IsNullOrEmpty(controllo_firme))
                {
                    results.Add(idGuid, controllo_firme);
                    continue;
                }

                var contatore = await _unitOfWork.DASI.GetContatore(atto.Tipo, atto.IDTipo_Risposta);
                var contatore_progressivo = contatore.Inizio + contatore.Contatore;
                var etichetta_progressiva =
                    $"{Utility.GetText_Tipo(atto.Tipo)}_{contatore_progressivo}_{legislatura.num_legislatura}";
                var etichetta_encrypt =
                    BALHelper.EncryptString(etichetta_progressiva, AppSettingsConfiguration.masterKey);
                var checkProgressivo_unique =
                    await _unitOfWork.DASI.CheckProgressivo(etichetta_encrypt);

                if (!checkProgressivo_unique)
                {
                    results.Add(idGuid, $"ERROR: Progressivo {etichetta_progressiva} occupato");
                    continue;
                }

                atto.NAtto_search = contatore_progressivo;
                atto.Etichetta = etichetta_progressiva;
                atto.UIDPersonaPresentazione = persona.UID_persona;
                atto.OrdineVisualizzazione = contatore_progressivo;
                atto.Timestamp = DateTime.Now;
                atto.DataPresentazione = BALHelper.EncryptString(atto.Timestamp.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                    AppSettingsConfiguration.masterKey);
                atto.IDStato = (int)StatiAttoEnum.PRESENTATO;

                atto.NAtto = etichetta_encrypt;

                atto.chkf = count_firme.ToString();

                _unitOfWork.DASI.IncrementaContatore(contatore);

                await _unitOfWork.CompleteAsync();

                var new_nome_atto =
                    $"{Utility.GetText_Tipo(atto.Tipo)} {GetNome(atto.NAtto, atto.Progressivo)}";

                var content = await PDFIstantaneo(atto, null);
                attachList.Add(new AllegatoMail(content, $"{new_nome_atto}.pdf"));
                results.Add(idGuid, $"{new_nome_atto} - OK");

                if (atto.Tipo == (int)TipoAttoEnum.ODG)
                    if (atto.Non_Passaggio_In_Esame)
                        try
                        {
                            var mailModel = new MailModel
                            {
                                DA = persona.email,
                                A = AppSettingsConfiguration.EmailInvioDASI,
                                OGGETTO =
                                    "[ODG DI NON PASSAGGIO ALL'ESAME]",
                                MESSAGGIO =
                                    $"Il consigliere {persona.DisplayName_GruppoCode} ha depositato l' {nome_atto} di non passaggio all'esame per il provvedimento: <br> {Utility.GetText_Tipo(attoPEM.IDTipoAtto)} {attoPEM.NAtto} - {attoPEM.Oggetto}. {GetBodyFooterMail()}"
                            };
                            await _logicUtil.InvioMail(mailModel);
                        }
                        catch (Exception)
                        {
                            Log.Error("Invio Mail - ODG non passaggio all'esame");
                        }


                counterPresentazioni++;

                try
                {
                    // https://github.com/Consiglio-Regionale-della-Lombardia/PEM/issues/906
                    _unitOfWork.Stampe.Add(new STAMPE
                    {
                        UIDStampa = Guid.NewGuid(),
                        UIDUtenteRichiesta = persona.UID_persona,
                        CurrentRole = (int)persona.CurrentRole,
                        DataRichiesta = DateTime.Now,
                        UIDAtto = idGuid,
                        Da = 1,
                        A = 1,
                        Ordine = 1,
                        Notifica = true,
                        Scadenza = DateTime.Now.AddDays(Convert.ToDouble(AppSettingsConfiguration.GiorniValiditaLink)),
                        DASI = true
                    });
                    await _unitOfWork.CompleteAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (attachList.Any())
            {
                //#864 Notifiche per responsabili di segreteria
                var responsabili = await _logicPersona.GetSegreteriaPolitica(id_gruppo, false, true);
                var destinatari = AppSettingsConfiguration.EmailInvioDASI;
                if (responsabili.Any())
                    destinatari += ";" + responsabili.Select(p => p.email).Aggregate((i, j) => i + ";" + j);

                var mailModel = new MailModel
                {
                    DA = persona.email,
                    A = destinatari,
                    OGGETTO = $"Deposito effettuato da parte di {persona.DisplayName_GruppoCode}",
                    MESSAGGIO =
                        $"E' stato effettuato il deposito da {persona.DisplayName_GruppoCode} degli atti in allegato. {GetBodyFooterMail()}",
                    ATTACHMENTS = attachList
                };
                await _logicUtil.InvioMail(mailModel);
            }

            return results;
        }

        internal async Task<string> ControlloFirmePresentazione(AttoDASIDto atto, int count_firme,
            SEDUTE seduta_attiva = null,
            bool solo_anomalie = false,
            string error_title = "Atto non presentabile")
        {
            if (atto.Tipo == (int)TipoAttoEnum.IQT
                || (atto.Tipo == (int)TipoAttoEnum.MOZ && atto.TipoMOZ == (int)TipoMOZEnum.URGENTE)
                || (atto.Tipo == (int)TipoAttoEnum.MOZ && atto.TipoMOZ == (int)TipoMOZEnum.SFIDUCIA)
                || (atto.Tipo == (int)TipoAttoEnum.MOZ && atto.TipoMOZ == (int)TipoMOZEnum.CENSURA))
            {
                var firmatari =
                    await _logicAttiFirme.GetFirme(Mapper.Map<AttoDASIDto, ATTI_DASI>(atto), FirmeTipoEnum.TUTTE);
                var firme = firmatari.Where(i => string.IsNullOrEmpty(i.Data_ritirofirma) && i.Prioritario).ToList();
                var firmatari_di_altri_gruppi = firme.Any(i => i.id_gruppo != atto.id_gruppo);

                var minimo_consiglieri_config = AppSettingsConfiguration.MinimoConsiglieriIQT;
                if (atto.Tipo == (int)TipoAttoEnum.MOZ)
                {
                    if (atto.TipoMOZ == (int)TipoMOZEnum.URGENTE)
                        minimo_consiglieri_config = AppSettingsConfiguration.MinimoConsiglieriMOZU;
                    if (atto.TipoMOZ == (int)TipoMOZEnum.SFIDUCIA
                        || atto.TipoMOZ == (int)TipoMOZEnum.CENSURA)
                        minimo_consiglieri_config = AppSettingsConfiguration.MinimoConsiglieriMOZC_MOZS;
                }

                var minimo_consiglieri = minimo_consiglieri_config;
                var consiglieriGruppo =
                    await _unitOfWork.Gruppi.GetConsiglieriGruppo(atto.Legislatura, atto.id_gruppo);
                var count_consiglieri = consiglieriGruppo.Count();
                var minimo_firme = count_consiglieri < minimo_consiglieri && !firmatari_di_altri_gruppi
                                                                          && atto.TipoMOZ != (int)TipoMOZEnum.SFIDUCIA
                                                                          && atto.TipoMOZ != (int)TipoMOZEnum.CENSURA
                    ? count_consiglieri
                    : minimo_consiglieri;

                if (count_firme < minimo_firme)
                    return
                        $"{error_title}. Firme {count_firme}/{minimo_firme}. Mancano {minimo_firme - count_firme} firme.";

                if (seduta_attiva == null)
                    return default;

                var anomalie = new StringBuilder();
                if (atto.Tipo == (int)TipoAttoEnum.MOZ && atto.TipoMOZ == (int)TipoMOZEnum.URGENTE)
                {
                    var moz_in_seduta = await _unitOfWork.DASI.GetAttiBySeduta(seduta_attiva.UIDSeduta,
                        TipoAttoEnum.MOZ, TipoMOZEnum.URGENTE);
                    var moz_proposte = await _unitOfWork.DASI.GetProposteAtti(
                        BALHelper.EncryptString(seduta_attiva.Data_seduta.ToString("dd/MM/yyyy"),
                            AppSettingsConfiguration.masterKey),
                        TipoAttoEnum.MOZ, TipoMOZEnum.URGENTE);
                    var moz_da_esaminare = new List<ATTI_DASI>();
                    moz_da_esaminare.AddRange(moz_in_seduta.Where(a => !a.TipoChiusuraIter.HasValue));
                    moz_da_esaminare.AddRange(moz_proposte);
                    if (moz_da_esaminare.FindIndex(i => i.UIDAtto == atto.UIDAtto) != -1)
                        moz_da_esaminare.RemoveAt(moz_da_esaminare.FindIndex(i => i.UIDAtto == atto.UIDAtto));

                    var moz_firmatari =
                        await _unitOfWork.Atti_Firme.GetFirmatari(moz_da_esaminare.Select(i => i.UIDAtto).ToList(),
                            minimo_consiglieri_config);

                    foreach (var firma in firme.Take(minimo_consiglieri_config).ToList())
                    {
                        var firmatario_indagato =
                            $"{firma.FirmaCert}, firma non valida perchè già presente in [[LISTA]]; ";
                        var firmatario_valido = true;
                        foreach (var moz in moz_da_esaminare)
                        {
                            var firmatari_moz = moz_firmatari.Where(f => f.UIDAtto == moz.UIDAtto
                                                                         && string.IsNullOrEmpty(f.Data_ritirofirma))
                                .ToList();
                            if (firmatari_moz.All(item => item.UID_persona != firma.UID_persona)) continue;
                            firmatario_valido = false;
                            firmatario_indagato = firmatario_indagato.Replace("[[LISTA]]",
                                $"{Utility.GetText_Tipo(moz.Tipo)} {GetNome(moz.NAtto, moz.Progressivo)}");
                            break;
                        }

                        if (firmatario_valido) continue;
                        count_firme--;
                        anomalie.AppendLine(firmatario_indagato);
                    }
                }

                if (atto.Tipo == (int)TipoAttoEnum.IQT)
                {
                    var iqt_in_seduta = await _unitOfWork.DASI.GetAttiBySeduta(seduta_attiva.UIDSeduta,
                        TipoAttoEnum.IQT, 0);
                    var iqt_proposte = await _unitOfWork.DASI.GetProposteAtti(
                        BALHelper.EncryptString(atto.DataRichiestaIscrizioneSeduta, AppSettingsConfiguration.masterKey),
                        TipoAttoEnum.IQT, 0);
                    var iqt_da_esaminare = new List<ATTI_DASI>();
                    iqt_da_esaminare.AddRange(iqt_in_seduta.Where(a => !a.TipoChiusuraIter.HasValue));
                    iqt_da_esaminare.AddRange(iqt_proposte);
                    if (iqt_da_esaminare.FindIndex(i => i.UIDAtto == atto.UIDAtto) != -1)
                        iqt_da_esaminare.RemoveAt(iqt_da_esaminare.FindIndex(i => i.UIDAtto == atto.UIDAtto));

                    var iqt_firmatari =
                        await _unitOfWork.Atti_Firme.GetFirmatari(iqt_da_esaminare.Select(i => i.UIDAtto).ToList(),
                            minimo_consiglieri_config);

                    count_firme = minimo_consiglieri_config;

                    foreach (var firma in firme.Take(minimo_consiglieri_config).ToList())
                    {
                        var firmatario_indagato =
                            $"{firma.FirmaCert}, firma non valida perchè già presente in [[LISTA]]; ";
                        var firmatario_valido = true;
                        foreach (var iqt in iqt_da_esaminare)
                        {
                            var firmatari_iqt = iqt_firmatari.Where(f => f.UIDAtto == iqt.UIDAtto
                                                                         && string.IsNullOrEmpty(f.Data_ritirofirma))
                                .ToList();
                            if (firmatari_iqt.All(item => item.UID_persona != firma.UID_persona)) continue;
                            firmatario_valido = false;
                            firmatario_indagato = firmatario_indagato.Replace("[[LISTA]]",
                                $"{Utility.GetText_Tipo(iqt.Tipo)} {GetNome(iqt.NAtto, iqt.Progressivo)}");
                            break;
                        }

                        if (firmatario_valido) continue;
                        count_firme--;
                        anomalie.AppendLine(firmatario_indagato);
                    }
                }

                if (anomalie.Length > 0)
                {
                    if (solo_anomalie) return $"Riscontrate le seguenti anomalie: {anomalie}";

                    return
                        $"{error_title}. Firme {count_firme}/{minimo_firme}. Mancano firme valide. Riscontrate le seguenti anomalie: {anomalie}";
                }
            }

            return default;
        }

        internal async Task<string> ControlloFirmePresentazione(AttoDASIDto atto, bool solo_anomalie = false,
            SEDUTE seduta_attiva = null)
        {
            var count_firme = await _unitOfWork.Atti_Firme.CountFirmePrioritarie(atto.UIDAtto);
            return await ControlloFirmePresentazione(atto, count_firme, seduta_attiva, solo_anomalie);
        }

        public async Task<string> GetBodyDASI(ATTI_DASI atto, IEnumerable<AttiFirmeDto> firme, PersonaDto persona,
            TemplateTypeEnum template, bool privacy = false, bool enableQr = true)
        {
            try
            {
                var dto = await GetAttoDto(atto.UIDAtto, persona);

                try
                {
                    var body = GetTemplate(template, true);

                    body =
                        "<link href=\"https://fonts.googleapis.com/icon?family=Material+Icons\" rel=\"stylesheet\">" +
                        "<link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/materialize/1.0.0/css/materialize.min.css\">" +
                        $"<link rel=\"stylesheet\" href=\"{AppSettingsConfiguration.url_CLIENT}/content/site.css\">" +
                        body;
                    switch (template)
                    {
                        case TemplateTypeEnum.MAIL:
                            GetBody(dto, firme, persona, false, privacy, ref body);
                            break;
                        case TemplateTypeEnum.PDF:
                            GetBody(dto, firme, persona, enableQr, privacy, ref body);
                            break;
                        case TemplateTypeEnum.HTML:
                            GetBody(dto, firme, persona, false, true, ref body);
                            break;
                        case TemplateTypeEnum.FIRMA:
                            GetBodyTemporaneo(dto, privacy, ref body);
                            break;
                        case TemplateTypeEnum.HTML_MODIFICABILE:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(template), template, null);
                    }

                    return body;
                }
                catch (Exception e)
                {
                    Log.Error("GetBodyDASI", e);
                    throw e;
                }
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetBodyDASI", e);
                throw e;
            }
        }

        public async Task Elimina(ATTI_DASI atto, PersonaDto persona)
        {
            if (atto.DataIscrizioneSeduta.HasValue)
                throw new InvalidOperationException(
                    "L'atto è iscritto in seduta. Rivolgiti alla Segreteria dell'Assemblea per effettuare l'operazione.");

            if (!string.IsNullOrEmpty(atto.DataPresentazione))
            {
                // #977
                throw new InvalidOperationException(
                    "L'atto è già stato presentato. Puoi solamente effettuare il ritiro.");
            }

            atto.Eliminato = true;
            atto.DataElimina = DateTime.Now;
            atto.UIDPersonaElimina = persona.UID_persona;

            await _unitOfWork.CompleteAsync();

            // Matteo Cattapan #526 - Avviso eliminazione bozza
            //Se un atto in bozza è stato firmato da altri consiglieri e il proponente lo elimina,
            //il sistema invia un alert a tutti i firmatari comunicando l’eliminazione dell’Atto da parte del proponente
            if (atto.IDStato == (int)StatiAttoEnum.BOZZA
                || atto.IDStato == (int)StatiAttoEnum.BOZZA_RISERVATA)
            {
                var firme = await _logicAttiFirme.GetFirme(atto, FirmeTipoEnum.TUTTE);
                var firmatari = new List<string>();
                foreach (var attiFirmeDto in firme.Where(i => string.IsNullOrEmpty(i.Data_ritirofirma)))
                {
                    if (attiFirmeDto.UID_persona == persona.UID_persona)
                        continue;

                    var firmatario = await _logicPersona.GetPersona(attiFirmeDto.UID_persona);
                    firmatari.Add(firmatario.email);
                }

                if (firmatari.Count <= 0) return;

                try
                {
                    var nome_atto = $"{Utility.GetText_Tipo(atto.Tipo)} {atto.NAtto}";
                    var mailModel = new MailModel
                    {
                        DA = AppSettingsConfiguration.EmailInvioDASI,
                        A = firmatari.Aggregate((i, j) => i + ";" + j),
                        OGGETTO =
                            "Avviso di eliminazione bozza atto",
                        MESSAGGIO =
                            $"Il consigliere {persona.DisplayName_GruppoCode} ha eliminato la bozza {nome_atto} <br> {atto.Oggetto}. {GetBodyFooterMail()}"
                    };
                    await _logicUtil.InvioMail(mailModel);
                }
                catch (Exception e)
                {
                    Log.Error("Invio mail", e);
                }
            }
        }

        public async Task Ritira(ATTI_DASI atto, PersonaDto persona)
        {
            if (atto.IsChiuso)
                throw new InvalidOperationException(
                    "Non è possibile ritirare un atto chiuso.");

            if (atto.DataIscrizioneSeduta.HasValue)
                throw new InvalidOperationException(
                    "Per ritirare un atto già iscritto ad una seduta contatta la Segreteria dell’Assemblea.");

            atto.IDStato = (int)StatiAttoEnum.COMPLETATO;
            atto.TipoChiusuraIter = (int)TipoChiusuraIterEnum.RITIRATO;
            atto.UIDPersonaRitiro = persona.UID_persona;
            atto.DataRitiro = DateTime.Now;

            await _unitOfWork.CompleteAsync();

            var dto = await GetAttoDto(atto.UIDAtto);
            try
            {
                //#754
                var mailModel = new MailModel
                {
                    DA = AppSettingsConfiguration.EmailInvioDASI,
                    A = AppSettingsConfiguration.EmailInvioDASI,
                    OGGETTO =
                        $"Atto {dto.Display} ritirato dal proponente",
                    MESSAGGIO =
                        $"Il consigliere {persona.DisplayName_GruppoCode} ha ritirato l'atto {dto.Display}. {GetBodyFooterMail()}"
                };
                await _logicUtil.InvioMail(mailModel);
            }
            catch (Exception e)
            {
                Log.Error("Invio mail", e);
            }

            // Matteo Cattapan #530 - Avviso ritiro atto
            // Quando viene ritirato un Atto sottoscritto da più firmatari, il sistema deve inviare ai firmatari rimasti (che non hanno già ritirato la propria firma)
            // un messaggio email che notifica il ritiro dell’atto

            var firme = await _logicAttiFirme.GetFirme(atto, FirmeTipoEnum.TUTTE);
            var firmatari = new List<string>();
            foreach (var attiFirmeDto in firme.Where(i => string.IsNullOrEmpty(i.Data_ritirofirma)))
            {
                if (attiFirmeDto.UID_persona == persona.UID_persona)
                    continue;

                var firmatario = await _logicPersona.GetPersona(attiFirmeDto.UID_persona);
                firmatari.Add(firmatario.email);
            }

            if (firmatari.Count <= 0) return;

            try
            {
                var mailModel = new MailModel
                {
                    DA = AppSettingsConfiguration.EmailInvioDASI,
                    A = firmatari.Aggregate((i, j) => i + ";" + j),
                    OGGETTO =
                        $"Atto {dto.Display} ritirato dal proponente",
                    MESSAGGIO =
                        $"Il consigliere {persona.DisplayName_GruppoCode} ha appena ritirato l'atto {dto.Display} che anche lei aveva sottoscritto. {GetBodyFooterMail()}"
                };
                await _logicUtil.InvioMail(mailModel);
            }
            catch (Exception e)
            {
                Log.Error("Invio mail", e);
            }
        }

        public async Task<DASIFormModel> NuovoModello(TipoAttoEnum tipo, PersonaDto persona)
        {
            var result = new DASIFormModel
            {
                Atto = new AttoDASIDto
                {
                    Tipo = (int)tipo
                },
                CommissioniAttive = await GetCommissioniAttive()
            };

            var legislatura = await _unitOfWork.Legislature.Legislatura_Attiva();
            var progressivo =
                await _unitOfWork.DASI.GetProgressivo(tipo, persona.Gruppo.id_gruppo, legislatura);
            result.Atto.Progressivo = progressivo;

            if (persona.IsSegreteriaAssemblea
                || persona.IsPresidente)
                result.Atto.IDStato = (int)StatiAttoEnum.BOZZA;
            else
                result.Atto.IDStato = persona.Gruppo.abilita_em_privati
                    ? (int)StatiAttoEnum.BOZZA_RISERVATA
                    : (int)StatiAttoEnum.BOZZA;

            result.Atto.NAtto = GetNome(result.Atto.NAtto, progressivo);

            if (persona.IsConsigliereRegionale ||
                persona.IsAssessore)
            {
                result.Atto.UIDPersonaProponente = persona.UID_persona;
                result.Atto.PersonaProponente = new PersonaLightDto
                {
                    UID_persona = persona.UID_persona,
                    cognome = persona.cognome,
                    nome = persona.nome
                };
            }

            if (persona.IsSegreteriaPolitica)
                result.ListaGruppo = await _logicPersona.GetConsiglieriGruppo(persona.Gruppo.id_gruppo);

            result.Atto.UIDPersonaCreazione = persona.UID_persona;
            result.Atto.DataCreazione = DateTime.Now;
            result.Atto.idRuoloCreazione = (int)persona.CurrentRole;
            if (!persona.IsSegreteriaAssemblea
                && !persona.IsPresidente)
                result.Atto.id_gruppo = persona.Gruppo.id_gruppo;
            result.Atto.Organi = new List<OrganoDto>();

            var testo_richiesta = "<strong>{{RICHIESTA}}</strong>";
            switch (tipo)
            {
                case TipoAttoEnum.ITR:
                    result.Atto.Richiesta = testo_richiesta.Replace("{{RICHIESTA}}", "INTERROGA");
                    break;
                case TipoAttoEnum.ITL:
                    result.Atto.Richiesta = testo_richiesta.Replace("{{RICHIESTA}}", "INTERPELLA");
                    break;
            }

            return result;
        }

        public async Task<DASIFormModel> ModificaModello(ATTI_DASI atto, PersonaDto persona)
        {
            var dto = await GetAttoDto(atto.UIDAtto, persona);
            var result = new DASIFormModel
            {
                Atto = dto,
                CommissioniAttive = await GetCommissioniAttive()
            };

            if (persona.IsSegreteriaPolitica)
                result.ListaGruppo = await _logicPersona.GetConsiglieriGruppo(persona.Gruppo.id_gruppo);
            if (persona.IsSegreteriaAssemblea)
                result.ListaGruppo = await _logicPersona.GetConsiglieri();

            return result;
        }

        public async Task<List<AssessoreInCaricaDto>> GetSoggettiInterrogabili()
        {
            var result = await _unitOfWork.DASI.GetSoggettiInterrogabili();
            return result
                .Select(Mapper.Map<View_cariche_assessori_in_carica, AssessoreInCaricaDto>)
                .ToList();
        }

        public async Task<List<AttoDASIDto>> GetMOZAbbinabili(PersonaDto persona)
        {
            //Ricava tutte le mozioni abbinate proposte dal gruppo
            var proposte_di_abbinata = await _unitOfWork.DASI.GetProposteAtti(persona.Gruppo.id_gruppo,
                TipoAttoEnum.MOZ,
                TipoMOZEnum.ABBINATA);

            //Se è già presente una mozione abbinata allora escludo la seduta per evitare che vengano proposte altre mozioni abbinate dalla stessa persona
            var sedute_da_escludere = new List<Guid>();
            foreach (var proposta_abbinata in proposte_di_abbinata)
            {
                var moz_abbinata = await Get(proposta_abbinata.UID_MOZ_Abbinata.Value);
                sedute_da_escludere.Add(moz_abbinata.UIDSeduta.Value);
            }

            //Matteo Cattapan #460
            //Vengono prese tutte le sedute attive NON CHIUSE (con data chiusura vuota o con data chiusura futura) E CONVOCATE (data_apertura <= “a ora”)
            var sedute_attive = await _unitOfWork.Sedute.GetAttive(false, true);

            //Ricavo dalle sedute le mozioni a cui è possibile creare l'abbinamento
            var result = new List<AttoDASIDto>();
            foreach (var seduta in sedute_attive.Where(i => !sedute_da_escludere.Contains(i.UIDSeduta)))
            {
                var atti = await _unitOfWork.DASI.GetMOZAbbinabili(seduta.UIDSeduta);
                foreach (var atto in atti) result.Add(await GetAttoDto(atto.UIDAtto));
            }

            return result;
        }

        public async Task<List<AttiDto>> GetAttiSeduteAttive(PersonaDto persona)
        {
            var sedute_attive = await _unitOfWork.Sedute.GetAttive(false, true);

            var result = new List<AttiDto>();
            foreach (var seduta in sedute_attive)
            {
                var atti = await _logicAtti
                    .GetAtti(
                        new BaseRequest<AttiDto> { id = seduta.UIDSeduta, page = 1, size = 99 },
                        (int)ClientModeEnum.TRATTAZIONE,
                        persona);

                foreach (var atto in atti.Results)
                {
                    //Matteo Cattapan #439
                    //Aggiunta funzione per controllare che l'atto in seduta sia effettivamente aperto
                    if (!atto.IsAperto())
                        continue;

                    var tipo = Utility.GetText_Tipo(atto.IDTipoAtto);
                    var titolo_atto = $"{tipo} {atto.NAtto}";
                    if (atto.IDTipoAtto == (int)TipoAttoEnum.ALTRO) titolo_atto = atto.Oggetto;

                    atto.NAtto = $"{titolo_atto} - Seduta del {seduta.Data_seduta:dd/MM/yyyy HH:mm}";
                    result.Add(atto);
                }
            }

            return result;
        }

        public async Task<List<OrganoDto>> GetCommissioniAttive()
        {
            var result = await _unitOfWork.DASI.GetCommissioniAttive();
            return result
                .Select(Mapper.Map<View_Commissioni_attive, OrganoDto>)
                .ToList();
        }

        public async Task<Dictionary<Guid, string>> ModificaStato(ModificaStatoAttoModel model)
        {
            try
            {
                var results = new Dictionary<Guid, string>();
                foreach (var idGuid in model.Lista)
                {
                    var atto = await Get(idGuid);
                    if (atto == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    if (string.IsNullOrEmpty(atto.DataPresentazione))
                        continue;

                    atto.IDStato = (int)model.Stato;
                    await _unitOfWork.CompleteAsync();
                    results.Add(idGuid, "OK");
                }

                return results;
            }
            catch (Exception e)
            {
                Log.Error("Logic - ModificaStato", e);
                throw e;
            }
        }

        public async Task IscrizioneSeduta(IscriviSedutaDASIModel model,
            PersonaDto persona)
        {
            var listaRichieste = new Dictionary<Guid, string>();

            foreach (var guid in model.Lista)
            {
                var atto = await Get(guid);
                if (atto == null) throw new Exception("ERROR: NON TROVATO");
                if (atto.Tipo == (int)TipoAttoEnum.ITL
                    && (atto.IDTipo_Risposta == (int)TipoRispostaEnum.SCRITTA
                        || atto.IDTipo_Risposta == (int)TipoRispostaEnum.COMMISSIONE))
                    throw new Exception(
                        "ERROR: Non è possibile iscrivere in seduta per le ITL SCRITTE o IN COMMISSIONE.");
                if (atto.Tipo == (int)TipoAttoEnum.ITR)
                    throw new Exception(
                        "ERROR: Non è possibile iscrivere in seduta le ITR.");
                atto.UIDSeduta = model.UidSeduta;
                atto.DataIscrizioneSeduta = DateTime.Now;
                atto.UIDPersonaIscrizioneSeduta = persona.UID_persona;
                await _unitOfWork.CompleteAsync();
                var nomeAtto =
                    $"{Utility.GetText_Tipo(atto.Tipo)} {GetNome(atto.NAtto, atto.Progressivo)}";
                if (!listaRichieste.Any(item => (item.Value == nomeAtto
                                                 && item.Key == atto.UIDPersonaRichiestaIscrizione.Value)
                                                || item.Key == atto.UIDPersonaPresentazione.Value))
                    listaRichieste.Add(atto.UIDPersonaRichiestaIscrizione ?? atto.UIDPersonaPresentazione.Value,
                        nomeAtto);
            }

            try
            {
                var seduta = await _unitOfWork.Sedute.Get(model.UidSeduta);
                var gruppiMail = listaRichieste.GroupBy(item => item.Key);
                foreach (var gruppo in gruppiMail)
                {
                    var personaMail = await _unitOfWork.Persone.Get(gruppo.Key);
                    var mailModel = new MailModel
                    {
                        DA =
                            AppSettingsConfiguration.EmailInvioDASI,
                        A = personaMail.email,
                        OGGETTO =
                            "[ISCRIZIONE ATTI]",
                        MESSAGGIO =
                            $"La segreteria ha iscritto i seguenti atti alla seduta del {seduta.Data_seduta:dd/MM/yyyy}: <br> {gruppo.Select(item => item.Value).Aggregate((i, j) => i + "<br>" + j)}. {GetBodyFooterMail()}"
                    };
                    await _logicUtil.InvioMail(mailModel);
                }
            }
            catch (Exception e)
            {
                Log.Error("Invio mail", e);
            }
        }

        public async Task RichiediIscrizione(RichiestaIscrizioneDASIModel model, PersonaDto persona)
        {
            var dataRichiesta = BALHelper.EncryptString(model.DataRichiesta.ToString("dd/MM/yyyy"),
                AppSettingsConfiguration.masterKey);

            foreach (var guid in model.Lista)
            {
                var atto = await Get(guid);
                if (atto == null) throw new Exception("ERROR: NON TROVATO");
                if (atto.Tipo == (int)TipoAttoEnum.ITL
                    || atto.Tipo == (int)TipoAttoEnum.ITR)
                    throw new Exception(
                        $"ERROR: Non è possibile richiesere l'iscrizione in seduta per le {Utility.GetText_Tipo(atto.Tipo)}.");
                if (atto.Tipo == (int)TipoAttoEnum.IQT)
                {
                    var checkIscrizioneSeduta =
                        await _unitOfWork.DASI.CheckIscrizioneSedutaIQT(dataRichiesta,
                            atto.UIDPersonaProponente.Value);
                    if (!checkIscrizioneSeduta)
                        throw new Exception(
                            $"ERROR: Hai già presentato o sottoscritto 1 {Utility.GetText_Tipo(atto.Tipo)} per la seduta richiesta.");
                }

                if (atto.Tipo == (int)TipoAttoEnum.MOZ && atto.TipoMOZ == (int)TipoMOZEnum.ABBINATA)
                    throw new Exception(
                        "ERROR: La mozione è abbinata e non si può proporre un altra seduta.");
                if (atto.Tipo == (int)TipoAttoEnum.MOZ && atto.TipoMOZ == (int)TipoMOZEnum.URGENTE)
                    throw new Exception(
                        "ERROR: La mozione è urgente e non si può proporre un altra seduta.");

                if (atto.DataIscrizioneSeduta.HasValue)
                    throw new Exception(
                        "ERROR: L'atto è già iscritto in seduta. Contatta la segreteria dell'assemblea per cambiare la data di iscrizione.");

                atto.DataRichiestaIscrizioneSeduta = dataRichiesta;
                if (atto.Tipo == (int)TipoAttoEnum.MOZ)
                    atto.DataPresentazione_MOZ = BALHelper.EncryptString(
                        DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                        AppSettingsConfiguration.masterKey);
                atto.UIDPersonaRichiestaIscrizione = persona.UID_persona;
                await _unitOfWork.CompleteAsync();

                // Matteo Cattapan #533 
                // Avviso UOLA se atto fuori termine
                if (atto.IDStato < (int)StatiAttoEnum.PRESENTATO)
                    return;
                var attoDto = await GetAttoDto(atto.UIDAtto);
                attoDto.Seduta =
                    Mapper.Map<SEDUTE, SeduteDto>(
                        await _unitOfWork.Sedute.Get(Convert.ToDateTime(attoDto.DataRichiestaIscrizioneSeduta)));
                var out_of_date = IsOutdate(attoDto);
                try
                {
                    var mailModel = new MailModel
                    {
                        DA = persona.email,
                        A = AppSettingsConfiguration.EmailInvioDASI,
                        OGGETTO =
                            $"[RICHIESTA ISCRIZIONE] - {attoDto.Display} - {(out_of_date ? " FUORI TERMINE" : "")}",
                        MESSAGGIO =
                            $"Il consigliere {persona.DisplayName_GruppoCode} ha richiesto l'iscrizione dell' atto: <br> {attoDto.Display} <br> per la seduta del {model.DataRichiesta:dd/MM/yyyy}. {GetBodyFooterMail()}"
                    };
                    await _logicUtil.InvioMail(mailModel);
                }
                catch (Exception)
                {
                    Log.Error("Invio Mail - Richiesta Iscrizione");
                }
            }
        }

        public async Task RimuoviSeduta(IscriviSedutaDASIModel model)
        {
            try
            {
                foreach (var guid in model.Lista)
                {
                    var atto = await Get(guid);
                    if (atto == null) throw new Exception("ERROR: NON TROVATO");

                    if (atto.Tipo == (int)TipoAttoEnum.MOZ) atto.TipoMOZ = (int)TipoMOZEnum.ORDINARIA;

                    atto.UIDSeduta = null;
                    atto.DataIscrizioneSeduta = null;
                    atto.UIDPersonaIscrizioneSeduta = null;
                    await _unitOfWork.CompleteAsync();
                }
            }
            catch (Exception e)
            {
                Log.Error("Logic - RimuoviSeduta", e);
                throw e;
            }
        }

        public async Task RimuoviRichiesta(RichiestaIscrizioneDASIModel model, PersonaDto currentUser)
        {
            try
            {
                foreach (var guid in model.Lista)
                {
                    var atto = await Get(guid);
                    if (atto == null) throw new Exception("ERROR: NON TROVATO");

                    if (atto.UIDSeduta.HasValue)
                        throw new Exception(
                            "ERROR: Non è possibile rimuovere la richiesta. L'atto risulta già iscritto ad una seduta.");

                    if (atto.Tipo == (int)TipoAttoEnum.MOZ)
                    {
                        atto.DataPresentazione_MOZ = null;
                        atto.DataPresentazione_MOZ_ABBINATA = null;
                        atto.DataPresentazione_MOZ_URGENTE = null;
                        atto.TipoMOZ = (int)TipoMOZEnum.ORDINARIA;
                        atto.DataRichiestaIscrizioneSeduta = null;
                        atto.UIDPersonaRichiestaIscrizione = null;
                        await _unitOfWork.CompleteAsync();

                        //#844
                        if (atto.IDStato < (int)StatiAttoEnum.PRESENTATO)
                            return;

                        var dto = await GetAttoDto(guid);
                        try
                        {
                            var mailModel = new MailModel
                            {
                                DA = AppSettingsConfiguration.EmailInvioDASI,
                                A = AppSettingsConfiguration.EmailInvioDASI,
                                OGGETTO =
                                    $"Rimossa richiesta iscrizione per l'atto {dto.Display}",
                                MESSAGGIO =
                                    $"Il consigliere {currentUser.DisplayName_GruppoCode} ha rimosso la richiesta di iscrizione dall'atto {dto.Display}. {GetBodyFooterMail()}"
                            };
                            await _logicUtil.InvioMail(mailModel);
                        }
                        catch (Exception e)
                        {
                            Log.Error("Invio Mail", e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Logic - RimuoviRichiesta", e);
                throw e;
            }
        }

        public async Task ProponiMozioneUrgente(PromuoviMozioneModel model, PersonaDto persona)
        {
            try
            {
                var guid = model.Lista.First();
                var attoInDb = await Get(guid);
                if (attoInDb == null) throw new InvalidOperationException("ERROR: NON TROVATO");
                var atto = await GetAttoDto(guid);
                if (atto.Tipo != (int)TipoAttoEnum.MOZ)
                    throw new InvalidOperationException("ERROR: Operazione abilitata solo per le mozioni");
                if (atto.DataIscrizioneSeduta.HasValue)
                    throw new InvalidOperationException(
                        "L'atto è iscritto in seduta. Rivolgiti alla Segreteria dell'Assemblea per effettuare l'operazione.");

                var seduta = await _logicSedute.GetSeduta(model.DataRichiesta);
                var dateNow = DateTime.Now;
                var data_seduta_odierna = dateNow.Day == seduta.Data_seduta.Day
                                          && dateNow.Month == seduta.Data_seduta.Month
                                          && dateNow.Year == seduta.Data_seduta.Year;
                //E' possibile richiedere l'urgenza dalle 00 del giorno della seduta fino all'effettivo inizio
                if (!data_seduta_odierna)
                {
                    if (seduta.Data_effettiva_inizio < DateTime.Now)
                        //durante la seduta
                        throw new InvalidOperationException(
                            "Non è possibile richiedere l'urgenza durante la seduta. Rivolgiti alla Segreteria dell'Assemblea per chiedere informazioni.");

                    throw new InvalidOperationException(
                        "Attendi l’inizio della seduta per chiedere la trattazione d’urgenza.");
                }

                attoInDb.TipoMOZ = (int)TipoMOZEnum.URGENTE;
                atto.TipoMOZ = (int)TipoMOZEnum.URGENTE;
                attoInDb.DataPresentazione_MOZ_URGENTE = BALHelper.EncryptString(
                    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    AppSettingsConfiguration.masterKey);
                attoInDb.DataRichiestaIscrizioneSeduta = BALHelper.EncryptString(
                    seduta.Data_seduta.ToString("dd/MM/yyyy"),
                    AppSettingsConfiguration.masterKey);
                attoInDb.UIDPersonaRichiestaIscrizione = persona.UID_persona;

                //Matteo Cattapan #501
                //Controllo se la mozione è firmata dai capigruppo, in questo caso la mozione può passare avanti senza ulteriori controlli

                var checkIfFirmatoDaiCapigruppo = await _unitOfWork.DASI.CheckIfFirmatoDaiCapigruppo(attoInDb.UIDAtto);
                if (!checkIfFirmatoDaiCapigruppo)
                {
                    //Se non sono presenti tutti i capigruppo è necessario che si possa proporre l'urgenza solo di 1 mozione per seduta
                    var checkMozUrgente = await _unitOfWork.DASI.CheckMOZUrgente(seduta,
                        attoInDb.DataRichiestaIscrizioneSeduta, attoInDb.UIDPersonaProponente.Value);
                    if (!checkMozUrgente)
                        throw new InvalidOperationException(
                            $"ERROR: Hai già presentato o sottoscritto 1 MOZ Urgente per la seduta del {seduta.Data_seduta:dd/MM/yyyy}.");

                    //Controllo validità firme
                    var count_firme = await _unitOfWork.Atti_Firme.CountFirmePrioritarie(atto.UIDAtto);
                    var controllo_firme =
                        await ControlloFirmePresentazione(atto, count_firme, seduta,
                            false, "Non è possibile proporre l'urgenza");
                    if (!string.IsNullOrEmpty(controllo_firme)) throw new InvalidOperationException(controllo_firme);
                }

                attoInDb.MOZU_Capigruppo = checkIfFirmatoDaiCapigruppo;
                await _unitOfWork.CompleteAsync();

                // Matteo Cattapan #533
                // Invio mail a UOLA per avviso proposta urgenza fuori termine stabilito
                if (attoInDb.IDStato < (int)StatiAttoEnum.PRESENTATO)
                    return;
                atto = await GetAttoDto(guid);
                atto.Seduta =
                    Mapper.Map<SEDUTE, SeduteDto>(
                        await _unitOfWork.Sedute.Get(Convert.ToDateTime(atto.DataRichiestaIscrizioneSeduta)));
                if (IsOutdate(atto))
                    try
                    {
                        var mailModel = new MailModel
                        {
                            DA = persona.email,
                            A = AppSettingsConfiguration.EmailInvioDASI,
                            OGGETTO =
                                $"{atto.Display} – FUORI TERMINE",
                            MESSAGGIO =
                                $"Il consigliere {persona.DisplayName_GruppoCode} ha richiesto l'iscrizione effettuata fuori termine per il provvedimento: <br> {atto.Display}. {GetBodyFooterMail()}"
                        };
                        await _logicUtil.InvioMail(mailModel);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Invio Mail - Urgenza Fuori Termine", e);
                    }
                else
                    try
                    {
                        var mailModel = new MailModel
                        {
                            DA = AppSettingsConfiguration.EmailInvioDASI,
                            A = AppSettingsConfiguration.EmailInvioDASI,
                            OGGETTO =
                                $"Richiesta di trattazione urgente per la {atto.Display}",
                            MESSAGGIO =
                                $"Il consigliere {persona.DisplayName_GruppoCode} ha richiesto la trattazione d'urgenza per l'atto {atto.Display}. {GetBodyFooterMail()}"
                        };
                        await _logicUtil.InvioMail(mailModel);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Invio Mail - Urgenza", e);
                    }
            }
            catch (Exception e)
            {
                Log.Error("Logic - ProponiMozioneUrgente", e);
                throw e;
            }
        }

        public async Task ProponiMozioneAbbinata(PromuoviMozioneModel model, PersonaDto currentUser)
        {
            try
            {
                //#713 
                if (model.AttoUId == Guid.Empty)
                    throw new InvalidOperationException(
                        "Selezionare un atto da abbinare");

                var guid = model.Lista.First();
                var atto = await Get(guid);
                var dto = await GetAttoDto(guid, currentUser);
                if (atto == null) throw new InvalidOperationException("ERROR: NON TROVATO");
                if (atto.Tipo != (int)TipoAttoEnum.MOZ)
                    throw new InvalidOperationException("ERROR: Operazione abilitata solo per le mozioni");
                if (atto.DataIscrizioneSeduta.HasValue)
                    throw new InvalidOperationException(
                        "L'atto è iscritto in seduta. Rivolgiti alla Segreteria dell'Assemblea per effettuare l'operazione.");

                atto.TipoMOZ = (int)TipoMOZEnum.ABBINATA;
                atto.UID_MOZ_Abbinata = model.AttoUId;
                atto.DataPresentazione_MOZ_ABBINATA = BALHelper.EncryptString(
                    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    AppSettingsConfiguration.masterKey);

                // #842 MOZ Abbinate: iscrizione in seduta
                var attoAbbinato = await Get(model.AttoUId);
                var seduta = await _logicSedute.GetSeduta(attoAbbinato.UIDSeduta.Value);
                var dataRichiesta = BALHelper.EncryptString(seduta.Data_seduta.ToString("dd/MM/yyyy"),
                    AppSettingsConfiguration.masterKey);

                atto.DataRichiestaIscrizioneSeduta = dataRichiesta;
                atto.UIDPersonaRichiestaIscrizione = currentUser.UID_persona;

                await _unitOfWork.CompleteAsync();

                try
                {
                    //#844
                    if (atto.IDStato < (int)StatiAttoEnum.PRESENTATO)
                        return;
                    var dtoAbbinato = await GetAttoDto(model.AttoUId);
                    var mailModel = new MailModel
                    {
                        DA = AppSettingsConfiguration.EmailInvioDASI,
                        A = AppSettingsConfiguration.EmailInvioDASI,
                        OGGETTO =
                            $"Proposta di abbinata {dto.Display}",
                        MESSAGGIO =
                            $"Il consigliere {currentUser.DisplayName_GruppoCode} ha richiesto l'abbinamento dell'atto {dto.Display} con l'atto {dtoAbbinato.Display}. {GetBodyFooterMail()}"
                    };
                    await _logicUtil.InvioMail(mailModel);
                }
                catch (Exception e)
                {
                    Log.Error("Invio Mail - Abbinamento", e);
                }
            }
            catch (Exception e)
            {
                Log.Error("Logic - ProponiMozioneAbbinata", e);
                throw e;
            }
        }

        public async Task<List<Guid>> ScaricaAtti_UID(StatiAttoEnum stato, TipoAttoEnum tipo, PersonaDto persona)
        {
            var result = new List<Guid>();
            var filtro = new List<FilterStatement<AttoDASIDto>>();
            var filtroStato = new FilterStatement<AttoDASIDto>
            {
                PropertyId = nameof(AttoDASIDto.IDStato),
                Operation = Operation.EqualTo,
                Value = (int)stato,
                Connector = FilterStatementConnector.And
            };
            filtro.Add(filtroStato);
            if (tipo != TipoAttoEnum.TUTTI)
            {
                var filtroTipo = new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.Tipo),
                    Operation = Operation.EqualTo,
                    Value = (int)tipo
                };
                filtro.Add(filtroTipo);
            }

            var queryFilter = new Filter<ATTI_DASI>();
            queryFilter.ImportStatements(filtro);

            var counter_dasi = await _unitOfWork.DASI.Count(queryFilter);

            var dasiList = await GetDASI_UID_RawChunk(new BaseRequest<AttoDASIDto>
                {
                    page = 1,
                    size = counter_dasi
                },
                queryFilter,
                persona);

            result.AddRange(dasiList);

            return result;
        }

        public async Task<List<AttiFirmeDto>> ScaricaAtti_Firmatari(List<AttoDASIDto> attiList)
        {
            return await GetDASI_Firmatari_RawChunk(attiList);
        }

        public async Task<List<Guid>> GetDASI_UID_RawChunk(BaseRequest<AttoDASIDto> model, Filter<ATTI_DASI> filtro,
            PersonaDto persona)
        {
            try
            {
                var atti_in_db = await _unitOfWork
                    .DASI
                    .GetAll(persona
                        , model.page
                        , model.size
                        , ClientModeEnum.GRUPPI
                        , filtro
                        , new QueryExtendedRequest()
                        , new List<SortingInfo>());
                return atti_in_db;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetDASI_UID_RawChunk", e);
                throw e;
            }
        }

        public async Task<List<AttiFirmeDto>> GetDASI_Firmatari_RawChunk(List<AttoDASIDto> attiList)
        {
            try
            {
                var result = new List<AttiFirmeDto>();
                foreach (var atto in attiList)
                {
                    var firmatari = await _logicAttiFirme
                        .GetFirme(atto.UIDAtto);

                    result.AddRange(firmatari);
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetDASI_Firmatari_RawChunk", e);
                throw e;
            }
        }

        public async Task<IEnumerable<DestinatariNotificaDto>> GetInvitati(ATTI_DASI atto)
        {
            try
            {
                var invitati = await _unitOfWork
                    .DASI
                    .GetInvitati(atto.UIDAtto);
                var destinatari = invitati
                    .Select(Mapper.Map<NOTIFICHE_DESTINATARI, DestinatariNotificaDto>)
                    .ToList();
                var result = new List<DestinatariNotificaDto>();
                foreach (var destinatario in destinatari)
                {
                    destinatario.Firmato = await _unitOfWork
                        .Atti_Firme
                        .CheckFirmato(atto.UIDAtto, destinatario.UIDPersona);
                    result.Add(destinatario);
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetInvitati", e);
                throw e;
            }
        }

        public async Task<int> CountByQuery(ByQueryModel model)
        {
            try
            {
                var list = JsonConvert.DeserializeObject<List<Guid>>(model.Query);
                return list.Count;
            }
            catch (Exception e)
            {
                Log.Error("Logic - Count DASI By JsonQuery", e);
            }

            try
            {
                return await _unitOfWork.DASI.CountByQuery(model);
            }
            catch (Exception e)
            {
                Log.Error("Logic - Count DASI By Query", e);
                throw e;
            }
        }

        public async Task<IEnumerable<AttoDASIDto>> GetByQuery(ByQueryModel model)
        {
            try
            {
                var atti = new List<Guid>();

                try
                {
                    atti = JsonConvert.DeserializeObject<List<Guid>>(model.Query);
                }
                catch (Exception)
                {
                    atti = _unitOfWork
                        .DASI
                        .GetByQuery(model);
                }

                var result = new List<AttoDASIDto>();
                foreach (var idAtto in atti)
                {
                    var dto = await GetAttoDto(idAtto, null);
                    result.Add(dto);
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetByQuery", e);
                throw e;
            }
        }

        public async Task<string> GetCopertina(List<AttoDASIDto> atti)
        {
            var legislatura = await _unitOfWork.Legislature.Get(atti.First().Legislatura);
            var body = GetTemplate(TemplateTypeEnum.PDF_COPERTINA, true);
            body =
                "<link href=\"https://fonts.googleapis.com/icon?family=Material+Icons\" rel=\"stylesheet\">" +
                "<link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/materialize/1.0.0/css/materialize.min.css\">" +
                $"<link rel=\"stylesheet\" href=\"{AppSettingsConfiguration.url_CLIENT}/content/site.css\">" +
                body;
            body = body.Replace("{LEGISLATURA}", legislatura.num_legislatura);
            body = body.Replace("{nomePiattaforma}", AppSettingsConfiguration.Titolo);
            body = body.Replace("{urlLogo}", AppSettingsConfiguration.Logo);

            var templateItemIndice = GetTemplate(TemplateTypeEnum.INDICE_DASI);
            var bodyIndice = new StringBuilder();
            foreach (var dasiDto in atti)
                bodyIndice.Append(templateItemIndice
                    .Replace("{TipoAtto}", Utility.GetText_Tipo(dasiDto.Tipo))
                    .Replace("{NAtto}", dasiDto.NAtto)
                    .Replace("{Oggetto}", dasiDto.OggettoView())
                    .Replace("{Firmatari}",
                        $"{dasiDto.PersonaProponente.DisplayName} ({dasiDto.gruppi_politici.codice_gruppo}){(!string.IsNullOrEmpty(dasiDto.Firme) ? ", " + dasiDto.Firme.Replace("<br>", ", ") : "")}")
                    .Replace("{DataDeposito}",
                        string.IsNullOrEmpty(dasiDto.DataPresentazione)
                            ? ""
                            : "<br>Depositato il " + dasiDto.Timestamp.Value.ToString("dd/MM/yyyy")));

            body = body.Replace("{LISTA_LIGHT}", bodyIndice.ToString());

            return body;
        }

        public async Task<string> GetCopertina(ByQueryModel model)
        {
            var count = await CountByQuery(model);
            var atti = new List<AttoDASIDto>();
            atti.AddRange(await GetByQuery(model));
            while (atti.Count < count)
            {
                model.page += 1;
                atti.AddRange(await GetByQuery(model));
            }

            var legislatura = await _unitOfWork.Legislature.Get(atti.First().Legislatura);
            var body = GetTemplate(TemplateTypeEnum.PDF_COPERTINA, true);
            body =
                "<link href=\"https://fonts.googleapis.com/icon?family=Material+Icons\" rel=\"stylesheet\">" +
                "<link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/materialize/1.0.0/css/materialize.min.css\">" +
                $"<link rel=\"stylesheet\" href=\"{AppSettingsConfiguration.url_CLIENT}/content/site.css\">" +
                body;
            body = body.Replace("{LEGISLATURA}", legislatura.num_legislatura);
            body = body.Replace("{nomePiattaforma}", AppSettingsConfiguration.Titolo);
            body = body.Replace("{urlLogo}", AppSettingsConfiguration.Logo);

            var templateItemIndice = GetTemplate(TemplateTypeEnum.INDICE_DASI);
            var bodyIndice = new StringBuilder();
            foreach (var dasiDto in atti)
                bodyIndice.Append(templateItemIndice
                    .Replace("{TipoAtto}", Utility.GetText_Tipo(dasiDto.Tipo))
                    .Replace("{NAtto}", dasiDto.NAtto)
                    .Replace("{Oggetto}", dasiDto.OggettoView())
                    .Replace("{Firmatari}",
                        $"{dasiDto.PersonaProponente.DisplayName}{(!string.IsNullOrEmpty(dasiDto.Firme) ? ", " + dasiDto.Firme.Replace("<br>", ", ") : "")}")
                    .Replace("{DataDeposito}",
                        string.IsNullOrEmpty(dasiDto.DataPresentazione)
                            ? ""
                            : "<br>Depositato il " + dasiDto.Timestamp.Value.ToString("dd/MM/yyyy")));

            body = body.Replace("{LISTA_LIGHT}", bodyIndice.ToString());

            return body;
        }

        public IEnumerable<StatiDto> GetStati(PersonaDto persona)
        {
            var result = new List<StatiDto>();
            var stati = Enum.GetValues(typeof(StatiAttoEnum));
            foreach (var stato in stati)
            {
                if (persona.IsSegreteriaAssemblea)
                {
                    if (Utility.statiNonVisibili_Segreteria.Contains(Convert.ToInt16(stato)))
                        continue;
                }
                else
                {
                    if (Utility.statiNonVisibili_Standard.Contains(Convert.ToInt16(stato)))
                        continue;
                }

                result.Add(new StatiDto
                {
                    IDStato = (int)stato,
                    Stato = Utility.GetText_StatoDASI((int)stato)
                });
            }

            return result;
        }

        public IEnumerable<Tipi_AttoDto> GetTipiMOZ()
        {
            var result = new List<Tipi_AttoDto>();
            var tipi = Enum.GetValues(typeof(TipoMOZEnum));
            foreach (var tipo in tipi)
                result.Add(new Tipi_AttoDto
                {
                    IDTipoAtto = (int)tipo,
                    Tipo_Atto = Utility.GetText_TipoMOZDASI((int)tipo)
                });

            return result;
        }

        public async Task ModificaMetaDati(AttoDASIDto model, ATTI_DASI atto, PersonaDto persona)
        {
            atto.UIDPersonaModifica = persona.UID_persona;
            atto.DataModifica = DateTime.Now;
            if (!string.IsNullOrEmpty(model.Oggetto_Modificato))
                atto.Oggetto_Modificato = model.Oggetto_Modificato;
            else if (string.IsNullOrEmpty(model.Oggetto_Modificato) &&
                     !string.IsNullOrEmpty(atto.Oggetto_Modificato))
                //caso in cui l'utente voglia tornare allo stato precedente
                atto.Oggetto_Modificato = string.Empty;

            if (!string.IsNullOrEmpty(model.Oggetto_Privacy) && !model.Oggetto_Privacy.Equals(model.Oggetto))
                atto.Oggetto_Privacy = model.Oggetto_Privacy;

            if (!string.IsNullOrEmpty(model.Premesse_Modificato))
                atto.Premesse_Modificato = model.Premesse_Modificato;
            else if (string.IsNullOrEmpty(model.Premesse_Modificato) &&
                     !string.IsNullOrEmpty(atto.Premesse_Modificato))
                //caso in cui l'utente voglia tornare allo stato precedente
                atto.Premesse_Modificato = string.Empty;

            if (!string.IsNullOrEmpty(model.Richiesta_Modificata))
                atto.Richiesta_Modificata = model.Richiesta_Modificata;
            else if (string.IsNullOrEmpty(model.Richiesta_Modificata) &&
                     !string.IsNullOrEmpty(atto.Richiesta_Modificata))
                //caso in cui l'utente voglia tornare allo stato precedente
                atto.Richiesta_Modificata = string.Empty;

            await _unitOfWork.DASI.RimuoviCommissioni(atto.UIDAtto);
            if (model.Organi.Any(o => o.tipo_organo == TipoOrganoEnum.COMMISSIONE))
                foreach (var commissioneDto in model.Organi.Where(o => o.tipo_organo == TipoOrganoEnum.COMMISSIONE))
                    _unitOfWork.DASI.AggiungiCommissione(atto.UIDAtto, commissioneDto.id_organo);

            await _unitOfWork.CompleteAsync();
        }

        private bool IsOutdate(AttoDASIDto atto)
        {
            try
            {
                if (atto.IDStato < (int)StatiAttoEnum.PRESENTATO) return false;
                if (atto.Seduta == null) return false;
                var result = false;
                switch ((TipoAttoEnum)atto.Tipo)
                {
                    case TipoAttoEnum.IQT:
                    {
                        if (atto.Seduta.DataScadenzaPresentazioneIQT == null)
                            break;
                        if (atto.Seduta.DataScadenzaPresentazioneIQT.HasValue)
                            if (atto.Timestamp > atto.Seduta.DataScadenzaPresentazioneIQT)
                                result = true;
                        break;
                    }
                    case TipoAttoEnum.MOZ:
                    {
                        switch ((TipoMOZEnum)atto.TipoMOZ)
                        {
                            case TipoMOZEnum.URGENTE:
                            {
                                if (atto.Seduta.DataScadenzaPresentazioneMOZU == null)
                                    break;
                                if (DateTime.TryParseExact(
                                        atto.DataPresentazione_MOZ_URGENTE,
                                        "dd/MM/yyyy HH:mm:ss",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out DateTime dataPresentazioneUrgente) &&
                                    dataPresentazioneUrgente > atto.Seduta.DataScadenzaPresentazioneMOZU)
                                {
                                    result = true;
                                }
                                break;
                            }
                            case TipoMOZEnum.ABBINATA:
                            {
                                if (atto.Seduta.DataScadenzaPresentazioneMOZA == null)
                                    break;
                                if (DateTime.TryParseExact(
                                        atto.DataPresentazione_MOZ_ABBINATA,
                                        "dd/MM/yyyy HH:mm:ss",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out DateTime dataPresentazioneAbbinata) &&
                                    dataPresentazioneAbbinata > atto.Seduta.DataScadenzaPresentazioneMOZA)
                                {
                                    result = true;
                                }
                                break;
                            }
                            case TipoMOZEnum.ORDINARIA:
                            {
                                if (atto.Seduta.DataScadenzaPresentazioneMOZ == null)
                                    break;
                                if (DateTime.TryParseExact(
                                        atto.DataPresentazione_MOZ,
                                        "dd/MM/yyyy HH:mm:ss",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out DateTime dataPresentazioneMoz) &&
                                    dataPresentazioneMoz > atto.Seduta.DataScadenzaPresentazioneMOZ)
                                {
                                    result = true;
                                }
                                break;
                            }
                        }

                        break;
                    }
                    case TipoAttoEnum.ODG:
                    {
                        if (atto.CapogruppoNeiTermini) break;
                        if (atto.Seduta.DataScadenzaPresentazioneODG == null)
                            break;
                        if (atto.Seduta.DataScadenzaPresentazioneODG.HasValue)
                            if (atto.Timestamp > atto.Seduta.DataScadenzaPresentazioneODG)
                                result = true;
                        break;
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"Errore Presentato oltre i termini - {atto.Etichetta}", e);
                throw;
            }
        }

        public async Task RichiestaPresentazioneCartacea(PresentazioneCartaceaModel model, PersonaDto currentUser)
        {
            var atti_cartacei = new List<ATTI_DASI>();
            var data_presentazione = DateTime.Now;
            var legislaturaId = await _unitOfWork.Legislature.Legislatura_Attiva();
            var legislatura = await _unitOfWork.Legislature.Get(legislaturaId);

            if (model.Tipo == (int)TipoAttoEnum.RIS)
            {
                for (var i = 0; i < model.Salto; i++)
                {
                    atti_cartacei.Add(new ATTI_DASI
                    {
                        IDStato = (int)StatiAttoEnum.BOZZA_CARTACEA,
                        Tipo = model.Tipo,
                        UIDAtto = Guid.NewGuid(),
                        UID_QRCode = Guid.NewGuid(),
                        Timestamp = data_presentazione,
                        DataPresentazione = BALHelper.EncryptString(data_presentazione.ToString("dd/MM/yyyy HH:mm:ss"),
                            AppSettingsConfiguration.masterKey),
                        DataCreazione = data_presentazione,
                        UIDPersonaCreazione = currentUser.UID_persona,
                        Legislatura = legislatura.id_legislatura,
                        idRuoloCreazione = (int)currentUser.CurrentRole
                    });
                }
            }
            else
            {
                var contatore = await _unitOfWork.DASI.GetContatore(model.Tipo, model.TipoRisposta);
                _unitOfWork.DASI.IncrementaContatore(contatore, model.Salto);
                await _unitOfWork.CompleteAsync();

                // Matteo Cattapan #520 - Inserimento di atti presentati in forma cartacea
                for (var i = 0; i < model.Salto; i++)
                {
                    var contatore_progressivo = contatore.Inizio + (contatore.Contatore - (model.Salto - i));
                    var etichetta_progressiva =
                        $"{Utility.GetText_Tipo(model.Tipo)}_{contatore_progressivo}_{legislatura.num_legislatura}";
                    var etichetta_encrypt =
                        BALHelper.EncryptString(etichetta_progressiva, AppSettingsConfiguration.masterKey);

                    atti_cartacei.Add(new ATTI_DASI
                    {
                        IDStato = (int)StatiAttoEnum.BOZZA_CARTACEA,
                        Tipo = model.Tipo,
                        IDTipo_Risposta = model.TipoRisposta,
                        UIDAtto = Guid.NewGuid(),
                        UID_QRCode = Guid.NewGuid(),
                        Timestamp = data_presentazione,
                        DataPresentazione = BALHelper.EncryptString(data_presentazione.ToString("dd/MM/yyyy HH:mm:ss"),
                            AppSettingsConfiguration.masterKey),
                        NAtto_search = contatore_progressivo,
                        OrdineVisualizzazione = contatore_progressivo,
                        Etichetta = etichetta_progressiva,
                        NAtto = etichetta_encrypt,
                        DataCreazione = data_presentazione,
                        UIDPersonaCreazione = currentUser.UID_persona,
                        Legislatura = legislatura.id_legislatura,
                        idRuoloCreazione = (int)currentUser.CurrentRole
                    });
                }
            }

            if (atti_cartacei.Any())
            {
                _unitOfWork.DASI.AddRange(atti_cartacei);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<HttpResponseMessage> DownloadPDFIstantaneo(ATTI_DASI atto, PersonaDto persona,
            bool privacy = false)
        {
            var content = await PDFIstantaneo(atto, persona, privacy);
            var res = ComposeFileResponse(content,
                $"{Utility.GetText_Tipo(atto.Tipo)} {GetNome(atto.NAtto, atto.Progressivo)}.pdf");
            return res;
        }

        internal async Task<byte[]> PDFIstantaneo(ATTI_DASI atto, PersonaDto persona, bool privacy = false)
        {
            var attoDto = await GetAttoDto(atto.UIDAtto);
            var listAttachments = new List<string>();
            if (!string.IsNullOrEmpty(attoDto.PATH_AllegatoGenerico))
            {
                var complete_path = Path.Combine(
                    AppSettingsConfiguration.PercorsoCompatibilitaDocumenti,
                    Path.GetFileName(attoDto.PATH_AllegatoGenerico));
                listAttachments.Add(complete_path);
            }

            var firme = await _logicAttiFirme.GetFirme(atto, FirmeTipoEnum.TUTTE);
            var body = await GetBodyDASI(atto, firme, persona, TemplateTypeEnum.PDF, privacy);
            var stamper = new PdfStamper_IronPDF(AppSettingsConfiguration.PDF_LICENSE);
            return await stamper.CreaPDFInMemory(body, $"{Utility.GetText_Tipo(attoDto.Tipo)} {attoDto.NAtto}",
                listAttachments);
        }

        public async Task InviaAlProtocollo(Guid id)
        {
            var atto = await _unitOfWork.DASI.Get(id);
            var nome_atto = $"{Utility.GetText_Tipo(atto.Tipo)}-{GetNome(atto.NAtto, atto.Progressivo)}";
            var content = await PDFIstantaneo(atto, null);
            var mailModel = new MailModel
            {
                DA = AppSettingsConfiguration.EmailInvioDASI,
                A = AppSettingsConfiguration.EmailProtocolloDASI,
                OGGETTO = $"Richiesta di protocollazione dell’atto {nome_atto}",
                MESSAGGIO =
                    $"Si invia in allegato l'atto {nome_atto} con oggetto \"{atto.Oggetto}\". " +
                    $"Si chiede l'apertura del fascicolo dedicato e la protocollazione dell'atto con preghiera di comunicare i relativi protocolli inviando una email a: {AppSettingsConfiguration.EmailInvioDASI} " +
                    "<br> Cordiali saluti, <br><br>Segreteria dell’Assemblea Consiliare",
                ATTACHMENTS = new List<AllegatoMail> { new AllegatoMail(content, $"{nome_atto}.pdf") }
            };
            await _logicUtil.InvioMail(mailModel);

            atto.Inviato_Al_Protocollo = true;
            atto.DataInvioAlProtocollo = DateTime.Now;

            await _unitOfWork.CompleteAsync();
        }

        public async Task DeclassaMozione(List<string> data)
        {
            foreach (var moz_id in data)
            {
                var moz = await Get(new Guid(moz_id));

                if (moz.DataIscrizioneSeduta.HasValue)
                    throw new Exception(
                        "ERROR: L'atto è iscritto in seduta. Contatta la segreteria dell'assemblea per modificare l'atto.");

                if (moz.TipoMOZ == (int)TipoMOZEnum.ABBINATA) moz.UID_MOZ_Abbinata = null;
                moz.TipoMOZ = (int)TipoMOZEnum.ORDINARIA;
                moz.DataPresentazione_MOZ = null;
                moz.DataPresentazione_MOZ_ABBINATA = null;
                moz.DataPresentazione_MOZ_URGENTE = null;
                moz.DataRichiestaIscrizioneSeduta = null;
                moz.UIDPersonaRichiestaIscrizione = null;
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<List<AttoDASIDto>> GetCartacei()
        {
            var legislatura = await _unitOfWork.Legislature.Legislatura_Attiva();
            var atti_cartacei = await _unitOfWork.DASI.GetAllCartacei(legislatura);
            var result = new List<AttoDASIDto>();
            foreach (var id in atti_cartacei)
            {
                var dto = await GetAttoDto(id);
                result.Add(dto);
            }

            return result;
        }

        public async Task CambiaPrioritaFirma(AttiFirmeDto firma)
        {
            var firma_in_db = await _unitOfWork.Atti_Firme.Get(firma.UIDAtto, firma.UID_persona);
            var priorita_originale = firma_in_db.Prioritario;
            firma_in_db.Prioritario = !priorita_originale;

            await _unitOfWork.CompleteAsync();
            var dto = await GetAttoDto(firma.UIDAtto);

            SEDUTE sedutaRichiesta = null;
            if (!string.IsNullOrEmpty(dto.DataRichiestaIscrizioneSeduta))
                sedutaRichiesta =
                    await _unitOfWork.Sedute.Get(Convert.ToDateTime(dto.DataRichiestaIscrizioneSeduta));
            var res = await ControlloFirmePresentazione(dto, true, sedutaRichiesta);
            if (!string.IsNullOrEmpty(res))
            {
                firma_in_db.Prioritario = priorita_originale;
                await _unitOfWork.CompleteAsync();

                throw new Exception(res);
            }
        }

        public async Task CambiaOrdineVisualizzazione(List<AttiFirmeDto> firme)
        {
            var uidAtto = firme.First().UIDAtto;
            var firmeInDb = await _unitOfWork
                .Atti_Firme
                .GetFirmatari(uidAtto);

            foreach (var attiFirmeDto in firme)
            {
                var firmaInDb = firmeInDb.First(f => f.UID_persona.Equals(attiFirmeDto.UID_persona));
                firmaInDb.OrdineVisualizzazione = attiFirmeDto.OrdineVisualizzazione;
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task SalvaCartaceo(AttoDASIDto attoDto, PersonaDto currentUser)
        {
            // #1083
            if(!attoDto.IsRIS())
            {
                if (!attoDto.UIDPersonaProponente.HasValue)
                    throw new InvalidOperationException("Indicare un proponente");
                if (attoDto.UIDPersonaProponente.Value == Guid.Empty)
                    throw new InvalidOperationException("Indicare un proponente");
            }

            //Modifica
            var attoInDb = await _unitOfWork.DASI.Get(attoDto.UIDAtto);
            if (attoInDb == null)
                throw new InvalidOperationException("Atto non trovato");

            if (!attoDto.IsRIS())
            {
                attoInDb.UIDPersonaProponente = attoDto.UIDPersonaProponente;
                if (attoInDb.id_gruppo <= 0 && attoInDb.UIDPersonaProponente.HasValue)
                {
                    var gruppo =
                        await _logicPersona.GetGruppoAttualePersona(attoInDb.UIDPersonaProponente.Value, false);
                    attoInDb.id_gruppo = gruppo.id_gruppo;
                }
            }

            if (attoDto.IsMOZ())
            {
                attoInDb.TipoMOZ = attoDto.TipoMOZ;
                attoInDb.UID_MOZ_Abbinata =
                    attoInDb.TipoMOZ == (int)TipoMOZEnum.ABBINATA ? attoDto.UID_MOZ_Abbinata : null;
            }

            if (attoDto.IsRIS() && !attoInDb.Progressivo.Equals(attoDto.Progressivo))
            {
                var legislatura = await _unitOfWork.Legislature.Get(attoInDb.Legislatura);
                var contatore_progressivo = attoDto.Progressivo;
                var etichetta_progressiva =
                    $"{Utility.GetText_Tipo(attoDto.Tipo)}_{contatore_progressivo}_{legislatura.num_legislatura}";
                var etichetta_encrypt =
                    BALHelper.EncryptString(etichetta_progressiva, AppSettingsConfiguration.masterKey);

                attoInDb.NAtto_search = contatore_progressivo;
                attoInDb.OrdineVisualizzazione = contatore_progressivo;
                attoInDb.Etichetta = etichetta_progressiva;
                attoInDb.NAtto = etichetta_encrypt;
                attoInDb.Progressivo = contatore_progressivo;

                var controlloEtichetta = await _unitOfWork.DASI.GetByEtichetta(etichetta_progressiva);
                if (controlloEtichetta != null)
                {
                    throw new InvalidOperationException("Numero atto già occupato.");
                }
            }

            if (attoDto.IsODG())
            {
                if (!attoDto.UID_Atto_ODG.HasValue || attoDto.UID_Atto_ODG == Guid.Empty)
                    throw new InvalidOperationException(
                        "Seleziona un atto a cui iscrivere l'ordine del giorno");

                attoInDb.UID_Atto_ODG = attoDto.UID_Atto_ODG;
                var attoPEM = await _unitOfWork.Atti.Get(attoInDb.UID_Atto_ODG.Value);
                var seduta = await _unitOfWork.Sedute.Get(attoPEM.UIDSeduta.Value);
                attoInDb.UIDSeduta = seduta.UIDSeduta;
                attoInDb.DataRichiestaIscrizioneSeduta = BALHelper.EncryptString(
                    seduta.Data_seduta.ToString("dd/MM/yyyy"),
                    AppSettingsConfiguration.masterKey);
                attoInDb.UIDPersonaRichiestaIscrizione = currentUser.UID_persona;
                attoInDb.Non_Passaggio_In_Esame = attoDto.Non_Passaggio_In_Esame;
            }

            attoInDb.UIDPersonaModifica = currentUser.UID_persona;
            attoInDb.DataModifica = DateTime.Now;
            attoInDb.Oggetto = attoDto.Oggetto;
            attoInDb.Premesse = attoDto.Premesse;
            attoInDb.Richiesta = attoDto.Richiesta;
            attoInDb.IDTipo_Risposta = attoDto.IDTipo_Risposta;
            attoInDb.FirmeCartacee = attoDto.FirmeCartacee_string;

            if (attoDto.DocAllegatoGenerico_Stream.Length > 0)
            {
                await Salva_Documento(new SalvaDocumentoRequest
                {
                    Tipo = (int)TipoDocumentoEnum.TESTO_ALLEGATO,
                    Contenuto = attoDto.DocAllegatoGenerico_Stream,
                    UIDAtto = attoInDb.UIDAtto
                }, currentUser);
            }

            await _unitOfWork.CompleteAsync();
            await GestioneCommissioni(attoDto, true);
            await GestioneProponentiCommissioni(attoDto, true);

            if (attoDto.IDStato == (int)StatiAttoEnum.PRESENTATO)
            {
                //Presenta atto
                var dto = await GetAttoDto(attoInDb.UIDAtto);
                await PresentaCartaceo(attoInDb, dto);
            }
        }

        private async Task PresentaCartaceo(ATTI_DASI atto, AttoDASIDto dto)
        {
            if (atto.Tipo == (int)TipoAttoEnum.RIS)
            {
                // ignored
            }
            else
            {
                if (!dto.FirmeCartacee.Any())
                    throw new InvalidOperationException(
                        "Inserire i firmatari.");

                if (dto.FirmeCartacee.First().uid != dto.UIDPersonaProponente.Value.ToString())
                    throw new InvalidOperationException(
                        "Il proponente deve essere anche il primo firmatario.");

                await FirmaAttoUfficio(dto);
                var count_firme = await _unitOfWork.Atti_Firme.CountFirme(atto.UIDAtto);
                if (dto.Tipo == (int)TipoAttoEnum.IQT
                    && string.IsNullOrEmpty(dto.DataRichiestaIscrizioneSeduta))
                    throw new Exception(
                        $"Requisiti presentazione: {nameof(AttoDASIDto.DataRichiestaIscrizioneSeduta)} non specificato.");

                var controllo_firme = await ControlloFirmePresentazione(dto, count_firme);

                if (!string.IsNullOrEmpty(controllo_firme)) throw new Exception(controllo_firme);

                atto.chkf = count_firme.ToString();
                atto.UIDPersonaPresentazione = atto.UIDPersonaProponente;
            }

            atto.IDStato = (int)StatiAttoEnum.PRESENTATO;

            await _unitOfWork.CompleteAsync();
        }

        private async Task FirmaAttoUfficio(AttoDASIDto dto)
        {
            foreach (var firma_cartacea in dto.FirmeCartacee)
            {
                var uid_persona = new Guid(firma_cartacea.uid);
                var check_firmato = await _unitOfWork.Atti_Firme.CheckFirmato(dto.UIDAtto, uid_persona);
                if (check_firmato) continue;

                var persona = await _logicPersona.GetPersona(uid_persona);
                persona.Gruppo = await _logicPersona.GetGruppoAttualePersona(persona.UID_persona, false);
                var result_firma = await Firma(
                    new ComandiAzioneModel
                    {
                        Azione = ActionEnum.FIRMA,
                        IsDASI = true,
                        Lista = new List<Guid> { dto.UIDAtto }
                    },
                    persona,
                    null,
                    true);

                var listaErroriFirma = new List<string>();
                foreach (var itemFirma in result_firma.Where(i => i.Value.Contains("ERROR")))
                    listaErroriFirma.Add($"{itemFirma.Value}");
                if (listaErroriFirma.Count > 0)
                    throw new Exception($"{listaErroriFirma.Aggregate((i, j) => i + ", " + j)}");
            }
        }

        public async Task SalvaReport(ReportDto report, PersonaDto currentUser)
        {
            if (string.IsNullOrEmpty(report.reportname))
                throw new Exception("E' necessario dare un nome al report per poterlo salvare.");

            var reportInDb = await _unitOfWork.Reports.Get(report.reportname, currentUser.UID_persona);
            if (reportInDb == null)
            {
                var item = new REPORTS
                {
                    UId_persona = currentUser.UID_persona,
                    Filtri = report.filters,
                    Nome = report.reportname,
                    Colonne = report.columns,
                    FormatoEsportazione = report.exportformat,
                    TipoCopertina = report.covertype,
                    TipoVisualizzazione = report.dataviewtype,
                    TipoVisualizzazione_Card_Template = report.dataviewtype_template,
                    DettagliOrdinamento = report.sorting
                };

                _unitOfWork.Reports.Add(item);
            }
            else
            {
                reportInDb.Filtri = report.filters;
                reportInDb.Colonne = report.columns;
                reportInDb.FormatoEsportazione = report.exportformat;
                reportInDb.TipoCopertina = report.covertype;
                reportInDb.TipoVisualizzazione = report.dataviewtype;
                reportInDb.TipoVisualizzazione_Card_Template = report.dataviewtype_template;
                reportInDb.DettagliOrdinamento = report.sorting;
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task SalvaGruppoFiltri(FiltroPreferitoDto request, PersonaDto currentUser)
        {
            if (string.IsNullOrEmpty(request.name))
                throw new Exception("E' necessario dare un nome al filtro per poterlo salvare.");

            var filtro = new FILTRI
            {
                UId_persona = currentUser.UID_persona,
                Filtri = request.filters,
                Nome = request.name,
                Preferito = request.favourite
            };

            _unitOfWork.Filtri.Add(filtro);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<List<FiltroPreferitoDto>> GetGruppoFiltri(PersonaDto currentUser)
        {
            var listFromDb = await _unitOfWork.Filtri.GetByUser(currentUser.UID_persona);
            var res = new List<FiltroPreferitoDto>();
            foreach (var f in listFromDb)
                res.Add(new FiltroPreferitoDto
                {
                    name = f.Nome,
                    favourite = f.Preferito,
                    filters = f.Filtri
                });

            return res;
        }

        public async Task EliminaGruppoFiltri(string nomeFiltro, PersonaDto currentUser)
        {
            var filtro = await _unitOfWork.Filtri.Get(nomeFiltro, currentUser.UID_persona);
            _unitOfWork.Filtri.Remove(filtro);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<List<ReportDto>> GetReports(PersonaDto currentUser)
        {
            var listFromDb = await _unitOfWork.Reports.GetByUser(currentUser.UID_persona);
            var res = new List<ReportDto>();
            foreach (var f in listFromDb)
                res.Add(new ReportDto
                {
                    reportname = f.Nome,
                    filters = f.Filtri,
                    columns = f.Colonne,
                    covertype = f.TipoCopertina,
                    dataviewtype = f.TipoVisualizzazione,
                    dataviewtype_template = f.TipoVisualizzazione_Card_Template,
                    exportformat = f.FormatoEsportazione,
                    sorting = f.DettagliOrdinamento
                });

            return res;
        }

        public async Task EliminaReport(string nomeReport, PersonaDto currentUser)
        {
            var report = await _unitOfWork.Reports.Get(nomeReport, currentUser.UID_persona);
            _unitOfWork.Reports.Remove(report);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<HttpResponseMessage> GeneraReport(ReportDto model, PersonaDto currentUser)
        {
            if (string.IsNullOrEmpty(model.reportname)) model.reportname = "Report";

            var tempFolderPath = HttpContext.Current.Server.MapPath("~/esportazioni");
            var filePath = Path.Combine(tempFolderPath, $"{model.reportname}_{DateTime.Now.Ticks}");
            switch ((ExportFormatEnum)model.exportformat)
            {
                case ExportFormatEnum.PDF:
                    filePath += ".pdf";
                    var bodyPDF = await ComposeReportBodyFromTemplate(model, currentUser);
                    var stamper = new PdfStamper_IronPDF(AppSettingsConfiguration.PDF_LICENSE);
                    await stamper.CreaPDFAsync(filePath, bodyPDF, "Report");
                    break;
                case ExportFormatEnum.WORD:
                    try
                    {
                        filePath += ".docx";
                        var bodyWord = await ComposeReportBodyFromTemplate(model, currentUser);
                        CreateWordReport(filePath, bodyWord, (WordSizeEnum)model.wordsize);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                    break;
                case ExportFormatEnum.EXCEL:
                    filePath += ".xlsx";
                    await CreateExcelReport(filePath, model, currentUser);
                    break;
                case ExportFormatEnum.EXCEL_SITUAZIONE:
                    filePath += ".xlsx";
                    await CreateExcelSituazioneReport(filePath, model, currentUser);
                    break;
                case ExportFormatEnum.EXCEL_COMMISSIONI:
                    filePath += ".xlsx";
                    await CreateExcelCommissioniReport(filePath, model, currentUser);
                    break;
                default:
                    throw new ArgumentException("Formato di esportazione non supportato");
            }

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    $"{AppSettingsConfiguration.URL_API}/esportazioni/{Path.GetFileName(filePath)}")
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

            return result;
        }

        public async Task<HttpResponseMessage> GeneraZip(ReportDto model, PersonaDto currentUser)
        {
            var filtri = JsonConvert.DeserializeObject<List<FilterItem>>(model.filters);
            var filterStatements = Utility.ParseFilterDasi(filtri);
            var request = new BaseRequest<AttoDASIDto>
            {
                filtro = filterStatements,
                param = new Dictionary<string, object> { { "CLIENT_MODE", (int)ClientModeEnum.GRUPPI } }
            };

            var attiList = new List<AttoDASIDto>();
            var idsList = await GetSoloIds(request, currentUser, null);
            foreach (var uid in idsList)
            {
                var dto = await GetAttoDto(uid);
                if (dto.IDStato == (int)StatiAttoEnum.BOZZA_CARTACEA) continue;
                attiList.Add(dto);
            }

            var pdfs = await GetPDF(attiList);
            return ResponseZip(pdfs);
        }

        private HttpResponseMessage ResponseZip(List<FileModel> pdfs)
        {
            var outputMemoryStream = new MemoryStream();
            var zipStream = new ZipOutputStream(outputMemoryStream);
            zipStream.SetLevel(9);
            foreach (var internalZipFile in pdfs) AddToZip(zipStream, internalZipFile);
            zipStream.IsStreamOwner = false; // to stop the close and underlying stream
            zipStream.Close();

            outputMemoryStream.Position = 0;
            var zipByteArray = outputMemoryStream.ToArray();

            var tempFolderPath = HttpContext.Current.Server.MapPath("~/esportazioni");
            var filename = $"EsportazioneDASI{DateTime.Now.Ticks}.zip";
            var pathZip = Path.Combine(tempFolderPath, filename);

            using (var fileStream = new FileStream(pathZip, FileMode.Create))
            {
                fileStream.Write(zipByteArray, 0, zipByteArray.Length);
            }

            // Creazione della risposta con il link di download
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent($"{AppSettingsConfiguration.URL_API}/esportazioni/{filename}");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

            return response;
        }

        private void AddToZip(ZipOutputStream zipStream, FileModel internalZipFile)
        {
            var inputMemoryStream = new MemoryStream(internalZipFile.Content);

            var newZipEntry = new ZipEntry(internalZipFile.Name);
            newZipEntry.DateTime = DateTime.Now;
            newZipEntry.Size = internalZipFile.Content.Length;

            zipStream.PutNextEntry(newZipEntry);

            StreamUtils.Copy(inputMemoryStream, zipStream, new byte[1024]);
            zipStream.CloseEntry();
        }

        private async Task<List<FileModel>> GetPDF(List<AttoDASIDto> attiList)
        {
            var pdfs = new List<FileModel>();
            foreach (var dto in attiList)
            {
                var pdf = await PDFIstantaneo(Mapper.Map<AttoDASIDto, ATTI_DASI>(dto), null);
                pdfs.Add(new FileModel
                {
                    Name = dto.Display + ".pdf",
                    Content = pdf
                });
            }

            return pdfs;
        }

        private async Task CreateExcelReport(string filePath, ReportDto model, PersonaDto currentUser)
        {
            var filtri = JsonConvert.DeserializeObject<List<FilterItem>>(model.filters);
            var filterStatements = Utility.ParseFilterDasi(filtri);

            var request = new BaseRequest<AttoDASIDto>
            {
                filtro = filterStatements,
                param = new Dictionary<string, object> { { "CLIENT_MODE", (int)ClientModeEnum.GRUPPI } }
            };

            if (!string.IsNullOrEmpty(model.sorting))
            {
                var ordinamento = JsonConvert.DeserializeObject<List<SortingInfo>>(model.sorting);
                request.dettagliOrdinamento = ordinamento;
            }

            var idsList = await GetSoloIds(request, currentUser, null);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                // Definisci le colonne
                var columns = JsonConvert.DeserializeObject<List<string>>(model.columns);
                var properties = typeof(AttoDASIReportDto).GetProperties();

                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    var prop = properties.FirstOrDefault(p => p.Name == column);
                    if (prop != null)
                    {
                        var displayName = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                            .Cast<DisplayNameAttribute>()
                            .FirstOrDefault()?.DisplayName ?? prop.Name;

                        worksheet.Cells[1, i + 1].Value = displayName;
                    }
                }

                // Aggiungi righe di dati
                for (var rowIndex = 0; rowIndex < idsList.Count; rowIndex++)
                {
                    var guid = idsList[rowIndex];
                    var atto = await GetAttoDto(guid);

                    for (var colIndex = 0; colIndex < columns.Count; colIndex++)
                    {
                        var column = columns[colIndex];
                        var cellValue = GetPropertyValue(atto, column, ExportFormatEnum.EXCEL);
                        var cell = worksheet.Cells[rowIndex + 2, colIndex + 1];

                        // #1019
                        // Imposta il formato della cella in base al tipo di dato
                        if (cellValue == null)
                        {
                            cell.Value = "";
                        }
                        else if (DateTime.TryParse(cellValue.ToString(), out var resDate))
                        {
                            // Prova un formato che Excel riconosce come "Data" standard
                            cell.Style.Numberformat.Format = "dd/mm/yyyy"; // formato data senza orario
                            cell.Value = resDate;
                        }
                        else if (int.TryParse(cellValue.ToString(), out var resInt))
                        {
                            cell.Style.Numberformat.Format = "#,##0"; // formato numerico intero
                            cell.Value = resInt;
                        }
                        else
                        {
                            cell.Value = cellValue;
                        }
                    }
                }

                await package.SaveAsAsync(new FileInfo(filePath));
            }
        }

        public async Task CreateExcelSituazioneReport(string filePath, ReportDto model, PersonaDto currentUser)
        {
            var filtri = JsonConvert.DeserializeObject<List<FilterItem>>(model.filters);
            var filterStatements = Utility.ParseFilterDasi(filtri);

            var request = new BaseRequest<AttoDASIDto>
            {
                filtro = filterStatements,
                param = new Dictionary<string, object> { { "CLIENT_MODE", (int)ClientModeEnum.GRUPPI } }
            };

            if (!string.IsNullOrEmpty(model.sorting))
            {
                var ordinamento = JsonConvert.DeserializeObject<List<SortingInfo>>(model.sorting);
                request.dettagliOrdinamento = ordinamento;
            }

            var idsList = await GetSoloIds(request, currentUser, null);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                // Intestazioni delle colonne
                worksheet.Cells[1, 1].Value = "Atto";
                worksheet.Cells[1, 2].Value = "Risposta richiesta";
                worksheet.Cells[1, 3].Value = "Presentate";
                worksheet.Cells[1, 4].Value = "Ritirate/Inammissibili";
                worksheet.Cells[1, 5].Value = "Annunciate";
                worksheet.Cells[1, 6].Value = "Risposte pervenute";
                worksheet.Cells[1, 7].Value = "In attesa risposta";
                worksheet.Cells[1, 8].Value = "di cui in ritardo oltre 20gg";
                worksheet.Cells[1, 9].Value = "% risposte";

                var currentRow = 2;
                var groupedByAtto = new Dictionary<string, List<AttoDASIDto>>();

                // Raggruppa gli atti per tipo (es. ITR, ITL, IQT)
                foreach (var guid in idsList)
                {
                    var atto = await GetAttoDto(guid);
                    if (!groupedByAtto.ContainsKey(atto.DisplayTipo))
                        groupedByAtto[atto.DisplayTipo] = new List<AttoDASIDto>();
                    groupedByAtto[atto.DisplayTipo].Add(atto);
                }

                // Per ogni tipo di Atto, crea una sezione nella tabella
                foreach (var gruppo in groupedByAtto)
                {
                    var tipoAtto = gruppo.Key;
                    var attiList = gruppo.Value;

                    var tipoRisposte = attiList.GroupBy(a => a.DisplayTipoRispostaRichiesta).ToList();

                    foreach (var gruppoRisposta in tipoRisposte)
                    {
                        var atti = gruppoRisposta.ToList();
                        var presentate = atti.Count;
                        var ritirate = atti.Count(r =>
                            r.TipoChiusuraIter == (int)TipoChiusuraIterEnum.RITIRATO
                            || r.TipoChiusuraIter == (int)TipoChiusuraIterEnum.INAMMISSIBILE);
                        var annunciate = atti.Count(r => r.DataAnnunzio.HasValue
                                                         && r.TipoChiusuraIter != (int)TipoChiusuraIterEnum.RITIRATO
                                                         && r.TipoChiusuraIter !=
                                                         (int)TipoChiusuraIterEnum.INAMMISSIBILE);
                        var rispostePerv = atti.Count(r =>
                            r.DataAnnunzio.HasValue
                            && r.Risposte.Any(r => r.Data.HasValue)
                            && r.TipoChiusuraIter != (int)TipoChiusuraIterEnum.RITIRATO
                            && r.TipoChiusuraIter != (int)TipoChiusuraIterEnum.INAMMISSIBILE);
                        var inAttesa = annunciate - rispostePerv;


                        ///
                        ///  escludere gli atti che hanno ricevuto una risposta 
                        ///

                        var ritardoOltre20gg = atti.Count(r =>
                            r.TipoChiusuraIter != (int)TipoChiusuraIterEnum.RITIRATO
                            && r.TipoChiusuraIter != (int)TipoChiusuraIterEnum.INAMMISSIBILE
                            && r.DataAnnunzio.HasValue
                            && r.Risposte.Any(r => !r.Data.HasValue)
                            && (DateTime.Now - r.DataAnnunzio.Value).TotalDays > 20);

                        ///
                        ///
                        ///
                        /// 

                        var percentualeRisposte = annunciate - inAttesa > 0
                            ? rispostePerv / (double)(annunciate - inAttesa) * 100
                            : 0;

                        if (annunciate == 0)
                        {
                            rispostePerv = 0;
                            inAttesa = 0;
                            ritardoOltre20gg = 0;
                            percentualeRisposte = 0;
                        }

                        // Scrivi i dati nella riga corrente
                        worksheet.Cells[currentRow, 1].Value = tipoAtto;
                        worksheet.Cells[currentRow, 2].Value = gruppoRisposta.Key; // Scritta/Orale/Commissione
                        worksheet.Cells[currentRow, 3].Value = presentate;
                        worksheet.Cells[currentRow, 4].Value = ritirate;
                        worksheet.Cells[currentRow, 5].Value = annunciate;
                        worksheet.Cells[currentRow, 6].Value = rispostePerv;
                        worksheet.Cells[currentRow, 7].Value = inAttesa;
                        worksheet.Cells[currentRow, 8].Value = ritardoOltre20gg;
                        worksheet.Cells[currentRow, 9].Value = percentualeRisposte;

                        currentRow++;
                    }

                    // Totale per tipo di atto
                    worksheet.Cells[currentRow, 2].Value = "Totale";
                    worksheet.Cells[currentRow, 3].Formula =
                        $"SUM(C{currentRow - tipoRisposte.Count}:C{currentRow - 1})";
                    worksheet.Cells[currentRow, 4].Formula =
                        $"SUM(D{currentRow - tipoRisposte.Count}:D{currentRow - 1})";
                    worksheet.Cells[currentRow, 5].Formula =
                        $"SUM(E{currentRow - tipoRisposte.Count}:E{currentRow - 1})";
                    worksheet.Cells[currentRow, 6].Formula =
                        $"SUM(F{currentRow - tipoRisposte.Count}:F{currentRow - 1})";
                    worksheet.Cells[currentRow, 7].Formula =
                        $"SUM(G{currentRow - tipoRisposte.Count}:G{currentRow - 1})";
                    worksheet.Cells[currentRow, 8].Formula =
                        $"SUM(H{currentRow - tipoRisposte.Count}:H{currentRow - 1})";

                    worksheet.Cells[currentRow, 9].Formula =
                        $"IF(C{currentRow}-D{currentRow}<>0,F{currentRow}/(C{currentRow}-D{currentRow})*100,0)";
                    worksheet.Cells[currentRow, 9].Style.Numberformat.Format = "0.00";

                    worksheet.Cells[currentRow, 1, currentRow, 9].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, 1, currentRow, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[currentRow, 1, currentRow, 9].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

                    currentRow++;
                    currentRow++;
                }

                // Formattazione delle celle e salvataggio
                worksheet.Cells[1, 1, currentRow - 1, 9].AutoFitColumns();
                worksheet.Cells[1, 1, 1, 9].Style.Font.Bold = true;
                worksheet.Cells[1, 1, currentRow - 1, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                await package.SaveAsAsync(new FileInfo(filePath));
            }
        }

        public async Task CreateExcelCommissioniReport(string filePath, ReportDto model, PersonaDto currentUser)
        {
            var filtri = JsonConvert.DeserializeObject<List<FilterItem>>(model.filters);
            var filterStatements = Utility.ParseFilterDasi(filtri);

            var request = new BaseRequest<AttoDASIDto>
            {
                filtro = filterStatements,
                param = new Dictionary<string, object> { { "CLIENT_MODE", (int)ClientModeEnum.GRUPPI } }
            };

            if (!string.IsNullOrEmpty(model.sorting))
            {
                var ordinamento = JsonConvert.DeserializeObject<List<SortingInfo>>(model.sorting);
                request.dettagliOrdinamento = ordinamento;
            }

            var idsList = await GetSoloIds(request, currentUser, null);

            var commissionCounts = new Dictionary<string, int>();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                var currentRow = 1;

                foreach (var guid in idsList)
                {
                    var atto = await GetAttoDto(guid);
                    if (atto.Risposte.Any())
                        foreach (var attiRisposteDto in atto.Risposte)
                        {
                            worksheet.Cells[currentRow, 1].Value = atto.DisplayTipo;
                            worksheet.Cells[currentRow, 2].Value = atto.NAtto;
                            worksheet.Cells[currentRow, 3].Value = attiRisposteDto.DescrizioneOrgano;
                            if (attiRisposteDto.DataTrasmissione.HasValue)
                                worksheet.Cells[currentRow, 4].Value =
                                    attiRisposteDto.DataTrasmissione.Value.ToString("dd/MM/yyyy");
                            else
                                worksheet.Cells[currentRow, 4].Value = "--";

                            if (!commissionCounts.ContainsKey(attiRisposteDto.DescrizioneOrgano))
                                commissionCounts[attiRisposteDto.DescrizioneOrgano] = 0;
                            commissionCounts[attiRisposteDto.DescrizioneOrgano]++;

                            currentRow++;
                        }
                }

                currentRow += 2;

                worksheet.Cells[currentRow, 1].Value = "Commissione";
                worksheet.Cells[currentRow, 2].Value = "Totale";

                currentRow++;

                foreach (var commission in commissionCounts.OrderBy(c => c.Key))
                {
                    worksheet.Cells[currentRow, 1].Value = commission.Key;
                    worksheet.Cells[currentRow, 2].Value = commission.Value;
                    currentRow++;
                }

                await package.SaveAsAsync(new FileInfo(filePath));
            }
        }

        private object GetPropertyValue(AttoDASIDto atto, string propertyName, ExportFormatEnum exportFormat)
        {
            if (propertyName.Equals(nameof(AttoDASIDto.DCR))
                || propertyName.Equals(nameof(AttoDASIDto.DCCR))
                || propertyName.Equals(nameof(AttoDASIDto.DCRL)))
            {
                if (string.IsNullOrEmpty(atto.DCRL)) return "--";

                if (atto.DCCR > 0)
                    return $"{atto.DCRL}/{atto.DCR}/{atto.DCCR}";
                return $"{atto.DCRL}/{atto.DCR}";
            }

            if (propertyName.Equals(nameof(AttoDASIDto.Firme)))
            {
                if (string.IsNullOrEmpty(atto.Firme)) return "--";
                return atto.Firme.Replace("<br>", "; ");
            }

            if (propertyName.Equals(nameof(AttoDASIDto.CodiceMateria))) return atto.CodiceMateria;

            if (propertyName.Equals(nameof(AttoDASIDto.UIDSeduta)))
            {
                if (atto.Seduta != null)
                    return atto.Seduta.Data_seduta.ToString("dd/MM/yyyy");
                return "--";
            }

            if (propertyName.Equals(nameof(AttoDASIDto.Abbinamenti)))
            {
                if (!atto.Abbinamenti.Any())
                    return "--";
                return atto.Abbinamenti.Select(a => $"{a.TipoAttoAbbinato} {a.NumeroAttoAbbinato}")
                    .Aggregate((i, j) => i + ", " + j);
            }

            if (propertyName.Equals(nameof(AttoDASIDto.Documenti)))
            {
                if (!atto.Documenti.Any())
                    return "--";
                if (exportFormat == ExportFormatEnum.PDF)
                    return atto.Documenti.Select(a => $"{a.Tipo} - <a href='{a.Link}' target='_blank'>Download</a>")
                        .Aggregate((i, j) => i + "<br>" + j);

                return atto.Documenti.Select(a => $"{a.Tipo} - {a.Link}").Aggregate((i, j) => i + "<br>" + j);
            }

            if (propertyName.Equals(nameof(AttoDASIDto.Risposte)))
            {
                if (!atto.Risposte.Any())
                    return "--";

                var bodyRisposte = string.Empty;
                foreach (var attiRisposteDto in atto.Risposte)
                {
                    bodyRisposte += $"{attiRisposteDto.DisplayTipo}: {attiRisposteDto.DescrizioneOrgano}";

                    if (attiRisposteDto.Data.HasValue)
                    {
                        if (exportFormat == ExportFormatEnum.PDF || exportFormat == ExportFormatEnum.WORD)
                            bodyRisposte += $" - <b>Data risposta:</b> {attiRisposteDto.Data.Value:dd/MM/yyyy}";
                        else
                            bodyRisposte += $" - Data risposta: {attiRisposteDto.Data.Value:dd/MM/yyyy}";
                    }

                    if (attiRisposteDto.DataTrasmissione.HasValue)
                    {
                        if (exportFormat == ExportFormatEnum.PDF || exportFormat == ExportFormatEnum.WORD)
                            bodyRisposte +=
                                $" - <b>Trasmessa il</b> {attiRisposteDto.DataTrasmissione.Value:dd/MM/yyyy}";
                        else
                            bodyRisposte += $" - Trasmessa il {attiRisposteDto.DataTrasmissione.Value:dd/MM/yyyy}";
                    }

                    if (attiRisposteDto.DataTrattazione.HasValue)
                    {
                        if (exportFormat == ExportFormatEnum.PDF || exportFormat == ExportFormatEnum.WORD)
                            bodyRisposte += $" - <b>Trattata il</b> {attiRisposteDto.DataTrattazione.Value:dd/MM/yyyy}";
                        else
                            bodyRisposte += $" - Trattata il {attiRisposteDto.DataTrattazione.Value:dd/MM/yyyy}";
                    }

                    if (exportFormat == ExportFormatEnum.PDF || exportFormat == ExportFormatEnum.WORD)
                        bodyRisposte += "<br>";
                    else
                        bodyRisposte += "; ";
                }

                return bodyRisposte;
            }
            
            if (propertyName.Equals(nameof(AttoDASIDto.Monitoraggi)))
            {
                var bodyMonitoraggio = string.Empty;
                if (exportFormat == ExportFormatEnum.PDF || exportFormat == ExportFormatEnum.WORD)
                    bodyMonitoraggio += "<ul>";
                foreach (var monitoraggio in atto.Monitoraggi)
                {
                    if (exportFormat == ExportFormatEnum.PDF || exportFormat == ExportFormatEnum.WORD)
                        bodyMonitoraggio += $"<li>{monitoraggio.DescrizioneOrgano}</li>";
                    else
                        bodyMonitoraggio += $"{monitoraggio.DescrizioneOrgano}; ";
                }

                if (exportFormat == ExportFormatEnum.PDF || exportFormat == ExportFormatEnum.WORD)
                    bodyMonitoraggio += "</ul>";

                return bodyMonitoraggio;
            }

            if (propertyName.Equals(nameof(AttoDASIDto.CommissioniProponenti)))
            {
                if (!atto.CommissioniProponenti.Any()) return "--";

                return atto.CommissioniProponenti.Select(c => c.descr).Aggregate((i, j) => i + "; " + j);
            }

            if (propertyName.Equals(nameof(AttoDASIDto.Note)))
            {
                if (!atto.Note.Any())
                    return "--";
                // #1018
                var note = atto.Note.Where(a => a.TipoEnum != TipoNotaEnum.GENERALE_PRIVATA).ToList();
                if (note.Any())
                {
                    return note.Select(a => $"{a.Tipo} - {a.Nota}").Aggregate((i, j) => i + ", " + j);
                }

                return "";
            }

            if (propertyName.Equals(nameof(AttoDASIDto.IDStato))) return atto.DisplayStato;

            if (propertyName.Equals(nameof(AttoDASIDto.Tipo))) return atto.DisplayTipo;

            if (propertyName.Equals(nameof(AttoDASIDto.TipoMOZ))) return atto.DisplayTipoMozione;

            if (propertyName.Equals(nameof(AttoDASIDto.AreaPolitica))) return atto.DisplayAreaPolitica;

            if (propertyName.Equals(nameof(AttoDASIDto.TipoChiusuraIter))) return atto.DisplayTipoChiusuraIter;

            if (propertyName.Equals(nameof(AttoDASIDto.IDTipo_Risposta))) return atto.DisplayTipoRispostaRichiesta;

            if (propertyName.Equals(nameof(AttoDASIDto.TipoVotazioneIter))) return atto.DisplayTipoVotazioneIter;

            if (propertyName.Equals(nameof(AttoDASIDto.Oggetto))) return atto.OggettoView();

            if (propertyName.Equals(nameof(AttoDASIDto.UIDPersonaProponente)))
            {
                if (atto.Tipo == (int)TipoAttoEnum.RIS)
                    return "--";
                return $"{atto.PersonaProponente.DisplayName} ({atto.gruppi_politici.codice_gruppo})"; // #1021
            }

            if (propertyName.Equals(nameof(AttoDASIDto.id_gruppo)))
            {
                if (atto.id_gruppo > 0)
                    return atto.gruppi_politici.codice_gruppo;
                return "--";
            }

            // #1021
            if (propertyName.Equals(nameof(AttoDASIDto.Legislatura)))
            {
                return atto.GetLegislatura();
            }

            // #1021
            if (propertyName.Equals(nameof(AttoDASIDto.UID_QRCode)))
            {
                var qrLink =
                    $"{AppSettingsConfiguration.urlDASI_ViewATTO.Replace("{{UIDATTO}}", atto.UIDAtto.ToString())}";
                return qrLink;
            }

            var propertyInfo = typeof(AttoDASIDto).GetProperty(propertyName);
            if (propertyInfo == null) return "--";

            var propValue = propertyInfo.GetValue(atto);
            if (propValue == null)
                return "--";

            if (DateTime.TryParse(propValue.ToString(), out var resDate)) return resDate.ToString("dd/MM/yyyy");
            if (bool.TryParse(propValue.ToString(), out var resBool)) return resBool ? "Si" : "No";

            return propertyInfo.GetValue(atto);
        }

        private async Task<string> ComposeReportBodyFromTemplate(ReportDto model, PersonaDto currentUser)
        {
            var filtri = JsonConvert.DeserializeObject<List<FilterItem>>(model.filters);
            var filterStatements = Utility.ParseFilterDasi(filtri);

            var request = new BaseRequest<AttoDASIDto>
            {
                filtro = filterStatements,
                param = new Dictionary<string, object> { { "CLIENT_MODE", (int)ClientModeEnum.GRUPPI } }
            };

            if (!string.IsNullOrEmpty(model.sorting))
            {
                var ordinamento = JsonConvert.DeserializeObject<List<SortingInfo>>(model.sorting);
                request.dettagliOrdinamento = ordinamento;
            }

            // get dati dal database
            var idsList = await GetSoloIds(request, currentUser, null);

            // comporre il body con la lista dei dati
            var body =
                "<link href=\"https://fonts.googleapis.com/icon?family=Material+Icons\" rel=\"stylesheet\">" +
                "<link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/materialize/1.0.0/css/materialize.min.css\">" +
                $"<link rel=\"stylesheet\" href=\"{AppSettingsConfiguration.url_CLIENT}/content/site.css\">";

            if (Guid.TryParse(model.covertype, out var covertUid))
            {
                var cover = await _unitOfWork.Templates.Get(covertUid);
                body += cover.Corpo;
            }

            var templateHeader = GetTemplate(TemplateTypeEnum.REPORT_HEADER_DEFAULT, true);
            templateHeader = templateHeader.Replace("{{TOTALE_ATTI}}", idsList.Count.ToString());
            body += templateHeader;

            var properties = typeof(AttoDASIReportDto).GetProperties();

            switch ((DataViewTypeEnum)model.dataviewtype)
            {
                case DataViewTypeEnum.GRID:
                    body += "<table>";
                    body += "<tr>";
                    var columns = JsonConvert.DeserializeObject<List<string>>(model.columns);
                    foreach (var column in columns)
                    {
                        var prop = properties.FirstOrDefault(p => p.Name == column);
                        if (prop != null)
                        {
                            var displayName = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                                .Cast<DisplayNameAttribute>()
                                .FirstOrDefault()?.DisplayName ?? prop.Name;

                            body += $"<th>{displayName}</th>";
                        }
                    }

                    body += "</tr>";

                    foreach (var guid in idsList)
                    {
                        body += "<tr>";
                        var atto = await GetAttoDto(guid, currentUser);

                        body += GetBodyItemGrid(atto, model.columns);

                        body += "</tr>";
                    }

                    body += "</table>";
                    break;
                case DataViewTypeEnum.CARD:
                    var templateItemCard_Standard = GetTemplate(TemplateTypeEnum.REPORT_ITEM_CARD, true);

                    foreach (var guid in idsList)
                    {
                        var atto = await GetAttoDto(guid, currentUser);
                        body += templateItemCard_Standard.Replace("{{ITEM}}", GetBodyItemCard(atto, model.columns));
                    }

                    break;
                case DataViewTypeEnum.TEMPLATE:
                    var templateFromDb = await _unitOfWork.Templates.Get(Guid.Parse(model.dataviewtype_template));
                    var templateItemCard = templateFromDb.Corpo;

                    if (templateFromDb.Tipo == (int)TemplateTypeEnum.REPORT_ITEM_GRID)
                    {
                        body += "<table>";
                        body += "<thead>";
                        body += templateFromDb.Testata;
                        body += "</thead>";
                        body += "<tbody>";
                    }

                    foreach (var guid in idsList)
                    {
                        var atto = await GetAttoDto(guid, currentUser);

                        if (templateItemCard.Contains("{{ITEM}}"))
                        {
                            body += templateItemCard.Replace("{{ITEM}}", GetBodyItemCard(atto, model.columns));
                        }
                        else
                        {
                            var itemAtto = templateItemCard;
                            foreach (var prop in typeof(AttoDASIReportDto).GetProperties())
                                if (itemAtto.Contains("{{" + prop.Name + "}}"))
                                    itemAtto = itemAtto.Replace("{{" + prop.Name + "}}",
                                        GetPropertyValue(atto, prop.Name, ExportFormatEnum.PDF).ToString());

                            body += itemAtto;
                        }
                    }

                    if (templateFromDb.Tipo == (int)TemplateTypeEnum.REPORT_ITEM_GRID)
                    {
                        body += "</tbody>";
                        body += "</table>";
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException("Visualizzazione non supportata");
            }

            if (body.Contains("{{OGGI}}")) body = body.Replace("{{OGGI}}", DateTime.Now.ToString("dd/MM/yyyy"));

            return body;
        }

        private void CreateWordReport(string filePath, string body, WordSizeEnum wordSize = WordSizeEnum.A4)
        {
            using var document = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
            var mainPart = document.MainDocumentPart;
    
            if (mainPart == null)
            {
                mainPart = document.AddMainDocumentPart();
                new Document(new Body()).Save(mainPart);
            }

            // Configura la dimensione della pagina
            var pageSize = new PageSize();
            if (wordSize == WordSizeEnum.A3)
            {
                pageSize.Width = 16840;  // Dimensione in twips per A3 (297 mm x 420 mm)
                pageSize.Height = 11900;
            }
            else
            {
                pageSize.Width = 11900;  // Dimensione in twips per A4 (210 mm x 297 mm)
                pageSize.Height = 16840;
            }

            var sectionProps = new SectionProperties();
            sectionProps.Append(pageSize);
            mainPart.Document.Body.Append(sectionProps);

            // Converti l'HTML in contenuto Word e aggiungilo al documento
            var converter = new HtmlConverter(mainPart);
            converter.ParseHtml(body);

            // Salva il documento
            mainPart.Document.Save();
        }

        private string GetBodyItemCard(AttoDASIDto dto, string modelColumns)
        {
            var columns = new List<string>();
            if (!string.IsNullOrEmpty(modelColumns))
                columns = JsonConvert.DeserializeObject<List<string>>(modelColumns);
            else
                columns.Add(nameof(AttoDASIDto.Display));

            var sb = new StringBuilder();

            foreach (var column in columns)
            {
                if (column.Equals(nameof(AttoDASIDto.Emendato)))
                {
                    if (dto.IsIQT() || dto.IsITL() || dto.IsITR())
                    {
                        // #1043
                        continue;
                    }
                }
                
                var value = GetPropertyValueForHtml(dto, column);
                if (!string.IsNullOrEmpty(value)) sb.AppendLine(value);
            }

            return sb.ToString();
        }

        private string GetPropertyValueForHtml(AttoDASIDto dto, string propertyName)
        {
            switch (propertyName)
            {
                case nameof(AttoDASIReportDto.Display):
                    return $"<b>{dto.Display}</b>";
                case nameof(AttoDASIReportDto.DisplayExtended):
                    return $"<b>{dto.DisplayExtended}</b>";
                default:
                    var displayName = GetPropertyDisplayName(typeof(AttoDASIReportDto), propertyName);
                    var value = GetPropertyValue(dto, propertyName, ExportFormatEnum.PDF);
                    return value != null ? $"<b>{displayName}:</b> {value}</br>" : null; // #1094
            }
        }

        private string GetPropertyDisplayName(Type type, string propertyName)
        {
            var propertyInfo = type.GetProperty(propertyName);
            var displayNameAttribute = propertyInfo?.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                .Cast<DisplayNameAttribute>()
                .FirstOrDefault();
            return displayNameAttribute?.DisplayName ?? propertyName;
        }

        private string GetBodyItemGrid(AttoDASIDto dto, string modelColumns)
        {
            var columns = new List<string>();
            if (!string.IsNullOrEmpty(modelColumns))
                columns = JsonConvert.DeserializeObject<List<string>>(modelColumns);
            else
                columns.Add(nameof(AttoDASIDto.Display));

            var sb = new StringBuilder();

            foreach (var column in columns)
            {
                var value = GetPropertyValueForHtmlGrid(dto, column);
                if (!string.IsNullOrEmpty(value)) sb.AppendLine(value);
            }

            return sb.ToString();
        }

        private string GetPropertyValueForHtmlGrid(AttoDASIDto dto, string propertyName)
        {
            var value = GetPropertyValue(dto, propertyName, ExportFormatEnum.EXCEL);
            return value != null ? $"<td>{value}</td>" : null;
        }

        public async Task<List<AttoLightDto>> GetAbbinamentiDisponibili(int legislaturaId, int page, int size)
        {
            var res = await _unitOfWork.DASI.GetAbbinamentiDisponibili(legislaturaId, page, size);
            foreach (var item in res)
            {
                item.tipo_esteso = Utility.GetText_Tipo(int.Parse(item.tipo));
                item.display = $"{item.tipo_esteso} {item.natto}";
            }

            return res;
        }

        public async Task<List<GruppiDto>> GetGruppiDisponibili(int legislaturaId, int page, int size)
        {
            var res = await _unitOfWork.DASI.GetGruppiDisponibili(legislaturaId, page, size);
            return res;
        }

        public async Task<List<OrganoDto>> GetOrganiDisponibili(int legislaturaId)
        {
            var res = await _unitOfWork.DASI.GetOrganiDisponibili(legislaturaId);
            return res;
        }

        public async Task Salva_Documento(SalvaDocumentoRequest request, PersonaDto currentUser)
        {
            var atto = await GetAttoDto(request.UIDAtto, currentUser);
            if (atto == null)
                throw new Exception("Atto non trovato");

            switch ((TipoDocumentoEnum)request.Tipo)
            {
                case TipoDocumentoEnum.TESTO_ALLEGATO:
                {
                    request.Nome = "Allegato parte integrante dell'atto.pdf";
                    break;
                }
                case TipoDocumentoEnum.AGGIUNTIVO:
                    break;
                case TipoDocumentoEnum.MONITORAGGIO:
                    break;
                case TipoDocumentoEnum.ABBINAMENTO:
                    break;
                case TipoDocumentoEnum.CHIUSURA_ITER:
                {
                    request.Nome = "Testo dell'atto approvato.pdf";
                    break;
                }
                case TipoDocumentoEnum.RISPOSTA:
                {
                    request.Nome = "Testo della risposta.pdf";
                    break;
                }
                case TipoDocumentoEnum.TESTO_PRIVACY:
                    break;
                case TipoDocumentoEnum.VERBALE_VOTAZIONE:
                {
                    request.Nome = "Verbale di votazione.pdf";
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var dir = $"{atto.GetLegislatura()}/{Utility.GetText_Tipo(atto.Tipo)}/{atto.Etichetta}";
            var pathRepository = $"{AppSettingsConfiguration.PercorsoCompatibilitaDocumenti}/{dir}";

            if (!Directory.Exists(pathRepository))
                Directory.CreateDirectory(pathRepository);

            var nomeFile =
                $"{Path.GetFileNameWithoutExtension(request.Nome)}_{DateTime.Now.Ticks}{Path.GetExtension(request.Nome)}";

            var destinazioneDeposito = Path.Combine(pathRepository, nomeFile);
            File.WriteAllBytes(destinazioneDeposito, request.Contenuto);
            var doc = new ATTI_DOCUMENTI
            {
                UIDAtto = atto.UIDAtto,
                Data = DateTime.Now,
                Path = Path.Combine($"{dir}", nomeFile),
                Pubblica = false,
                Tipo = request.Tipo,
                Titolo = request.Nome
            };
            _unitOfWork.DASI.AggiungiDocumento(doc);

            if (request.Uid.HasValue)
            {
                switch ((TipoDocumentoEnum)request.Tipo)
                {
                    case TipoDocumentoEnum.TESTO_ALLEGATO:
                        break;
                    case TipoDocumentoEnum.AGGIUNTIVO:
                        break;
                    case TipoDocumentoEnum.MONITORAGGIO:
                        break;
                    case TipoDocumentoEnum.ABBINAMENTO:
                        break;
                    case TipoDocumentoEnum.CHIUSURA_ITER:
                        break;
                    case TipoDocumentoEnum.RISPOSTA:
                    {
                        var risposta = await _unitOfWork.DASI.GetRisposta(request.Uid.Value);
                        risposta.UIDDocumento = doc.Uid;
                        break;
                    }
                    case TipoDocumentoEnum.TESTO_PRIVACY:
                        break;
                    case TipoDocumentoEnum.VERBALE_VOTAZIONE:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task Rimuovi_Documento(AttiDocumentiDto request)
        {
            var doc = await _unitOfWork.DASI.GetDocumento(request.Uid);
            _unitOfWork.DASI.RimuoviDocumento(doc);
            switch ((TipoDocumentoEnum)doc.Tipo)
            {
                case TipoDocumentoEnum.TESTO_ALLEGATO:
                    break;
                case TipoDocumentoEnum.AGGIUNTIVO:
                    break;
                case TipoDocumentoEnum.MONITORAGGIO:
                    break;
                case TipoDocumentoEnum.ABBINAMENTO:
                    break;
                case TipoDocumentoEnum.CHIUSURA_ITER:
                    break;
                case TipoDocumentoEnum.RISPOSTA:
                {
                    var risposte = await _unitOfWork.DASI.GetRisposte(doc.UIDAtto);
                    if (risposte.Any(risp => risp.UIDDocumento == doc.Uid))
                    {
                        foreach (var risposta in risposte.Where(risp => risp.UIDDocumento == doc.Uid))
                        {
                            var rispostaInDb = await _unitOfWork.DASI.GetRisposta(risposta.Uid);
                            rispostaInDb.UIDDocumento = null;
                        }
                    }
                    break;
                }
                case TipoDocumentoEnum.TESTO_PRIVACY:
                    break;
                case TipoDocumentoEnum.VERBALE_VOTAZIONE:
                    break;
            }

            await _unitOfWork.CompleteAsync();

            var pathFile = $"{AppSettingsConfiguration.PercorsoCompatibilitaDocumenti}/{doc.Path}";

            if (File.Exists(pathFile))
            {
                try
                {
                    File.Delete(pathFile);
                }
                catch (IOException)
                {
                    // ignored
                }
            }
        }

        public async Task Pubblica_Documento(AttiDocumentiDto request, PersonaDto currentUser)
        {
            var doc = await _unitOfWork.DASI.GetDocumento(request.Uid);
            doc.Pubblica = !doc.Pubblica;
            await _unitOfWork.CompleteAsync();
        }

        public async Task Salva_ComandoMassivo(SalvaComandoMassivoRequest request, PersonaDto currentUser)
        {
            foreach (var datiInline in request.Lista)
            {
                var atto = await _unitOfWork.DASI.Get(datiInline.Uid);

                if (string.IsNullOrEmpty(atto.DCRL))
                {
                    if (!string.IsNullOrEmpty(atto.Etichetta))
                    {
                        var parti = atto.Etichetta.Split('_');
                        if (parti.Length > 0)
                            atto.DCRL = parti[parti.Length - 1];
                    }
                }

                if (!atto.DCR.Equals(int.Parse(datiInline.DCR))
                    || !atto.DCCR.Equals(int.Parse(datiInline.DCCR)))
                {
                    var filtroDCR = new List<FilterItem>
                    {
                        new FilterItem
                        {
                            property = nameof(AttoDASIDto.DCRL),
                            value = atto.DCRL
                        },
                        new FilterItem
                        {
                            property = nameof(AttoDASIDto.DCR),
                            value = datiInline.DCR
                        },
                        new FilterItem
                        {
                            property = nameof(AttoDASIDto.DCCR),
                            value = datiInline.DCCR
                        }
                    };

                    var query = Utility.ParseFilterDasi(filtroDCR);
                    var queryFilter = new Filter<ATTI_DASI>();
                    queryFilter.ImportStatements(query);
                    var res = await _unitOfWork.DASI.GetAll(currentUser, 1, 1, ClientModeEnum.GRUPPI, queryFilter,
                        new QueryExtendedRequest(), new List<SortingInfo>());

                    if (res.Any()) continue;

                    atto.DCR = int.Parse(datiInline.DCR);
                    atto.DCCR = int.Parse(datiInline.DCCR);
                }

                if (!string.IsNullOrEmpty(datiInline.Protocollo))
                    atto.Protocollo = datiInline.Protocollo;

                if (request.Dati.StatoCheck && int.Parse(request.Dati.Stato) >= 1)
                {
                    atto.IDStato = int.Parse(request.Dati.Stato);
                }

                if (request.Dati.DataAnnunzioCheck)
                {
                    if (string.IsNullOrEmpty(request.Dati.DataAnnunzio))
                        atto.DataAnnunzio = null;
                    else
                    {
                        atto.DataAnnunzio = DateTime.Parse(request.Dati.DataAnnunzio);
                    }
                }

                if (request.Dati.TipoChiusuraIterCheck)
                {
                    if (string.IsNullOrEmpty(request.Dati.TipoChiusuraIter))
                        atto.TipoChiusuraIter = null;
                    else
                    {
                        atto.TipoChiusuraIter = int.Parse(request.Dati.TipoChiusuraIter);
                    }
                }

                if (request.Dati.DataChiusuraIterCheck)
                {
                    if (string.IsNullOrEmpty(request.Dati.DataChiusuraIter))
                        atto.DataChiusuraIter = null;
                    else
                    {
                        atto.DataChiusuraIter = DateTime.Parse(request.Dati.DataChiusuraIter);
                    }
                }

                if (request.Dati.TipoVotazioneCheck)
                {
                    if (string.IsNullOrEmpty(request.Dati.TipoVotazione))
                        atto.TipoVotazioneIter = null;
                    else
                    {
                        atto.TipoVotazioneIter = int.Parse(request.Dati.TipoVotazione);
                    }
                }

                if (request.Dati.DataComunicazioneAssembleaCheck)
                {
                    if (string.IsNullOrEmpty(request.Dati.DataComunicazioneAssemblea))
                        atto.DataComunicazioneAssemblea = null;
                    else
                    {
                        atto.DataComunicazioneAssemblea = DateTime.Parse(request.Dati.DataComunicazioneAssemblea);
                    }
                }

                if (request.Dati.EmendatoCheck)
                {
                    atto.Emendato = request.Dati.Emendato;
                }

                if (request.Dati.PubblicatoCheck)
                {
                    atto.Pubblicato = request.Dati.Pubblicato;
                }

                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<ATTI_DASI> Get_ByQR(Guid id)
        {
            var uidAtto = await _unitOfWork
                .DASI
                .GetByQR(id);

            return await Get(uidAtto);
        }
    }
}