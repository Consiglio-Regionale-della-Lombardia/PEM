﻿/*
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

using PortaleRegione.DTO.Enum;

namespace PortaleRegione.DTO.Domain;

public class ReportDto
{
    public string reportname { get; set; }
    public string covertype { get; set; }
    public int dataviewtype { get; set; }
    public string dataviewtype_template { get; set; }
    public string columns { get; set; }
    public int exportformat { get; set; }
    public string filters { get; set; }
    public string sorting { get; set; }
    public int wordsize { get; set; }
}