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
using PortaleRegione.API.Controllers;
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
        private readonly Worker _worker;

        public StampeLogic(IUnitOfWork unitOfWork, DASILogic logicDasi, Worker worker)
        {
            _worker = worker;
            _unitOfWork = unitOfWork;
            _logicDasi = logicDasi;
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

        public async Task<StampaDto> InserisciStampa(BaseRequest<EmendamentiDto, StampaDto> model, PersonaDto persona)
        {
            var stampa = model.entity;

            var queryFilter = new Filter<EM>();
            var firmatari = new List<Guid>();
            var firmatari_request = new List<FilterStatement<EmendamentiDto>>();

            var gruppi = new List<int>();
            var gruppi_request = new List<FilterStatement<EmendamentiDto>>();

            var stati = new List<int>();
            var stati_request = new List<FilterStatement<EmendamentiDto>>();

            var proponenti = new List<Guid>();
            var proponenti_request = new List<FilterStatement<EmendamentiDto>>();
            if (model.filtro != null)
            {
                if (model.filtro.Any(statement => statement.PropertyId == "Firmatario"))
                {
                    firmatari_request =
                        new List<FilterStatement<EmendamentiDto>>(model.filtro.Where(statement =>
                            statement.PropertyId == "Firmatario"));
                    firmatari.AddRange(
                        firmatari_request.Select(firmatario => new Guid(firmatario.Value.ToString())));
                    foreach (var firmatarioStatement in firmatari_request) model.filtro.Remove(firmatarioStatement);
                }

                if (model.filtro.Any(statement =>
                        statement.PropertyId == nameof(EmendamentiDto.UIDPersonaProponente)))
                {
                    proponenti_request =
                        new List<FilterStatement<EmendamentiDto>>(model.filtro.Where(statement =>
                            statement.PropertyId == nameof(EmendamentiDto.UIDPersonaProponente)));
                    proponenti.AddRange(proponenti_request.Select(proponente =>
                        new Guid(proponente.Value.ToString())));
                    foreach (var proponenteStatement in proponenti_request)
                        model.filtro.Remove(proponenteStatement);
                }

                if (model.filtro.Any(statement => statement.PropertyId == nameof(EmendamentiDto.id_gruppo)))
                {
                    gruppi_request =
                        new List<FilterStatement<EmendamentiDto>>(model.filtro.Where(statement =>
                            statement.PropertyId == nameof(EmendamentiDto.id_gruppo)));
                    gruppi.AddRange(gruppi_request.Select(gruppo => Convert.ToInt32(gruppo.Value.ToString())));
                    foreach (var gruppiStatement in gruppi_request) model.filtro.Remove(gruppiStatement);
                }


                if (model.filtro.Any(statement => statement.PropertyId == nameof(EmendamentiDto.IDStato)))
                {
                    stati_request =
                        new List<FilterStatement<EmendamentiDto>>(model.filtro.Where(statement =>
                            statement.PropertyId == nameof(EmendamentiDto.IDStato)));
                    stati.AddRange(stati_request.Select(stato => Convert.ToInt32(stato.Value.ToString())));
                    foreach (var statiStatement in stati_request) model.filtro.Remove(statiStatement);
                }
            }

            queryFilter.ImportStatements(model.filtro);

            model.param.TryGetValue("CLIENT_MODE", out var CLIENT_MODE); // per trattazione aula

            var queryEM =
                await _unitOfWork.Emendamenti.GetAll_Query(persona, Convert.ToInt16(CLIENT_MODE), queryFilter, model.ordine, firmatari, proponenti, gruppi, stati);
            stampa.Query = queryEM;

            stampa.DataRichiesta = DateTime.Now;
            stampa.CurrentRole = (int)persona.CurrentRole;
            stampa.UIDStampa = Guid.NewGuid();
            stampa.UIDUtenteRichiesta = persona.UID_persona;
            stampa.Lock = false;
            stampa.Tentativi = 0;
            if (stampa.A == 0 && stampa.Da == 0 && !stampa.UIDEM.HasValue)
                stampa.Scadenza = null;
            else
                stampa.Scadenza =
                    DateTime.Now.AddDays(Convert.ToDouble(AppSettingsConfiguration.GiorniValiditaLink));

            var stampa_In_Db = stampa;
            _unitOfWork.Stampe.Add(stampa_In_Db);

            await _unitOfWork.CompleteAsync();

            return stampa;
        }

        public async Task<StampaDto> InserisciStampa(BaseRequest<AttoDASIDto, StampaDto> model, PersonaDto persona)
        {
            model.param.TryGetValue("CLIENT_MODE", out var CLIENT_MODE); // per trattazione aula
            var mode = (ClientModeEnum)Convert.ToInt16(CLIENT_MODE);
            var stampa = model.entity;
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
            var query = await _unitOfWork.DASI.GetAll_Query(persona, mode, queryFilter, soggetti);
            stampa.Query = query;

            stampa.DataRichiesta = DateTime.Now;
            stampa.CurrentRole = (int)persona.CurrentRole;
            stampa.UIDStampa = Guid.NewGuid();
            stampa.UIDUtenteRichiesta = persona.UID_persona;
            stampa.Lock = false;
            stampa.Tentativi = 0;
            if (stampa.A == 0 && stampa.Da == 0 && !stampa.UIDAtto.HasValue)
            {
                stampa.Scadenza = null;
            }
            else
            {
                stampa.Scadenza =
                    DateTime.Now.AddDays(Convert.ToDouble(AppSettingsConfiguration.GiorniValiditaLink));
            }

            stampa.DASI = true;
            var stampa_In_Db = stampa;
            _unitOfWork.Stampe.Add(stampa_In_Db);

            await _unitOfWork.CompleteAsync();

            return stampa;
        }

        public async Task LockStampa(IEnumerable<StampaDto> listaStampe)
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

        public async Task UnLockStampa(Guid stampaUId)
        {
            var stampa = await _unitOfWork.Stampe.Get(stampaUId);
            stampa.Lock = false;

            await _unitOfWork.CompleteAsync();
        }

        public async Task ResetStampa(STAMPE stampa)
        {
            stampa.Lock = false;
            stampa.Tentativi = 0;
            stampa.DataLock = null;
            stampa.DataInizioEsecuzione = null;
            stampa.MessaggioErrore = string.Empty;

            await _unitOfWork.CompleteAsync();
        }

        public async Task ErroreStampa(StampaRequest model)
        {
            var stampa = await _unitOfWork.Stampe.Get(model.stampaUId);
            stampa.DataFineEsecuzione = DateTime.Now;
            stampa.MessaggioErrore = model.messaggio;

            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateFileStampa(StampaDto stampa)
        {
            var stampaInDb = await _unitOfWork.Stampe.Get(stampa.UIDStampa);
            stampaInDb.DataFineEsecuzione = DateTime.Now;
            stampaInDb.PathFile = stampa.PathFile;
            stampaInDb.MessaggioErrore = string.Empty;

            await _unitOfWork.CompleteAsync();
        }

        public async Task SetInvioStampa(StampaDto stampa)
        {
            var stampaInDb = await _unitOfWork.Stampe.Get(stampa.UIDStampa);
            stampaInDb.Invio = true;
            stampaInDb.DataInvio = DateTime.Now;

            await _unitOfWork.CompleteAsync();
        }

        public async Task<HttpResponseMessage> DownloadStampa(STAMPE stampa)
        {
            var _pathTemp = stampa.Notifica
                ? AppSettingsConfiguration.RootRepository
                : AppSettingsConfiguration.CartellaLavoroStampe;

            var result = await ComposeFileResponse(Path.Combine(_pathTemp, stampa.PathFile));

            return result;
        }

        public async Task<BaseResponse<StampaDto>> GetStampe(BaseRequest<StampaDto> model, PersonaDto persona, Uri url)
        {
            var queryFilter = new Filter<STAMPE>();
            queryFilter.ImportStatements(model.filtro);

            var stampe = await _unitOfWork.Stampe.GetAll(persona, model.page, model.size, queryFilter);
            var result = new List<StampaDto>();
            if (persona.IsSegreteriaAssemblea
                || persona.IsAmministratoreGiunta)
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

        public async Task<BaseResponse<StampaDto>> GetStampe(BaseRequest<StampaDto> model, Uri url)
        {
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

        public async Task<HttpResponseMessage> Print(string uidStampa)
        {
            var stampa = await _unitOfWork.Stampe.Get(new Guid(uidStampa));

            var thread = new ThreadWorkerModel
            {
                PDF_LICENSE = AppSettingsConfiguration.PDF_LICENSE,
                CartellaLavoroStampe = AppSettingsConfiguration.CartellaLavoroStampe,
                CartellaLavoroTemporanea = AppSettingsConfiguration.CartellaTemp,
                EmailFrom = AppSettingsConfiguration.EmailFrom,
                NumMaxTentativi = "3",
                PercorsoCompatibilitaDocumenti = AppSettingsConfiguration.PercorsoCompatibilitaDocumenti,
                RootRepository = AppSettingsConfiguration.RootRepository,
                UrlAPI = AppSettingsConfiguration.URL_API,
                UrlCLIENT = AppSettingsConfiguration.urlPEM,
                Username = AppSettingsConfiguration.Service_Username,
                Password = AppSettingsConfiguration.Service_Password
            };

            var content = await _worker.ExecuteAsync(stampa.ToDto(), thread);
            var res = ComposeFileResponse(content,
                $"fascicolo_{DateTime.Now.Ticks}.pdf");
            return res;
        }
    }
}
