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

namespace PortaleRegione.Api.Public
{
    public static class ApiRoutes
    {
        private const string Root = "api";

        public const string Test = Root + "/test";
        public const string GetLegislature = Root + "/legislature";
        public const string GetTipi = Root + "/tipi";
        public const string GetTipiRisposte = Root + "/tipi/risposte";
        public const string GetStati = Root + "/stati";
        public const string GetGruppi = Root + "/gruppi";
        public const string GetFirmatari = Root + "/{legislatura}/firmatari";
        public const string GetCaricheGiunta = Root + "/{legislatura}/cariche";
        public const string GetCommissioni = Root + "/{legislatura}/commissioni";
        public const string GetAtto = Root + "/atto/{id}";
        public const string GetSearch = Root + "/cerca";
    }
}