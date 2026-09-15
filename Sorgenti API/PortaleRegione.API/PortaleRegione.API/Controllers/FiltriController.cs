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
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using PortaleRegione.API.Helpers;
using PortaleRegione.BAL;
using PortaleRegione.Contracts;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Routes;
using PortaleRegione.Logger;

namespace PortaleRegione.API.Controllers
{
    /// <summary>
    ///     Controller per la gestione unificata dei filtri preferiti utente.
    ///     Sostituisce le vecchie route /dasi/filters/* e copre anche il
    ///     modulo PEM (introdotto in v2026.5.1).
    ///     Il modulo viene passato come parametro di route per Get/Elimina e
    ///     come campo del DTO per Salva.
    /// </summary>
    [Authorize]
    public class FiltriController : BaseApiController
    {
        private readonly FiltriLogic _filtriLogic;

        public FiltriController(IUnitOfWork unitOfWork, AuthLogic authLogic, PersoneLogic personeLogic,
            LegislatureLogic legislatureLogic, SeduteLogic seduteLogic, AttiLogic attiLogic, DASILogic dasiLogic,
            FirmeLogic firmeLogic, AttiFirmeLogic attiFirmeLogic, EmendamentiLogic emendamentiLogic,
            EMPublicLogic publicLogic, NotificheLogic notificheLogic, EsportaLogic esportaLogic,
            StampeLogic stampeLogic, UtilsLogic utilsLogic, AdminLogic adminLogic,
            FiltriLogic filtriLogic, IMapper mapper)
            : base(unitOfWork, authLogic, personeLogic, legislatureLogic, seduteLogic, attiLogic, dasiLogic,
                firmeLogic, attiFirmeLogic, emendamentiLogic, publicLogic, notificheLogic, esportaLogic,
                stampeLogic, utilsLogic, adminLogic, mapper)
        {
            _filtriLogic = filtriLogic;
        }

        /// <summary>
        ///     Salva un filtro preferito. Il modulo (PEM o DASI) e' nel payload DTO.
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.Filtri.Salva)]
        public async Task<IHttpActionResult> Salva(FiltroPreferitoDto request)
        {
            try
            {
                await _filtriLogic.Salva(request, CurrentUser, request.modulo);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Filtri preferiti - salva", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Recupera i filtri preferiti dell'utente per il modulo richiesto.
        /// </summary>
        [HttpGet]
        [Route(ApiRoutes.Filtri.Get)]
        public async Task<IHttpActionResult> Get(ModuloEnum modulo)
        {
            try
            {
                var res = await _filtriLogic.GetByUser(CurrentUser, modulo);
                return Ok(res);
            }
            catch (Exception e)
            {
                Log.Error("Filtri preferiti - get", e);
                return ErrorHandler(e);
            }
        }

        /// <summary>
        ///     Elimina il filtro preferito identificato da nome+utente sul modulo richiesto.
        /// </summary>
        [HttpDelete]
        [Route(ApiRoutes.Filtri.Elimina)]
        public async Task<IHttpActionResult> Elimina(ModuloEnum modulo, string nome)
        {
            try
            {
                await _filtriLogic.Elimina(nome, CurrentUser, modulo);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error("Filtri preferiti - elimina", e);
                return ErrorHandler(e);
            }
        }
    }
}
