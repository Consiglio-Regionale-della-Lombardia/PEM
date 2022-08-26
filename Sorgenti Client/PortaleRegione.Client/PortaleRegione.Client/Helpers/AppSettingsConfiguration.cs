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
using System.Configuration;

namespace PortaleRegione.Client.Helpers
{
    public class AppSettingsConfiguration
    {
        public static bool IsDebugMode => ConfigurationManager.AppSettings["Environment"]
            .Equals("Debug", StringComparison.InvariantCultureIgnoreCase);
        public static string Logo => ConfigurationManager.AppSettings["logo"];
        public static string Title => ConfigurationManager.AppSettings["title"];
        public static string LimiteDocumentiDaProcessare => ConfigurationManager.AppSettings["LimiteDocumentiDaProcessare"];
        public static string URL_CLIENT => ConfigurationManager.AppSettings["URL_CLIENT"];
        public static string URL_API => ConfigurationManager.AppSettings["URL_API"];
        public static string GEASI_URL => ConfigurationManager.AppSettings["GEASI_URL"];
        public static string GEASI_USERNAME => ConfigurationManager.AppSettings["GEASI_USERNAME"];
        public static string GEASI_PASSWORD => ConfigurationManager.AppSettings["GEASI_PASSWORD"];
        public static int COOKIE_EXPIRE_IN => Convert.ToInt16(ConfigurationManager.AppSettings["COOKIE_EXPIRE_IN"]);
    }
}