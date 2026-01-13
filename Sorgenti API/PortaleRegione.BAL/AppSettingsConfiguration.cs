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
        public static string url_CLIENT => ConfigurationManager.AppSettings["URLPEM"];
        public static string URL_API => ConfigurationManager.AppSettings["URL_API"];
        public static string SMTP => ConfigurationManager.AppSettings["SMTP"];

        public static int AutenticazioneAD => Convert.ToInt16(ConfigurationManager.AppSettings["AutenticazioneAD"]);
        public static int Invio_Notifiche => Convert.ToInt16(ConfigurationManager.AppSettings["InvioNotifiche"]);
        public static string FirmaUfficio => ConfigurationManager.AppSettings["FirmaUfficio"];
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
        public static string EmailFrom => ConfigurationManager.AppSettings["EmailFrom"];

        //STAMPE
        public static string Logo => ConfigurationManager.AppSettings["Logo"];
        public static string Titolo => ConfigurationManager.AppSettings["Titolo"];
        public static string NomePiattaforma => ConfigurationManager.AppSettings["NomePiattaforma"];
        public static string CartellaLavoroStampe => ConfigurationManager.AppSettings["CartellaLavoroStampe"];
        public static string LimiteGeneraStampaImmediata => ConfigurationManager.AppSettings["LimiteGeneraStampaImmediata"];
        public static int LimiteEmendamentiFascicoloWord => Convert.ToInt32(ConfigurationManager.AppSettings["LimiteEmendamentiFascicoloWord"] ?? "1000");
        public static string MessaggioInizialeDeposito => ConfigurationManager.AppSettings["MessaggioInizialeDeposito"];
        public static string MessaggioInizialeInvito => ConfigurationManager.AppSettings["MessaggioInizialeInvito"];
        public static string urlPEM_ViewEM => ConfigurationManager.AppSettings["urlPEM_ViewEM"];
        public static string urlPEM_RiepilogoEM => ConfigurationManager.AppSettings["urlPEM_RiepilogoEM"];

        //FILE
        public static string RootRepository => ConfigurationManager.AppSettings["RootRepository"];
        public static string PrefissoCompatibilitaDocumenti => ConfigurationManager.AppSettings["PrefissoCompatibilitaDocumenti"];
        public static string PercorsoCompatibilitaDocumenti => ConfigurationManager.AppSettings["PercorsoCompatibilitaDocumenti"];
        public static string urlDASI_ViewATTO => ConfigurationManager.AppSettings["urlDASI_ViewATTO"];

        //DASI
        public static string EmailInvioDASI => ConfigurationManager.AppSettings["EmailInvioDASI"];
        public static string EmailProtocolloDASI => ConfigurationManager.AppSettings["EmailProtocolloDASI"];
        public static string LimitePresentazioneMassivo => ConfigurationManager.AppSettings["LimitePresentazioneMassivo"];
        public static int MinimoConsiglieriIQT => Convert.ToInt16(ConfigurationManager.AppSettings["MinimoConsiglieriIQT"]);
        public static int MinimoConsiglieriMOZU => Convert.ToInt16(ConfigurationManager.AppSettings["MinimoConsiglieriMOZU"]);
        public static int MinimoConsiglieriMOZC_MOZS => Convert.ToInt16(ConfigurationManager.AppSettings["MinimoConsiglieriMOZC_MOZS"]);
        public static int MassimoODG => Convert.ToInt16(ConfigurationManager.AppSettings["MassimoODG"]);
        public static int MassimoODG_DuranteSeduta => Convert.ToInt16(ConfigurationManager.AppSettings["MassimoODG_DuranteSeduta"]);
        public static int MassimoODG_Jolly => Convert.ToInt16(ConfigurationManager.AppSettings["MassimoODG_Jolly"]);
        
        /*INTEGRAZIONE GEA*/
        public static string GEA_Username => ConfigurationManager.AppSettings["GEA_Username"];
        public static string GEA_Password => ConfigurationManager.AppSettings["GEA_Password"];
    }
}
