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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortaleRegione.API.Controllers
{
    public class DASILogic : BaseLogic
    {
        private readonly AttiLogic _logicAtti;
        private readonly AttiFirmeLogic _logicFirme;
        private readonly PersoneLogic _logicPersona;
        private readonly SeduteLogic _logicSedute;
        private readonly UtilsLogic _logicUtil;
        private readonly IUnitOfWork _unitOfWork;

        public DASILogic(IUnitOfWork unitOfWork, PersoneLogic logicPersona, AttiFirmeLogic logicFirme,
            SeduteLogic logicSedute, AttiLogic logicAtti, UtilsLogic logicUtil)
        {
            _unitOfWork = unitOfWork;
            _logicPersona = logicPersona;
            _logicFirme = logicFirme;
            _logicSedute = logicSedute;
            _logicAtti = logicAtti;
            _logicUtil = logicUtil;
        }

        public async Task<ATTI_DASI> Salva(AttoDASIDto attoDto, PersonaDto persona)
        {
            try
            {
                if (!attoDto.UIDPersonaProponente.HasValue)
                    throw new InvalidOperationException("Indicare un proponente");
                if (attoDto.UIDPersonaProponente.Value == Guid.Empty)
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
                        result.DataRichiestaIscrizioneSeduta = EncryptString(seduta.Data_seduta.ToString("dd/MM/yyyy"),
                            AppSettingsConfiguration.masterKey);
                        result.UIDPersonaRichiestaIscrizione = persona.UID_persona;

                        result.Non_Passaggio_In_Esame = attoDto.Non_Passaggio_In_Esame;
                    }

                    var legislatura = await _unitOfWork.Legislature.Legislatura_Attiva();
                    result.Legislatura = legislatura;
                    var progressivo =
                        await _unitOfWork.DASI.GetProgressivo((TipoAttoEnum)attoDto.Tipo, persona.Gruppo.id_gruppo,
                            legislatura);
                    result.Progressivo = progressivo;

                    if (persona.IsSegreteriaAssemblea
                        || persona.IsPresidente)
                        result.IDStato = (int)StatiAttoEnum.BOZZA;
                    else
                        result.IDStato = persona.Gruppo.abilita_em_privati
                            ? (int)StatiAttoEnum.BOZZA_RISERVATA
                            : (int)StatiAttoEnum.BOZZA;

                    if (persona.IsConsigliereRegionale ||
                        persona.IsAssessore)
                        result.UIDPersonaProponente = persona.UID_persona;
                    else
                        result.UIDPersonaProponente = attoDto.UIDPersonaProponente;

                    result.UIDPersonaCreazione = persona.UID_persona;
                    result.DataCreazione = DateTime.Now;
                    result.idRuoloCreazione = (int)persona.CurrentRole;
                    if (!persona.IsSegreteriaAssemblea
                        && !persona.IsPresidente)
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
                    attoInDb.DataRichiestaIscrizioneSeduta = EncryptString(seduta.Data_seduta.ToString("dd/MM/yyyy"),
                        AppSettingsConfiguration.masterKey);
                    attoInDb.UIDPersonaRichiestaIscrizione = persona.UID_persona;
                    attoInDb.Non_Passaggio_In_Esame = attoDto.Non_Passaggio_In_Esame;
                }

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
                var requestStato = GetResponseStatusFromFilters(model.filtro);
                var requestTipo = GetResponseTypeFromFilters(model.filtro);

                model.param.TryGetValue("CLIENT_MODE", out var CLIENT_MODE); // per trattazione aula
                var filtro_seduta =
                    model.filtro.FirstOrDefault(item => item.PropertyId == nameof(AttoDASIDto.UIDSeduta));
                var sedutaId = Guid.Empty;
                if (filtro_seduta != null) sedutaId = new Guid(filtro_seduta.Value.ToString());
                var soggetti = new List<int>();
                var soggetti_request = new List<FilterStatement<AttoDASIDto>>();
                if (model.filtro.Any(statement => statement.PropertyId == "SoggettiDestinatari"))
                {
                    soggetti_request =
                        new List<FilterStatement<AttoDASIDto>>(model.filtro.Where(statement =>
                            statement.PropertyId == "SoggettiDestinatari"));
                    soggetti.AddRange(soggetti_request.Select(i => Convert.ToInt32(i.Value)));
                    foreach (var s in soggetti_request) model.filtro.Remove(s);
                }

                var queryFilter = new Filter<ATTI_DASI>();
                queryFilter.ImportStatements(model.filtro);
                var atti_in_db = await _unitOfWork
                    .DASI
                    .GetAll(persona,
                        model.page,
                        model.size,
                        (ClientModeEnum)Convert.ToInt16(CLIENT_MODE),
                        queryFilter,
                        soggetti);

                var stati_request = new List<FilterStatement<AttoDASIDto>>();
                if (model.filtro.Any(statement => statement.PropertyId == nameof(AttoDASIDto.IDStato)))
                {
                    stati_request =
                        new List<FilterStatement<AttoDASIDto>>(model.filtro.Where(statement =>
                            statement.PropertyId == nameof(AttoDASIDto.IDStato)));
                    foreach (var s in stati_request) model.filtro.Remove(s);
                }

                if (!atti_in_db.Any())
                {
                    queryFilter.ImportStatements(model.filtro);
                    var defaultCounterBar =
                        await GetResponseCountBar(persona, requestStato, requestTipo, sedutaId, CLIENT_MODE,
                            queryFilter, soggetti);
                    if (soggetti_request.Any())
                        model.filtro.AddRange(soggetti_request);
                    if (stati_request.Any())
                        model.filtro.AddRange(stati_request);
                    return new RiepilogoDASIModel
                    {
                        Data = new BaseResponse<AttoDASIDto>(
                            model.page
                            , model.size
                            , new List<AttoDASIDto>()
                            , model.filtro
                            , 0
                            , uri),
                        Stato = requestStato,
                        Tipo = requestTipo,
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
                        (ClientModeEnum)Convert.ToInt16(CLIENT_MODE),
                        queryFilter,
                        soggetti);

                queryFilter.ImportStatements(model.filtro);
                var responseModel = new RiepilogoDASIModel
                {
                    Data = new BaseResponse<AttoDASIDto>(
                        model.page
                        , model.size
                        , result
                        , model.filtro
                        , totaleAtti
                        , uri),
                    Stato = requestStato,
                    Tipo = requestTipo,
                    CountBarData = await GetResponseCountBar(persona, requestStato, requestTipo, sedutaId, CLIENT_MODE,
                        queryFilter, soggetti)
                };

                if (persona.IsSegreteriaAssemblea) responseModel.CommissioniAttive = await GetCommissioniAttive();

                if (soggetti_request.Any())
                    model.filtro.AddRange(soggetti_request);
                if (stati_request.Any())
                    model.filtro.AddRange(stati_request);

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
                if (!string.IsNullOrEmpty(attoInDb.DataRichiestaIscrizioneSeduta))
                    dto.DataRichiestaIscrizioneSeduta = Decrypt(attoInDb.DataRichiestaIscrizioneSeduta);

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

                    dto.Firmabile = await _unitOfWork
                        .Atti_Firme
                        .CheckIfFirmabile(dto,
                            persona);

                    if (!dto.DataRitiro.HasValue && dto.IDStato == (int)StatiAttoEnum.PRESENTATO)
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
                dto.Commissioni = commissioni
                    .Select(Mapper.Map<View_Commissioni_attive, CommissioneDto>).ToList();

                if (attoInDb.UIDSeduta.HasValue)
                {
                    var sedutaInDb = await _logicSedute.GetSeduta(attoInDb.UIDSeduta.Value);
                    dto.Seduta = Mapper.Map<SEDUTE, SeduteDto>(sedutaInDb);

                    var presentato_oltre_termini = IsOutdate(dto);
                    dto.PresentatoOltreITermini = presentato_oltre_termini;
                }

                if (attoInDb.Tipo == (int)TipoAttoEnum.MOZ && attoInDb.TipoMOZ == (int)TipoMOZEnum.ABBINATA)
                {
                    var attoAbbinato = await _unitOfWork.DASI.Get(attoInDb.UID_MOZ_Abbinata.Value);
                    dto.MOZ_Abbinata =
                        $"{Utility.GetText_Tipo(attoAbbinato.Tipo)} {GetNome(attoAbbinato.NAtto, attoAbbinato.Progressivo.Value)}";
                }

                if (attoInDb.Tipo == (int)TipoAttoEnum.ODG)
                {
                    var attoPem = await _unitOfWork.Atti.Get(attoInDb.UID_Atto_ODG.Value);
                    dto.ODG_Atto_PEM = $"{Utility.GetText_Tipo(attoPem.IDTipoAtto)} {attoPem.NAtto}";
                }

                return dto;
            }
            catch (Exception e)
            {
                Log.Error($"Logic - Get DTO Atto - DASI - [{attoUid}]", e);
                throw;
            }
        }

        public async Task<AttoDASIDto> GetAttoDto(Guid attoUid)
        {
            try
            {
                var attoInDb = await _unitOfWork.DASI.Get(attoUid);

                var dto = Mapper.Map<ATTI_DASI, AttoDASIDto>(attoInDb);

                dto.NAtto = GetNome(attoInDb.NAtto, attoInDb.Progressivo.Value);

                dto.Firma_da_ufficio = await _unitOfWork.Atti_Firme.CheckFirmatoDaUfficio(attoUid);
                dto.Firmato_Dal_Proponente =
                    await _unitOfWork.Atti_Firme.CheckFirmato(attoUid, attoInDb.UIDPersonaProponente.Value);

                dto.ConteggioFirme = await _logicFirme.CountFirme(attoUid);

                dto.gruppi_politici =
                    Mapper.Map<View_gruppi_politici_con_giunta, GruppiDto>(
                        await _unitOfWork.Gruppi.Get(attoInDb.id_gruppo));

                return dto;
            }
            catch (Exception e)
            {
                Log.Error($"Logic - Get DTO Atto - DASI - [{attoUid}]", e);
                throw;
            }
        }

        private async Task<CountBarData> GetResponseCountBar(PersonaDto persona, StatiAttoEnum stato, TipoAttoEnum tipo,
            Guid sedutaId, object _clientMode, Filter<ATTI_DASI> _filtro, List<int> soggetti)
        {
            var clientMode = (ClientModeEnum)Convert.ToInt16(_clientMode);
            var filtro = PulisciFiltro(_filtro);
            var result = new CountBarData
            {
                ITL = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.ITL
                        , StatiAttoEnum.TUTTI, sedutaId, clientMode, filtro, soggetti),
                ITR = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.ITR
                        , StatiAttoEnum.TUTTI, sedutaId, clientMode, filtro, soggetti),
                IQT = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.IQT
                        , StatiAttoEnum.TUTTI, sedutaId, clientMode, filtro, soggetti),
                MOZ = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.MOZ
                        , StatiAttoEnum.TUTTI, sedutaId, clientMode, filtro, soggetti),
                ODG = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.ODG
                        , StatiAttoEnum.TUTTI, sedutaId, clientMode, filtro, soggetti),
                TUTTI = await _unitOfWork
                    .DASI
                    .Count(persona,
                        TipoAttoEnum.TUTTI
                        , StatiAttoEnum.TUTTI, sedutaId, clientMode, filtro, soggetti),
                BOZZE = await _unitOfWork
                    .DASI
                    .Count(persona,
                        tipo
                        , StatiAttoEnum.BOZZA, sedutaId, clientMode, filtro, soggetti),
                PRESENTATI = await _unitOfWork
                    .DASI
                    .Count(persona,
                        tipo
                        , StatiAttoEnum.PRESENTATO, sedutaId, clientMode, filtro, soggetti),
                IN_TRATTAZIONE = await _unitOfWork
                    .DASI
                    .Count(persona,
                        tipo
                        , StatiAttoEnum.IN_TRATTAZIONE, sedutaId, clientMode, filtro, soggetti),
                CHIUSO = await _unitOfWork
                    .DASI
                    .Count(persona,
                        tipo
                        , StatiAttoEnum.CHIUSO, sedutaId, clientMode, filtro, soggetti)
            };

            return result;
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
                if (!persona.IsConsigliereRegionale)
                {
                    throw new Exception("Ruolo non abilitato alla firma di atti");
                }

                var results = new Dictionary<Guid, string>();
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                var counterFirme = 1;

                foreach (var idGuid in firmaModel.Lista)
                {
                    if (counterFirme == Convert.ToInt32(AppSettingsConfiguration.LimiteFirmaMassivo) + 1) break;

                    var attoInDb = await _unitOfWork.DASI.Get(idGuid);
                    var atto = await GetAttoDto(idGuid, persona, personeInDbLight);
                    var nome_atto = $"{Utility.GetText_Tipo(atto.Tipo)} {atto.NAtto}";

                    if (atto.IDStato >= (int)StatiAttoEnum.IN_TRATTAZIONE)
                    {
                        results.Add(idGuid,
                            $"ERROR: Atto {nome_atto} si trova nello stato: [{Utility.GetText_StatoDASI(atto.IDStato)}] e non è più sottoscrivibile");
                        continue;
                    }

                    var firmaCert = string.Empty;
                    var primoFirmatario = false;

                    if (firmaUfficio)
                    {
                        //Controllo se l'utente ha già firmato
                        if (atto.Firma_da_ufficio)
                        {
                            results.Add(idGuid, $"ERROR: Atto {nome_atto} già firmato dall'ufficio");
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
                            results.Add(idGuid, $"ERROR: Il Proponente non ha ancora firmato l'atto {nome_atto}");
                            continue;
                        }

                        var info_codice_carica_gruppo = persona.Gruppo.codice_gruppo;

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
                        primoFirmatario = true;
                    }

                    var destinatario_notifica =
                        await _unitOfWork.Notifiche_Destinatari.ExistDestinatarioNotifica(idGuid, persona.UID_persona,
                            true);

                    var id_gruppo = persona.Gruppo?.id_gruppo ?? 0;
                    var valida = !(id_gruppo != attoInDb.id_gruppo && destinatario_notifica == null);
                    await _unitOfWork.Atti_Firme.Firma(idGuid, persona.UID_persona, id_gruppo, firmaCert, dataFirma,
                        firmaUfficio, primoFirmatario, valida);

                    if (destinatario_notifica != null)
                    {
                        await _unitOfWork.Notifiche_Destinatari.SetSeen_DestinatarioNotifica(destinatario_notifica,
                            persona.UID_persona);
                    }
                    else if (!valida)
                    {
                        var newNotifica = new NOTIFICHE
                        {
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
            try
            {
                var results = new Dictionary<Guid, string>();
                var jumpMail = false;

                foreach (var idGuid in firmaModel.Lista)
                {
                    var atto = await _unitOfWork.DASI.Get(idGuid);
                    if (atto == null)
                    {
                        results.Add(idGuid, "ERROR: NON TROVATO");
                        continue;
                    }

                    var dto = await GetAttoDto(idGuid);
                    var nome_atto = $"{Utility.GetText_Tipo(dto.Tipo)} {dto.NAtto}";
                    SEDUTE seduta = null;
                    if (atto.UIDSeduta.HasValue)
                    {
                        seduta = await _unitOfWork.Sedute.Get(atto.UIDSeduta.Value);
                        if (DateTime.Now > seduta.Data_seduta)
                            throw new InvalidOperationException(
                                "Non è possibile ritirare la firma durante lo svolgimento della seduta: annuncia in Aula l'intenzione di ritiro");
                    }

                    var richiestaPresente =
                        await _unitOfWork.Notifiche.EsisteRitiroDasi(atto.UIDAtto, persona.UID_persona);
                    if (richiestaPresente)
                        throw new InvalidOperationException(
                            "Richiesta di ritiro già inviata al proponente.");

                    var countFirme = await _unitOfWork.Atti_Firme.CountFirme(idGuid);
                    var result_check = await ControlloFirmePresentazione(dto, countFirme - 1, seduta);
                    if (!string.IsNullOrEmpty(result_check))
                        switch ((TipoAttoEnum)atto.Tipo)
                        {
                            case TipoAttoEnum.IQT:
                            case TipoAttoEnum.MOZ:
                                {
                                    if (atto.TipoMOZ == (int)TipoMOZEnum.URGENTE)
                                    {
                                        atto.TipoMOZ = (int)TipoMOZEnum.ORDINARIA;
                                        break;
                                    }

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
                                        SyncGUID = Guid.NewGuid()
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

                                    throw new InvalidOperationException(
                                        "INFO: Se ritiri la firma l'atto decadrà in quanto non ci sarà più il numero di firme necessario. La richiesta di ritiro firma è stata inviata al proponente dell'atto.");
                                }
                        }

                    if (countFirme == 1)
                    {
                        //RITIRA ATTO
                        atto.IDStato = (int)StatiAttoEnum.CHIUSO;
                        atto.IDStato_Motivazione = (int)MotivazioneStatoAttoEnum.RITIRATO;
                        atto.UIDPersonaRitiro = persona.UID_persona;
                        atto.DataRitiro = DateTime.Now;
                    }

                    //RITIRA FIRMA
                    var firmeAttive = await _unitOfWork
                        .Atti_Firme
                        .GetFirmatari(atto, FirmeTipoEnum.ATTIVI);
                    ATTI_FIRME firma_utente;
                    firma_utente = persona.IsSegreteriaAssemblea
                        ? firmeAttive.Single(f => f.ufficio)
                        : firmeAttive.Single(f => f.UID_persona == persona.UID_persona);

                    firma_utente.Data_ritirofirma =
                        EncryptString(DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            AppSettingsConfiguration.masterKey);

                    await _unitOfWork.CompleteAsync();
                    results.Add(idGuid, $"{dto} - OK");
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
                if (!persona.IsConsigliereRegionale)
                {
                    throw new Exception("Ruolo non abilitato alla presentazione di atti");
                }

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
                    var nome_atto = $"{Utility.GetText_Tipo(attoDto.Tipo)} {attoDto.NAtto}";
                    if (atto.IDStato >= (int)StatiAttoEnum.PRESENTATO) continue;
                    if (atto.Tipo == (int)TipoAttoEnum.IQT
                        && string.IsNullOrEmpty(atto.DataRichiestaIscrizioneSeduta))
                    {
                        results.Add(idGuid,
                            $"ERROR: {nome_atto} non presentabile. Data seduta non indicata: scegli prima la data della seduta a cui iscrivere l’IQT.");
                        continue;
                    }

                    if (atto.Tipo == (int)TipoAttoEnum.ODG)
                    {
                        var atti = await _unitOfWork.DASI.GetAttiBySeduta(atto.UIDSeduta.Value, TipoAttoEnum.ODG,
                            TipoMOZEnum.ORDINARIA);
                        var my_atti = atti.Where(a =>
                                a.UID_Atto_ODG == atto.UID_Atto_ODG && a.UIDPersonaPrimaFirma == persona.UID_persona)
                            .ToList();
                        var attoPEM = await _unitOfWork.Atti.Get(atto.UID_Atto_ODG.Value);
                        var seduta = await _unitOfWork.Sedute.Get(attoPEM.UIDSeduta.Value);
                        if (attoPEM.BloccoODG)
                        {
                            results.Add(idGuid,
                                $"ERROR: {nome_atto} non presentabile. Non puoi presentare altri ordini del giorno.");
                            continue;
                        }

                        if (attoPEM.Jolly)
                            if (my_atti.Count > AppSettingsConfiguration.MassimoODG_Jolly)
                            {
                                results.Add(idGuid,
                                    $"ERROR: {nome_atto} non presentabile. Non puoi presentare altri ordini del giorno per l'atto {Utility.GetText_Tipo(attoPEM.IDTipoAtto)} {attoPEM.NAtto}.");
                                continue;
                            }

                        if (seduta.Data_effettiva_inizio.HasValue)
                        {
                            if (my_atti.Count > AppSettingsConfiguration.MassimoODG +
                                AppSettingsConfiguration.MassimoODG_DuranteSeduta)
                            {
                                results.Add(idGuid,
                                    $"ERROR: {nome_atto} non presentabile. Non puoi presentare altri ordini del giorno per l'atto {Utility.GetText_Tipo(attoPEM.IDTipoAtto)} {attoPEM.NAtto}.");
                                continue;
                            }
                        }
                        else
                        {
                            if (my_atti.Count > AppSettingsConfiguration.MassimoODG)
                            {
                                results.Add(idGuid,
                                    $"ERROR: {nome_atto} non presentabile. Non puoi presentare più di {AppSettingsConfiguration.MassimoODG} ordini del giorno per l'atto {Utility.GetText_Tipo(attoPEM.IDTipoAtto)} {attoPEM.NAtto}.");
                                continue;
                            }
                        }
                    }

                    if (!attoDto.Presentabile)
                    {
                        results.Add(idGuid, $"ERROR: {nome_atto} non presentabile");
                        continue;
                    }

                    //controllo max firme
                    var count_firme = await _unitOfWork.Atti_Firme.CountFirme(idGuid);
                    var controllo_firme = await ControlloFirmePresentazione(attoDto, count_firme);
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
                        EncryptString(etichetta_progressiva, AppSettingsConfiguration.masterKey);
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
                    atto.DataPresentazione = EncryptString(atto.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                        AppSettingsConfiguration.masterKey);
                    atto.IDStato = (int)StatiAttoEnum.PRESENTATO;

                    atto.NAtto = etichetta_encrypt;

                    atto.chkf = count_firme.ToString();

                    await _unitOfWork.CompleteAsync();

                    var new_nome_atto = $"{Utility.GetText_Tipo(attoDto.Tipo)} {attoDto.NAtto}";

                    results.Add(idGuid, $"{new_nome_atto} - OK");

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

                    var ruoloSegreterie =
                        await _unitOfWork.Ruoli.Get((int)RuoliIntEnum.Segreteria_Assemblea);
                    if (atto.Tipo == (int)TipoAttoEnum.ODG && atto.UID_Atto_ODG.HasValue)
                    {
                        //proposta di iscrizione in seduta
                        var attoPem = await _unitOfWork.Atti.Get(atto.UID_Atto_ODG.Value);
                        var seduta = await _unitOfWork.Sedute.Get(attoPem.UIDSeduta.Value);
                        var dataRichiesta = EncryptString(seduta.Data_seduta.ToString("dd/MM/yyyy"),
                            AppSettingsConfiguration.masterKey);
                        atto.DataRichiestaIscrizioneSeduta = dataRichiesta;
                        atto.UIDPersonaRichiestaIscrizione = persona.UID_persona;

                        if (atto.Non_Passaggio_In_Esame)
                            try
                            {
                                var mailModel = new MailModel
                                {
                                    DA = persona.email,
                                    A =
                                        $"{ruoloSegreterie.ADGroup.Replace(@"CONSIGLIO\", string.Empty)}@consiglio.regione.lombardia.it",
                                    OGGETTO =
                                        "[ODG DI NON PASSAGGIO ALL'ESAME]",
                                    MESSAGGIO =
                                        $"Il consigliere {persona.DisplayName_GruppoCode} ha presentato un ODG di non passaggio all'esame per il provvedimento: <br> {Utility.GetText_Tipo(attoPem.IDTipoAtto)} {attoPem.NAtto} - {attoPem.Oggetto}."
                                };
                                await _logicUtil.InvioMail(mailModel);
                            }
                            catch (Exception e)
                            {
                                Log.Error("Logic - ODG DI NON PASSAGGIO ALL'ESAME - Invio Mail", e);
                            }
                    }

                    await _unitOfWork.CompleteAsync();
                    counterPresentazioni++;

                    try
                    {
                        var mailModel = new MailModel
                        {
                            DA = persona.email,
                            A =
                                $"{ruoloSegreterie.ADGroup.Replace(@"CONSIGLIO\", string.Empty)}@consiglio.regione.lombardia.it",
                            OGGETTO = $"[PRESENTATO] {new_nome_atto}",
                            MESSAGGIO =
                                $"Il consigliere {persona.DisplayName_GruppoCode} ha presentato l'atto {new_nome_atto} con oggetto: </br> {atto.Oggetto}."
                        };
                        await _logicUtil.InvioMail(mailModel);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Logic - Presentazione - Invio Mail", e);
                    }
                }

                return results;
            }
            catch (Exception e)
            {
                Log.Error("Logic - Presenta - DASI", e);
                throw e;
            }
        }

        internal async Task<string> ControlloFirmePresentazione(AttoDASIDto atto, int count_firme,
            SEDUTE seduta_attiva = null,
            string error_title = "Atto non presentabile")
        {
            if (atto.Tipo == (int)TipoAttoEnum.IQT
                || atto.Tipo == (int)TipoAttoEnum.MOZ && atto.TipoMOZ == (int)TipoMOZEnum.URGENTE
                || atto.Tipo == (int)TipoAttoEnum.MOZ && atto.TipoMOZ == (int)TipoMOZEnum.SFIDUCIA
                || atto.Tipo == (int)TipoAttoEnum.MOZ && atto.TipoMOZ == (int)TipoMOZEnum.CENSURA)
            {
                var minimo_consiglieri = AppSettingsConfiguration.MinimoConsiglieriIQT;
                if (atto.Tipo == (int)TipoAttoEnum.MOZ)
                {
                    if (atto.TipoMOZ == (int)TipoMOZEnum.URGENTE)
                        minimo_consiglieri = AppSettingsConfiguration.MinimoConsiglieriMOZU;
                    if (atto.TipoMOZ == (int)TipoMOZEnum.SFIDUCIA
                        || atto.TipoMOZ == (int)TipoMOZEnum.CENSURA)
                        minimo_consiglieri = AppSettingsConfiguration.MinimoConsiglieriMOZC_MOZS;
                }

                var consiglieriGruppo =
                    await _unitOfWork.Gruppi.GetConsiglieriGruppo(atto.Legislatura, atto.id_gruppo);
                var count_consiglieri = consiglieriGruppo.Count();
                var minimo_firme = count_consiglieri < minimo_consiglieri
                                   && atto.TipoMOZ != (int)TipoMOZEnum.SFIDUCIA
                                   && atto.TipoMOZ != (int)TipoMOZEnum.CENSURA
                    ? count_consiglieri
                    : minimo_consiglieri;

                if (count_firme < minimo_firme)
                    return
                        $"{error_title}. Firme {count_firme}/{minimo_firme}. Mancano {minimo_firme - count_firme} firme.";

                if (seduta_attiva == null)
                    return default;

                var firmatari = await _unitOfWork.Atti_Firme.GetFirmatari(atto.UIDAtto);
                var anomalie = new StringBuilder();
                if (atto.Tipo == (int)TipoAttoEnum.MOZ && atto.TipoMOZ == (int)TipoMOZEnum.URGENTE)
                {
                    var moz_in_seduta = await _unitOfWork.DASI.GetAttiBySeduta(seduta_attiva.UIDSeduta,
                        TipoAttoEnum.MOZ, TipoMOZEnum.URGENTE);
                    foreach (var firma in firmatari)
                    {
                        var firmatario_indagato =
                            $"{Decrypt(firma.FirmaCert)}, firma non valida perchè già presente in [[LISTA]]; ";
                        var firmatario_valido = true;
                        foreach (var moz in moz_in_seduta)
                        {
                            var firmatari_moz = await _unitOfWork.Atti_Firme.GetFirmatari(moz.UIDAtto);
                            if (firmatari_moz.All(item => item.FirmaCert != firma.FirmaCert)) continue;
                            firmatario_valido = false;
                            var mozDto = await GetAttoDto(moz.UIDAtto);
                            firmatario_indagato = firmatario_indagato.Replace("[[LISTA]]",
                                $"{Utility.GetText_Tipo(atto.Tipo)} {mozDto.NAtto}");
                            break;
                        }

                        if (firmatario_valido) continue;
                        count_firme--;
                        anomalie.AppendLine(firmatario_indagato);
                    }
                }
                else if (atto.Tipo == (int)TipoAttoEnum.IQT && count_consiglieri < minimo_consiglieri)
                {
                    var firmatari_di_altri_gruppi = firmatari.Any(i => i.id_gruppo != atto.id_gruppo);
                    if (firmatari_di_altri_gruppi) minimo_firme = minimo_consiglieri;
                }


                if (anomalie.Length > 0)
                    return
                        $"{error_title}. Firme {count_firme}/{minimo_firme}. Mancano {minimo_firme - count_firme} firme. Riscontrate le seguenti anomalie: {anomalie}";

                if (count_firme < minimo_firme)
                    return
                        $"{error_title}. Firme {count_firme}/{minimo_firme}. Mancano {minimo_firme - count_firme} firme.";
            }

            return default;
        }

        internal async Task<string> ControlloFirmePresentazione(AttoDASIDto atto, SEDUTE seduta_attiva = null)
        {
            var count_firme = await _unitOfWork.Atti_Firme.CountFirme(atto.UIDAtto);
            return await ControlloFirmePresentazione(atto, count_firme, seduta_attiva);
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
                    var tipo = Utility.GetText_Tipo(dto);

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
                            GetBodyTemporaneo(dto, ref body);
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
                atto.IDStato = (int)StatiAttoEnum.CHIUSO;
                atto.IDStato_Motivazione = (int)MotivazioneStatoAttoEnum.RITIRATO;
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
                result.Atto.Commissioni = new List<CommissioneDto>();

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
                    Atto = dto,
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

        public async Task<List<AttoDASIDto>> GetMOZAbbinabili()
        {
            try
            {
                var sedute_attive = await _unitOfWork.Sedute.GetAttive();

                var result = new List<AttoDASIDto>();
                foreach (var seduta in sedute_attive)
                {
                    var atti = await _unitOfWork.DASI.GetMOZAbbinabili(seduta.UIDSeduta);
                    foreach (var atto in atti) result.Add(await GetAttoDto(atto.UIDAtto));
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetMOZAbbinabili - DASI", e);
                throw;
            }
        }

        public async Task<List<AttiDto>> GetAttiSeduteAttive(PersonaDto persona, List<PersonaLightDto> personeInDbLight)
        {
            try
            {
                var sedute_attive = await _unitOfWork.Sedute.GetAttive();

                var result = new List<AttiDto>();
                foreach (var seduta in sedute_attive)
                {
                    var atti = await _logicAtti
                        .GetAtti(
                            new BaseRequest<AttiDto> { id = seduta.UIDSeduta, page = 1, size = 99 },
                            (int)ClientModeEnum.TRATTAZIONE,
                            persona,
                            personeInDbLight);

                    foreach (var atto in atti.Results)
                    {
                        var tipo = Utility.GetText_Tipo(atto.IDTipoAtto);
                        var titolo_atto = $"{tipo} {atto.NAtto}";
                        if (atto.IDTipoAtto == (int)TipoAttoEnum.ALTRO) titolo_atto = atto.Oggetto;

                        atto.NAtto = $"{titolo_atto} - Seduta del {seduta.Data_seduta:dd/MM/yyyy HH:mm}";
                        result.Add(atto);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetAttiSeduteAttive - DASI", e);
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
            try
            {
                var listaRichieste = new Dictionary<Guid, string>();
                var dataSeduta = "";

                foreach (var guid in model.Lista)
                {
                    var atto = await Get(guid);
                    if (atto == null) throw new Exception("ERROR: NON TROVATO");
                    atto.UIDSeduta = model.UidSeduta;
                    atto.DataIscrizioneSeduta = DateTime.Now;
                    atto.UIDPersonaIscrizioneSeduta = persona.UID_persona;
                    await _unitOfWork.CompleteAsync();
                    var nomeAtto =
                        $"{Utility.GetText_Tipo(atto.Tipo)} {GetNome(atto.NAtto, atto.Progressivo.Value)}";
                    if (!listaRichieste.Any(item => item.Value == nomeAtto
                                                    && item.Key == atto.UIDPersonaRichiestaIscrizione.Value
                                                    || item.Key == atto.UIDPersonaPresentazione.Value))
                        listaRichieste.Add(
                            atto.UIDPersonaRichiestaIscrizione.HasValue
                                ? atto.UIDPersonaRichiestaIscrizione.Value
                                : atto.UIDPersonaPresentazione.Value, nomeAtto);
                }

                try
                {
                    var seduta = await _unitOfWork.Sedute.Get(model.UidSeduta);
                    var ruoloSegreterie = await _unitOfWork.Ruoli.Get((int)RuoliIntEnum.Segreteria_Assemblea);

                    var gruppiMail = listaRichieste.GroupBy(item => item.Key);
                    foreach (var gruppo in gruppiMail)
                    {
                        var personaMail = await _unitOfWork.Persone.Get(gruppo.Key);
                        var mailModel = new MailModel
                        {
                            DA =
                                $"{ruoloSegreterie.ADGroup.Replace(@"CONSIGLIO\", string.Empty)}@consiglio.regione.lombardia.it",
                            A = personaMail.email,
                            OGGETTO =
                                "[ISCRIZIONE ATTI]",
                            MESSAGGIO =
                                $"La segreteria ha iscritto i seguenti atti alla seduta del {seduta.Data_seduta:dd/MM/yyyy}: <br> {gruppo.Select(item => item.Value).Aggregate((i, j) => i + "<br>" + j)}."
                        };
                        await _logicUtil.InvioMail(mailModel);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Logic - IscrizioneSeduta - Invio Mail", e);
                }
            }
            catch (Exception e)
            {
                Log.Error("Logic - IscrizioneSeduta", e);
                throw e;
            }
        }

        public async Task RichiediIscrizione(RichiestaIscrizioneDASIModel model, PersonaDto persona)
        {
            try
            {
                var dataRichiesta = EncryptString(model.DataRichiesta.ToString("dd/MM/yyyy"),
                    AppSettingsConfiguration.masterKey);
                var listaRichieste = new List<string>();
                foreach (var guid in model.Lista)
                {
                    var atto = await Get(guid);
                    if (atto == null) throw new Exception("ERROR: NON TROVATO");
                    atto.DataRichiestaIscrizioneSeduta = dataRichiesta;
                    atto.UIDPersonaRichiestaIscrizione = persona.UID_persona;
                    await _unitOfWork.CompleteAsync();

                    var nomeAtto =
                        $"{Utility.GetText_Tipo(atto.Tipo)} {GetNome(atto.NAtto, atto.Progressivo.Value)}";
                    listaRichieste.Add(nomeAtto);
                }

                try
                {
                    var ruoloSegreterie = await _unitOfWork.Ruoli.Get((int)RuoliIntEnum.Segreteria_Assemblea);

                    var mailModel = new MailModel
                    {
                        DA = persona.email,
                        A =
                            $"{ruoloSegreterie.ADGroup.Replace(@"CONSIGLIO\", string.Empty)}@consiglio.regione.lombardia.it",
                        OGGETTO =
                            "[RICHIESTA ISCRIZIONE]",
                        MESSAGGIO =
                            $"Il consigliere {persona.DisplayName_GruppoCode} ha richiesto l'iscrizione dei seguenti atti: <br> {listaRichieste.Aggregate((i, j) => i + "<br>" + j)} <br> per la seduta del {model.DataRichiesta:dd/MM/yyyy}."
                    };
                    await _logicUtil.InvioMail(mailModel);
                }
                catch (Exception e)
                {
                    Log.Error("Logic - RichiediIscrizione - Invio Mail", e);
                }
            }
            catch (Exception e)
            {
                Log.Error("Logic - RichiediIscrizione", e);
                throw e;
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

        public async Task RimuoviRichiesta(RichiestaIscrizioneDASIModel model)
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

                    if (atto.Tipo == (int)TipoAttoEnum.MOZ) atto.TipoMOZ = (int)TipoMOZEnum.ORDINARIA;

                    atto.DataRichiestaIscrizioneSeduta = null;
                    atto.UIDPersonaRichiestaIscrizione = null;
                    await _unitOfWork.CompleteAsync();
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

                attoInDb.TipoMOZ = (int)TipoMOZEnum.URGENTE;
                atto.TipoMOZ = (int)TipoMOZEnum.URGENTE;

                var sedute_attive = await _unitOfWork.Sedute.GetAttive();
                var seduta_attiva = sedute_attive.OrderBy(s => s.Data_seduta).First();

                attoInDb.DataRichiestaIscrizioneSeduta = EncryptString(seduta_attiva.Data_seduta.ToString("dd/MM/yyyy"),
                    AppSettingsConfiguration.masterKey);
                attoInDb.UIDPersonaRichiestaIscrizione = persona.UID_persona;

                var count_firme = atto.ConteggioFirme;
                var controllo_firme =
                    await ControlloFirmePresentazione(atto, count_firme, seduta_attiva,
                        "Non è possibile proporre l'urgenza");
                if (!string.IsNullOrEmpty(controllo_firme)) throw new InvalidOperationException(controllo_firme);

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - ProponiMozioneUrgente", e);
                throw e;
            }
        }

        public async Task ProponiMozioneAbbinata(PromuoviMozioneModel model)
        {
            try
            {
                var guid = model.Lista.First();
                var atto = await Get(guid);
                if (atto == null) throw new InvalidOperationException("ERROR: NON TROVATO");
                if (atto.Tipo != (int)TipoAttoEnum.MOZ)
                    throw new InvalidOperationException("ERROR: Operazione abilitata solo per le mozioni");

                atto.TipoMOZ = (int)TipoMOZEnum.ABBINATA;
                atto.UID_MOZ_Abbinata = model.AttoUId;

                await _unitOfWork.CompleteAsync();
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
            var filtroTipo = new FilterStatement<AttoDASIDto>
            {
                PropertyId = nameof(AttoDASIDto.Tipo),
                Operation = Operation.EqualTo,
                Value = (int)tipo
            };
            filtro.Add(filtroTipo);

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
                        , filtro);
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
                    var firmatari = await _logicFirme
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

                var templateItemIndice = GetTemplate(TemplateTypeEnum.INDICE_DASI);

                var bodyIndice = new StringBuilder();
                foreach (var dasiDto in atti)
                    bodyIndice.Append(templateItemIndice
                        .Replace("{TipoAtto}", Utility.GetText_Tipo(dasiDto.Tipo))
                        .Replace("{NAtto}", dasiDto.NAtto)
                        .Replace("{DataPresentazione}", dasiDto.DataPresentazione)
                        .Replace("{Oggetto}", dasiDto.Oggetto)
                        .Replace("{Firmatari}",
                            $"{dasiDto.PersonaProponente.DisplayName}{(!string.IsNullOrEmpty(dasiDto.Firme) ? ", " + dasiDto.Firme.Replace("<br>", ", ") : "")}")
                        .Replace("{Stato}", Utility.GetText_StatoDASI(dasiDto.IDStato)));

                body = body.Replace("{LISTA_LIGHT}", bodyIndice.ToString());

                return body;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetCopertina", e);
                throw e;
            }
        }

        public IEnumerable<StatiDto> GetStati(PersonaDto persona)
        {
            var result = new List<StatiDto>();
            var stati = Enum.GetValues(typeof(StatiAttoEnum));
            foreach (var stato in stati)
            {
                if (persona.IsSegreteriaAssemblea)
                    if (Utility.statiNonVisibili_Segreteria.Contains(Convert.ToInt16(stato)))
                        continue;

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
            try
            {
                atto.UIDPersonaModifica = persona.UID_persona;
                atto.DataModifica = DateTime.Now;
                if (!string.IsNullOrEmpty(model.Oggetto_Modificato))
                    atto.Oggetto_Modificato = model.Oggetto_Modificato;
                else if (string.IsNullOrEmpty(model.Oggetto_Modificato) &&
                         !string.IsNullOrEmpty(atto.Oggetto_Modificato))
                    //caso in cui l'utente voglia tornare allo stato precedente
                    atto.Oggetto_Modificato = string.Empty;
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

                atto.IDTipo_Risposta = model.IDTipo_Risposta;
                await _unitOfWork.DASI.RimuoviCommissioni(atto.UIDAtto);
                foreach (var commissioneDto in model.Commissioni)
                    _unitOfWork.DASI.AggiungiCommissione(atto.UIDAtto, commissioneDto.id_organo);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - ModificaMetaDati", e);
                throw e;
            }
        }

        private bool IsOutdate(AttoDASIDto atto)
        {
            if (atto.IDStato < (int)StatiAttoEnum.PRESENTATO) return false;
            var data_presentazione = Convert.ToDateTime(atto.DataPresentazione);
            var result = false;
            switch ((TipoAttoEnum)atto.Tipo)
            {
                case TipoAttoEnum.IQT:
                    {
                        if (data_presentazione > atto.Seduta.DataScadenzaPresentazioneIQT) result = true;

                        break;
                    }
                case TipoAttoEnum.MOZ:
                    {
                        switch ((TipoMOZEnum)atto.TipoMOZ)
                        {
                            case TipoMOZEnum.URGENTE:
                                {
                                    if (data_presentazione > atto.Seduta.DataScadenzaPresentazioneMOZU) result = true;

                                    break;
                                }
                            case TipoMOZEnum.ABBINATA:
                                {
                                    if (data_presentazione > atto.Seduta.DataScadenzaPresentazioneMOZA) result = true;

                                    break;
                                }
                        }

                        break;
                    }
                case TipoAttoEnum.ODG:
                    {
                        if (data_presentazione > atto.Seduta.DataScadenzaPresentazioneODG) result = true;

                        break;
                    }
            }

            return result;
        }

        public async Task PresentazioneCartacea(PresentazioneCartaceaModel model)
        {
            try
            {
                var contatore = await _unitOfWork.DASI.GetContatore(model.Tipo, model.TipoRisposta);
                _unitOfWork.DASI.IncrementaContatore(contatore, model.Salto);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - PresentazioneCartacea", e);
                throw e;
            }
        }
    }
}