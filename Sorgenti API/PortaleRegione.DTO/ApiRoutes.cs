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

namespace PortaleRegione.DTO
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

        public static class Public
        {
            // api/public
            private const string Base = Root + "/public";

            public const string ViewEM = Base + "/{id}";
        }

        public static class DASI
        {
            // api/dasi
            private const string Base = Root + "/dasi";

            public const string EsportaGrigliaExcel = Base + "/esporta-griglia-xls";
            public const string GetAllDestinatari = Base + "/destinatari/{tipo}";
        }

        public static class PEM
        {
            // api/pem
            private const string Base = Root + "/pem";

            public const string EsportaGrigliaExcel = Base + "/esporta-griglia-xls";
            public const string EsportaGrigliaExcelUOLA = Base + "/esporta-griglia-xls-uola";
            public const string EsportaGrigliaWord = Base + "/esporta-griglia-doc/{id}/{ordine}/{mode}";

            public const string GetAllDestinatari = Base + "/destinatari/{atto}/{tipo}";

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
        }

        public static class Legislature
        {
            // api/legislature
            private const string Base = Root + "/legislatura";

            public const string GetAll = Base + "/all";
            public const string Get = Base + "/{id}";
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

        public static class Ruoli
        {
            // api/ruoli
            private const string Base = Root + "/ruoli";

            public const string GetRuolo = Base + "/{id}";
        }
    }
}