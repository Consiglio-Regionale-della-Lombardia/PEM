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
using System.Web;
using System.Web.Http;
using PortaleRegione.API;
using Swashbuckle.Application;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace PortaleRegione.API
{
    /// <summary>
    ///     Configura Swagger per documentare e creare un'interfaccia utente per esplorare le API RESTful.
    ///     Questo setup permette di visualizzare e interagire con le API direttamente dal browser.
    /// </summary>
    public class SwaggerConfig
    {
        /// <summary>
        ///     Registra la configurazione di Swagger per l'applicazione. Questo metodo viene chiamato automaticamente
        ///     prima dell'avvio dell'applicazione grazie all'attributo PreApplicationStartMethod.
        /// </summary>
        public static void Register()
        {
            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                {
                    // Consente di specificare i protocolli http e https per le API.
                    c.Schemes(new[] { "http", "https" });

                    // Definisce una versione singola dell'API per Swagger UI con un titolo specifico.
                    c.SingleApiVersion("v1", "PortaleRegione.API.Public");

                    // Abilita la stampa formattata del JSON di output per una migliore leggibilità.
                    c.PrettyPrint();

                    // Ignora le azioni marcate come obsolete nell'API.
                    c.IgnoreObsoleteActions();

                    // Include i commenti XML generati dalla compilazione per arricchire la documentazione di Swagger.
                    c.IncludeXmlComments($@"{AppDomain.CurrentDomain.BaseDirectory}\bin\PortaleRegione.API.Public.xml");

                    // Utilizza il nome completo dei tipi nei riferimenti dello schema per evitare conflitti.
                    c.UseFullTypeNameInSchemaIds();

                    // Ignora le proprietà obsolete nei modelli dell'API.
                    c.IgnoreObsoleteProperties();

                    // Descrive tutti gli enum come stringhe per facilitare la comprensione.
                    c.DescribeAllEnumsAsStrings();
                })
                .EnableSwaggerUi(c =>
                {
                    // Personalizza il titolo della pagina Swagger UI.
                    c.DocumentTitle("Portale Regione API Pubblica");

                    // Imposta i valori booleani da visualizzare come "0" e "1" anziché "true" e "false".
                    c.BooleanValues(new[] { "0", "1" });

                    // Disabilita il validator integrato di Swagger UI.
                    c.DisableValidator();

                    // Configura l'espansione iniziale delle operazioni elencate in Swagger UI.
                    c.DocExpansion(DocExpansion.List);
                });
        }
    }
}