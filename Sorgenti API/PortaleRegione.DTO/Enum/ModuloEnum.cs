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
    /// <summary>
    ///     Discrimina i moduli applicativi a cui appartengono entita' trasversali
    ///     come le richieste di stampa o i filtri preferiti.
    ///     Sostituisce il vecchio ModuloStampaEnum, ora rimosso.
    ///
    ///     Il backing type e' esplicitamente byte: la colonna FILTRI.Modulo e'
    ///     tinyint su SQL Server e EF6 richiede che il tipo CLR dell'enum
    ///     combaci (altrimenti errore di mapping "type ModuloEnum is not
    ///     compatible with SqlServer.tinyint"). Per gli altri consumer (es.
    ///     NuovaStampaRequest) il cambio di backing type e' trasparente.
    /// </summary>
    public enum ModuloEnum : byte
    {
        PEM = 1,
        DASI = 2
    }
}
