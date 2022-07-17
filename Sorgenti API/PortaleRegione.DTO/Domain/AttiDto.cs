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

using PortaleRegione.DTO.Domain.Essentials;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PortaleRegione.DTO.Domain
{
    public class AttiDto
    {
        public bool Chiuso => Data_chiusura.HasValue && Data_chiusura.Value < DateTime.Now;

        [Key] public Guid UIDAtto { get; set; }

        [Required] [StringLength(50)] public string NAtto { get; set; }

        public int IDTipoAtto { get; set; }

        [AllowHtml] [StringLength(500)] public string Oggetto { get; set; }

        [AllowHtml] [StringLength(255)] public string Note { get; set; }

        public string Path_Testo_Atto { get; set; }

        public Guid? UIDSeduta { get; set; }

        public DateTime? Data_apertura { get; set; }

        public DateTime? Data_chiusura { get; set; }

        public bool VIS_Mis_Prog { get; set; }

        public Guid? UIDAssessoreRiferimento { get; set; }

        public bool Notifica_deposito_differita { get; set; }

        public bool? OrdinePresentazione { get; set; } = false;

        public bool? OrdineVotazione { get; set; } = false;

        public int? Priorita { get; set; }

        public DateTime? DataCreazione { get; set; }

        public Guid? UIDPersonaCreazione { get; set; }

        public DateTime? DataModifica { get; set; }

        public Guid? UIDPersonaModifica { get; set; }

        public bool? Eliminato { get; set; }

        public string LinkFascicoloPresentazione { get; set; }

        public DateTime? DataCreazionePresentazione { get; set; }

        public string LinkFascicoloVotazione { get; set; }

        public DateTime? DataCreazioneVotazione { get; set; }

        public DateTime? DataUltimaModificaEM { get; set; }

        public virtual Tipi_AttoDto TIPI_ATTO { get; set; }
        public virtual SeduteDto SEDUTE { get; set; }

        public int Conteggio_EM { get; set; }
        public int Conteggio_SubEM { get; set; }
        public IEnumerable<PersonaLightDto> Relatori { get; set; }
        public bool Informazioni_Mancanti { get; set; }
        public bool CanMoveDown { get; set; } = false;
        public bool CanMoveUp { get; set; } = false;
        public int Stato { get; set; }
        public PersonaLightDto PersonaAssessore { get; set; }
        public int TipoDibattito { get; set; }
    }
}