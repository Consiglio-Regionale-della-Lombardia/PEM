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

using PortaleRegione.API.Controllers;
using PortaleRegione.BAL;
using PortaleRegione.Contracts;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.Logger;
using System;
using System.Data.Entity.Validation;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace PortaleRegione.API.Helpers
{

    /// <summary>
    ///     Controller di base
    /// </summary>
    public class BaseApiController : ApiController
    {
        internal readonly AttiFirmeLogic _attiFirmeLogic;
        internal readonly AttiLogic _attiLogic;
        internal readonly AuthLogic _authLogic;
        internal readonly DASILogic _dasiLogic;
        internal readonly EmendamentiLogic _emendamentiLogic;
        internal readonly EMPublicLogic _publicLogic;
        internal readonly EsportaLogic _esportaLogic;
        internal readonly FirmeLogic _firmeLogic;
        internal readonly LegislatureLogic _legislatureLogic;
        internal readonly NotificheLogic _notificheLogic;
        internal readonly PersoneLogic _personeLogic;
        internal readonly SeduteLogic _seduteLogic;
        internal readonly StampeLogic _stampeLogic;
        internal readonly IUnitOfWork _unitOfWork;
        internal readonly UtilsLogic _utilsLogic;
        internal readonly AdminLogic _adminLogic;

        /// <summary>
        ///     Costruttore
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="authLogic"></param>
        /// <param name="personeLogic"></param>
        /// <param name="legislatureLogic"></param>
        /// <param name="seduteLogic"></param>
        /// <param name="attiLogic"></param>
        /// <param name="dasiLogic"></param>
        /// <param name="firmeLogic"></param>
        /// <param name="attiFirmeLogic"></param>
        /// <param name="emendamentiLogic"></param>
        /// <param name="publicLogic"></param>
        /// <param name="notificheLogic"></param>
        /// <param name="esportaLogic"></param>
        /// <param name="stampeLogic"></param>
        /// <param name="utilsLogic"></param>
        /// <param name="adminLogic"></param>
        public BaseApiController(
            IUnitOfWork unitOfWork,
            AuthLogic authLogic,
            PersoneLogic personeLogic,
            LegislatureLogic legislatureLogic,
            SeduteLogic seduteLogic,
            AttiLogic attiLogic,
            DASILogic dasiLogic,
            FirmeLogic firmeLogic,
            AttiFirmeLogic attiFirmeLogic,
            EmendamentiLogic emendamentiLogic,
            EMPublicLogic publicLogic,
            NotificheLogic notificheLogic,
            EsportaLogic esportaLogic,
            StampeLogic stampeLogic,
            UtilsLogic utilsLogic,
            AdminLogic adminLogic)
        {
            _unitOfWork = unitOfWork;
            _authLogic = authLogic;
            _personeLogic = personeLogic;
            _legislatureLogic = legislatureLogic;
            _seduteLogic = seduteLogic;
            _attiLogic = attiLogic;
            _dasiLogic = dasiLogic;
            _firmeLogic = firmeLogic;
            _attiFirmeLogic = attiFirmeLogic;
            _emendamentiLogic = emendamentiLogic;
            _publicLogic = publicLogic;
            _notificheLogic = notificheLogic;
            _esportaLogic = esportaLogic;
            _stampeLogic = stampeLogic;
            _utilsLogic = utilsLogic;
            _adminLogic = adminLogic;

            Log.Initialize();
        }

        /// <summary>
        ///     Utente richiesta corrente
        /// </summary>
        public PersonaDto CurrentUser => GetCurrentUser();
        /// <summary>
        ///     Sessione ricavata dal json di autenticazione
        /// </summary>
        public SessionManager Session => GetSession();

        private PersonaDto GetCurrentUser()
        {
            try
            {
                var task_op = Task.Run(async () => await _personeLogic.GetPersona(Session));
                var persona = task_op.Result;
                if (persona == null)
                    return null;
                persona.CurrentRole = Session._currentRole;
                return persona;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        ///     Handler per catturare i messaggi di errore
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected IHttpActionResult ErrorHandler(Exception e)
        {
            var message = e.Message;
            if (e.GetType() == typeof(DbEntityValidationException))
            {
                var entityError = e as DbEntityValidationException;
                foreach (var entityValidationError in entityError.EntityValidationErrors)
                {
                    foreach (var validationError in entityValidationError.ValidationErrors)
                    {
                        message = validationError.ErrorMessage;
                        break;
                    }

                    break;
                }
            }

            if (e.InnerException != null)
                if (e.InnerException.Source == "EntityFramework")
                    message = e.InnerException.InnerException.Message;

            Console.WriteLine(message);
            return BadRequest(message);
        }

        /// <summary>
        ///     Metodo per avere l'utente loggato dal jwt
        /// </summary>
        /// <returns></returns>
        private SessionManager GetSession()
        {
            var identity = RequestContext.Principal.Identity as ClaimsIdentity;
            var session = new SessionManager();
            foreach (var identityClaim in identity.Claims)
                switch (identityClaim.Type)
                {
                    case ClaimTypes.Role:
                        session._currentRole = (RuoliIntEnum)Convert.ToInt16(identityClaim.Value);
                        break;
                    case "gruppo":
                        session._currentGroup = Convert.ToInt32(identityClaim.Value);
                        break;
                    case ClaimTypes.Name:
                        {
                            session._currentUId = new Guid(identityClaim.Value);
                            break;
                        }
                }

            return session;
        }
    }
}