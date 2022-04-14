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

namespace PortaleRegione.BAL
{
    public class AppSettingsConfiguration
    {
        public const int GIUNTA_REGIONALE_ID = 10000;

        public static string CartellaTemp => ConfigurationManager.AppSettings["CartellaTemp"];
        public static string JWT_MASTER => ConfigurationManager.AppSettings["JWT_MASTER"];
        public static double JWT_EXPIRATION => Convert.ToDouble(ConfigurationManager.AppSettings["JWT_EXPIRATION"]);

        public static string TOKEN_R => ConfigurationManager.AppSettings["TOKEN_R"];
        public static string TOKEN_W => ConfigurationManager.AppSettings["TOKEN_W"];
        public static string MasterPIN => ConfigurationManager.AppSettings["MasterPIN"];
        public static string masterKey => ConfigurationManager.AppSettings["masterKey"];
        public static string urlPEM => ConfigurationManager.AppSettings["URLPEM"];
        public static string URL_API => ConfigurationManager.AppSettings["URL_API"];
        public static string SMTP => ConfigurationManager.AppSettings["SMTP"];
        public static bool Invio_Notifiche => Convert.ToBoolean(Convert.ToInt16(ConfigurationManager.AppSettings["InvioNotifiche"]));
        public static string FirmaUfficio => ConfigurationManager.AppSettings["UtenteFirmaUfficio"];
        public static string LimiteFirmaMassivo => ConfigurationManager.AppSettings["LimiteFirmaMassivo"];
        public static string LimiteDepositoMassivo => ConfigurationManager.AppSettings["LimiteDepositoMassivo"];
        public static string GiorniValiditaLink => ConfigurationManager.AppSettings["GiorniValiditaLink"];
        public static string AbilitaOpenData => ConfigurationManager.AppSettings["AbilitaOpenData"];
        public static string OpenData_PrivateToken => ConfigurationManager.AppSettings["OpenData_PrivateToken"];
        public static string OpenData_Separatore => ConfigurationManager.AppSettings["OpenData_Separatore"];

        public static string TestoEMCartaceo => ConfigurationManager.AppSettings["TestoEMCartaceo"];

        //Service JOBBER
        public static string Service_Username => ConfigurationManager.AppSettings["Service_Username"];
        public static string Service_Password => ConfigurationManager.AppSettings["Service_Password"];

        //STAMPE
        public static string CartellaLavoroStampe => ConfigurationManager.AppSettings["CartellaLavoroStampe"];
        public static string LimiteGeneraStampaImmediata => ConfigurationManager.AppSettings["LimiteGeneraStampaImmediata"];
        public static string MessaggioInizialeDeposito => ConfigurationManager.AppSettings["MessaggioInizialeDeposito"];
        public static string MessaggioInizialeInvito => ConfigurationManager.AppSettings["MessaggioInizialeInvito"];
        public static string urlPEM_ViewEM => ConfigurationManager.AppSettings["urlPEM_ViewEM"];
        public static string urlPEM_RiepilogoEM => ConfigurationManager.AppSettings["urlPEM_RiepilogoEM"];

        //FILE
        public static string RootRepository => ConfigurationManager.AppSettings["RootRepository"];
        public static string PrefissoCompatibilitaDocumenti => ConfigurationManager.AppSettings["PrefissoCompatibilitaDocumenti"];
        public static string PercorsoCompatibilitaDocumenti => ConfigurationManager.AppSettings["PercorsoCompatibilitaDocumenti"];
        public static string urlDASI_ViewATTO => ConfigurationManager.AppSettings["urlDASI_ViewATTO"];
    }
}