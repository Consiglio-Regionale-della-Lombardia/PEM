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
using System.Web.Mvc;

namespace PortaleRegione.DTO.Domain
{
    public class SeduteDto
    {
        public Guid UIDSeduta { get; set; }

        [Display(Name = "Data seduta")]
        public DateTime Data_seduta { get; set; }
        [Display(Name = "Data apertura")]
        public DateTime? Data_apertura { get; set; }
        [Display(Name = "Data effettiva inizio")]
        public DateTime? Data_effettiva_inizio { get; set; }
        [Display(Name = "Data effettiva fine")]
        public DateTime? Data_effettiva_fine { get; set; }

        public int? IDOrgano { get; set; }
        [Display(Name = "Data scadenza presentazione - Emendamenti")]
        public DateTime? Scadenza_presentazione { get; set; }
        [Display(Name = "Data scadenza presentazione - Interrogation question time")]

        public DateTime? DataScadenzaPresentazioneIQT { get; set; }
        [Display(Name = "Data scadenza presentazione - Mozioni")]
        public DateTime? DataScadenzaPresentazioneMOZ { get; set; }
        [Display(Name = "Data scadenza presentazione - Mozioni abbinate")]
        public DateTime? DataScadenzaPresentazioneMOZA { get; set; }
        [Display(Name = "Data scadenza presentazione - Mozioni urgenti")]
        public DateTime? DataScadenzaPresentazioneMOZU { get; set; }
        [Display(Name = "Data scadenza presentazione - Ordini del giorno")]
        public DateTime? DataScadenzaPresentazioneODG { get; set; }

        public int id_legislatura { get; set; }

        [AllowHtml]
        public string Intervalli { get; set; }

        public Guid? UIDPersonaCreazione { get; set; }

        public DateTime? DataCreazione { get; set; }

        public Guid? UIDPersonaModifica { get; set; }

        public DateTime? DataModifica { get; set; }

        [Display(Name = "Dedicata agli atti d’indirizzo e sindacato ispettivo")]

        public bool Riservato_DASI { get; set; }
    }

    public class SeduteFormUpdateDto
    {
        public Guid UIDSeduta { get; set; }

        [Display(Name = "Data seduta")]
        public DateTime Data_seduta { get; set; }
        [Display(Name = "Data apertura")]
        public DateTime? Data_apertura { get; set; }
        [Display(Name = "Data effettiva inizio")]
        public DateTime? Data_effettiva_inizio { get; set; }
        [Display(Name = "Data effettiva fine")]
        public DateTime? Data_effettiva_fine { get; set; }

        [Display(Name = "Data scadenza presentazione - Emendamenti")]
        public DateTime? Scadenza_presentazione { get; set; }
        [Display(Name = "Data scadenza presentazione - Interrogation question time")]

        public DateTime? DataScadenzaPresentazioneIQT { get; set; }
        [Display(Name = "Data scadenza presentazione - Mozioni")]
        public DateTime? DataScadenzaPresentazioneMOZ { get; set; }
        [Display(Name = "Data scadenza presentazione - Mozioni abbinate")]
        public DateTime? DataScadenzaPresentazioneMOZA { get; set; }
        [Display(Name = "Data scadenza presentazione - Mozioni urgenti")]
        public DateTime? DataScadenzaPresentazioneMOZU { get; set; }
        [Display(Name = "Data scadenza presentazione - Ordini del giorno")]
        public DateTime? DataScadenzaPresentazioneODG { get; set; }

        [AllowHtml]
        public string Intervalli { get; set; }

        [Display(Name = "Dedicata agli atti d’indirizzo e sindacato ispettivo")]

        public bool Riservato_DASI { get; set; }
    }
}