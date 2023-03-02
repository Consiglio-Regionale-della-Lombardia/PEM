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
using System.ComponentModel.DataAnnotations;

namespace PortaleRegione.DTO.Domain
{
    public class AttiFirmeDto
    {
        public Guid UIDAtto { get; set; }

        public Guid UID_persona { get; set; }

        public string FirmaCert { get; set; }

        [StringLength(255)]
        public string Data_firma { get; set; }

        [StringLength(255)]
        public string Data_ritirofirma { get; set; }

        public int? id_AreaPolitica { get; set; }

        public DateTime Timestamp { get; set; }

        public bool ufficio { get; set; }

        public bool PrimoFirmatario { get; set; } = false;
        public int id_gruppo { get; set; } = 0;
        public bool Capogruppo { get; set; } = false;


        public virtual PersonaDto UTENTI_NoCons { get; set; }

        public static implicit operator AttiFirmeDto(FirmeDto firma)
        {
            return new AttiFirmeDto
            {
                UID_persona = firma.UID_persona,
                ufficio = firma.ufficio,
                FirmaCert = firma.FirmaCert,
                Data_firma = firma.Data_firma,
                Data_ritirofirma = firma.Data_ritirofirma,
                Timestamp = firma.Timestamp,
                id_AreaPolitica = firma.id_AreaPolitica
            };
        }
    }
}