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

namespace PortaleRegione.SDK.EDMA.Models
{
    /// <summary>
    ///     Rappresenta una FascicoloPratica EDMA. Per gli atti DASI la pratica
    ///     viene creata sotto un sotto-fascicolo titolario gia' esistente, di
    ///     conseguenza nel payload sono obbligatori titolo, codice procedimento,
    ///     date, referente, livello di appartenenza e l'identificativo del
    ///     sotto-fascicolo padre.
    /// </summary>
    public class FascicoloPratica
    {
        public string Titolo { get; set; }
        public string CodProcedimento { get; set; }
        public DateTime DataApertura { get; set; }
        public int AnniConservazione { get; set; }
        public DateTime DataChiusura { get; set; }

        // Codici dell'organigramma EDMA. Solo "Codice" e' richiesto: EDMA si
        // occupa di risolvere ufficio/assegnazione a partire da quello.
        public string ResponsabileCodPersona { get; set; }
        public string ReferenteCodPersona { get; set; }

        public SedeSoggetto SedeSoggetto { get; set; }

        // Id del FascicoloLivello (terzo livello "Pratica") sotto cui creare il
        // documento. Si ottiene leggendo il fascicolo padre tramite caricaDocumento.
        public string LivelloAppartenenzaId { get; set; }

        // Riferimento al FascicoloProcedimento padre (sotto-fascicolo titolario).
        public string ProcedimentoIdEdma { get; set; }
        public string ProcedimentoCodice { get; set; }

        // Codice del metadocumento della pratica (es. "FascicoloPratica").
        public string MetadocumentoCodice { get; set; }
        // Id metamodulo del modulo FascicoloPratica (100129 in EDMA).
        public int Metamodulo { get; set; } = 100129;

        // Attributi liberi (chiave/valore) per il titolario o altre proprieta'
        // proprietarie del metadocumento.
        public Dictionary<string, string> Attributi { get; set; } = new Dictionary<string, string>();

        public string Note { get; set; }
    }
}
