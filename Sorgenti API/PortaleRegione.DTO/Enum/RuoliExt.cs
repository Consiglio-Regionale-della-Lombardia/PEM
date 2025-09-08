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

namespace PortaleRegione.DTO.Enum
{
    public static class RuoliExt
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
        public const string Segreteria_Assemblea_Read = "13"; // #1035

        public static string ConvertToAD(RuoliIntEnum ruolo)
        {
            switch (ruolo)
            {
                case RuoliIntEnum.Amministratore_PEM:
                    return "PEM_Admin";
                case RuoliIntEnum.Amministratore_Giunta:
                    return "PEM_Admin_Giunta";
                case RuoliIntEnum.Consigliere_Regionale:
                    return "PEM_Consiglieri";
                case RuoliIntEnum.Responsabile_Segreteria_Politica:
                    return "PEM_Resp_Segreteria";
                case RuoliIntEnum.Segreteria_Politica:
                    return "PEM_Segreteria_politica";
                case RuoliIntEnum.Assessore_Sottosegretario_Giunta:
                    return "PEM_Assessori";
                case RuoliIntEnum.Responsabile_Segreteria_Giunta:
                    return "PEM_Resp_Segreteria_Giunta";
                case RuoliIntEnum.Segreteria_Giunta_Regionale:
                    return "PEM_Segreteria_Giunta";
                case RuoliIntEnum.Presidente_Regione:
                    return "PEM_Presidente";
                case RuoliIntEnum.Segreteria_Assemblea:
                    return "PEM_Segr_Assemblea";
                case RuoliIntEnum.Utente:
                    return "PEM_Generic";
                case RuoliIntEnum.Segreteria_Assemblea_Read: // #1035
                    return "PEM_Segr_Assemblea_read";
                case RuoliIntEnum.SERVIZIO_JOB:
                    return default;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ruolo), ruolo, null);
            }
        }
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
        SERVIZIO_JOB = 99,
        Segreteria_Assemblea_Read = 12 // #1035
    }
}