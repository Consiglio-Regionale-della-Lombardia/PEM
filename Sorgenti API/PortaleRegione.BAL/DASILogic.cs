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
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ExpressionBuilder.Generics;
using PortaleRegione.BAL;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;

namespace PortaleRegione.API.Controllers
{
    public class DASILogic : BaseLogic
    {
        private readonly AttiFirmeLogic _logicFirme;
        private readonly IUnitOfWork _unitOfWork;

        public DASILogic(IUnitOfWork unitOfWork, AttiFirmeLogic logicFirme)
        {
            _unitOfWork = unitOfWork;
            _logicFirme = logicFirme;
        }

        public async Task<ATTI_DASI> Salva(AttoDASIDto attoDto, PersonaDto persona)
        {
            try
            {
                var result = new ATTI_DASI();
                if (attoDto.UIDAtto == Guid.Empty)
                {
                    //Nuovo inserimento
                    result.UIDAtto = Guid.NewGuid();
                    result.UIDPersonaCreazione = persona.UID_persona;
                    result.UIDPersonaProponente = attoDto.UIDPersonaProponente;
                    result.DataCreazione = DateTime.Now;
                    result.IDStato = (int) StatiAttoEnum.BOZZA;
                    result.Oggetto = attoDto.Oggetto;
                    result.Tipo = attoDto.Tipo;
                    result.IDTipo_Risposta = (int) TipoRispostaEnum.ORALE;
                    result.UID_QRCode = Guid.NewGuid();
                    result.idRuoloCreazione = (int) persona.CurrentRole;
                    result.id_gruppo = persona.Gruppo.id_gruppo;
                    var random = new Random();
                    result.Progressivo = random.Next(1, 1000);

                    _unitOfWork.DASI.Add(result);
                }
                else
                {
                    //Modifica
                    result = Mapper.Map<AttoDASIDto, ATTI_DASI>(attoDto);
                }

                await _unitOfWork.CompleteAsync();
                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - SalvaAtto - DASI", e);
                throw;
            }
        }

        public async Task<ATTI_DASI> Get(Guid id)
        {
            try
            {
                var attoInDb = await _unitOfWork.DASI.Get(id);
                return attoInDb;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetAtto - DASI", e);
                throw;
            }
        }

        public async Task<RiepilogoDASIModel> Get(BaseRequest<AttoDASIDto> model, PersonaDto persona, Uri uri)
        {
            try
            {
                var queryFilter = new Filter<ATTI_DASI>();
                queryFilter.ImportStatements(model.filtro);
                var atti_in_db = await _unitOfWork
                    .DASI
                    .GetAll(persona,
                        model.page,
                        model.size,
                        queryFilter);
                if (!atti_in_db.Any())
                    return new RiepilogoDASIModel
                    {
                        Data = new BaseResponse<AttoDASIDto>(
                            model.page
                            , model.size
                            , new List<AttoDASIDto>()
                            , model.filtro
                            , 0
                            , uri),
                        Stato = GetResponseStatusFromFilters(model.filtro),
                        Tipo = GetResponseTypeFromFilters(model.filtro),
                        CountBarData = new CountBarData()
                    };

                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                var result = new List<AttoDASIDto>();
                foreach (var attoUId in atti_in_db)
                {
                    var dto = await GetAttoDto(attoUId, persona, personeInDbLight);
                    result.Add(dto);
                }

                var totaleAtti = await _unitOfWork
                    .DASI
                    .Count(persona,
                        queryFilter);

                var responseModel = new RiepilogoDASIModel
                {
                    Data = new BaseResponse<AttoDASIDto>(
                        model.page
                        , model.size
                        , result
                        , model.filtro
                        , totaleAtti
                        , uri),
                    Stato = GetResponseStatusFromFilters(model.filtro),
                    Tipo = GetResponseTypeFromFilters(model.filtro)
                };

                responseModel.CountBarData =
                    await GetResponseCountBar(persona, responseModel.Stato);

                return responseModel;
            }
            catch (Exception e)
            {
                Log.Error("Logic - Get Riepilogo Atti - DASI", e);
                throw;
            }
        }

