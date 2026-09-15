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
using PortaleRegione.DTO.Domain;

namespace PortaleRegione.DTO.Request;

/// <summary>
///     Contenitore dei filtri specializzati estratti dal BaseRequest per il modulo Emendamenti.
///     Popolato in EmendamentiLogic da CreateQueryExtendedRequestEM e consumato dal repository
///     all'interno di ApplyFilters. Non serializzato sulla rete.
/// </summary>
public class QueryExtendedRequestEM
{
    public List<int> Stati { get; set; } = new();
    public List<int> Tipi { get; set; } = new();
    public List<int> Parti { get; set; } = new();
    public List<int> GruppiProponenti { get; set; } = new();
    public List<Guid> Proponenti { get; set; } = new();
    public List<Guid> Firmatari { get; set; } = new();
    public List<TagDto> Tags { get; set; } = new();

    public List<Guid> Articoli { get; set; } = new();
    public List<Guid> Commi { get; set; } = new();
    public List<Guid> Lettere { get; set; } = new();
    public List<string> LettereLegacy { get; set; } = new();
    public List<string> NTitoli { get; set; } = new();
    public List<string> NCapi { get; set; } = new();
    public List<int> NMissioni { get; set; } = new();
    public List<int> NProgrammi { get; set; } = new();

    public bool MyEM { get; set; }
    public bool EMDaFirmare { get; set; }

    // #1645 - Chip booleana "Effetti finanziari": true => solo EM con effetti (== 1),
    // false => solo EM senza effetti (== 0), null => nessun vincolo (chip assente).
    public bool? EffettiFinanziari { get; set; }

    // #1644 - Chip booleana "Sub-emendamenti": true => solo SUBEM (Rif_UIDEM valorizzato),
    // false => solo EM (senza riferimento), null => nessun vincolo.
    public bool? SoloSubEM { get; set; }

    public string TestoLibero1 { get; set; }
    public string TestoLibero2 { get; set; }
    public int TestoLiberoConnettore { get; set; }

    public Guid? UIDAtto { get; set; }
    public Guid? UIDPersonaCorrente { get; set; }

    // #1626 - Ricerca trasversale EM/SUBEM (Area Aula): quando RicercaGlobale e' true il
    // repository non vincola la query a un singolo atto ma applica i filtri qui sotto su
    // tutto l'archivio dei depositati. Legislature e AreePolitiche sono filtri a scelta
    // multipla; TipoRicerca discrimina EM / SUBEM / entrambi.
    public bool RicercaGlobale { get; set; }
    public List<int> Legislature { get; set; } = new();
    public List<int> AreePolitiche { get; set; } = new();
    public int TipoRicerca { get; set; }
}
