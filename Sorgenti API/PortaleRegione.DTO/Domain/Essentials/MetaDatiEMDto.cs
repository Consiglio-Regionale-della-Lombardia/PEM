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

namespace PortaleRegione.DTO.Domain.Essentials
{
    public class MetaDatiEMDto
    {
        public MetaDatiEMDto(DateTime? dataModifica, Guid? uidPersonaModifica, int idTipoEm, int idParte, string nTitolo, string nCapo, Guid? uidArticolo, Guid? uidComma, Guid? uidLettera, string nNumero, int? nMissione, int? nProgramma, int? nTitoloB, string noteEm, string noteGriglia, int? areaPolitica)
        {
            DataModifica = dataModifica;
            UIDPersonaModifica = uidPersonaModifica;
            IDTipo_EM = idTipoEm;
            IDParte = idParte;
            NTitolo = nTitolo;
            NCapo = nCapo;
            UIDArticolo = uidArticolo;
            UIDComma = uidComma;
            UIDLettera = uidLettera;
            NNumero = nNumero;
            NMissione = nMissione;
            NProgramma = nProgramma;
            NTitoloB = nTitoloB;
            NOTE_EM = noteEm;
            NOTE_Griglia = noteGriglia;
            AreaPolitica = areaPolitica;
        }

        public DateTime? DataModifica { get; set; }

        public Guid? UIDPersonaModifica { get; set; }

        public int IDTipo_EM { get; set; }

        public int IDParte { get; set; }

        [StringLength(5)]
        public string NTitolo { get; set; }

        [StringLength(5)]
        public string NCapo { get; set; }

        public Guid? UIDArticolo { get; set; }

        public Guid? UIDComma { get; set; }

        public Guid? UIDLettera { get; set; }

        [StringLength(5)]
        public string NLettera { get; set; } = string.Empty; //https://github.com/Consiglio-Regionale-della-Lombardia/PEM/issues/885

        [StringLength(5)] 
        public string NNumero { get; set; }

        public int? NMissione { get; set; }

        public int? NProgramma { get; set; }

        public int? NTitoloB { get; set; }

        [AllowHtml] public string NOTE_EM { get; set; }

        [AllowHtml] public string NOTE_Griglia { get; set; }

        public int? AreaPolitica { get; set; }
    }
}