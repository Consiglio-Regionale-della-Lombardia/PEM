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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using PortaleRegione.BAL;
using PortaleRegione.Common;
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
        private readonly SeduteLogic _logicSedute;
        private readonly IUnitOfWork _unitOfWork;

        public DASILogic(IUnitOfWork unitOfWork, PersoneLogic logicPersona, AttiFirmeLogic logicFirme,
            SeduteLogic logicSedute)
        {
            _unitOfWork = unitOfWork;
            _logicPersona = logicPersona;
            _logicFirme = logicFirme;
            _logicSedute = logicSedute;
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
                    attoDto.UIDAtto = result.UIDAtto;
                    result.UID_QRCode = Guid.NewGuid();
                    result.Oggetto = attoDto.Oggetto;
                    result.Premesse = attoDto.Premesse;
                    result.Richiesta = attoDto.Richiesta;
                    result.IDTipo_Risposta = attoDto.IDTipo_Risposta;

                    if (attoDto.DocAllegatoGenerico_Stream != null)
                    {
                        var path = ByteArrayToFile(attoDto.DocAllegatoGenerico_Stream);
                        result.PATH_AllegatoGenerico =
                            Path.Combine(AppSettingsConfiguration.PrefissoCompatibilitaDocumenti, path);
                    }

                    _unitOfWork.DASI.Add(result);

                    await _unitOfWork.CompleteAsync();

                    await GestioneSoggettiInterrogati(attoDto);
                    await GestioneCommissioni(attoDto);
                    return result;
                }

                //Modifica
                var attoInDb = await _unitOfWork.DASI.Get(attoDto.UIDAtto);
                if (attoInDb == null)
                    throw new InvalidOperationException("Atto non trovato");

                attoInDb.UIDPersonaModifica = persona.UID_persona;
                attoInDb.DataModifica = DateTime.Now;
                attoInDb.Oggetto = attoDto.Oggetto;
                attoInDb.Premesse = attoDto.Premesse;
                attoInDb.Richiesta = attoDto.Richiesta;
                attoInDb.IDTipo_Risposta = attoDto.IDTipo_Risposta;

                if (attoDto.DocAllegatoGenerico_Stream != null)
                {
                    var path = ByteArrayToFile(attoDto.DocAllegatoGenerico_Stream);
                    attoInDb.PATH_AllegatoGenerico =
                        Path.Combine(AppSettingsConfiguration.PrefissoCompatibilitaDocumenti, path);
                }

                await _unitOfWork.CompleteAsync();

                await GestioneSoggettiInterrogati(attoDto, true);
                await GestioneCommissioni(attoDto, true);

                if (!string.IsNullOrEmpty(attoInDb.Atto_Certificato))
                {
                    //Il proponente ha firmato l'atto, ed è l'unico firmatario,
                    //in questo caso re-crypt del testo certificato
                    var body = await GetBodyDASI(attoInDb, null, persona,
                        TemplateTypeEnum.FIRMA);
                    var body_encrypt = EncryptString(body, Decrypt(attoInDb.Hash));

                    attoInDb.Atto_Certificato = body_encrypt;
                    await _unitOfWork.CompleteAsync();
                }

                return attoInDb;
            }
            catch (Exception e)
            {
                Log.Error("Logic - SalvaAtto - DASI", e);
                throw;
            }
        }

        private async Task GestioneCommissioni(AttoDASIDto attoDto, bool isUpdate = false)
        {
            try
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
            catch (Exception e)
            {
                Log.Error("Logic - GestioneCommissioni - DASI", e);
                throw;
            }
        }

        private async Task GestioneSoggettiInterrogati(AttoDASIDto attoDto, bool isUpdate = false)
        {
            try
            {
                if (isUpdate) await _unitOfWork.DASI.RimuoviSoggetti(attoDto.UIDAtto);

                if (!string.IsNullOrEmpty(attoDto.SoggettiInterrogati_client))
                {
                    var soggetti = attoDto
                        .SoggettiInterrogati_client
                        .Split(',')
                        .Select(item => Convert.ToInt32(item));
                    foreach (var soggetto in soggetti) _unitOfWork.DASI.AggiungiSoggetto(attoDto.UIDAtto, soggetto);
                }

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - GestioneSoggettiInterrogati - DASI", e);
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
                model.param.TryGetValue("CLIENT_MODE", out var CLIENT_MODE); // per trattazione aula
                var filtro_seduta =
                    model.filtro.FirstOrDefault(item => item.PropertyId == nameof(AttoDASIDto.UIDSeduta));
                var sedutaId = Guid.Empty;
                if (filtro_seduta != null) sedutaId = new Guid(filtro_seduta.Value.ToString());

                var queryFilter = new Filter<ATTI_DASI>();
                queryFilter.ImportStatements(model.filtro);
                var atti_in_db = await _unitOfWork
                    .DASI
                    .GetAll(persona,
                        model.page,
                        model.size,
                        queryFilter);
                if (!atti_in_db.Any())
                {
                    var defaultCounterBar =
                        await GetResponseCountBar(persona, GetResponseStatusFromFilters(model.filtro),
                            GetResponseTypeFromFilters(model.filtro), sedutaId, CLIENT_MODE,queryFilter);
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
                        CountBarData = defaultCounterBar
                    };
                }

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
                    await GetResponseCountBar(persona, responseModel.Stato, responseModel.Tipo, sedutaId, CLIENT_MODE, queryFilter);

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

                dto.NAtto = GetNome(attoInDb.NAtto, attoInDb.Progressivo.Value);

                if (!string.IsNullOrEmpty(attoInDb.DataPresentazione))
                    dto.DataPresentazione = Decrypt(attoInDb.DataPresentazione);
                if (!string.IsNullOrEmpty(attoInDb.Atto_Certificato))
                    dto.Atto_Certificato = Decrypt(attoInDb.Atto_Certificato, attoInDb.Hash);

                if (persona != null && (persona.CurrentRole == RuoliIntEnum.Consigliere_Regionale ||
                                        persona.CurrentRole == RuoliIntEnum.Assessore_Sottosegretario_Giunta))
                    dto.Firmato_Da_Me = await _unitOfWork.Atti_Firme.CheckFirmato(attoUid, persona.UID_persona);

                dto.Firma_da_ufficio = await _unitOfWork.Atti_Firme.CheckFirmatoDaUfficio(attoUid);
                dto.Firmato_Dal_Proponente =
                    await _unitOfWork.Atti_Firme.CheckFirmato(attoUid, attoInDb.UIDPersonaProponente.Value);

                dto.PersonaCreazione = personeInDbLight.First(p => p.UID_persona == attoInDb.UIDPersonaCreazione);
                dto.PersonaProponente =
                    personeInDbLight.First(p => p.UID_persona == attoInDb.UIDPersonaProponente);
                if (dto.UIDPersonaModifica.HasValue)
                    dto.PersonaModifica =
                        personeInDbLight.First(p => p.UID_persona == attoInDb.UIDPersonaModifica);

                dto.ConteggioFirme = await _logicFirme.CountFirme(attoUid);

                if (dto.ConteggioFirme > 1)
                {
                    var firme = await _logicFirme.GetFirme(attoInDb, FirmeTipoEnum.ATTIVI);
                    dto.Firme = firme
                        .Where(f => f.UID_persona != attoInDb.UIDPersonaProponente)
                        .Select(f => f.FirmaCert)
                        .Aggregate((i, j) => i + "<br>" + j);
                }

                dto.gruppi_politici =
                    Mapper.Map<View_gruppi_politici_con_giunta, GruppiDto>(
                        await _unitOfWork.Gruppi.Get(attoInDb.id_gruppo));
                if (persona != null)
                {
                    if (string.IsNullOrEmpty(attoInDb.DataPresentazione))
                        dto.Presentabile = await _unitOfWork
                            .DASI
                            .CheckIfPresentabile(dto,
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

                var soggettiInterrogati = await _unitOfWork.DASI.GetSoggettiInterrogati(dto.UIDAtto);
                dto.SoggettiInterrogati = soggettiInterrogati
                    .Select(Mapper.Map<View_cariche_assessori_in_carica, AssessoreInCaricaDto>).ToList();

                var commissioni = await _unitOfWork.DASI.GetCommissioni(dto.UIDAtto);
                dto.Commissioni = commissioni
                    .Select(Mapper.Map<View_Commissioni_attive, CommissioneDto>).ToList();

                if (attoInDb.UIDSeduta.HasValue)
                {
                    var sedutaInDb = await _logicSedute.GetSeduta(attoInDb.UIDSeduta.Value);
                    dto.Seduta = Mapper.Map<SEDUTE, SeduteDto>(sedutaInDb);
                }

                return dto;
            }
            catch (Exception e)
            {
                Log.Error("Logic - Get DTO Atto - DASI", e);
                throw;
            }
        }

        private async Task<CountBarData> GetResponseCountBar(PersonaDto persona, StatiAttoEnum stato, TipoAttoEnum tipo,
            Guid sedutaId, object _clientMode, Filter<ATTI_DASI> filtro)
        {
            var clientMode = (ClientModeEnum) Convert.ToInt16(_clientMode);

            var result = new CountBarData
            {
                ITL = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.ITL
                        , stato, sedutaId, clientMode, filtro),
                ITR = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.ITR
                        , stato, sedutaId, clientMode, filtro),
                IQT = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.IQT
                        , stato, sedutaId, clientMode, filtro),
                MOZ = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.MOZ
                        , stato, sedutaId, clientMode, filtro),
                ODG = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.ODG
                        , stato, sedutaId, clientMode, filtro),
                TUTTI = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.TUTTI
                        , stato, sedutaId, clientMode, filtro),
                BOZZE = await _unitOfWork
                    .DASI
                    .Count(persona,
                        tipo
                        , StatiAttoEnum.BOZZA, sedutaId, clientMode, filtro),
                PRESENTATI = await _unitOfWork
                    .DASI
                    .Count(persona,
                        tipo
                        , StatiAttoEnum.PRESENTATO, sedutaId, clientMode, filtro),
                IN_TRATTAZIONE = await _unitOfWork
                    .DASI
                    .Count(persona,
                        tipo
                        , StatiAttoEnum.IN_TRATTAZIONE, sedutaId, clientMode),
                CHIUSO = await _unitOfWork
                    .DASI
                    .Count(persona,
                        tipo
                        , StatiAttoEnum.CHIUSO, sedutaId, clientMode, filtro)
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
                return StatiAttoEnum.TUTTI;
            if (!modelFiltro.Any()) return StatiAttoEnum.TUTTI;

            var statusFilter = modelFiltro
                .FirstOrDefault(item => item.PropertyId == nameof(AttoDASIDto.IDStato));
            if (statusFilter == null)
                return StatiAttoEnum.TUTTI;

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

                    results.Add(idGuid, $"{atto.NAtto} - OK");
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

        public async Task<Dictionary<Guid, string>> Presenta(ComandiAzioneModel model,
            PersonaDto persona)
        {
            try
            {
                var results = new Dictionary<Guid, string>();
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                var counterPresentazioni = 1;
                var legislaturaId = await _unitOfWork.Legislature.Legislatura_Attiva();
                var legislatura = await _unitOfWork.Legislature.Get(legislaturaId);

                ManagerLogic.BloccaPresentazione = true;

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

                    var attoDto = await GetAttoDto(idGuid, persona, personeInDbLight);
                    if (atto.IDStato >= (int) StatiAttoEnum.PRESENTATO) continue;

                    if (!attoDto.Presentabile)
                    {
                        results.Add(idGuid, $"ERROR: Atto {attoDto.NAtto} non presentabile");
                        continue;
                    }

                    var contatore = await _unitOfWork.DASI.GetContatore((TipoAttoEnum) atto.Tipo, atto.IDTipo_Risposta);
                    var contatore_progressivo = contatore.Inizio + contatore.Contatore;
                    var etichetta_progressiva =
                        contatore_progressivo +
                        $"_{legislatura.num_legislatura}";
                    var etichetta_encrypt =
                        EncryptString(etichetta_progressiva, AppSettingsConfiguration.masterKey);

                    var checkProgressivo_unique =
                        await _unitOfWork.DASI.CheckProgressivo(etichetta_encrypt);

                    if (!checkProgressivo_unique)
                    {
                        results.Add(idGuid, $"ERROR: Progressivo {etichetta_progressiva} occupato");
                        continue;
                    }

                    atto.Etichetta = etichetta_progressiva;
                    atto.UIDPersonaPresentazione = persona.UID_persona;
                    atto.OrdineVisualizzazione = contatore_progressivo;
                    atto.Timestamp = DateTime.Now;
                    atto.DataPresentazione = EncryptString(atto.Timestamp.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                        AppSettingsConfiguration.masterKey);
                    atto.IDStato = (int) StatiAttoEnum.PRESENTATO;

                    atto.NAtto = etichetta_encrypt;

                    var count_firme = await _unitOfWork.Atti_Firme.CountFirme(idGuid);
                    atto.chkf = count_firme.ToString();

                    await _unitOfWork.CompleteAsync();

                    results.Add(idGuid, $"{attoDto.NAtto} - OK");

                    _unitOfWork.DASI.IncrementaContatore(contatore);
                    _unitOfWork.Stampe.Add(new STAMPE
                    {
                        UIDStampa = Guid.NewGuid(),
                        UIDUtenteRichiesta = atto.UIDPersonaPresentazione.Value,
                        DataRichiesta = DateTime.Now,
                        UIDAtto = atto.UIDAtto,
                        Da = 1,
                        A = 1,
                        Ordine = 1,
                        Notifica = true,
                        Scadenza = DateTime.Now.AddDays(Convert.ToDouble(AppSettingsConfiguration.GiorniValiditaLink)),
                        DASI = true
                    });
                    await _unitOfWork.CompleteAsync();
                    counterPresentazioni++;
                }

                return results;
            }
            catch (Exception e)
            {
                Log.Error("Logic - Presenta - DASI", e);
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
                atto.IDStato = (int) StatiAttoEnum.RITIRATO;
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
                    },
                    SoggettiInterrogabili = await GetSoggettiInterrogabili(),
                    CommissioniAttive = await GetCommissioniAttive()
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
                result.Atto.SoggettiInterrogati = new List<AssessoreInCaricaDto>();
                result.Atto.Commissioni = new List<CommissioneDto>();
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
                var result = new DASIFormModel
                {
                    Atto = dto, SoggettiInterrogabili = await GetSoggettiInterrogabili(),
                    CommissioniAttive = await GetCommissioniAttive()
                };

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - ModificaModello - DASI", e);
                throw e;
            }
        }

        public async Task<List<AssessoreInCaricaDto>> GetSoggettiInterrogabili()
        {
            try
            {
                var result = await _unitOfWork.DASI.GetSoggettiInterrogabili();
                return result
                    .Select(Mapper.Map<View_cariche_assessori_in_carica, AssessoreInCaricaDto>)
                    .ToList();
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetSoggettiInterrogabili - DASI", e);
                throw;
            }
        }

        public async Task<List<CommissioneDto>> GetCommissioniAttive()
        {
            try
            {
                var result = await _unitOfWork.DASI.GetCommissioniAttive();
                return result
                    .Select(Mapper.Map<View_Commissioni_attive, CommissioneDto>)
                    .ToList();
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetCommissioniAttive - DASI", e);
                throw;
            }
        }

        public async Task<Dictionary<Guid, string>> ModificaStato(ModificaStatoAttoModel model,
            PersonaDto personaDto)
        {
            try
            {
                var results = new Dictionary<Guid, string>();
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                if (model.All && !model.Lista.Any())
                {
                    model.Lista = await ScaricaAtti_UID(model.CurrentStatus, model.CurrentType, personaDto);
                }
                else if (model.All && model.Lista.Any())
                {
                    var attiInDb =
                        await ScaricaAtti_UID(model.CurrentStatus, model.CurrentType, personaDto);
                    attiInDb.RemoveAll(guid => model.Lista.Contains(guid));
                    model.Lista = attiInDb;
                }

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

                    atto.IDStato = (int) model.Stato;
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
            PersonaDto personaDto)
        {
            try
            {
                var atto = await Get(model.UidAtto);
                if (atto == null) throw new Exception("ERROR: NON TROVATO");

                atto.UIDSeduta = model.UidSeduta;
                atto.DataIscrizioneSeduta = DateTime.Now;
                atto.UIDPersonaIscrizioneSeduta = personaDto.UID_persona;
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - IscrizioneSeduta", e);
                throw e;
            }
        }

        public async Task RimuoviSeduta(IscriviSedutaDASIModel model)
        {
            try
            {
                var atto = await Get(model.UidAtto);
                if (atto == null) throw new Exception("ERROR: NON TROVATO");

                atto.UIDSeduta = null;
                atto.DataIscrizioneSeduta = null;
                atto.UIDPersonaIscrizioneSeduta = null;
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - RimuoviSeduta", e);
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
                Value = (int) stato,
                Connector = FilterStatementConnector.And
            };
            filtro.Add(filtroStato);
            if (tipo != TipoAttoEnum.TUTTI)
            {
                var filtroTipo = new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.Tipo),
                    Operation = Operation.EqualTo,
                    Value = (int) tipo
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

        public async Task<List<Guid>> GetDASI_UID_RawChunk(BaseRequest<AttoDASIDto> model, Filter<ATTI_DASI> filtro,
            PersonaDto persona)
        {
            try
            {
                var em_in_db = await _unitOfWork
                    .DASI
                    .GetAll(persona
                        , model.page
                        , model.size
                        , filtro);
                return em_in_db;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetDASI_UID_RawChunk", e);
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

        public async Task<int> CountByQuery(string query)
        {
            try
            {
                return await _unitOfWork.DASI.CountByQuery(query);
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
                var atti = _unitOfWork
                    .DASI
                    .GetByQuery(model);
                var result = new List<AttoDASIDto>();
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                foreach (var idAtto in atti)
                {
                    var dto = await GetAttoDto(idAtto, null, personeInDbLight);
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

        public async Task<string> GetCopertina(ByQueryModel model)
        {
            try
            {

                var count = await CountByQuery(model.Query);
                var atti = new List<AttoDASIDto>();
                atti.AddRange(await GetByQuery(model));
                while (atti.Count < count)
                {
                    model.page += 1;
                    atti.AddRange(await GetByQuery(model));
                }

                var legislatura = await _unitOfWork.Legislature.Get(atti.First().Legislatura);
                var body = GetTemplate(TemplateTypeEnum.PDF_COPERTINA, true);
                body = body.Replace("{LEGISLATURA}", legislatura.num_legislatura);

                var templateItemIndice = "<div style='text-align:left;'>" +
                                         "<b>{TipoAtto}{NAtto}</b><br/>" +
                                         "Data Presentazione: {DataPresentazione}<br/>" +
                                         "<p>{Oggetto}</p><br/>" +
                                         "Iniziativa: {Firmatari}<br/>" +
                                         "Stato: {Stato}<br/>" +
                                         "</div><br/>";

                var bodyIndice = new StringBuilder();
                foreach (var dasiDto in atti)
                {
                    bodyIndice.Append(templateItemIndice
                        .Replace("TipoAtto", Utility.GetText_TipoDASI(dasiDto.Tipo))
                        .Replace("NAtto", dasiDto.NAtto)
                        .Replace("DataPresentazione", dasiDto.DataPresentazione)
                        .Replace("Oggetto", dasiDto.Oggetto)
                        .Replace("Firmatari", $"{dasiDto.PersonaProponente.DisplayName}, {dasiDto.Firme.Replace("<br>", ", ")}")
                        .Replace("Stato", Utility.GetText_StatoDASI(dasiDto.IDStato)));
                }

                body = body.Replace("{LISTA_LIGHT}", bodyIndice.ToString());

                return body;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetCopertina", e);
                throw e;
            }

        }
    }
}