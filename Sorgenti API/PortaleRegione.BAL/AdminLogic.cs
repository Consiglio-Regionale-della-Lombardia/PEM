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
using PortaleRegione.BAL.proxyAd;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace PortaleRegione.BAL
{
    public class AdminLogic : BaseLogic
    {
        public AdminLogic(IUnitOfWork unitOfWork, PersoneLogic logicPersona, UtilsLogic logicUtil)
        {
            _unitOfWork = unitOfWork;
            _logicPersona = logicPersona;
            _logicUtil = logicUtil;
        }

        public async Task<IEnumerable<PersonaDto>> GetPersoneIn_DB(BaseRequest<PersonaDto> model,
            PersonaDto personaDto = null)
        {
            try
            {
                var result_filtro = new List<FilterStatement<PersonaDto>>(model.filtro);
                var filtro_ruoli = new List<int>();
                var filtro_gruppi = new List<int>();
                if (model.filtro.Any(f => f.PropertyId == nameof(PersonaDto.Ruoli)))
                {
                    var filtro_ruoli_da_rimuovere = model.filtro.Where(f => f.PropertyId == nameof(PersonaDto.Ruoli));
                    foreach (var ruolo in filtro_ruoli_da_rimuovere)
                    {
                        filtro_ruoli.Add(Convert.ToInt16(ruolo.Value));
                        result_filtro.Remove(ruolo);
                    }
                }

                if (model.filtro.Any(f => f.PropertyId == nameof(PersonaDto.id_gruppo_politico_rif)))
                {
                    var filtro_gruppi_da_rimuovere =
                        model.filtro.Where(f => f.PropertyId == nameof(PersonaDto.id_gruppo_politico_rif));
                    foreach (var gruppo in filtro_gruppi_da_rimuovere)
                    {
                        filtro_gruppi.Add(Convert.ToInt32(gruppo.Value));
                        result_filtro.Remove(gruppo);
                    }
                }

                var queryFilter = new Filter<View_UTENTI>();
                queryFilter.ImportStatements(result_filtro);

                var listaPersone = (await _unitOfWork
                        .Persone
                        .GetAll(model.page,
                            model.size,
                            personaDto,
                            queryFilter))
                    .Select(Mapper.Map<View_UTENTI, PersonaDto>);

                return listaPersone;
            }
            catch (Exception e)
            {
                //Log.Error("Logic - GetPersoneIn_DB", e);
                throw e;
            }
        }

        public async Task<IEnumerable<PersonaDto>> GetPersoneIn_DB_ByGiunta(BaseRequest<PersonaDto> model)
        {
            try
            {
                var result_filtro = new List<FilterStatement<PersonaDto>>(model.filtro);
                if (model.filtro.Any(f => f.PropertyId == nameof(PersonaDto.Ruoli)))
                {
                    var filtro_ruoli = model.filtro.Where(f => f.PropertyId == nameof(PersonaDto.Ruoli));
                    foreach (var ruolo in filtro_ruoli)
                    {
                        result_filtro.Remove(ruolo);
                    }
                }

                if (model.filtro.Any(f => f.PropertyId == nameof(PersonaDto.id_gruppo_politico_rif)))
                {
                    var filtro_gruppi =
                        model.filtro.Where(f => f.PropertyId == nameof(PersonaDto.id_gruppo_politico_rif));
                    foreach (var gruppo in filtro_gruppi)
                    {
                        result_filtro.Remove(gruppo);
                    }
                }

                var queryFilter = new Filter<View_UTENTI>();
                queryFilter.ImportStatements(result_filtro);

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
                //Log.Error("Logic - GetPersoneIn_DB_ByGiunta", e);
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
                var result_vuoto = new List<string> { "" };
                return result_vuoto;
            }
        }

        public async Task<int> Count(BaseRequest<PersonaDto> model, PersonaDto personaDto = null)
        {
            try
            {
                var result_filtro = new List<FilterStatement<PersonaDto>>(model.filtro);
                if (model.filtro.Any(f => f.PropertyId == nameof(PersonaDto.Ruoli)))
                {
                    var filtro_ruoli = model.filtro.Where(f => f.PropertyId == nameof(PersonaDto.Ruoli));
                    foreach (var ruolo in filtro_ruoli)
                    {
                        result_filtro.Remove(ruolo);
                    }
                }

                if (model.filtro.Any(f => f.PropertyId == nameof(PersonaDto.id_gruppo_politico_rif)))
                {
                    var filtro_gruppi =
                        model.filtro.Where(f => f.PropertyId == nameof(PersonaDto.id_gruppo_politico_rif));
                    foreach (var gruppo in filtro_gruppi)
                    {
                        result_filtro.Remove(gruppo);
                    }
                }

                var queryFilter = new Filter<View_UTENTI>();
                queryFilter.ImportStatements(result_filtro);

                return await _unitOfWork
                    .Persone
                    .CountAll(personaDto, queryFilter);
            }
            catch (Exception e)
            {
                //Log.Error("Logic - CountAll", e);
                throw e;
            }
        }

        public async Task<int> CountByGiunta(BaseRequest<PersonaDto> model)
        {
            try
            {
                var result_filtro = new List<FilterStatement<PersonaDto>>(model.filtro);
                if (model.filtro.Any(f => f.PropertyId == nameof(PersonaDto.Ruoli)))
                {
                    var filtro_ruoli = model.filtro.Where(f => f.PropertyId == nameof(PersonaDto.Ruoli));
                    foreach (var ruolo in filtro_ruoli)
                    {
                        result_filtro.Remove(ruolo);
                    }
                }

                if (model.filtro.Any(f => f.PropertyId == nameof(PersonaDto.id_gruppo_politico_rif)))
                {
                    var filtro_gruppi =
                        model.filtro.Where(f => f.PropertyId == nameof(PersonaDto.id_gruppo_politico_rif));
                    foreach (var gruppo in filtro_gruppi)
                    {
                        result_filtro.Remove(gruppo);
                    }
                }

                var queryFilter = new Filter<View_UTENTI>();
                queryFilter.ImportStatements(result_filtro);

                return await _unitOfWork
                    .Persone
                    .CountAllByGiunta(queryFilter);
            }
            catch (Exception e)
            {
                //Log.Error("Logic - CountAllByGiunta", e);
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
            await _unitOfWork.Persone.SavePin(request.persona_UId
                , BALHelper.EncryptString(request.new_value, AppSettingsConfiguration.masterKey)
                , true);
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
                persona.Gruppo = await _unitOfWork.Gruppi.GetGruppoPersona(gruppi_utente, persona.IsGiunta);
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

        public async Task<BaseResponse<PersonaDto>> GetUtenti(BaseRequest<PersonaDto> model, PersonaDto persona,
            Uri url)
        {
            try
            {
                var intranetAdService = new proxyAD();

                var filtri_ruoli_gruppi = new List<string>();
                if (model.filtro.Any(f => f.PropertyId == nameof(PersonaDto.Ruoli)))
                {
                    var filtro_ruoli_da_applicare = model.filtro.Where(f => f.PropertyId == nameof(PersonaDto.Ruoli));
                    foreach (var ruolo in filtro_ruoli_da_applicare)
                    {
                        var integer_role_value = Convert.ToInt32(ruolo.Value);
                        filtri_ruoli_gruppi.Add(RuoliExt.ConvertToAD((RuoliIntEnum)integer_role_value));
                    }
                }

                if (model.filtro.Any(f => f.PropertyId == nameof(PersonaDto.id_gruppo_politico_rif)))
                {
                    var filtro_gruppi_da_applicare =
                        model.filtro.Where(f => f.PropertyId == nameof(PersonaDto.id_gruppo_politico_rif));
                    foreach (var gruppo in filtro_gruppi_da_applicare)
                    {
                        var integer_group_value = Convert.ToInt32(gruppo.Value);
                        var gruppo_ad = await _unitOfWork.Gruppi.GetJoinGruppoAdmin(integer_group_value);
                        filtri_ruoli_gruppi.Add(gruppo_ad.GruppoAD.Replace(@"CONSIGLIO\", ""));
                    }
                }
                else if (persona.IsCapoGruppo || persona.IsResponsabileSegreteriaPolitica)
                {
                    var gruppo_ad = await _unitOfWork.Gruppi.GetJoinGruppoAdmin(persona.Gruppo.id_gruppo);
                    filtri_ruoli_gruppi.Add(gruppo_ad.GruppoAD.Replace(@"CONSIGLIO\", ""));
                }

                if (filtri_ruoli_gruppi.Any())
                {
                    var user_filtered = intranetAdService.GetUser_in_Group(
                        filtri_ruoli_gruppi.Aggregate((i, j) => i + "," + j), AppSettingsConfiguration.TOKEN_R);
                    foreach (var user in user_filtered)
                    {
                        model.filtro.Add(new FilterStatement<PersonaDto>
                        {
                            PropertyId = nameof(PersonaDto.userAD),
                            Operation = Operation.Contains,
                            Value = user,
                            Connector = FilterStatementConnector.Or
                        });
                    }
                }

                var results = new List<PersonaDto>();
                var persone_In_Db = new List<PersonaDto>();

                var counter = await Count(model, persona);
                persone_In_Db.AddRange(await GetPersoneIn_DB(model, persona));

                foreach (var persona_in_db in persone_In_Db)
                {
                    if (!string.IsNullOrEmpty(persona_in_db.userAD))
                    {
                        var gruppiUtente_PEM = new List<string>(intranetAdService.GetGroups(
                            persona_in_db.userAD.Replace(@"CONSIGLIO\", ""), "PEM_", AppSettingsConfiguration.TOKEN_R));

                        if (gruppiUtente_PEM.Any())
                        {
                            persona_in_db.Gruppi = gruppiUtente_PEM.Aggregate((i, j) => i + "; " + j);
                        }

                        var gruppiUtente_AD = GetADGroups(persona_in_db.userAD.Replace(@"CONSIGLIO\", ""));
                        if (gruppiUtente_AD.Any())
                        {
                            persona_in_db.GruppiAD = gruppiUtente_AD.Aggregate((i, j) => i + "; " + j);
                        }

                        persona_in_db.Stato_Pin = await CheckPin(persona_in_db);
                    }

                    results.Add(persona_in_db);
                }

                return new BaseResponse<PersonaDto>(
                    model.page,
                    persona.IsCapoGruppo || persona.IsResponsabileSegreteriaPolitica ? counter : model.size,
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

        public async Task<List<AdminGruppiModel>> GetGruppi(BaseRequest<GruppiDto> model)
        {
            try
            {
                var gruppi = await _logicPersona.GetGruppi(model);
                var result = new List<AdminGruppiModel>();
                var intranetAdService = new proxyAD();

                foreach (var gruppiDto in gruppi)
                {
                    var gruppoModel = new AdminGruppiModel
                    {
                        Gruppo = gruppiDto
                    };

                    var users_ad = intranetAdService.GetUser_in_Group(gruppiDto.GruppoAD.Replace(@"CONSIGLIO\", ""),
                        AppSettingsConfiguration.TOKEN_R);

                    if (gruppiDto.id_gruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID)
                    {
                        var assessori = await _unitOfWork.Gruppi.GetAssessoriInCarica();
                        foreach (var assessore in assessori)
                        {
                            if (!users_ad.Contains(assessore.Replace(@"CONSIGLIO\", "")))
                            {
                                gruppoModel.Error_AD_Message += $"{assessore};";
                            }
                        }
                    }
                    else
                    {
                        var consiglieri = await _unitOfWork.Gruppi.GetConsiglieriInCarica(gruppiDto.id_gruppo);
                        foreach (var consigliere in consiglieri)
                        {
                            if (!users_ad.Contains(consigliere.Replace(@"CONSIGLIO\", "")))
                            {
                                gruppoModel.Error_AD_Message += $"{consigliere};";
                            }
                        }
                    }

                    gruppoModel.Error_AD = !string.IsNullOrEmpty(gruppoModel.Error_AD_Message);
                    if (gruppoModel.Error_AD_Message.Length > 0)
                        gruppoModel.Error_AD_Message =
                            gruppoModel.Error_AD_Message.Substring(0, gruppoModel.Error_AD_Message.Length - 1);
                    result.Add(gruppoModel);
                }

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<Guid> SalvaUtente(PersonaUpdateRequest request, RuoliIntEnum ruolo)
        {
            try
            {
                var intranetAdService = new proxyAD();

                if (request.UID_persona == Guid.Empty)
                {
                    //NUOVO

                    //string ldapPath = "OU=PEM,OU=Intranet,OU=Gruppi,DC=consiglio,DC=lombardia";
                    var autoPassword = _logicUtil.GenerateRandomCode();
                    intranetAdService.CreatePEMADUser(
                        request.userAD,
                        autoPassword,
                        ruolo == RuoliIntEnum.Amministratore_Giunta,
                        AppSettingsConfiguration.TOKEN_W
                    );
                    autoPassword = ruolo == RuoliIntEnum.Amministratore_Giunta
                        ? $"{autoPassword}"
                        : "[Stessa usata per accedere al tuo pc]";

                    request.UID_persona = Guid.NewGuid();
                    request.no_Cons = 1;
                    UTENTI_NoCons newUser = request;
                    _unitOfWork.Persone.Add(newUser);
                    await _unitOfWork.CompleteAsync();


                    await _logicUtil.InvioMail(new MailModel
                    {
                        DA = "pem@consiglio.regione.lombardia.it",
                        A = request.email,
                        CC = "max.pagliaro@consiglio.regione.lombardia.it",
                        OGGETTO = "PEM - Utenza aperta",
                        MESSAGGIO =
                            $"Benvenuto in PEM, <br/> utilizza le seguenti credenziali: <br/> <b>Username</b> <br/> {request.userAD.Replace(@"CONSIGLIO\", "")}<br/> <b>Password</b> <br/> {autoPassword}<br/><br/> {AppSettingsConfiguration.urlPEM}"
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
                                if (resultAdd != 0)
                                    throw new InvalidOperationException(
                                        $"Errore inserimento gruppo AD [{item.GruppoAD}]");
                            }
                            catch (Exception e)
                            {
                                //Log.Error($"Add - {item.GruppoAD}", e);
                            }
                        }
                        else
                        {
                            try
                            {
                                var resultRemove = intranetAdService.RemoveUserFromADGroup(item.GruppoAD,
                                    request.userAD,
                                    AppSettingsConfiguration.TOKEN_W);
                                if (resultRemove == false)
                                    throw new InvalidOperationException(
                                        $"Errore rimozione gruppo AD [{item.GruppoAD}]");
                            }
                            catch (Exception e)
                            {
                                //Log.Error($"Remove - {item.GruppoAD}", e);
                            }
                        }
                    }


                    try
                    {
                        if (request.no_Cons == 1)
                        {
                            //Consigliere/Assessore
                            var persona = await _unitOfWork.Persone.Get_NoCons(request.UID_persona);
                            persona.nome = request.nome;
                            persona.cognome = request.cognome;
                            persona.email = request.email;
                            persona.foto = request.foto;
                            persona.id_gruppo_politico_rif = request.id_gruppo_politico_rif;
                            persona.UserAD = request.userAD;
                            persona.notifica_firma = request.notifica_firma;
                            persona.notifica_deposito = request.notifica_deposito;
                            await _unitOfWork.CompleteAsync();
                        }
                        else
                        {
                            await _unitOfWork.Persone.UpdateUtente_NoCons(request.UID_persona, request.id_persona,
                                request.userAD.Replace(@"CONSIGLIO\", ""));
                        }
                    }
                    catch (Exception e)
                    {
                        //Log.Error($"Salvataggio", e);
                        throw;
                    }
                }

                return request.UID_persona;
            }
            catch (Exception e)
            {
                //Log.Error($"SalvaUtente", e);
                throw e;
            }
        }

        public async Task EliminaUtente(Guid uid_persona)
        {
            try
            {
                var user = await _unitOfWork.Persone.Get_NoCons(uid_persona);
                user.deleted = true;
                user.attivo = false;
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SalvaGruppo(SalvaGruppoRequest request)
        {
            try
            {
                var gruppo = await _unitOfWork.Gruppi.GetJoinGruppoAdmin(request.Id_Gruppo);
                gruppo.GruppoAD = request.GruppoAD;
                gruppo.AbilitaEMPrivati = request.abilita_em_privati;
                gruppo.GiuntaRegionale = request.giunta;
                await _unitOfWork.CompleteAsync();

#if DEBUG == false
                //CREO IL GRUPPO IN ACTIVE DIRECTORY
                try
                {
                    var user_list = "";
                    if (request.Id_Gruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID)
                    {
                        var assessori = await _unitOfWork.Gruppi.GetAssessoriInCarica();
                        user_list = assessori.Aggregate((i, j) => i + ";" + j);
                    }
                    else
                    {
                        var consiglieri = await _unitOfWork.Gruppi.GetConsiglieriInCarica(request.Id_Gruppo);
                        user_list = consiglieri.Aggregate((i, j) => i + ";" + j);
                    }

                    user_list = user_list.Replace(@"CONSIGLIO\", "");

                    var intranetAdService = new proxyAD();

                    intranetAdService.CreatePEMADGroup(request.GruppoAD.Replace(@"CONSIGLIO\", ""), user_list,
                        AppSettingsConfiguration.TOKEN_W);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
#endif
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}