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

namespace PortaleRegione.DTO.Routes
{
    public static class ApiRoutes
    {
        private const string Root = "api";

        public static class Admin
        {
            // api/admin
            private const string Base = Root + "/admin";

            public const string GetPersona = Base + "/persone/{id}";
            public const string GetUtenti = Base + "/persone/all";
            public const string ResetPassword = Base + "/reset-password";
            public const string ResetPin = Base + "/reset-pin";
            public const string GetRuoliAD = Base + "/ad/ruoli";
            public const string GetGruppiPoliticiAD = Base + "/ad/gruppi-politici";
            public const string GetGruppiInDb = Base + "/gruppi/gruppi-in-db";
            public const string SalvaUtente = Base + "/persone/salva";
            public const string EliminaUtente = Base + "/persone/{id}/elimina";
            public const string SalvaGruppo = Base + "/gruppi/salva-gruppo";
            public const string GetGruppi = Base + "/gruppi/all";
        }

        public static class Autenticazione
        {
            // api/auth
            private const string Base = Root + "/auth";

            public const string Login = Base + "/login";
            public const string CambioRuolo = Base + "/cambio-ruolo/{ruolo}";
            public const string CambioGruppo = Base + "/cambio-gruppo/{gruppo}";
        }

        public static class PEM
        {
            // api/pem
            private const string Base = Root + "/pem";

            public const string GetAllDestinatari = Base + "/destinatari/{atto}/{tipo}";
            public const string InserisciStampaDifferita = Base + "/inserisci-stampa-differita";

            public static class Sedute
            {
                // api/pem/sedute
                private const string Base = PEM.Base + "/sedute";

                public const string GetAll = Base + "/all";
                public const string Get = Base + "/{id}";
                public const string GetAttive = Base + "/attive";
                public const string GetAttiveMOZU = Base + "/attive/mozioni-urgenti";
                public const string GetAttiveDashboard = Base + "/attive/dashboard";
                public const string Delete = Base + "/{id}";
                public const string Create = Base + "/create";
                public const string Edit = Base + "/edit";

            }

            public static class Atti
            {
                // api/pem/atti
                private const string Base = PEM.Base + "/atti";

                public const string GetAll = Base + "/all";
                public const string Get = Base + "/{id}";
                public const string Delete = Base + "/{id}";
                public const string Create = Base + "/create";
                public const string Edit = Base + "/edit";
                public const string ModificaFascicoli = Base + "/modifica-fascicoli";
                public const string DownloadDoc = Base + "/scarica-documento/{path}";
                public const string GrigliaTesti = Base + "/{id}/griglia-testi/{view}";
                public const string GrigliaOrdinamento = Base + "/{id}/griglia-ordinamento";
                public const string AggiornaRelatori = Base + "/aggiorna-relatori";
                public const string AggiornaTesto = Base + "/aggiorna-testo";
                public const string AbilitaFascicolo = Base + "/abilita-fascicolo";
                public const string BloccoODG = Base + "/odg/blocca";
                public const string JollyODG = Base + "/odg/jolly";
                public const string SpostaUp = Base + "/{id}/sposta/up";
                public const string SpostaDown = Base + "/{id}/sposta/down";
                public const string GetTipi = Base + "/tipi/{dasi}";

                public static class Articoli
                {
                    // api/pem/atti/articoli
                    private const string Base = Atti.Base + "/articoli";

                    public const string GetAll = Base + "/all/{id}";
                    public const string Create = Base + "/create/{id}/{articoli}";
                    public const string Delete = Base + "/{id}";
                }

                public static class Commi
                {
                    // api/pem/atti/commi
                    private const string Base = Atti.Base + "/commi";

                    public const string GetAll = Base + "/all/{id}/{expanded}";
                    public const string Create = Base + "/create/{id}/{commi}";
                    public const string Delete = Base + "/{id}";
                }

                public static class Lettere
                {
                    // api/pem/atti/lettere
                    private const string Base = Atti.Base + "/lettere";

                    public const string GetAll = Base + "/all/{id}";
                    public const string Create = Base + "/create/{id}/{lettere}";
                    public const string Delete = Base + "/{id}";
                }
            }

            public static class Emendamenti
            {
                private const string Base = PEM.Base + "/emendamenti";

