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
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using ExpressionBuilder.Generics;
using Newtonsoft.Json;
using PortaleRegione.API.Controllers;
using PortaleRegione.Common;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;

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

        public async Task<List<StampaDto>> InserisciStampa(NuovaStampaRequest request, PersonaDto persona)
        {
            var stampe = new List<StampaDto>();
            var listaCount = request.Lista.Count;
            var scadenza = DateTime.Now.AddDays(Convert.ToDouble(AppSettingsConfiguration.GiorniValiditaLink));
            var dataRichiesta = DateTime.Now;
            var currentRole = (int)persona.CurrentRole;
            var uidPersonaRichiesta = persona.UID_persona;

            // Genera un UIDFascicolo per raggruppare i slice
            var uidFascicolo = Guid.NewGuid();

            // Verifica se la lista supera i 1000 elementi
            if (listaCount > 1000)
            {
                // Suddividi la lista in slice da 1000
                var listaSuddivisa = Utility.Split(request.Lista, 500);

                // Ordine progressivo per ogni slice
                var numeroFascicolo = 1;

                foreach (var slice in listaSuddivisa)
                {
                    var stampa = new StampaDto
                    {
                        DataRichiesta = dataRichiesta,
                        CurrentRole = currentRole,
                        UIDStampa = Guid.NewGuid(),
                        UIDUtenteRichiesta = uidPersonaRichiesta,
                        Lock = false,
                        Tentativi = 0,
                        Query = JsonConvert.SerializeObject(slice), // Serializza il slice attuale
                        Da = request.Da,
                        A = request.A,
                        DASI = request.Modulo == ModuloStampaEnum.DASI,
                        Ordine = (int)request.Ordinamento,
                        UIDFascicolo = uidFascicolo, // GUID di raggruppamento
                        NumeroFascicolo = numeroFascicolo // Ordine progressivo
                    };

                    if (request.UIDAtto != Guid.Empty)
                    {
                        stampa.UIDAtto = request.UIDAtto;
                    }

                    if (stampa.A == 0 && stampa.Da == 0)
                    {
                        stampa.Scadenza = null;
                    }
                    else
                    {
                        stampa.Scadenza = scadenza;
                    }

                    _unitOfWork.Stampe.Add(stampa);
                    stampe.Add(stampa);

                    numeroFascicolo++;
                }
            }
            else
            {
                // Caso in cui la lista è inferiore o uguale a 1000
                var stampa = new StampaDto
                {
                    DataRichiesta = dataRichiesta,
                    CurrentRole = currentRole,
                    UIDStampa = Guid.NewGuid(),
                    UIDUtenteRichiesta = uidPersonaRichiesta,
                    Lock = false,
                    Tentativi = 0,
                    Query = JsonConvert.SerializeObject(request.Lista), // Serializza la lista intera
                    Da = request.Da,
                    A = request.A,
                    DASI = request.Modulo == ModuloStampaEnum.DASI,
                    Ordine = (int)request.Ordinamento
                };

                if (request.UIDAtto != Guid.Empty)
                {
                    stampa.UIDAtto = request.UIDAtto;
                }

                if (stampa.A == 0 && stampa.Da == 0)
                {
                    stampa.Scadenza = null;
                }
                else
                {
                    stampa.Scadenza = scadenza;
                }

                _unitOfWork.Stampe.Add(stampa);
                stampe.Add(stampa);
            }

            await _unitOfWork.CompleteAsync();

            return stampe;
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
        
        public async Task<HttpResponseMessage> DownloadFascicoloStampa(string nomeFile)
        {
            nomeFile += ".pdf";
            var result = await ComposeFileResponse(Path.Combine(AppSettingsConfiguration.CartellaLavoroStampe, nomeFile));
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
                UrlCLIENT = AppSettingsConfiguration.url_CLIENT,
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