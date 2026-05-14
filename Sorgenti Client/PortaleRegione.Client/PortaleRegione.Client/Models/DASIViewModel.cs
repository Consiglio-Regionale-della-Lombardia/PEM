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

using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Response;

namespace PortaleRegione.Client.Models
{
    public class DASIViewModel
    {
        public BaseResponse<AttiDto> Data { get; set; }
        public int ITR { get; set; }
        public int IQT { get; set; }
        public int ITL { get; set; }
        public int MOZ { get; set; }
        public int MOZ_U { get; set; }
        public int MOZ_A { get; set; }
        public int MOZ_C { get; set; }
        public int MOZ_S { get; set; }
        public int ODG { get; set; }
    }
}