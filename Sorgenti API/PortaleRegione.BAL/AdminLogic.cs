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
using PortaleRegione.BAL.proxyAd;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace PortaleRegione.BAL
{
    public class AdminLogic : BaseLogic
    {
        private readonly PersoneLogic _logicPersona;
        private readonly UtilsLogic _logicUtil;
        private readonly IUnitOfWork _unitOfWork;

        public AdminLogic(IUnitOfWork unitOfWork, PersoneLogic logicPersona, UtilsLogic logicUtil)
        {
            _unitOfWork = unitOfWork;
            _logicPersona = logicPersona;
            _logicUtil = logicUtil;
        }

        public async Task<IEnumerable<PersonaDto>> GetPersoneIn_DB(BaseRequest<PersonaDto> model)
        {
            try
            {
                var queryFilter = new Filter<View_UTENTI>();
                queryFilter.ImportStatements(model.filtro);

                var listaPersone = (await _unitOfWork
                        .Persone
                        .GetAll(model.page,
                            model.size,
                            queryFilter))
                    .Select(Mapper.Map<View_UTENTI, PersonaDto>);

                return listaPersone;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetPersoneIn_DB", e);
                throw e;
            }
        }

        public async Task<IEnumerable<PersonaDto>> GetPersoneIn_DB_ByGiunta(BaseRequest<PersonaDto> model)
        {
            try
            {
                var queryFilter = new Filter<View_UTENTI>();
                queryFilter.ImportStatements(model.filtro);

                var listaPersone = (await _unitOfWork
                        .Persone
                        .GetAllByGiunta(model.page,
                            model.size,
                            queryFilter))
                    .Select(Mapper.Map<View_UTENTI, PersonaDto>);

                return listaPersone;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetPersoneIn_DB_ByGiunta", e);
                throw e;
            }
        }

        public List<string> GetADGroups(string userName, string simplefilter = "")
        {
            var result = new List<string>();
            try
            {
                var wi = new WindowsIdentity(userName);

                foreach (var group in wi.Groups)
                {
                    try
                    {
                        if (simplefilter != "")
                        {
                            if (group.Translate(typeof(NTAccount)).ToString().Contains(simplefilter))
                            {
                                result.Add(group.Translate(typeof(NTAccount)).ToString().Replace(@"CONSIGLIO\", ""));
                            }
                        }
                        else
                        {
                            result.Add(group.Translate(typeof(NTAccount)).ToString().Replace(@"CONSIGLIO\", ""));
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                result.Sort();
                return result;
            }
            catch (Exception)
            {
                var result_vuoto = new List<string>();
                result_vuoto.Add("");
                return result_vuoto;
            }
        }

        public async Task<int> Count(BaseRequest<PersonaDto> model)
        {
            try
            {
                var queryFilter = new Filter<View_UTENTI>();
                queryFilter.ImportStatements(model.filtro);

                return await _unitOfWork
                    .Persone
                    .CountAll(queryFilter);
            }
            catch (Exception e)
            {
                Log.Error("Logic - CountAll", e);
                throw e;
            }
        }

        public async Task<int> CountByGiunta(BaseRequest<PersonaDto> model)
        {
            try
            {
                var queryFilter = new Filter<View_UTENTI>();
                queryFilter.ImportStatements(model.filtro);

                return await _unitOfWork
                    .Persone
                    .CountAllByGiunta(queryFilter);
            }
            catch (Exception e)
            {
                Log.Error("Logic - CountAllByGiunta", e);
                throw e;
            }
        }

        /// <summary>
        ///     Controlla il pin della persona
        /// </summary>
        /// <param name="persona"></param>
        public async Task<StatoPinEnum> CheckPin(PersonaDto persona)
        {
            var pinInDb = await _logicPersona.GetPin(persona);
            if (pinInDb == null)
            {
                return StatoPinEnum.NESSUNO;
            }

            return pinInDb.RichiediModificaPIN ? StatoPinEnum.RESET : StatoPinEnum.VALIDO;
        }

        public async Task<IEnumerable<RuoliDto>> GetRuoliAD(bool SoloRuoliGiunta)
        {
            var listaRuoli = await _unitOfWork.Ruoli.GetAll(SoloRuoliGiunta);
            return listaRuoli.Select(Mapper.Map<RUOLI, RuoliDto>);
        }

        public async Task<IEnumerable<GruppoAD_Dto>> GetGruppiPoliticiAD(bool SoloRuoliGiunta)
        {
            var listaGruppiAD = await _unitOfWork
                .Gruppi
                .GetGruppiPoliticiAD(
                    await _unitOfWork.Legislature.Legislatura_Attiva(),
                    SoloRuoliGiunta);
            return listaGruppiAD.Select(Mapper.Map<JOIN_GRUPPO_AD, GruppoAD_Dto>);
        }

        public async Task ResetPin(ResetRequest request)
        {
            await _unitOfWork.Persone.SavePin(request.persona_UId, request.new_value, false);
            await _unitOfWork.CompleteAsync();
        }

        public async Task ResetPassword(ResetRequest request)
        {
            try
            {
                var persona = await _unitOfWork.Persone.Get(request.persona_UId);
                string ris;
                var IntranetADService = new proxyAD();
                ris = IntranetADService
                    .ChangeADUserPass(
                        persona.userAD.Replace(@"CONSIGLIO\", ""),
                        string.Empty,
                        request.new_value,
                        AppSettingsConfiguration.TOKEN_W);
                if (!ris.Contains("0:"))
                {
                    throw new Exception("Reset password non riuscito. Motivo: " + ris);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<PersonaDto> GetPersona(SessionManager session)
        {
            return await _logicPersona.GetPersona(session);
        }

        public async Task<PersonaDto> GetPersona(Guid id)
        {
            return await _logicPersona.GetPersona(id);
        }

        public async Task<PersonaDto> GetPersona(PersonaDto persona, List<string> gruppi_utente)
        {
            var ruoli_utente = (await _unitOfWork.Ruoli.RuoliUtente(gruppi_utente)).ToList();
            if (ruoli_utente.Any())
            {
                persona.Ruoli = ruoli_utente.Select(Mapper.Map<RUOLI, RuoliDto>);
                persona.CurrentRole = (RuoliIntEnum)ruoli_utente[0].IDruolo;
            }
            else
            {
                persona.CurrentRole = RuoliIntEnum.Utente;
            }

            if (gruppi_utente.Any())
            {
                persona.Gruppo = await _unitOfWork.Gruppi.GetGruppoPersona(gruppi_utente, persona.IsGiunta());
                persona.Gruppi = gruppi_utente.Aggregate((i, j) => i + "; " + j);
            }

            var gruppiUtente_AD = GetADGroups(persona.userAD.Replace(@"CONSIGLIO\", ""));
            if (gruppiUtente_AD.Any())
            {
                persona.GruppiAD = gruppiUtente_AD.Aggregate((i, j) => i + "; " + j);
            }

            persona.Stato_Pin = await CheckPin(persona);

            return persona;
        }

        public async Task<PersonaDto> GetUtente(Guid id)
        {
            var persona = Mapper.Map<View_UTENTI, PersonaDto>(await _unitOfWork.Persone.Get(id));

            var intranetAdService = new proxyAD();

            var lRuoli = new List<string>();
            var Gruppi_Utente = new List<string>(intranetAdService.GetGroups(
                persona.userAD.Replace(@"CONSIGLIO\", ""), "PEM_", AppSettingsConfiguration.TOKEN_R));

            foreach (var group in Gruppi_Utente)
            {
                lRuoli.Add($"CONSIGLIO\\{group}");
            }

            var personaResult = await GetPersona(persona, lRuoli);

            return personaResult;
        }

        public async Task<BaseResponse<PersonaDto>> GetUtenti(BaseRequest<PersonaDto> model, SessionManager session,
            Uri url)
        {
            try
            {
                var results = new List<PersonaDto>();
                var counter = 0;
                var persone_In_Db = new List<PersonaDto>();
                if (session._currentRole == RuoliIntEnum.Amministratore_PEM)
                {
                    persone_In_Db.AddRange(await GetPersoneIn_DB(model));
                    counter = await Count(model);
                }
                else
                {
                    persone_In_Db.AddRange(await GetPersoneIn_DB_ByGiunta(model));
                    counter = await CountByGiunta(model);
                }

                var intranetAdService = new proxyAD();
                foreach (var persona in persone_In_Db)
                {
                    if (!string.IsNullOrEmpty(persona.userAD))
                    {
                        var gruppiUtente_PEM = new List<string>(intranetAdService.GetGroups(
                            persona.userAD.Replace(@"CONSIGLIO\", ""), "PEM_", AppSettingsConfiguration.TOKEN_R));
                        if (gruppiUtente_PEM.Any())
                        {
                            persona.Gruppi = gruppiUtente_PEM.Aggregate((i, j) => i + "; " + j);
                        }

                        var gruppiUtente_AD = GetADGroups(persona.userAD.Replace(@"CONSIGLIO\", ""));
                        if (gruppiUtente_AD.Any())
                        {
                            persona.GruppiAD = gruppiUtente_AD.Aggregate((i, j) => i + "; " + j);
                        }

                        persona.Stato_Pin = await CheckPin(persona);
                    }

                    results.Add(persona);
                }

                return new BaseResponse<PersonaDto>(
                    model.page,
                    model.size,
                    results,
                    model.filtro,
                    counter,
                    url);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<BaseResponse<GruppiDto>> GetGruppi(BaseRequest<GruppiDto> model, Uri url)
        {
            try
            {
                var results = await _logicPersona.GetGruppi(model);

                return new BaseResponse<GruppiDto>(
                    model.page,
                    model.size,
                    results,
                    model.filtro,
                    results.Count(),
                    url);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task SalvaUtente(PersonaUpdateRequest request, RuoliIntEnum ruolo)
        {
            try
            {
                var intranetAdService = new proxyAD();

                if (request.UID_persona == Guid.Empty)
                {
                    //NUOVO

                    string ldapPath = "OU=PEM,OU=Intranet,OU=Gruppi,DC=consiglio,DC=lombardia";
                    string autoPassword = _logicUtil.GenerateRandomCode();
                    intranetAdService.CreatePEMADUser(
                        request.userAD,
                        autoPassword,
                        ruolo == RuoliIntEnum.Amministratore_Giunta,
                        AppSettingsConfiguration.TOKEN_W
                    );



                    await _logicUtil.InvioMail(new MailModel
                    {
                        DA = "pem@consiglio.regione.lombardia.it",
                        A = request.email,
                        CC = "max.pagliaro@consiglio.regione.lombardia.it",
                        OGGETTO = "PEM - Utenza aperta",
                        MESSAGGIO = $"Benvenuto in PEM, <br/> utilizza le seguenti credenziali: <br/> <b>Username</b> <br/> {request.userAD}<br/> <b>Password</b> <br/> {autoPassword}<br/><br/> {AppSettingsConfiguration.urlPEM}"
                    });
                }
                else
                {
                    foreach (var item in request.gruppiAd)
                    {
                        if (item.Membro)
                        {
                            try
                            {
                                var resultAdd = intranetAdService.AddUserToADGroup(item.GruppoAD, request.userAD,
                                    AppSettingsConfiguration.TOKEN_W);
                            }
                            catch (Exception e)
                            {
                                Log.Error($"Add - {item.GruppoAD}", e);
                            }
                        }
                        else
                        {
                            try
                            {
                                var resultRemove = intranetAdService.RemoveUserFromADGroup(item.GruppoAD,
                                    request.userAD,
                                    AppSettingsConfiguration.TOKEN_W);
                            }
                            catch (Exception e)
                            {
                                Log.Error($"Remove - {item.GruppoAD}", e);
                            }
                        }
                    }

                    if (request.no_Cons == 1)
                    {
                        //No CONS
                        await _unitOfWork.Persone.UpdateUtente_NoCons(request.UID_persona, request.id_persona,
                            request.userAD);
                    }
                    else
                    {
                        //Consigliere/Assessore
                        var persona = await _unitOfWork.Persone.Get(request.UID_persona);
                        persona.userAD = request.userAD;
                        persona.notifica_firma = request.notifica_firma;
                        persona.notifica_deposito = request.notifica_deposito;
                        await _unitOfWork.CompleteAsync();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}