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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortaleRegione.Domain
{
    [Table("EM")]
    public class EM
    {
        public EM()
        {
            EM1 = new HashSet<EM>();
            FIRME = new HashSet<FIRME>();
            NOTIFICHE = new HashSet<NOTIFICHE>();
        }

        [Key] public Guid UIDEM { get; set; }

        public int? Progressivo { get; set; }

        public Guid UIDAtto { get; set; }

        [StringLength(50)] public string N_EM { get; set; }

        public int id_gruppo { get; set; }

        public Guid? Rif_UIDEM { get; set; }

        [StringLength(50)] public string N_SUBEM { get; set; }

        public int? SubProgressivo { get; set; }

        public Guid UIDPersonaProponente { get; set; }

        public Guid? UIDPersonaProponenteOLD { get; set; }

        public DateTime? DataCreazione { get; set; }

        public Guid? UIDPersonaCreazione { get; set; }

        public int? idRuoloCreazione { get; set; }

        public DateTime? DataModifica { get; set; }

        public Guid? UIDPersonaModifica { get; set; }

        [StringLength(255)] public string DataDeposito { get; set; }

        public Guid? UIDPersonaPrimaFirma { get; set; }

        public DateTime? DataPrimaFirma { get; set; }

        public Guid? UIDPersonaDeposito { get; set; }

        public bool Proietta { get; set; } = false;

        public DateTime? DataProietta { get; set; }

        public Guid? UIDPersonaProietta { get; set; }

        public DateTime? DataRitiro { get; set; }

        public Guid? UIDPersonaRitiro { get; set; }

        public string Hash { get; set; }

        public int IDTipo_EM { get; set; }

        public int IDParte { get; set; }

        [StringLength(5)] public string NTitolo { get; set; } = string.Empty;

        [StringLength(5)] public string NCapo { get; set; } = string.Empty;

        public Guid? UIDArticolo { get; set; }

        public Guid? UIDComma { get; set; }

        public Guid? UIDLettera { get; set; }

        [StringLength(50) ]
        public string NLettera { get; set; } =
            string.Empty; // https://github.com/Consiglio-Regionale-della-Lombardia/PEM/issues/809

        [StringLength(5)] public string NNumero { get; set; }

        public int? NMissione { get; set; }

        public int? NProgramma { get; set; }

        public int? NTitoloB { get; set; }

        public int OrdinePresentazione { get; set; }

        public int OrdineVotazione { get; set; }

        public string TestoEM_originale { get; set; } = string.Empty;

        public string EM_Certificato { get; set; }

        public string TestoREL_originale { get; set; }

        public string PATH_AllegatoGenerico { get; set; }

        public string PATH_AllegatoTecnico { get; set; }

        public int EffettiFinanziari { get; set; } = 0;
        public string NOTE_EM { get; set; }

        public string NOTE_Griglia { get; set; }

        public int IDStato { get; set; }

        public bool Firma_su_invito { get; set; }

        public string TestoEM_Modificabile { get; set; }

        public Guid UID_QRCode { get; set; }

        public int? AreaPolitica { get; set; }

        public bool Eliminato { get; set; }

        public Guid? UIDPersonaElimina { get; set; }

        public DateTime? DataElimina { get; set; }

        [StringLength(255)] public string chkf { get; set; }

        public int? chkem { get; set; }

        public DateTime? Timestamp { get; set; }

        [StringLength(50)] public string Colore { get; set; }

        public string Tags { get; set; } = "[]";

        public virtual ARTICOLI ARTICOLI { get; set; }

        public virtual ATTI ATTI { get; set; }

        public virtual COMMI COMMI { get; set; }

        public virtual LETTERE LETTERE { get; set; }

        public virtual ICollection<EM> EM1 { get; set; }

        public virtual EM EM2 { get; set; }

        public virtual gruppi_politici gruppi_politici { get; set; }

        public virtual PARTI_TESTO PARTI_TESTO { get; set; }

        public virtual STATI_EM STATI_EM { get; set; }

        public virtual TIPI_EM TIPI_EM { get; set; }

        public virtual TITOLI_MISSIONI TITOLI_MISSIONI { get; set; }

        public virtual ICollection<FIRME> FIRME { get; set; }

        public virtual ICollection<NOTIFICHE> NOTIFICHE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("SubEM", TypeName = "int")]
        public int SubEM { get; set; }

        public int VersioneStampa { get; set; } = 0;
        public DateTime? DataUltimaStampa { get; set; }
        public string PathStampa { get; set; }
        public bool StampaValida { get; set; } = false;

        public static implicit operator EM(EmendamentiDto dto)
        {
            return new EM
            {
                UIDEM = dto.UIDEM,
                IDStato = dto.IDStato,
                UIDPersonaProponente = dto.UIDPersonaProponente,
                Timestamp = dto.Timestamp
            };
        }
    }
}