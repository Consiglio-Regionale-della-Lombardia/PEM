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

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PortaleRegione.Domain
{
    [Table("ATTI_DASI")]
    public class ATTI_DASI
    {
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ATTI_DASI()
        {
        }

        [Key] public Guid UIDAtto { get; set; }
        public Guid? UIDSeduta { get; set; }

        public int Tipo { get; set; }
        public int? Progressivo { get; set; }
        public string Etichetta { get; set; }

        public string Oggetto { get; set; }
        public string Oggetto_Pubblico { get; set; }
        public string Premesse { get; set; }
        public string Premesse_Pubbliche { get; set; }
        public string NAtto { get; set; }
        public DateTime DataCreazione { get; set; }
        public Guid UIDPersonaCreazione { get; set; }
        public int idRuoloCreazione { get; set; }
        public DateTime? DataModifica { get; set; }
        public Guid? UIDPersonaModifica { get; set; }
        public string DataPresentazione { get; set; }
        public Guid? UIDPersonaProponente { get; set; }
        public Guid? UIDPersonaPrimaFirma { get; set; }
        public DateTime? DataPrimaFirma { get; set; }
        public Guid? UIDPersonaPresentazione { get; set; }
        public bool Proietta { get; set; } = false;
        public DateTime? DataProietta { get; set; }
        public Guid? UIDPersonaProietta { get; set; }
        public DateTime? DataRitiro { get; set; }
        public Guid? UIDPersonaRitiro { get; set; }
        public string Hash { get; set; }
        public int IDTipo_Risposta { get; set; }
        public int OrdineVisualizzazione { get; set; }
        public string PATH_AllegatoGenerico { get; set; }
        public string Note_Pubbliche { get; set; }
        public string Note_Private { get; set; }
        public int IDStato { get; set; }
        public bool Firma_su_invito { get; set; } = false;
        public Guid UID_QRCode { get; set; }
        public int AreaPolitica { get; set; }
        public int id_gruppo { get; set; }
        public bool Eliminato { get; set; } = false;
        public string chkf { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Atto_Certificato { get; set; }
        public Guid? UIDPersonaElimina { get; set; }
        public DateTime? DataElimina { get; set; }
        public int Legislatura { get; set; }
        public string Richiesta { get; set; }
        public string Richiesta_Pubblica { get; set; }
        public DateTime? DataIscrizioneSeduta { get; set; }
        public Guid? UIDPersonaIscrizioneSeduta { get; set; }
    }
}