                public const string Get = Base + "/{id}";
                public const string Delete = Base + "/{id}";
                public const string Elimina = Base + "/{id}/elimina";
                public const string GetNuovoModello = Base + "/create/model/{id}/{sub_id}";
                public const string Create = Base + "/create";
                public const string GetModificaModello = Base + "/edit/model/{id}";
                public const string GetModificaModelloMetaDati = Base + "/edit/model/{id}/meta";
                public const string AggiornaMetaDati = Base + "/edit/model/meta";
                public const string Edit = Base + "/edit";
                public const string GetAll = Base + "/all";
                public const string GetAllRichiestaPropriaFirma = Base + "/all/richiesta-firma";
                public const string DownloadDoc = Base + "/scarica-documento/{path}";

                public const string GetFirmatari = Base + "/{id}/firme/{tipo}";
                public const string GetInvitati = Base + "/{id}/invitati";

                public const string GetBody = Base + "/get-corpo";
                public const string GetBodyCopertina = Base + "/get-copertina";

                public const string Firma = Base + "/commands/firma";
                public const string RitiroFirma = Base + "/commands/firma/ritiro";
                public const string EliminaFirma = Base + "/commands/firma/elimina";

                public const string Deposita = Base + "/commands/deposita";
                public const string Ritira = Base + "/commands/ritiro";

                public const string ModificaStato = Base + "/commands/stato";
                public const string AssegnaNuovoProponente = Base + "/commands/nuovo-proponente";
                public const string Raggruppa = Base + "/commands/raggruppa";
                public const string Ordina = Base + "/commands/{id}/ordina";
                public const string OrdinamentoConcluso = Base + "/commands/ordina/concluso";
                public const string OrdinaUp = Base + "/commands/{id}/ordina/up";
                public const string OrdinaDown = Base + "/commands/{id}/ordina/down";
                public const string Sposta = Base + "/commands/{id}/sposta/{pos}";

                public const string GetParti = Base + "/parti";
                public const string GetTipi = Base + "/tipi";
                public const string GetTags = Base + "/tags";
                public const string GetMissioni = Base + "/missioni";
                public const string GetTitoliMissioni = Base + "/titoli-missioni";
                public const string GetStati = Base + "/stati";
                public const string StampaImmediata = Base + "/{id}/stampa-immediata";
            }
        }

        public static class DASI
        {
            // api/dasi
            private const string Base = Root + "/dasi";

            public const string GetAllDestinatari = Base + "/destinatari/{tipo}";

            public const string InserisciStampaDifferita = Base + "/inserisci-stampa-differita";

            public const string GetNuovoModello = Base + "/create/model/{tipo}";
            public const string GetModificaModello = Base + "/edit/model/{id}";
            public const string AggiornaMetaDati = Base + "/edit/model/meta";
            public const string Save = Base + "/save";
            public const string Get = Base + "/{id}";
            public const string GetAll = Base + "/all";
            public const string GetAllCartacei = Base + "/all/cartacei";

            public const string Firma = Base + "/commands/firma";
            public const string RitiroFirma = Base + "/commands/firma/ritiro";
            public const string EliminaFirma = Base + "/commands/firma/elimina";
            public const string Presenta = Base + "/commands/presenta";
            public const string Ritira = Base + "/{id}/commands/ritiro";
            public const string ModificaStato = Base + "/commands/stato";
            public const string IscriviSeduta = Base + "/commands/iscrizione/iscrivi";
            public const string RichiediIscrizione = Base + "/commands/iscrizione/richiedi";
            public const string RimuoviRichiestaIscrizione = Base + "/commands/iscrizione/rimuovi";
            public const string RimuoviSeduta = Base + "/commands/seduta/rimuovi";
            public const string ProponiUrgenzaMozione = Base + "/commands/proponi-urgenza";
            public const string ProponiMozioneAbbinata = Base + "/commands/proponi-abbinata";
            public const string PresentazioneCartacea = Base + "/commands/presentazione-cartacea";


            public const string Delete = Base + "/{id}";
            public const string Elimina = Base + "/{id}/elimina";

            public const string GetFirmatari = Base + "/{id}/firme/{tipo}";
            public const string GetBody = Base + "/get-corpo";
            public const string GetBodyCopertina = Base + "/get-copertina";
            public const string DownloadDoc = Base + "/scarica-documento/{path}";

            public const string GetInvitati = Base + "/{id}/invitati";
            public const string GetStati = Base + "/stati";
            public const string GetTipiMOZ = Base + "/tipi/mozioni";
            public const string GetSoggettiInterrogabili = Base + "/soggetti-interrogabili";

            public const string GetMOZAbbinabili = Base + "/mozioni/abbinabili";
            public const string DeclassaMozione = Base + "/mozioni/declassa";

