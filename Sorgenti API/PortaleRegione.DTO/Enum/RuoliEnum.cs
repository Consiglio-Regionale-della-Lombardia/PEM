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

namespace PortaleRegione.DTO.Enum
{
    public static class RuoliEnum
    {
        public const string Amministratore_PEM = "1";
        public const string Amministratore_Giunta = "2";
        public const string Consigliere_Regionale = "3";
        public const string Responsabile_Segreteria_Politica = "4";
        public const string Segreteria_Politica = "5";
        public const string Assessore_Sottosegretario_Giunta = "6";
        public const string Responsabile_Segreteria_Giunta = "7";
        public const string Segreteria_Giunta_Regionale = "8";
        public const string Presidente_Regione = "9";
        public const string Segreteria_Assemblea = "10";
        public const string Utente = "11";
        public const string SERVIZIO_JOB = "12";
    }

    public enum RuoliIntEnum
    {
        Amministratore_PEM = 1,
        Amministratore_Giunta = 2,
        Consigliere_Regionale = 3,
        Responsabile_Segreteria_Politica = 4,
        Segreteria_Politica = 5,
        Assessore_Sottosegretario_Giunta = 6,
        Responsabile_Segreteria_Giunta = 7,
        Segreteria_Giunta_Regionale = 8,
        Presidente_Regione = 9,
        Segreteria_Assemblea = 10,
        Utente = 11,
        SERVIZIO_JOB = 12
    }
}