        public async Task<AttoDASIDto> GetAttoDto(Guid attoUid, PersonaDto persona,
            List<PersonaLightDto> personeInDbLight)
        {
            try
            {
                var attoInDb = await _unitOfWork.DASI.Get(attoUid);
                var dto = Mapper.Map<ATTI_DASI, AttoDASIDto>(attoInDb);

                if (!string.IsNullOrEmpty(dto.DataDeposito_crypt))
                {
                    //Atto certificato
                    dto.DataDeposito = Decrypt(dto.DataDeposito_crypt);
                }

                if (persona != null && (persona.CurrentRole == RuoliIntEnum.Consigliere_Regionale ||
                                        persona.CurrentRole == RuoliIntEnum.Assessore_Sottosegretario_Giunta))
                    dto.Firmato_Da_Me = await _unitOfWork.Atti_Firme.CheckFirmato(attoUid, persona.UID_persona);

                dto.Firma_da_ufficio = await _unitOfWork.Atti_Firme.CheckFirmatoDaUfficio(attoUid);
                dto.Firmato_Dal_Proponente =
                    await _unitOfWork.Atti_Firme.CheckFirmato(attoUid, dto.UIDPersonaProponente.Value);
                
                dto.PersonaCreazione = personeInDbLight.First(p => p.UID_persona == dto.UIDPersonaCreazione);
                dto.PersonaProponente =
                    personeInDbLight.First(p => p.UID_persona == dto.UIDPersonaProponente);
                if (dto.UIDPersonaModifica.HasValue)
                    dto.PersonaModifica =
                        personeInDbLight.First(p => p.UID_persona == dto.UIDPersonaModifica);
                
                dto.ConteggioFirme = await _logicFirme.CountFirme(attoUid);

                if (dto.ConteggioFirme > 1)
                {
                    var firme = await _logicFirme.GetFirme(attoInDb, FirmeTipoEnum.ATTIVI);
                    dto.Firme = firme
                        .Where(f => f.UID_persona != dto.UIDPersonaProponente)
                        .Select(f => f.FirmaCert)
                        .Aggregate((i, j) => i + "<br>" + j);
                }
                
                return dto;
            }
            catch (Exception e)
            {
                Log.Error("Logic - Get DTO Atto - DASI", e);
                throw;
            }
        }

