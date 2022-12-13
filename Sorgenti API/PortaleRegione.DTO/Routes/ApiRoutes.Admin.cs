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
    public static partial class ApiRoutes
    {
        public static class Admin
        {
            // api/admin
            private const string Base = ApiRoutes.Root + "/admin";

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
    }
}