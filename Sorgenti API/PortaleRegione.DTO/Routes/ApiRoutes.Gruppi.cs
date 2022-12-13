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
    }
}