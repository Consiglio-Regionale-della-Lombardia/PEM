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

using System.Configuration;

namespace PortaleRegione.Api.Public.Helpers
{
    /// <summary>
    ///     Fornisce un accesso semplificato alle configurazioni dell'applicazione, consentendo di recuperare i valori
    ///     specificati nel file di configurazione dell'app.
    /// </summary>
    public class AppSettingsConfigurationHelper
    {
        /// <summary>
        ///     Ottiene la chiave 'masterKey' dal file di configurazione dell'applicazione.
        ///     Questa chiave può essere utilizzata, ad esempio, per operazioni di crittografia/descrittografia all'interno
        ///     dell'applicazione.
        /// </summary>
        public static string masterKey => ConfigurationManager.AppSettings["masterKey"];
        
        /// <summary>
        ///     Ottiene la chiave 'masterKey' dal file di configurazione dell'applicazione.
        ///     Questa chiave può essere utilizzata, ad esempio, per operazioni di crittografia/descrittografia all'interno
        ///     dell'applicazione.
        /// </summary>
        public static string PercorsoCompatibilitaDocumenti => ConfigurationManager.AppSettings["PercorsoCompatibilitaDocumenti"];
    }
}