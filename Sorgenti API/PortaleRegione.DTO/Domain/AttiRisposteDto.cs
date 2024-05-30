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

namespace PortaleRegione.DTO.Domain;

public class AttiRisposteDto
{
    public Guid IdRisposta { get; set; }
    public Guid UIDAtto { get; set; }
    public int TipoOrgano { get; set; }
    public int Tipo { get; set; }
    public DateTime? Data { get; set; }
    public DateTime? DataTrasmissione { get; set; }
    public DateTime? DataTrattazione { get; set; }
    public int IdOrgano { get; set; }
    public string DescrizioneOrgano { get; set; }
    public string DisplayTipo { get; set; }
    public string DisplayTipoOrgano { get; set; }
}

public class AttiRispostePublicDto
{
    public string tipo_organo { get; set; }
    public string tipo_risposta { get; set; }
    public DateTime? data { get; set; }
    public DateTime? data_trasmissione { get; set; }
    public DateTime? data_trattazione { get; set; }
    public int id_organo { get; set; }
    public string organo { get; set; }
}