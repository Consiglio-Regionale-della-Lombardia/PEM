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
        public static string GEA_Url => ConfigurationManager.AppSettings["GEA_Url"];
        public static string GEA_Username => ConfigurationManager.AppSettings["GEA_Username"];
        public static string GEA_Password => ConfigurationManager.AppSettings["GEA_Password"];

        /*INTEGRAZIONE EDMA
         * Tutte le chiavi EDMA_* sono nella sezione custom <edmaSettings> del
         * Web.config, popolata via configSource="Edma.config" (file gitignored,
         * template in Edma.config.example). Vedi EdmaSettings.cs. */

        // -- Connessione EDMA --
        public static string EDMA_Url => EdmaSettings.Get("EDMA_Url");
        public static string EDMA_Username => EdmaSettings.Get("EDMA_Username");
        public static string EDMA_Password => EdmaSettings.Get("EDMA_Password");
        public static bool EDMA_NoSession => EdmaSettings.GetBool("EDMA_NoSession", true);
        public static int EDMA_TimeoutSeconds => EdmaSettings.GetInt("EDMA_TimeoutSeconds", 300);
        public static string EDMA_EdmaWebUrl => EdmaSettings.Get("EDMA_EdmaWebUrl");

        // -- Identita' autore e mittente Pratica --
        public static string EDMA_CodAutore => EdmaSettings.GetOrDefault("EDMA_CodAutore", "SYSTEM_GEDASI");
        public static string EDMA_CF_Consiglio => EdmaSettings.Get("EDMA_CF_Consiglio");
        public static string EDMA_DescrizioneSoggettoPratica => EdmaSettings.GetOrDefault("EDMA_DescrizioneSoggettoPratica", "Consiglio Regionale della Lombardia");

        // -- Metadocumenti --
        public static string EDMA_CodiceMetadocumento_Atto => EdmaSettings.GetOrDefault("EDMA_CodiceMetadocumento_Atto", "GEDASI_FILE");
        public static string EDMA_CodiceMetadocumento_Allegato => EdmaSettings.GetOrDefault("EDMA_CodiceMetadocumento_Allegato", "GEDASI_ALLEGATO");
        public static string EDMA_CodiceMetadocumento_Pratica => EdmaSettings.GetOrDefault("EDMA_CodiceMetadocumento_Pratica", "FascicoloPratica");

        // -- Sotto-fascicoli per tipologia atto --
        public static string EDMA_Sottofascicolo_ITL_Titolario => EdmaSettings.Get("EDMA_Sottofascicolo_ITL_Titolario");
        public static string EDMA_Sottofascicolo_ITL_IdEdma => EdmaSettings.Get("EDMA_Sottofascicolo_ITL_IdEdma");
        public static string EDMA_Sottofascicolo_ITR_Titolario => EdmaSettings.Get("EDMA_Sottofascicolo_ITR_Titolario");
        public static string EDMA_Sottofascicolo_ITR_IdEdma => EdmaSettings.Get("EDMA_Sottofascicolo_ITR_IdEdma");
        public static string EDMA_Sottofascicolo_MOZ_Titolario => EdmaSettings.Get("EDMA_Sottofascicolo_MOZ_Titolario");
        public static string EDMA_Sottofascicolo_MOZ_IdEdma => EdmaSettings.Get("EDMA_Sottofascicolo_MOZ_IdEdma");
        public static string EDMA_Sottofascicolo_ODG_Titolario => EdmaSettings.Get("EDMA_Sottofascicolo_ODG_Titolario");
        public static string EDMA_Sottofascicolo_ODG_IdEdma => EdmaSettings.Get("EDMA_Sottofascicolo_ODG_IdEdma");
        public static string EDMA_Sottofascicolo_IQT_Titolario => EdmaSettings.Get("EDMA_Sottofascicolo_IQT_Titolario");
        public static string EDMA_Sottofascicolo_IQT_IdEdma => EdmaSettings.Get("EDMA_Sottofascicolo_IQT_IdEdma");
        public static string EDMA_Sottofascicolo_RIS_Titolario => EdmaSettings.Get("EDMA_Sottofascicolo_RIS_Titolario");
        public static string EDMA_Sottofascicolo_RIS_IdEdma => EdmaSettings.Get("EDMA_Sottofascicolo_RIS_IdEdma");

        // -- Pratica - istruttore e parametri --
        public static string EDMA_Istruttore_CodPersona => EdmaSettings.Get("EDMA_Istruttore_CodPersona");
        public static string EDMA_Responsabile_CodPersona => EdmaSettings.Get("EDMA_Responsabile_CodPersona");
        public static int EDMA_AnniConservazione_Pratica => EdmaSettings.GetInt("EDMA_AnniConservazione_Pratica", 10);
        public static string EDMA_DataChiusura_Pratica => EdmaSettings.GetOrDefault("EDMA_DataChiusura_Pratica", "09/09/2099");

        // -- Protocollazione --
        public static bool EDMA_UseProtocollazioneApplicativa => EdmaSettings.GetBool("EDMA_UseProtocollazioneApplicativa", true);
        public static string EDMA_StrutturaProtocollante => EdmaSettings.Get("EDMA_StrutturaProtocollante");
        public static string EDMA_TipoProtocollo => EdmaSettings.GetOrDefault("EDMA_TipoProtocollo", "Arrivo");
        public static int EDMA_FlagRiscontro => EdmaSettings.GetInt("EDMA_FlagRiscontro", 1);
        public static string EDMA_MezzoSpedizione => EdmaSettings.GetOrDefault("EDMA_MezzoSpedizione", "Posta interna");
        public static string EDMA_TipoDocumento => EdmaSettings.GetOrDefault("EDMA_TipoDocumento", "Atto DASI");
        public static string EDMA_CodiceEnteCompetente_Destinatario_Competenza => EdmaSettings.GetOrDefault("EDMA_CodiceEnteCompetente_Destinatario_Competenza", "CRA0060102");
        public static string EDMA_CodiceEnteCompetente_Destinatari_PerConoscenza => EdmaSettings.GetOrDefault("EDMA_CodiceEnteCompetente_Destinatari_PerConoscenza", string.Empty);

        // -- Resilienza e modalita' degradata --
        public static int EDMA_RetryMaxAttempts => EdmaSettings.GetInt("EDMA_RetryMaxAttempts", 5);
        public static string EDMA_RetryBackoffSeconds => EdmaSettings.GetOrDefault("EDMA_RetryBackoffSeconds", "60,180,600,1800,3600");
        public static bool EDMA_UseFallbackEmail => EdmaSettings.GetBool("EDMA_UseFallbackEmail", false);

        // -- Feature flag UI --
        public static bool EDMA_AbilitaEditManualeProtocollo => EdmaSettings.GetBool("EDMA_AbilitaEditManualeProtocollo", true);
        public static bool EDMA_AbilitaAnnulloProtocollo => EdmaSettings.GetBool("EDMA_AbilitaAnnulloProtocollo", false);
    }
}
