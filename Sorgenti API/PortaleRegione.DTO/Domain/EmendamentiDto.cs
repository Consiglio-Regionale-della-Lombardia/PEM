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

using Newtonsoft.Json;
using PortaleRegione.DTO.Domain.Essentials;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace PortaleRegione.DTO.Domain
{
    public class EmendamentiDto
    {
        public bool Invito_Abilitato { get; set; } = false;
        public bool IsSUBEM => Rif_UIDEM.HasValue;

        [Key] public Guid UIDEM { get; set; }

        public int? Progressivo { get; set; }

        public Guid UIDAtto { get; set; }

        [StringLength(50)] public string N_EM { get; set; }

        public int id_gruppo { get; set; }

        public Guid? Rif_UIDEM { get; set; }

        [StringLength(50)] public string N_SUBEM { get; set; }

        public int? SubProgressivo { get; set; }

        public Guid? UIDPersonaProponente { get; set; }
        public PersonaLightDto PersonaProponente { get; set; }

        public Guid? UIDPersonaProponenteOLD { get; set; }

        public DateTime? DataCreazione { get; set; }

        public Guid? UIDPersonaCreazione { get; set; }
        public PersonaLightDto PersonaCreazione { get; set; }

        public int? idRuoloCreazione { get; set; }

        public DateTime? DataModifica { get; set; }

        public Guid? UIDPersonaModifica { get; set; }
        public PersonaLightDto PersonaModifica { get; set; }

        [StringLength(255)] public string DataDeposito { get; set; }

        public Guid? UIDPersonaDeposito { get; set; }
        public PersonaLightDto PersonaDeposito { get; set; }

        public bool? Proietta { get; set; } = false;

        public DateTime? DataRitiro { get; set; }

        public Guid? UIDPersonaRitiro { get; set; }

        [Display(Name = "Indica il modo")]
        [Required(ErrorMessage = "E' obbligatorio indicare il modo")]
        public int IDTipo_EM { get; set; }

        [Display(Name = "Indica l'elemento")]
        [Required(ErrorMessage = "E' obbligatorio indicare l'elemento da emendare")]
        public int IDParte { get; set; }

        [Display(Name = "Titolo")]
        [StringLength(5)]
        public string NTitolo { get; set; } = string.Empty;

        [Display(Name = "Capo")]
        [StringLength(5)]
        public string NCapo { get; set; } = string.Empty;

        [Display(Name = "Articolo")] public Guid? UIDArticolo { get; set; }

        [Display(Name = "Comma")] public Guid? UIDComma { get; set; }

        [Display(Name = "Lettera")] public Guid? UIDLettera { get; set; }

        [Display(Name = "Lettera")]
        [StringLength(5)]
        public string NLettera { get; set; }

        [StringLength(5)] public string NNumero { get; set; }

        [Display(Name = "Missione")] public int? NMissione { get; set; }

        [Display(Name = "Programma")] public int? NProgramma { get; set; }

        [Display(Name = "Titolo B")] public int? NTitoloB { get; set; }

        public int OrdinePresentazione { get; set; }

        public int OrdineVotazione { get; set; }

        [Display(Name = "Testo")] [AllowHtml] public string TestoEM_originale { get; set; }

        public string EM_Certificato { get; set; }

        [Display(Name = "Relazione illustrativa")]
        [AllowHtml]
        public string TestoREL_originale { get; set; }

        [DisplayName("Allegato generico")] public string PATH_AllegatoGenerico { get; set; }

        public string PATH_AllegatoTecnico { get; set; }

        public int EffettiFinanziari { get; set; }

        [AllowHtml] public string NOTE_EM { get; set; }

        [AllowHtml] public string NOTE_Griglia { get; set; }

        public int IDStato { get; set; }

        public bool Firma_su_invito { get; set; }

        [Display(Name = "Testo")] [AllowHtml] public string TestoEM_Modificabile { get; set; }

        public Guid UID_QRCode { get; set; }

        [Display(Name = "Area politica")] public int? AreaPolitica { get; set; }

        public DateTime? Timestamp { get; set; }

        [StringLength(50)] public string Colore { get; set; }

        public virtual ArticoliDto ARTICOLI { get; set; }

        public virtual AttiDto ATTI { get; set; }

        public virtual CommiDto COMMI { get; set; }

        public virtual LettereDto LETTERE { get; set; }

        public virtual EmendamentiDto EM2 { get; set; }

        public virtual GruppiDto gruppi_politici { get; set; }

        public virtual PartiTestoDto PARTI_TESTO { get; set; }

        public virtual StatiDto STATI_EM { get; set; }

        public virtual Tipi_EmendamentiDto TIPI_EM { get; set; }

        public virtual TitoloMissioniDto TITOLI_MISSIONI { get; set; }

        [JsonIgnore] public HttpPostedFileBase DocAllegatoGenerico { get; set; }

        public byte[] DocAllegatoGenerico_Stream { get; set; }

        [JsonIgnore] public HttpPostedFileBase DocEffettiFinanziari { get; set; }

        public byte[] DocEffettiFinanziari_Stream { get; set; }

        public bool Eliminato { get; set; }

        #region Campi consumabili lato client

        public bool Depositabile { get; set; } = false;
        public bool Firmabile { get; set; } = false;
        public bool Ritirabile { get; set; } = false;
        public bool Eliminabile { get; set; } = false;
        public bool Firmato_Dal_Proponente { get; set; } = false;
        public bool Firma_da_ufficio { get; set; } = false;
        public int ConteggioFirme { get; set; }
        public bool Modificabile { get; set; } = false;
        public bool AbilitaSUBEM { get; set; } = false;
        public string Destinatari { get; set; }
        public string Firmatari { get; set; }
        public string Firme { get; set; }
        public bool PresentatoOltreITermini { get; set; } = false;
        public bool Proponente_Relatore { get; set; } = false;

        public string BodyEM { get; set; }
        public string Firme_dopo_deposito { get; set; }
        public string Firme_OPENDATA { get; set; }

        public bool Firmato_Da_Me { get; set; } = false;
        public bool Proponente_Assessore_Riferimento { get; set; }
        public FirmeDto Firma_ufficio { get; set; }

        #endregion
    }
}