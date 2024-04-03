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

using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using PortaleRegione.Logger;

namespace PortaleRegione.Api.Public
{
    /// <summary>
    ///     Classe di avvio per l'applicazione Web API.
    ///     Configura i componenti globali necessari per il funzionamento dell'applicazione al suo avvio.
    /// </summary>
    public class WebApiApplication : HttpApplication
    {
        /// <summary>
        ///     Metodo eseguito all'avvio dell'applicazione.
        ///     Inizializza e configura vari aspetti dell'applicazione come il logging, le aree,
        ///     le configurazioni API, i filtri globali e le rotte.
        /// </summary>
        protected void Application_Start()
        {
            // Inizializza il sistema di logging.
            Log.Initialize();

            // Registra tutte le aree definite nell'applicazione. Le aree permettono di organizzare
            // grandi applicazioni Web in segmenti più piccoli.
            AreaRegistration.RegisterAllAreas();

            // Configura le impostazioni API specifiche dell'applicazione, come i formattatori di risposta,
            // le convenzioni di routing, ecc.
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Registra i filtri globali. I filtri possono essere usati per gestire eccezioni, autorizzazione,
            // azioni di logging e altro a livello globale.
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            // Configura le rotte globali dell'applicazione. Il routing determina come le richieste URL vengono 
            // mappate alle azioni dei controller.
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}