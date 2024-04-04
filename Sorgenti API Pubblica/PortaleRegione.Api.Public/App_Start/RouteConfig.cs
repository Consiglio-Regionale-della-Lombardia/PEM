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

using System.Web.Mvc;
using System.Web.Routing;

namespace PortaleRegione.Api.Public
{
    /// <summary>
    ///     Configura le regole di routing per l'applicazione. Il routing determina come le URL vengono
    ///     mappate alle azioni dei controller.
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        ///     Registra le rotte nell'applicazione, definendo come le richieste URL vengono mappate
        ///     alle azioni dei controller.
        /// </summary>
        /// <param name="routes">La collezione di rotte dell'applicazione.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            // Ignora le richieste per i file di risorse .axd, come i tracciati delle richieste ASP.NET.
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Imposta le URL generate dall'applicazione per essere in minuscolo, migliorando la consistenza
            // e potenzialmente evitando problemi di differenziazione maiuscole/minuscole su alcuni server.
            routes.LowercaseUrls = true;

            // Abilita il routing basato sugli attributi, permettendo di definire le rotte direttamente
            // negli attributi delle azioni dei controller.
            routes.MapMvcAttributeRoutes();

            // Definisce una rotta predefinita che mappa la struttura URL più comune: /{controller}/{action}/{id}
            // Dove `id` è opzionale. Questa rotta di default fa sì che, se non specificato diversamente,
            // le richieste siano dirette al controller "Home" e all'azione "Index".
            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}