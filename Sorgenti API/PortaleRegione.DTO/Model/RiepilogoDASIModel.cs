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

using System.Web.Mvc;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Response;

namespace PortaleRegione.DTO.Model
{
    public class RiepilogoDASIModel
    {
        public BaseResponse<AttoDASIDto> Data { get; set; }
        public StatiAttoEnum Stato { get; set; }
        public TipoAttoEnum Tipo { get; set; }
        public CountBarData CountBarData { get; set; }
        public ClientModeEnum ClientMode { get; set; } = ClientModeEnum.GRUPPI;
        public ViewModeEnum ViewMode { get; set; } = ViewModeEnum.GRID;
        public CommandRiepilogoModel CommandRiepilogo { get; set; }

        public PersonaDto CurrentUser { get; set; }
    }
}