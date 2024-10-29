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

using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace PortaleRegione.DTO.Domain;

public class AttiDASIColums
{
    [DisplayName("Tipo atto")] public int Tipo { get; set; }
    [DisplayName("Tipo mozione")] public int TipoMOZ { get; set; } = 0;
    [DisplayName("Etichetta")] public string Etichetta { get; set; }

    [DisplayName("Data presentazione")] public DateTime Timestamp { get; set; }
    [DisplayName("Data ritiro")] public DateTime? DataRitiro { get; set; }
    [DisplayName("Data annunzio")] public DateTime? DataAnnunzio { get; set; }
    [DisplayName("Data chiusura")] public DateTime? DataChiusuraIter { get; set; }
    [DisplayName("Data comunicazione assemblea")] public DateTime? DataComunicazioneAssemblea { get; set; }

    [DisplayName("Oggetto")] public string Oggetto { get; set; }
    [DisplayName("Premesse")] public string Premesse { get; set; }
    [DisplayName("Richiesta")] public string Richiesta { get; set; }
    [DisplayName("Impegni e scadenze")] public string ImpegniScadenze { get; set; } // #1021
    [DisplayName("Stato attuazione")] public string StatoAttuazione { get; set; }
    [DisplayName("Competenza monitoraggio")] public string CompetenzaMonitoraggio { get; set; }
    [DisplayName("Firmatari")] public string Firme { get; set; }
    [DisplayName("Conteggio firme")] public int ConteggioFirme { get; set; }

    [DisplayName("Commissioni proponenti")] public List<OrganoDto> CommissioniProponenti { get; set; }

    [DisplayName("Gruppo politico")] public int id_gruppo { get; set; }  // #1021
    [DisplayName("Stato dell’atto")] public int IDStato { get; set; } // #1021
    [DisplayName("Protocollo")] public string Protocollo { get; set; }
    [DisplayName("Codice materia")] public string CodiceMateria { get; set; }

    [DisplayName("Tipo risposta richiesta")]
    public int IDTipo_Risposta { get; set; }

    [DisplayName("Abbinamenti")] public string Abbinamenti { get; set; }
    [DisplayName("Informazioni risposte")] public string Risposte { get; set; }
    [DisplayName("Motivo chiusura iter")] public int? TipoChiusuraIter { get; set; } // #1021
    [DisplayName("Tipo votazione")] public int? TipoVotazioneIter { get; set; }
    [DisplayName("Emendato")] public bool Emendato { get; set; }
    [DisplayName("Pubblicato")] public bool Pubblicato { get; set; }
    [DisplayName("Sollecito")] public bool Sollecito { get; set; }
    [DisplayName("Area tematica")] public string AreaTematica { get; set; }
    [DisplayName("Altri soggetti")] public string AltriSoggetti { get; set; }
    [DisplayName("DCR/DCCR")] public int? DCR { get; set; }
    [DisplayName("Data richiesta iscrizione in seduta")] public string DataRichiestaIscrizioneSeduta { get; set; } // #1001
    [DisplayName("Data iscrizione in seduta")] public DateTime? DataIscrizioneSeduta { get; set; }
    [DisplayName("Seduta")] public Guid? UIDSeduta { get; set; }
    [DisplayName("Area politica")] public int AreaPolitica { get; set; }
    [DisplayName("Legislatura")] public int Legislatura { get; set; }
    [DisplayName("Non passaggio in esame")] public bool Non_Passaggio_In_Esame { get; set; } = false;
    [DisplayName("Privacy - divieto di pubblicazione")] public bool Privacy_Divieto_Pubblicazione { get; set; }
    [DisplayName("Privacy")] public bool Privacy { get; set; }
    [DisplayName("Allegati dell’atto")] public string Documenti { get; set; } // #1021
    [DisplayName("Note")] public string Note { get; set; }
    [DisplayName("Iter multiplo")] public bool IterMultiplo { get; set; } = false;
    [DisplayName("Presentato oltre i termini")] public bool PresentatoOltreITermini { get; set; } = false;
    public string BURL { get; set; }
    [DisplayName("UIDAtto")] public Guid UIDAtto { get; set; }
}