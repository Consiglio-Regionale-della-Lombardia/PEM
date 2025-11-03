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

public class AttoDASI_InformazioniGeneraliDto
{
    public Guid UIDAtto { get; set; }
    public int Stato { get; set; }             // ID dello stato dell'atto (Presentato, In Trattazione, Completato)
    public DateTime DataAnnunzio { get; set; } // Data dell'annuncio
    public DateTime? Timestamp { get; set; } // Data presentazione modificabile per le RIS #1297
    public string Oggetto { get; set; }        // Oggetto dell'atto
    public string Protocollo { get; set; }     // Codice del protocollo
    public string CodiceMateria { get; set; }  // Codice della materia
    public int AreaPolitica { get; set; }      // ID dell'area politica (Minoranza, Maggioranza, Misto, etc.)
    public int RispostaRichiesta { get; set; } // ID del tipo di risposta richiesta (Orale, Scritta, Commissione, etc.)
    public bool Pubblicato { get; set; }       // Stato di pubblicazione
    public bool Sollecito { get; set; }        // Stato di sollecito
    public Guid? UIDPersonaRelatore1 { get; set; }
    public Guid? UIDPersonaRelatore2 { get; set; }
    public Guid? UIDPersonaRelatoreMinoranza { get; set; }
}