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
using PortaleRegione.BAL;
using PortaleRegione.Contracts;
using PortaleRegione.Persistance;
using Unity;
using Unity.Lifetime;

namespace PortaleRegione.API
{
    /// <summary>
    ///     Classe per la configurazione dell'api
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        ///     Metodo per la configurazione dell'api
        /// </summary>
        /// <param name="config"></param>
        public static void Register(HttpConfiguration config)
        {
            // Servizi e configurazione dell'API Web
            var settings = config.Formatters.JsonFormatter.SerializerSettings;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Formatting = Formatting.Indented;

            // Tutte le chiamate vengono decorate con [AUTHORIZE]
            config.Filters.Add(new AuthorizeAttribute());

            // DI
            var container = new UnityContainer();
            container.RegisterType<IUnitOfWork, UnitOfWork>(new HierarchicalLifetimeManager());
            container.RegisterType<AuthLogic>(new HierarchicalLifetimeManager());
            container.RegisterType<PersoneLogic>(new HierarchicalLifetimeManager());
            container.RegisterType<SeduteLogic>(new HierarchicalLifetimeManager());
            container.RegisterType<AttiLogic>(new HierarchicalLifetimeManager());
            container.RegisterType<EmendamentiLogic>(new HierarchicalLifetimeManager());
            container.RegisterType<FirmeLogic>(new HierarchicalLifetimeManager());
            container.RegisterType<StampeLogic>(new HierarchicalLifetimeManager());
            container.RegisterType<EsportaLogic>(new HierarchicalLifetimeManager());
            container.RegisterType<UtilsLogic>(new HierarchicalLifetimeManager());
            container.RegisterType<NotificheLogic>(new HierarchicalLifetimeManager());
            config.DependencyResolver = new UnityResolver(container);

            // Route dell'API Web
            config.MapHttpAttributeRoutes();

            // Middleware JWT
            config.MessageHandlers.Add(new TokenValidationHandler());

            config.Routes.MapHttpRoute(
                "DefaultApi",
                "{controller}/{id}",
                new {id = RouteParameter.Optional}
            );
        }
    }
}