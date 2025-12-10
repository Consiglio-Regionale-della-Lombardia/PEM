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
using System.ComponentModel;

namespace PortaleRegione.DTO.Domain;

public class AttoDASIReportDto
{
    // L'ordinamento è stato fatto in base al ticket #1093

    [DisplayName("Legislatura")] public int Legislatura { get; set; }
    [DisplayName("Etichetta")] public string Etichetta { get; set; }
    [DisplayName("Tipo atto")] public int Tipo { get; set; }
    [DisplayName("Numero atto")] public string NAtto { get; set; }
    [DisplayName("Tipo e numero atto")] public string Display { get; set; }
    [DisplayName("Tipo e numero atto esteso")] public string DisplayExtended { get; set; }
    [DisplayName("Tipo mozione")] public int TipoMOZ { get; set; } = 0;
    [DisplayName("Oggetto")] public string Oggetto { get; set; }
    [DisplayName("Data presentazione")] public DateTime Timestamp { get; set; }
    [DisplayName("Data annunzio")] public DateTime? DataAnnunzio { get; set; }
    [DisplayName("Protocollo")] public string Protocollo { get; set; }
    [DisplayName("Codice materia")] public string CodiceMateria { get; set; }
    [DisplayName("Proponente")] public Guid? UIDPersonaProponente { get; set; }
    [DisplayName("Firmatari")] public string Firme { get; set; }
    [DisplayName("Di iniziativa")] public string Iniziativa { get; set; } // #1047
    [DisplayName("Firmatari per DCR")] public string Firme_DCR { get; set; } // #1493
    [DisplayName("Conteggio firme")] public int ConteggioFirme { get; set; }
    [DisplayName("Gruppo politico")] public int id_gruppo { get; set; }  // #1021
    [DisplayName("Area politica")] public int AreaPolitica { get; set; }
    [DisplayName("Commissioni proponenti RIS")] public List<OrganoDto> CommissioniProponenti { get; set; }
    [DisplayName("Stato dell’atto")] public int IDStato { get; set; } // #1021
    [DisplayName("Motivo chiusura iter")] public int? TipoChiusuraIter { get; set; } // #1021
    [DisplayName("DCR/DCCR")] public int? DCR { get; set; }
    [DisplayName("Data richiesta iscrizione in seduta")] public string DataRichiestaIscrizioneSeduta { get; set; }
    [DisplayName("Data iscrizione in seduta")] public DateTime? DataIscrizioneSeduta { get; set; }
    [DisplayName("Data seduta")] public Guid? UIDSeduta { get; set; }
    [DisplayName("Data chiusura")] public DateTime? DataChiusuraIter { get; set; }
    [DisplayName("Data comunicazione assemblea")] public DateTime? DataComunicazioneAssemblea { get; set; }
    [DisplayName("Data ritiro")] public DateTime? DataRitiro { get; set; }
    [DisplayName("Tipo votazione")] public int? TipoVotazioneIter { get; set; }
    [DisplayName("Emendato")] public bool Emendato { get; set; }
    [DisplayName("Abbinamenti")] public string Abbinamenti { get; set; }
    [DisplayName("Note")] public string Note { get; set; }
    [DisplayName("Allegati dell’atto")] public string Documenti { get; set; } // #1021
    [DisplayName("Tipo risposta richiesta")] public int IDTipo_Risposta { get; set; }
    [DisplayName("Tipo risposta fornita")] public int? IDTipo_Risposta_Effettiva { get; set; }
    [DisplayName("Informazioni risposte/Iter")] public string Risposte { get; set; } // #1503
    [DisplayName("Sollecito")] public bool Sollecito { get; set; }
    [DisplayName("Iter multiplo")] public bool IterMultiplo { get; set; } = false;
    [DisplayName("Non passaggio in esame")] public bool Non_Passaggio_In_Esame { get; set; } = false;
    [DisplayName("Presentato oltre i termini")] public bool PresentatoOltreITermini { get; set; } = false;
    [DisplayName("Pubblicato")] public bool Pubblicato { get; set; }
    [DisplayName("Premesse")] public string Premesse { get; set; }
    [DisplayName("Richiesta")] public string Richiesta { get; set; }
    [DisplayName("Impegni e scadenze")] public string ImpegniScadenze { get; set; } // #1021
    [DisplayName("Stato attuazione")] public string StatoAttuazione { get; set; }
    [DisplayName("Competenza monitoraggio")] public string CompetenzaMonitoraggio { get; set; }
    [DisplayName("Data trasmissione monitoraggio")] public DateTime? DataTrasmissioneMonitoraggio { get; set; }
    [DisplayName("Informazioni monitoraggio")] public string Monitoraggi { get; set; }
    [DisplayName("Monitoraggio concluso")] public bool MonitoraggioConcluso { get; set; }
    [DisplayName("Area tematica")] public string AreaTematica { get; set; }
    [DisplayName("Altri soggetti")] public string AltriSoggetti { get; set; }
    [DisplayName("Privacy - dati giudiziari")] public bool Privacy_Dati_Personali_Giudiziari { get; set; }
    [DisplayName("Privacy - dati sanitari")] public bool Privacy_Divieto_Pubblicazione_Salute { get; set; }
    [DisplayName("Privacy - dati di natura sessuale")] public bool Privacy_Divieto_Pubblicazione_Vita_Sessuale { get; set; }
    [DisplayName("Privacy - divieto di pubblicazione")] public bool Privacy_Divieto_Pubblicazione { get; set; }
    [DisplayName("Privacy - dati sensibili")] public bool Privacy_Dati_Personali_Sensibili { get; set; }
    [DisplayName("Privicy - altri motivi")] public bool Privacy_Divieto_Pubblicazione_Altri { get; set; }
    [DisplayName("Privacy - dati personali semplici")] public bool Privacy_Dati_Personali_Semplici { get; set; }
    [DisplayName("Privacy")] public bool Privacy { get; set; }
    public string BURL { get; set; }
    [DisplayName("Link pubblico")] public Guid UID_QRCode { get; set; }
    [DisplayName("UIDAtto")] public Guid UIDAtto { get; set; }
}