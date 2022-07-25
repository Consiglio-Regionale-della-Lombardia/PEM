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
using ExpressionBuilder.Generics;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PortaleRegione.BAL
{
    public class StampeLogic : BaseLogic
    {
        private readonly IUnitOfWork _unitOfWork;

        public StampeLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<STAMPE> GetStampa(Guid id)
        {
            try
            {
                var result = await _unitOfWork.Stampe.Get(id);
                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetStampa", e);
                throw e;
            }
        }

        public async Task InserisciStampa(BaseRequest<EmendamentiDto, StampaDto> model, PersonaDto persona)
        {
            try
            {
                var stampa = Mapper.Map<StampaDto, STAMPE>(model.entity);

                var queryFilter = new Filter<EM>();

                var firmatari = new List<Guid>();
                var firmatari_request = new List<FilterStatement<EmendamentiDto>>();
                if (model.filtro.Any(statement => statement.PropertyId == "Firmatario"))
                {
                    firmatari_request =
                        new List<FilterStatement<EmendamentiDto>>(model.filtro.Where(statement =>
                            statement.PropertyId == "Firmatario"));
                    firmatari.AddRange(firmatari_request.Select(firmatario => new Guid(firmatario.Value.ToString())));
                    foreach (var firmatarioStatement in firmatari_request) model.filtro.Remove(firmatarioStatement);
                }

                var proponenti = new List<Guid>();
                var proponenti_request = new List<FilterStatement<EmendamentiDto>>();
                if (model.filtro.Any(statement => statement.PropertyId == nameof(EmendamentiDto.UIDPersonaProponente)))
                {
                    proponenti_request =
                        new List<FilterStatement<EmendamentiDto>>(model.filtro.Where(statement =>
                            statement.PropertyId == nameof(EmendamentiDto.UIDPersonaProponente)));
                    proponenti.AddRange(proponenti_request.Select(proponente => new Guid(proponente.Value.ToString())));
                    foreach (var proponenteStatement in proponenti_request) model.filtro.Remove(proponenteStatement);
                }

                var gruppi = new List<int>();
                var gruppi_request = new List<FilterStatement<EmendamentiDto>>();
                if (model.filtro.Any(statement => statement.PropertyId == nameof(EmendamentiDto.id_gruppo)))
                {
                    gruppi_request =
                        new List<FilterStatement<EmendamentiDto>>(model.filtro.Where(statement =>
                            statement.PropertyId == nameof(EmendamentiDto.id_gruppo)));
                    gruppi.AddRange(gruppi_request.Select(proponente => Convert.ToInt32(proponente.Value.ToString())));
                    foreach (var gruppiStatement in gruppi_request) model.filtro.Remove(gruppiStatement);
                }

                queryFilter.ImportStatements(model.filtro);

                var queryEM =
                    await _unitOfWork.Emendamenti.GetAll_Query(queryFilter, model.ordine, firmatari, proponenti, gruppi);
                stampa.QueryEM = queryEM;

                stampa.DataRichiesta = DateTime.Now;
                stampa.CurrentRole = (int)persona.CurrentRole;
                stampa.UIDStampa = Guid.NewGuid();
                stampa.UIDUtenteRichiesta = persona.UID_persona;
                stampa.Lock = false;
                stampa.Tentativi = 0;
                if (stampa.A == 0 && stampa.Da == 0 && !stampa.UIDEM.HasValue)
                {
                    stampa.Scadenza = null;
                }
                else
                {
                    stampa.Scadenza =
                        DateTime.Now.AddDays(Convert.ToDouble(AppSettingsConfiguration.GiorniValiditaLink));
                }

                _unitOfWork.Stampe.Add(stampa);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - InserisciStampa", e);
                throw;
            }
        }

        public async Task LockStampa(IEnumerable<StampaDto> listaStampe)
        {
            try
            {
                foreach (var stampa in listaStampe)
                {
                    var stampaInDb = await GetStampa(stampa.UIDStampa);
                    stampaInDb.Lock = true;
                    stampaInDb.DataLock = DateTime.Now;
                    stampaInDb.DataInizioEsecuzione = DateTime.Now;
                    stampaInDb.Tentativi += 1;

                    await _unitOfWork.CompleteAsync();
                }
            }
            catch (Exception e)
            {
                Log.Error("Logic - LockStampa", e);
                throw;
            }
        }

        public async Task UnLockStampa(Guid stampaUId)
        {
            try
            {
                var stampa = await _unitOfWork.Stampe.Get(stampaUId);
                stampa.Lock = false;

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - UnLockStampa", e);
                throw;
            }
        }

        public async Task ResetStampa(STAMPE stampa)
        {
            try
            {
                stampa.Lock = false;
                stampa.Tentativi = 0;
                stampa.DataLock = null;
                stampa.DataInizioEsecuzione = null;
                stampa.MessaggioErrore = string.Empty;

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - ResetStampa", e);
                throw e;
            }
        }

        public async Task ErroreStampa(StampaRequest model)
        {
            try
            {
                var stampa = await _unitOfWork.Stampe.Get(model.stampaUId);
                stampa.DataFineEsecuzione = DateTime.Now;
                stampa.MessaggioErrore = model.messaggio;

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - ErroreStampa", e);
                throw;
            }
        }

        public async Task UpdateFileStampa(StampaDto stampa)
        {
            try
            {
                var stampaInDb = await _unitOfWork.Stampe.Get(stampa.UIDStampa);
                stampaInDb.DataFineEsecuzione = DateTime.Now;
                stampaInDb.PathFile = stampa.PathFile;
                stampaInDb.MessaggioErrore = string.Empty;

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - UpdateFileStampa", e);
                throw;
            }
        }

        public async Task SetInvioStampa(StampaDto stampa)
        {
            try
            {
                var stampaInDb = await _unitOfWork.Stampe.Get(stampa.UIDStampa);
                stampaInDb.Invio = true;
                stampaInDb.DataInvio = DateTime.Now;

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - SetInvioStampa", e);
                throw;
            }
        }

        public async Task<HttpResponseMessage> DownloadStampa(STAMPE stampa)
        {
            try
            {
                var _pathTemp = string.Empty;
                _pathTemp = stampa.NotificaDepositoEM
                    ? AppSettingsConfiguration.RootRepository
                    : AppSettingsConfiguration.CartellaLavoroStampe;

                var result = await ComposeFileResponse(Path.Combine(_pathTemp, stampa.PathFile));

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - DownloadStampa", e);
                throw e;
            }
        }

        public async Task<BaseResponse<StampaDto>> GetStampe(BaseRequest<StampaDto> model, PersonaDto persona, Uri url)
        {
            try
            {
                var queryFilter = new Filter<STAMPE>();
                queryFilter.ImportStatements(model.filtro);

                var stampe = await _unitOfWork.Stampe.GetAll(persona, model.page, model.size, queryFilter);
                var result = new List<StampaDto>();
                if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                    || persona.CurrentRole == RuoliIntEnum.Amministratore_Giunta
                    || persona.CurrentRole == RuoliIntEnum.Segreteria_Assemblea)
                {
                    foreach (var stampa in stampe)
                    {
                        var stampaDto = Mapper.Map<STAMPE, StampaDto>(stampa);
                        var infos = await GetLastInfo(stampa);
                        stampaDto.Info = infos?.Message;
                        stampaDto.Richiedente =
                            Mapper.Map<View_UTENTI, PersonaLightDto>(
                                 await _unitOfWork.Persone.Get(stampa.UIDUtenteRichiesta));
                        result.Add(stampaDto);
                    }
                }
                else
                {
                    foreach (var stampa in stampe)
                    {
                        var stampaDto = Mapper.Map<STAMPE, StampaDto>(stampa);
                        var infos = await GetLastInfo(stampa);
                        stampaDto.Info = infos?.Message;
                        result.Add(stampaDto);
                    }
                }

                return new BaseResponse<StampaDto>(
                    model.page,
                    model.size,
                    result,
                    model.filtro,
                    await _unitOfWork.Stampe.Count(persona, queryFilter),
                    url);
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetStampe", e);
                throw e;
            }
        }

        public async Task<BaseResponse<StampaDto>> GetStampe(BaseRequest<StampaDto> model, Uri url)
        {
            try
            {
                Log.Debug($"Logic - GetStampe - page[{model.page}], pageSize[{model.size}]");

                var result = (await _unitOfWork.Stampe.GetAll(model.page, model.size))
                    .Select(Mapper.Map<STAMPE, StampaDto>);

                await LockStampa(result);

                return new BaseResponse<StampaDto>(
                    model.page,
                    model.size,
                    result,
                    model.filtro,
                    await _unitOfWork.Stampe.Count(),
                    url);
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetStampe", e);
                throw e;
            }
        }

        public async Task AddInfo(STAMPE stampa, string messaggio)
        {
            _unitOfWork.Stampe.AddInfo(stampa.UIDStampa, messaggio);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<Stampa_InfoDto>> GetInfo(STAMPE stampa)
        {
            var result = await _unitOfWork.Stampe.GetInfo(stampa.UIDStampa);
            return result.Select(Mapper.Map<STAMPE_INFO, Stampa_InfoDto>);
        }

        public async Task<Stampa_InfoDto> GetLastInfo(STAMPE stampa)
        {
            var result = await _unitOfWork.Stampe.GetLastInfo(stampa.UIDStampa);
            return Mapper.Map<STAMPE_INFO, Stampa_InfoDto>(result);
        }

        public async Task<IEnumerable<Stampa_InfoDto>> GetInfo()
        {
            var result = await _unitOfWork.Stampe.GetInfo();
            return result.Select(Mapper.Map<STAMPE_INFO, Stampa_InfoDto>);
        }

        public async Task EliminaStampa(STAMPE stampa)
        {
            _unitOfWork.Stampe.Remove(stampa);
            await _unitOfWork.CompleteAsync();
        }
    }
}