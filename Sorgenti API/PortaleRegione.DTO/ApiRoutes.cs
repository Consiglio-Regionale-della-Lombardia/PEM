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
            public const string GetPersona = Base + "/users/{id}";
            public const string GetUtenti = Base + "/users";
            public const string ResetPassword = Base + "/reset-password";
            public const string ResetPin = Base + "/reset-pin";
            public const string GetRuoliAD = Base + "/ad/ruoli";
            public const string GetGruppiPoliticiAD = Base + "/ad/gruppi-politici";
            public const string GetGruppiInDb = Base + "/groups/gruppi-in-db";
            public const string SalvaUtente = Base + "/users/salva";
            public const string EliminaUtente = Base + "/users/{id}/elimina";
            public const string SalvaGruppo = Base + "/groups/salva-gruppo";
            public const string GetGruppi = Base + "/groups";
        }

        public static class Autenticazione
        {
            private const string Base = Root + "/auth";
            public const string Login = Base + "/login";
            public const string CambioRuolo = Base + "/cambio-ruolo/{ruolo}";
            public const string CambioGruppo = Base + "/cambio-gruppo/{gruppo}";
        }
    }
}