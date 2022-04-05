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
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.DTO.Domain
{
    public class AttoDASIDto
    {
        public Guid UIDAtto { get; set; }
        public string Oggetto { get; set; }
        public string Oggetto_Pubblico { get; set; }
        public string Testo { get; set; }
        public string Testo_Pubblico { get; set; }
        public TipoAttoEnum Tipo { get; set; }
        public string NAtto { get; set; }
        public DateTime DataCreazione { get; set; }
        public Guid UIDPersonaCreazione { get; set; }
        public int idRuoloCreazione { get; set; }
        public DateTime? DataModifica { get; set; }
        public Guid? UIDPersonaModifica { get; set; }
        public string DataDeposito_crypt { get; set; }
        public Guid? UIDPersonaProponente { get; set; }
        public Guid? UIDPersonaPrimaFirma { get; set; }
        public DateTime DataPrimaFirma { get; set; }
        public Guid? UIDPersonaDeposito { get; set; }
        public bool Proietta { get; set; } = false;
        public DateTime? DataProietta { get; set; }
        public Guid? UIDPersonaProietta { get; set; }
        public DateTime? DataRitiro { get; set; }
        public Guid? UIDPersonaRitiro { get; set; }
        public string Hash { get; set; }
        public TipoRispostaEnum IDTipo_Risposta { get; set; }
        public int OrdineVisualizzazione { get; set; }
        public string PATH_AllegatoGenerico { get; set; }
        public string Note_Pubbliche { get; set; }
        public string Note_Private { get; set; }
        public int IDStato { get; set; }
        public bool Firma_su_invito { get; set; } = false;
        public Guid UID_QRCode { get; set; }
        public int AreaPolitica { get; set; }
        public bool Firma_da_ufficio { get; set; } = false;
        public bool Firmato_Da_Me { get; set; } = false;
        public bool Firmato_Dal_Proponente { get; set; } = false;

        public List<Guid> SoggettiInterrogati { get; set; }

        [JsonIgnore] public HttpPostedFileBase DocAllegatoGenerico { get; set; }

        public byte[] DocAllegatoGenerico_Stream { get; set; }
    }
}