        private async Task<CountBarData> GetResponseCountBar(PersonaDto persona, StatiAttoEnum stato)
        {
            var result = new CountBarData
            {
                ITL = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.ITL
                        , stato),
                ITR = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.ITR
                        , stato),
                IQT = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.IQT
                        , stato),
                MOZ = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.MOZ
                        , stato),
                ODG = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.ODG
                        , stato),
                TUTTI = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.TUTTI
                        , stato)
            };

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

            return (TipoAttoEnum) Convert.ToInt16(typeFilter.Value);
        }

        private StatiAttoEnum GetResponseStatusFromFilters(List<FilterStatement<AttoDASIDto>> modelFiltro)
        {
            if (modelFiltro == null)
                return StatiAttoEnum.BOZZA;
            if (!modelFiltro.Any()) return StatiAttoEnum.BOZZA;

            var statusFilter = modelFiltro
                .FirstOrDefault(item => item.PropertyId == nameof(AttoDASIDto.IDStato));
            if (statusFilter == null)
                return StatiAttoEnum.BOZZA;

            return (StatiAttoEnum) Convert.ToInt16(statusFilter.Value);
        }

        public async Task<Dictionary<Guid, string>> Firma(ComandiAzioneModel firmaModel, PersonaDto persona,
            PinDto pin, bool firmaUfficio = false)
        {
            try
            {
                var results = new Dictionary<Guid, string>();
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                var counterFirme = 1;
                var carica = await _unitOfWork.Persone.GetCarica(persona.UID_persona);

                foreach (var idGuid in firmaModel.Lista)
                {
                    if (counterFirme == Convert.ToInt32(AppSettingsConfiguration.LimiteFirmaMassivo) + 1) break;

                    var attoInDb = await _unitOfWork.DASI.Get(idGuid);
                    var atto = await GetAttoDto(idGuid, persona, personeInDbLight);
                    if (atto == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    if (atto.IDStato >= (int) StatiAttoEnum.IN_TRATTAZIONE)
                    {
                        results.Add(idGuid,
                            $"ERROR: Atto {atto.NAtto} si trova in trattazione e non è più sottoscrivibile");
                        continue;
                    }

                    var firmaCert = string.Empty;

                    if (firmaUfficio)
                    {
                        //Controllo se l'utente ha già firmato
                        if (atto.Firma_da_ufficio)
                        {
                            results.Add(idGuid, $"ERROR: Atto {atto.NAtto} già firmato dall'ufficio");
                            continue;
                        }

                        firmaCert = EncryptString($"{AppSettingsConfiguration.FirmaUfficio}"
                            , AppSettingsConfiguration.masterKey);
                    }
                    else
                    {
                        //Controllo se l'utente ha già firmato
                        if (atto.Firmato_Da_Me) continue;

                        //Controllo la firma del proponente
                        if (!atto.Firmato_Dal_Proponente && atto.UIDPersonaProponente.Value != persona.UID_persona)
                        {
                            results.Add(idGuid, $"ERROR: Il Proponente non ha ancora firmato l'atto {atto.NAtto}");
                            continue;
                        }

                        var info_codice_carica_gruppo = string.Empty;
                        switch (persona.CurrentRole)
                        {
                            case RuoliIntEnum.Consigliere_Regionale:
                                info_codice_carica_gruppo = persona.Gruppo.codice_gruppo;
                                break;
                            case RuoliIntEnum.Assessore_Sottosegretario_Giunta:
                                info_codice_carica_gruppo = carica;
                                break;
                        }

                        var bodyFirmaCert =
                            $"{persona.DisplayName} ({info_codice_carica_gruppo})";
                        firmaCert = EncryptString(bodyFirmaCert
                            , AppSettingsConfiguration.masterKey);
                    }

                    var dataFirma = EncryptString(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                        AppSettingsConfiguration.masterKey);

                    var countFirme = await _unitOfWork.Atti_Firme.CountFirme(idGuid);
                    if (countFirme == 0)
                    {
                        //Se è la prima firma dell'atto, questo viene cryptato e così certificato e non modificabile
                        attoInDb.Hash = firmaUfficio
                            ? EncryptString(AppSettingsConfiguration.MasterPIN, AppSettingsConfiguration.masterKey)
                            : pin.PIN;
                        attoInDb.UIDPersonaPrimaFirma = persona.UID_persona;
                        attoInDb.DataPrimaFirma = DateTime.Now;
                        //TODO: GET BODY DASI
                        //var body = await GetBodyEM(atto, new List<FirmeDto>
                        //    {
                        //        new FirmeDto
                        //        {
                        //            UIDEM = idGuid,
                        //            UID_persona = persona.UID_persona,
                        //            FirmaCert = firmaCert,
                        //            Data_firma = dataFirma,
                        //            ufficio = firmaUfficio
                        //        }
                        //    }, persona,
                        //    TemplateTypeEnum.FIRMA);
                        var body_encrypt = EncryptString(attoInDb.Testo,
                            firmaUfficio ? AppSettingsConfiguration.MasterPIN : pin.PIN_Decrypt);

                        attoInDb.Testo_crypt = body_encrypt;
                    }

                    await _unitOfWork.Atti_Firme.Firma(idGuid, persona.UID_persona, firmaCert, dataFirma, firmaUfficio);

                    //TODO: DESTINATARI DASI
                    //var is_destinatario_notifica =
                    //    await _unitOfWork.Notifiche_Destinatari.ExistDestinatarioNotifica(idGuid, persona.UID_persona);
                    //if (is_destinatario_notifica)
                    //    await _unitOfWork.Notifiche_Destinatari.SetSeen_DestinatarioNotifica(idGuid,
                    //        persona.UID_persona);

                    results.Add(idGuid, $"{attoInDb.NAtto} - OK");
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
            try
            {
                var results = new Dictionary<Guid, string>();
                var ruoloSegreterie = await _unitOfWork.Ruoli.Get((int) RuoliIntEnum.Segreteria_Assemblea);
                var jumpMail = false;

                foreach (var idGuid in firmaModel.Lista)
                {
                    var atto = await _unitOfWork.DASI.Get(idGuid);
                    if (atto == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    var countFirme = await _unitOfWork.Firme.CountFirme(idGuid);
                    if (countFirme == 1)
                    {
                        //TODO: BLOCCO RITIRO FIRMA

                        //RITIRA EM
                        atto.IDStato = (int) StatiAttoEnum.RITIRATO;
                        atto.UIDPersonaRitiro = persona.UID_persona;
                        atto.DataRitiro = DateTime.Now;

                        //TODO: INVIO MAIL ALLA SEGRETERIA
                    }

                    //RITIRA FIRMA
                    var firmeAttive = await _unitOfWork
                        .Atti_Firme
                        .GetFirmatari(atto, FirmeTipoEnum.ATTIVI);
                    ATTI_FIRME firma_utente;
                    if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                        || persona.CurrentRole == RuoliIntEnum.Segreteria_Assemblea)
                        firma_utente = firmeAttive.Single(f => f.ufficio);
                    else
                        firma_utente = firmeAttive.Single(f => f.UID_persona == persona.UID_persona);

                    firma_utente.Data_ritirofirma =
                        EncryptString(DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            AppSettingsConfiguration.masterKey);

                    //TODO: INVIO MAIL ALLA SEGRETERIA

                    await _unitOfWork.CompleteAsync();
                    results.Add(idGuid, "OK");
                    jumpMail = false;
                }

                return results;
            }
            catch (Exception e)
            {
                Log.Error("Logic - RitiroFirma - DASI", e);
                throw e;
            }
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
                        atto.Testo_crypt = string.Empty;
                        atto.DataPrimaFirma = null;
                        atto.UIDPersonaPrimaFirma = null;
                    }

                    //RITIRA FIRMA
                    var firmeAttive = await _logicFirme.GetFirme(atto, FirmeTipoEnum.ATTIVI);
                    var firma_utente = firmeAttive.Single(f => f.UID_persona == persona.UID_persona);
                    var firma_da_ritirare =
                        await _unitOfWork.Atti_Firme.Get(firma_utente.UIDAtto, firma_utente.UID_persona);
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

        public async Task<Dictionary<Guid, string>> Deposita(ComandiAzioneModel depositoModel,
            PersonaDto persona)
        {
            try
            {
                var results = new Dictionary<Guid, string>();
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                ManagerLogic.BloccaDeposito = true;
                var counterDepositi = 1;

                foreach (var idGuid in depositoModel.Lista)
                {
                    if (counterDepositi == Convert.ToInt32(AppSettingsConfiguration.LimiteDepositoMassivo) + 1) break;

                    var atto = await _unitOfWork.DASI.Get(idGuid);
                    if (atto == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    var attoDto = await GetAttoDto(idGuid, persona, personeInDbLight);
                    var nAtto = atto.NAtto;
                    if (atto.IDStato >= (int) StatiAttoEnum.PRESENTATO) continue;

                    if (!attoDto.Depositabile)
                    {
                        results.Add(idGuid, $"ERROR: Atto {nAtto} non presentabile");
                        continue;
                    }

                    var etichetta_progressiva =
                        await _unitOfWork.DASI.GetEtichetta(atto) + 1;
                    var etichetta_encrypt =
                        EncryptString(etichetta_progressiva.ToString(), AppSettingsConfiguration.masterKey);

                    //TODO: CONTROLLO PROGRESSIVO SE UNIVOCO PER EVITARE DOPPIONI O BUCHI NELLA NUMERAZIONE

                    atto.UIDPersonaDeposito = persona.UID_persona;
                    atto.OrdineVisualizzazione = await _unitOfWork.DASI.GetOrdine(atto.Tipo) + 1;
                    atto.Timestamp = DateTime.Now;
                    atto.DataDeposito_crypt = EncryptString(atto.Timestamp.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                        AppSettingsConfiguration.masterKey);
                    atto.IDStato = (int) StatiAttoEnum.PRESENTATO;

                    var count_firme = await _unitOfWork.Atti_Firme.CountFirme(idGuid);
                    atto.chkf = count_firme.ToString();

                    await _unitOfWork.CompleteAsync();

                    results.Add(idGuid, $"{nAtto} - OK");

                    //TODO: INSERIRE STAMPA

                    await _unitOfWork.CompleteAsync();
                    counterDepositi++;
                }

                return results;
            }
            catch (Exception e)
            {
                Log.Error("Logic - Deposita - DASI", e);
                throw e;
            }
        }

        public async Task<T> GetFirme<T>(AttoDASIDto em, FirmeTipoEnum tipo)
        {
            throw new NotImplementedException();
        }
    }
}