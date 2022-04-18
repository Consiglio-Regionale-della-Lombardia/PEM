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
        private readonly PersoneLogic _logicPersona;
        private readonly IUnitOfWork _unitOfWork;

        public DASILogic(IUnitOfWork unitOfWork, PersoneLogic logicPersona, AttiFirmeLogic logicFirme)
        {
            _unitOfWork = unitOfWork;
            _logicPersona = logicPersona;
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
                    result.Tipo = attoDto.Tipo;
                    var legislatura = await _unitOfWork.Legislature.Legislatura_Attiva();
                    result.Legislatura = legislatura;
                    var progressivo =
                        await _unitOfWork.DASI.GetProgressivo((TipoAttoEnum) attoDto.Tipo, persona.Gruppo.id_gruppo,
                            legislatura);
                    result.Progressivo = progressivo;

                    if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                        || persona.CurrentRole == RuoliIntEnum.Segreteria_Assemblea
                        || persona.CurrentRole == RuoliIntEnum.Presidente_Regione)
                        result.IDStato = (int) StatiAttoEnum.BOZZA;
                    else
                        result.IDStato = persona.Gruppo.abilita_em_privati
                            ? (int) StatiAttoEnum.BOZZA_RISERVATA
                            : (int) StatiAttoEnum.BOZZA;

                    if (persona.CurrentRole == RuoliIntEnum.Consigliere_Regionale ||
                        persona.CurrentRole == RuoliIntEnum.Assessore_Sottosegretario_Giunta)
                        result.UIDPersonaProponente = persona.UID_persona;
                    else
                        result.UIDPersonaProponente = attoDto.UIDPersonaProponente;

                    result.UIDPersonaCreazione = persona.UID_persona;
                    result.DataCreazione = DateTime.Now;
                    result.idRuoloCreazione = (int) persona.CurrentRole;
                    if (persona.CurrentRole != RuoliIntEnum.Amministratore_PEM
                        && persona.CurrentRole != RuoliIntEnum.Segreteria_Assemblea
                        && persona.CurrentRole != RuoliIntEnum.Presidente_Regione)
                    {
                        result.id_gruppo = persona.Gruppo.id_gruppo;
                    }
                    else
                    {
                        var proponente = await _logicPersona.GetPersona(result.UIDPersonaProponente.Value);
                        result.id_gruppo = proponente.Gruppo.id_gruppo;
                    }

                    result.UIDAtto = Guid.NewGuid();
                    result.UID_QRCode = Guid.NewGuid();
                    result.Oggetto = attoDto.Oggetto;
                    result.Testo = attoDto.Testo;
                    result.IDTipo_Risposta = attoDto.IDTipo_Risposta;

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

                dto.NAtto = GetNome(dto.NAtto, dto.Progressivo);

                if (!string.IsNullOrEmpty(dto.DataDeposito_crypt))
                    //Atto certificato
                    dto.DataDeposito = Decrypt(dto.DataDeposito_crypt);
                if (!string.IsNullOrEmpty(dto.Atto_Certificato))
                    dto.Atto_Certificato = Decrypt(dto.Atto_Certificato, dto.Hash);

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

                dto.gruppi_politici =
                    Mapper.Map<View_gruppi_politici_con_giunta, GruppiDto>(
                        await _unitOfWork.Gruppi.Get(attoInDb.id_gruppo));

                if (string.IsNullOrEmpty(dto.DataDeposito))
                    dto.Depositabile = await _unitOfWork
                        .DASI
                        .CheckIfDepositabile(dto,
                            persona);

                if (dto.IDStato <= (int) StatiAttoEnum.PRESENTATO)
                    dto.Firmabile = await _unitOfWork
                        .Atti_Firme
                        .CheckIfFirmabile(dto,
                            persona);

                if (!dto.DataRitiro.HasValue && dto.IDStato == (int) StatiAttoEnum.PRESENTATO)
                    dto.Ritirabile = _unitOfWork
                        .DASI
                        .CheckIfRitirabile(dto,
                            persona);

                if (string.IsNullOrEmpty(dto.DataDeposito))
                    dto.Eliminabile = _unitOfWork
                        .DASI
                        .CheckIfEliminabile(dto,
                            persona);

                dto.Modificabile = await _unitOfWork
                    .DASI
                    .CheckIfModificabile(dto,
                        persona);

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
                        var body = await GetBodyDASI(attoInDb, new List<AttiFirmeDto>
                            {
                                new AttiFirmeDto
                                {
                                    UIDAtto = idGuid,
                                    UID_persona = persona.UID_persona,
                                    FirmaCert = firmaCert,
                                    Data_firma = dataFirma,
                                    ufficio = firmaUfficio
                                }
                            }, persona,
                            TemplateTypeEnum.FIRMA);
                        var body_encrypt = EncryptString(body,
                            firmaUfficio ? AppSettingsConfiguration.MasterPIN : pin.PIN_Decrypt);

                        attoInDb.Atto_Certificato = body_encrypt;
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
                        atto.Atto_Certificato = string.Empty;
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
                var legislaturaId = await _unitOfWork.Legislature.Legislatura_Attiva();
                var legislatura = await _unitOfWork.Legislature.Get(legislaturaId);

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
                    if (atto.IDStato >= (int) StatiAttoEnum.PRESENTATO) continue;

                    if (!attoDto.Depositabile)
                    {
                        results.Add(idGuid, $"ERROR: Atto {attoDto.NAtto} non presentabile");
                        continue;
                    }

                    var etichetta_progressiva =
                        await _unitOfWork.DASI.GetEtichetta((TipoAttoEnum) atto.Tipo, atto.IDTipo_Risposta) +
                        $"_{legislatura.num_legislatura}";
                    var etichetta_encrypt =
                        EncryptString(etichetta_progressiva, AppSettingsConfiguration.masterKey);

                    var checkProgressivo_unique =
                        await _unitOfWork.DASI.CheckProgressivo(etichetta_encrypt);

                    if (!checkProgressivo_unique)
                    {
                        results.Add(idGuid, $"ERROR: Progressivo {attoDto.NAtto} occupato");
                        continue;
                    }

                    atto.UIDPersonaDeposito = persona.UID_persona;
                    atto.OrdineVisualizzazione = await _unitOfWork.DASI.GetOrdine(atto.Tipo) + 1;
                    atto.Timestamp = DateTime.Now;
                    atto.DataDeposito_crypt = EncryptString(atto.Timestamp.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                        AppSettingsConfiguration.masterKey);
                    atto.IDStato = (int) StatiAttoEnum.PRESENTATO;

                    var count_firme = await _unitOfWork.Atti_Firme.CountFirme(idGuid);
                    atto.chkf = count_firme.ToString();

                    await _unitOfWork.CompleteAsync();

                    results.Add(idGuid, $"{attoDto.NAtto} - OK");

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

        public async Task<string> GetBodyDASI(ATTI_DASI atto, IEnumerable<AttiFirmeDto> firme, PersonaDto persona,
            TemplateTypeEnum template, bool isDeposito = false)
        {
            try
            {
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                var dto = await GetAttoDto(atto.UIDAtto, persona, personeInDbLight);

                try
                {
                    var tipo = atto.Tipo switch
                    {
                        (int) TipoAttoEnum.ITL => nameof(TipoAttoEnum.ITL),
                        (int) TipoAttoEnum.ITR => nameof(TipoAttoEnum.ITR),
                        (int) TipoAttoEnum.IQT => nameof(TipoAttoEnum.IQT),
                        (int) TipoAttoEnum.MOZ => nameof(TipoAttoEnum.MOZ),
                        (int) TipoAttoEnum.ODG => nameof(TipoAttoEnum.ODG),
                        _ => ""
                    };

                    var body = GetTemplate(template, true);

                    switch (template)
                    {
                        case TemplateTypeEnum.MAIL:
                            GetBody(dto, tipo, firme, persona, false, ref body);
                            break;
                        case TemplateTypeEnum.PDF:
                            GetBody(dto, tipo, firme, persona, true, ref body);
                            break;
                        case TemplateTypeEnum.HTML:
                            GetBody(dto, tipo, firme, persona, false, ref body);
                            break;
                        case TemplateTypeEnum.FIRMA:
                            GetBodyTemporaneo(dto, tipo, ref body);
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
                    Log.Error("GetBodyEM", e);
                    throw e;
                }
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetBodyEM", e);
                throw e;
            }
        }

        public async Task Elimina(ATTI_DASI atto, Guid sessionCurrentUId)
        {
            try
            {
                atto.Eliminato = true;
                atto.DataElimina = DateTime.Now;
                atto.UIDPersonaElimina = sessionCurrentUId;

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - Elimina Atto - DASI", e);
                throw;
            }
        }

        public async Task Ritira(ATTI_DASI atto, PersonaDto persona)
        {
            try
            {
                atto.IDStato = (int) StatiEnum.Ritirato;
                atto.UIDPersonaRitiro = persona.UID_persona;
                atto.DataRitiro = DateTime.Now;

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - Ritira Atto - DASI", e);
                throw e;
            }
        }

        public async Task<DASIFormModel> NuovoModello(TipoAttoEnum tipo, PersonaDto persona)
        {
            try
            {
                var result = new DASIFormModel
                {
                    Atto = new AttoDASIDto
                    {
                        Tipo = (int) tipo
                    }
                };

                var legislatura = await _unitOfWork.Legislature.Legislatura_Attiva();
                var progressivo =
                    await _unitOfWork.DASI.GetProgressivo(tipo, persona.Gruppo.id_gruppo, legislatura);
                result.Atto.Progressivo = progressivo;

                if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                    || persona.CurrentRole == RuoliIntEnum.Segreteria_Assemblea
                    || persona.CurrentRole == RuoliIntEnum.Presidente_Regione)
                    result.Atto.IDStato = (int) StatiAttoEnum.BOZZA;
                else
                    result.Atto.IDStato = persona.Gruppo.abilita_em_privati
                        ? (int) StatiAttoEnum.BOZZA_RISERVATA
                        : (int) StatiAttoEnum.BOZZA;

                result.Atto.NAtto = GetNome(result.Atto.NAtto, progressivo);

                if (persona.CurrentRole == RuoliIntEnum.Consigliere_Regionale ||
                    persona.CurrentRole == RuoliIntEnum.Assessore_Sottosegretario_Giunta)
                {
                    result.Atto.UIDPersonaProponente = persona.UID_persona;
                    result.Atto.PersonaProponente = new PersonaLightDto
                    {
                        UID_persona = persona.UID_persona,
                        cognome = persona.cognome,
                        nome = persona.nome
                    };
                }

                result.Atto.UIDPersonaCreazione = persona.UID_persona;
                result.Atto.DataCreazione = DateTime.Now;
                result.Atto.idRuoloCreazione = (int) persona.CurrentRole;
                if (persona.CurrentRole != RuoliIntEnum.Amministratore_PEM
                    && persona.CurrentRole != RuoliIntEnum.Segreteria_Assemblea
                    && persona.CurrentRole != RuoliIntEnum.Presidente_Regione)
                    result.Atto.id_gruppo = persona.Gruppo.id_gruppo;

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - NuovoModello - DASI", e);
                throw e;
            }
        }

        public async Task<DASIFormModel> ModificaModello(ATTI_DASI atto, PersonaDto persona)
        {
            try
            {
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                var dto = await GetAttoDto(atto.UIDAtto, persona, personeInDbLight);
                var result = new DASIFormModel {Atto = dto};

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - ModificaModello - DASI", e);
                throw e;
            }
        }
    }
}