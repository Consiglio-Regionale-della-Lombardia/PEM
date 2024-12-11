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
        public static string NomePiattaforma => ConfigurationManager.AppSettings["NomePiattaforma"];
        public static string LimiteDocumentiDaProcessare => ConfigurationManager.AppSettings["LimiteDocumentiDaProcessare"];
        public static string URL_CLIENT => ConfigurationManager.AppSettings["URL_CLIENT"];
        public static string URL_API => ConfigurationManager.AppSettings["URL_API"];
        public static string GEASI_URL => ConfigurationManager.AppSettings["GEASI_URL"];
        public static string GEASI_USERNAME => ConfigurationManager.AppSettings["GEASI_USERNAME"];
        public static string GEASI_PASSWORD => ConfigurationManager.AppSettings["GEASI_PASSWORD"];
        public static int COOKIE_EXPIRE_IN => Convert.ToInt16(ConfigurationManager.AppSettings["COOKIE_EXPIRE_IN"]);
        public static bool EnablePEM => Convert.ToBoolean(Convert.ToInt16(ConfigurationManager.AppSettings["PEM"]));
        public static bool EnableDASI => Convert.ToBoolean(Convert.ToInt16(ConfigurationManager.AppSettings["DASI"]));
        public static bool EnableITL => Convert.ToBoolean(Convert.ToInt16(ConfigurationManager.AppSettings["ITL"]));
        public static bool EnableITR => Convert.ToBoolean(Convert.ToInt16(ConfigurationManager.AppSettings["ITR"]));
        public static bool EnableIQT => Convert.ToBoolean(Convert.ToInt16(ConfigurationManager.AppSettings["IQT"]));
        public static bool EnableMOZ => Convert.ToBoolean(Convert.ToInt16(ConfigurationManager.AppSettings["MOZ"]));
        public static bool EnableODG => Convert.ToBoolean(Convert.ToInt16(ConfigurationManager.AppSettings["ODG"]));


        #region REPORT

        public static string MOZ_UIDTemplateReportDCR => ConfigurationManager.AppSettings["MOZ_UIDTemplateReportDCR"];
        public static string ODG_UIDTemplateReportDCR => ConfigurationManager.AppSettings["ODG_UIDTemplateReportDCR"];
        
        public static string ITL_SCRITTA_UIDTemplateReportCopertinaPresidente => ConfigurationManager.AppSettings["ITL_SCRITTA_UIDTemplateReportCopertinaPresidente"];
        public static string ITL_SCRITTA_UIDTemplateReportCopertinaUfficio => ConfigurationManager.AppSettings["ITL_SCRITTA_UIDTemplateReportCopertinaUfficio"];
        public static string ITL_ORALE_UIDTemplateReportCopertinaPresidente => ConfigurationManager.AppSettings["ITL_ORALE_UIDTemplateReportCopertinaPresidente"];
        public static string ITL_ORALE_UIDTemplateReportCopertinaUfficio => ConfigurationManager.AppSettings["ITL_ORALE_UIDTemplateReportCopertinaUfficio"];
        public static string ITL_COMMISSIONE_UIDTemplateReportCopertinaPresidente => ConfigurationManager.AppSettings["ITL_COMMISSIONE_UIDTemplateReportCopertinaPresidente"];
        public static string ITL_COMMISSIONE_UIDTemplateReportCopertinaUfficio => ConfigurationManager.AppSettings["ITL_COMMISSIONE_UIDTemplateReportCopertinaUfficio"];
        public static string ITR_SCRITTA_UIDTemplateReportCopertinaPresidente => ConfigurationManager.AppSettings["ITR_SCRITTA_UIDTemplateReportCopertinaPresidente"];
        public static string ITR_SCRITTA_UIDTemplateReportCopertinaUfficio => ConfigurationManager.AppSettings["ITR_SCRITTA_UIDTemplateReportCopertinaUfficio"];
        public static string ITR_COMMISSIONE_UIDTemplateReportCopertinaPresidente => ConfigurationManager.AppSettings["ITR_COMMISSIONE_UIDTemplateReportCopertinaPresidente"];
        public static string ITR_COMMISSIONE_UIDTemplateReportCopertinaUfficio => ConfigurationManager.AppSettings["ITR_COMMISSIONE_UIDTemplateReportCopertinaUfficio"];
        public static string IQT_UIDTemplateReportCopertinaPresidente => ConfigurationManager.AppSettings["IQT_UIDTemplateReportCopertinaPresidente"];
        public static string IQT_UIDTemplateReportCopertinaUfficio => ConfigurationManager.AppSettings["IQT_UIDTemplateReportCopertinaUfficio"];
        public static string MOZ_UIDTemplateReportCopertinaPresidente => ConfigurationManager.AppSettings["MOZ_UIDTemplateReportCopertinaPresidente"];
        public static string MOZ_UIDTemplateReportCopertinaUfficio => ConfigurationManager.AppSettings["MOZ_UIDTemplateReportCopertinaUfficio"];
        public static string ODG_UIDTemplateReportCopertinaPresidente => ConfigurationManager.AppSettings["ODG_UIDTemplateReportCopertinaPresidente"];
        public static string ODG_UIDTemplateReportCopertinaUfficio => ConfigurationManager.AppSettings["ODG_UIDTemplateReportCopertinaUfficio"];
        public static string RIS_UIDTemplateReportCopertinaPresidente => ConfigurationManager.AppSettings["RIS_UIDTemplateReportCopertinaPresidente"];
        public static string RIS_UIDTemplateReportCopertinaUfficio => ConfigurationManager.AppSettings["RIS_UIDTemplateReportCopertinaUfficio"];

        public static string RIS_UIDTemplateReportLettera => ConfigurationManager.AppSettings["RIS_UIDTemplateReportLettera"];
        public static string MOZ_UIDTemplateReportLettera => ConfigurationManager.AppSettings["MOZ_UIDTemplateReportLettera"];
        public static string MOZ_COMMISSIONE_UIDTemplateReportLettera => ConfigurationManager.AppSettings["MOZ_COMMISSIONE_UIDTemplateReportLettera"];
        public static string ODG_UIDTemplateReportLettera => ConfigurationManager.AppSettings["ODG_UIDTemplateReportLettera"];
        public static string ITL_SCRITTA_UIDTemplateReportLettera => ConfigurationManager.AppSettings["ITL_SCRITTA_UIDTemplateReportLettera"];
        public static string ITL_ORALE_UIDTemplateReportLettera => ConfigurationManager.AppSettings["ITL_ORALE_UIDTemplateReportLettera"];
        public static string ITL_COMMISSIONE_UIDTemplateReportLettera => ConfigurationManager.AppSettings["ITL_COMMISSIONE_UIDTemplateReportLettera"];
        public static string ITR_COMMISSIONE_UIDTemplateReportLettera => ConfigurationManager.AppSettings["ITR_COMMISSIONE_UIDTemplateReportLettera"];
        public static string ITR_SCRITTA_UIDTemplateReportLettera => ConfigurationManager.AppSettings["ITR_SCRITTA_UIDTemplateReportLettera"];


        #endregion
    }
}