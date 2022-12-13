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
            private const string Base = Root + "/auth";

            public const string Login = Base + "/login";
            public const string CambioRuolo = Base + "/cambio-ruolo/{ruolo}";
            public const string CambioGruppo = Base + "/cambio-gruppo/{gruppo}";
        }

        public static class Public
        {
            private const string Base = Root + "/public";

            public const string ViewEM = Base + "/{id}";
        }

        public static class DASI
        {
            private const string Base = Root + "/dasi";

            public const string EsportaGrigliaExcel = Base + "/esporta-griglia-xls";
            public const string GetAllDestinatari = Base + "/destinatari/{tipo}";
        }

        public static class PEM
        {
            private const string Base = Root + "/pem";

            public const string EsportaGrigliaExcel = Base + "/esporta-griglia-xls";
            public const string EsportaGrigliaExcelUOLA = Base + "/esporta-griglia-xls-uola";
            public const string EsportaGrigliaWord = Base + "/esporta-griglia-doc/{id}/{ordine}/{mode}";

            public const string GetAllDestinatari = Base + "/destinatari/{atto}/{tipo}";
        }

        public static class Legislature
        {
            private const string Base = Root + "/legislatura";

            public const string GetAll = Base + "/all";
            public const string Get = Base + "/{id}";
        }

        public static class Notifiche
        {
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
            private const string Base = Root + "/persone";

            public const string GetAll = Base + "/all";
            public const string GetPersona = Base + "/{id}/{is_giunta}";
            public const string CheckPin = Base + "/check-pin";
            public const string CambioPin = Base + "/cambio-pin";
            public const string ResetPin = Base + "/reset-pin";

        }

        public static class Gruppi
        {
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
            private const string Base = Root + "/ruoli";

            public const string GetRuolo = Base + "/{id}";

        }
    }
}