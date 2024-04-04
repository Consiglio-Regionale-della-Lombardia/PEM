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

using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PortaleRegione.Api.Public.Business_Layer;
using PortaleRegione.Contracts.Public;
using PortaleRegione.Persistance.Public;
using Unity;
using Unity.Lifetime;

namespace PortaleRegione.Api.Public
{
    /// <summary>
    ///     Classe di configurazione per l'API Web. Imposta la serializzazione, le route e la dependency injection.
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        ///     Configura i componenti globali dell'API Web.
        /// </summary>
        /// <param name="config">L'oggetto di configurazione dell'API Web.</param>
        public static void Register(HttpConfiguration config)
        {
            // Configura la serializzazione JSON per utilizzare la notazione camelCase nelle proprietà
            // e rende l'output JSON indentato per una migliore leggibilità.
            var settings = config.Formatters.JsonFormatter.SerializerSettings;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Formatting = Formatting.Indented;

            // Inizializza e configura il contenitore di Unity per la gestione delle dipendenze.
            var container = new UnityContainer();
            // Registra l'interfaccia IUnitOfWork con la sua implementazione concreta UnitOfWork.
            // Usa HierarchicalLifetimeManager per gestire il ciclo di vita delle istanze.
            container.RegisterType<IUnitOfWork, UnitOfWork>(new HierarchicalLifetimeManager());
            // Registra MainLogic per la dependency injection, anch'esso con un ciclo di vita gerarchico.
            container.RegisterType<MainLogic>(new HierarchicalLifetimeManager());
            // Imposta il resolver di dipendenze personalizzato basato su Unity.
            config.DependencyResolver = new UnityResolver(container);

            // Abilita il routing basato sugli attributi, permettendo di definire le route direttamente
            // nei controller tramite attributi.
            config.MapHttpAttributeRoutes();

            // Configura una route predefinita per l'API Web. Questa route cattura la maggior parte delle richieste
            // e le mappa ai controller e alle azioni basandosi sui segmenti dell'URL.
            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{id}",
                new { id = RouteParameter.Optional }
            );
        }
    }
}