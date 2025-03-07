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

namespace PortaleRegione.DTO.Model
{
    public class CountBarData
    {
        public int ITL { get; set; } = 0;
        public int ITR { get; set; } = 0;
        public int IQT { get; set; } = 0;
        public int MOZ { get; set; } = 0;
        public int ODG { get; set; } = 0;
        public int RIS { get; set; } = 0;
        public int TUTTI { get; set; } = 0;
        public int BOZZE { get; set; } = 0;
        public int PRESENTATI { get; set; } = 0;
        public int IN_TRATTAZIONE { get; set; } = 0;
        public int CHIUSO { get; set; } = 0;
    }
}