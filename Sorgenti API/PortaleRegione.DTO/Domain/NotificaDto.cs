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

using PortaleRegione.DTO.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace PortaleRegione.DTO.Domain
{
    public class NotificaDto
    {
        [Key] public string UIDNotifica { get; set; }

        public DateTime DataCreazione { get; set; }

        public Guid UIDEM { get; set; }

        public Guid UIDAtto { get; set; }

        public int IDTipo { get; set; }

        public Guid Mittente { get; set; }

        public RuoliIntEnum RuoloMittente { get; set; }

        public string Messaggio { get; set; }

        public DateTime? DataScadenza { get; set; }

        public Guid SyncGUID { get; set; }

        public int IdGruppo { get; set; }

        public bool Chiuso { get; set; }

        public DateTime? DataChiusura { get; set; }

        public Guid? BLOCCO_INVITI { get; set; }

        public virtual AttiDto ATTI { get; set; }

        public virtual AttoDASIDto ATTO_DASI { get; set; }

        public virtual EmendamentiDto EM { get; set; }

        public virtual GruppiDto gruppi_politici { get; set; }

        public virtual TipoNotificaDto TIPI_NOTIFICA { get; set; }

        public virtual PersonaDto UTENTI_NoCons { get; set; }
        public bool Valida { get; set; } = true;

        public bool IsDasi => UIDEM == Guid.Empty;
        public bool IsPem => UIDEM != Guid.Empty;
    }
}