            public const string GetAttiSeduteAttive = Base + "/ordini-del-giorno/sedute/attive";

            public const string StampaImmediata = Base + "/{id}/stampa-immediata";
            public const string StampaImmediataPrivacy = Base + "/{id}/stampa-immediata-privacy";
            public const string InviaAlProtocollo = Base + "/{id}/invia-al-protocollo";
            public const string SaveCartaceo = Base + "/save-draft";
        }

        public static class Notifiche
        {
            // api/notifiche
            private const string Base = Root + "/notifiche";

            public const string GetInviate = Base + "/inviate";
            public const string GetRicevute = Base + "/ricevute";
            public const string NotificaVista = Base + "/{id}/vista";
            public const string GetDestinatari = Base + "/{id}/destinatari";
            public const string InvitoAFirmare = Base + "/{id}/invito-a-firmare";
            public const string AccettaPropostaFirma = Base + "/{id}/accetta-proposta-firma";
            public const string AccettaRitiroFirma = Base + "/{id}/accetta-ritiro-firma";
            public const string Archivia = Base + "/archivia";

        }

        public static class Gruppi
        {
            // api/gruppi
            private const string Base = Root + "/gruppi";

            public const string GetAll = Base + "/all";
            public const string GetCapoGruppo = Base + "/{id}/capo-gruppo";
            public const string GetSegreteriaPoliticaGruppo = Base + "/{id}/segreteria-politica/{firma}/{deposito}";
            public const string GetSegreteriaGiunta = Base + "/segreteria-giunta-regionale/{firma}/{deposito}";
            public const string GetGiunta = Base + "/giunta";
            public const string GetAssessori = Base + "/assessori";
            public const string GetRelatori = Base + "/relatori/{id}";
        }

        public static class Legislature
        {
            // api/legislature
            private const string Base = Root + "/legislatura";

            public const string GetAll = Base + "/all";
            public const string Get = Base + "/{id}";
        }

        public static class Esporta
        {
            // api/legislature
            private const string Base = Root + "/esporta";

            public const string EsportaGrigliaZip = Base + "/dasi/griglia-zip";
            public const string EsportaGrigliaExcelDasi = Base + "/dasi/griglia-xlsx";
            public const string EsportaGrigliaExcel = Base + "/pem/griglia-xls";
            public const string EsportaGrigliaExcelUOLA = Base + "/pem/griglia-xls-uola";
            public const string EsportaGrigliaWord = Base + "/pem/griglia-doc/{id}/{ordine}/{mode}";
        }

        public static class Persone
        {
            // api/persone
            private const string Base = Root + "/persone";

            public const string GetAll = Base + "/all";
            public const string GetPersona = Base + "/{id}/{is_giunta}";
            public const string CheckPin = Base + "/check-pin";
            public const string CambioPin = Base + "/cambio-pin";
            public const string ResetPin = Base + "/reset-pin";
        }

        public static class Public
        {
            // api/public
            private const string Base = Root + "/public";

            public const string ViewEM = Base + "/{id}";
        }

        public static class Ruoli
        {
            // api/ruoli
            private const string Base = Root + "/ruoli";

            public const string GetRuolo = Base + "/{id}";
        }

        public static class Stampe
        {
            // api/stampe
            private const string Base = Root + "/stampe";

            public const string GetAll = Base + "/all";
            public const string Get = Base + "/{id}";
            public const string Delete = Base + "/{id}";
            public const string Download = Base + "/{id}/download";
            public const string Reset = Base + "/{id}/reset";
            public const string AddInfo = Base + "/{id}/info/add/{message}";
            public const string GetInfo = Base + "/{id}/info";
            public const string GetAllInfo = Base + "/all/info";
        }

        public static class Util
        {
            // api

            public const string InvioMail = Root + "/invio-mail";
            public const string UploadDocument = Root + "/upload-document";
        }

        public static class Job
        {
            // api/job
            private const string Base = Root + "/job";

            public static class Stampe
            {
                // api/job/stampe
                private const string Base = Job.Base + "/stampe";

                public const string GetAll = Base + "/all";
                public const string Unlock = Base + "/unlock";
                public const string ReportError = Base + "/report-error";
                public const string UpdateFileStampa = Base + "/update-file";
                public const string SetInvioStampa = Base + "/report-invio";
                public const string GetEmendamenti = Base + "/emendamenti";
                public const string GetAtti = Base + "/atti";
            }
        }
    }
}