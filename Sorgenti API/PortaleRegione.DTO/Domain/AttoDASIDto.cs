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
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;

namespace PortaleRegione.DTO.Domain;

public class AttoDASIDto
{
    public AttoDASIDto()
    {
        FirmeCartacee = new List<KeyValueDto>();
        FirmeAnte = new List<AttiFirmeDto>();
        FirmePost = new List<AttiFirmeDto>();
    }

    public Guid UIDAtto { get; set; }

    [DisplayName("Data seduta")] public Guid? UIDSeduta { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Guid? UID_MOZ_Abbinata { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Guid? UID_Atto_ODG { get; set; }

    public string Etichetta { get; set; }
    public string Oggetto { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Oggetto_Modificato { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Oggetto_Privacy { get; set; }

    [AllowHtml] public string Premesse { get; set; }

    [AllowHtml]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Premesse_Modificato { get; set; }

    [AllowHtml] public string Richiesta { get; set; }

    [AllowHtml]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Richiesta_Modificata { get; set; }


    [DisplayName("Tipo atto")] public int Tipo { get; set; }

    [DisplayName("Tipo mozione")] public int TipoMOZ { get; set; } = 0;
    public string DisplayTipoMozione { get; set; }

    [DisplayName("Numero atto")] public string NAtto { get; set; }
    public DateTime DataCreazione { get; set; }
    public Guid UIDPersonaCreazione { get; set; }
    public int idRuoloCreazione { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? DataModifica { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Guid? UIDPersonaModifica { get; set; }

    [DisplayName("Data presentazione")] public string DataPresentazione { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DataPresentazione_MOZ { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DataPresentazione_MOZ_URGENTE { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DataPresentazione_MOZ_ABBINATA { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DataRichiestaIscrizioneSeduta { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Guid? UIDPersonaRichiestaIscrizione { get; set; }

    [DisplayName("Proponente")] public Guid? UIDPersonaProponente { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Guid? UIDPersonaPrimaFirma { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? DataPrimaFirma { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Guid? UIDPersonaPresentazione { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? DataRitiro { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Guid? UIDPersonaRitiro { get; set; }

    [DisplayName("Tipo risposta richiesta")] public int IDTipo_Risposta { get; set; }
    public int OrdineVisualizzazione { get; set; }

    [DisplayName("Allegato")]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string PATH_AllegatoGenerico { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Note_Pubbliche { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Note_Private { get; set; }

    [DisplayName("Stato")] public int IDStato { get; set; }
    public bool Firma_su_invito { get; set; } = false;
    public Guid UID_QRCode { get; set; }
    [DisplayName("Area politica")] public int AreaPolitica { get; set; }
    public bool Firma_da_ufficio { get; set; } = false;
    public bool Firmato_Da_Me { get; set; } = false;
    public bool Firmato_Dal_Proponente { get; set; } = false;
    public bool Presentabile { get; set; } = false;
    public int Progressivo { get; set; }

    [DisplayName("Legislatura")] public int Legislatura { get; set; }

    [JsonIgnore] public HttpPostedFileBase DocAllegatoGenerico { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public byte[] DocAllegatoGenerico_Stream { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Atto_Certificato { get; set; } = "";

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string BodyAtto { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [DisplayName("Firmatari")] public string Firme { get; set; } = string.Empty;

    [DisplayName("Data presentazione")] public DateTime? Timestamp { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Firme_dopo_deposito { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Destinatari { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public PersonaLightDto PersonaModifica { get; set; }

    public PersonaLightDto PersonaProponente { get; set; }
    public PersonaLightDto PersonaCreazione { get; set; }
    public int ConteggioFirme { get; set; }
    public GruppiDto gruppi_politici { get; set; }
    public bool Firmabile { get; set; }
    public bool Eliminabile { get; set; }
    public bool Ritirabile { get; set; }
    public bool Modificabile { get; set; }
    [DisplayName("Gruppo proponente")]public int id_gruppo { get; set; }
    [DisplayName("Gruppo firmatario")]public int id_gruppo_firmatari { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<OrganoDto> Organi { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Commissioni_client { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public SeduteDto? Seduta { get; set; }

    [DisplayName("Data iscrizione in seduta")]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? DataIscrizioneSeduta { get; set; }
    
    [DisplayName("Data trasmissione")]
    public DateTime? DataTrasmissione { get; set; }

    public bool Invito_Abilitato { get; set; } = false;
    [DisplayName("Presentato oltre i termini")] public bool PresentatoOltreITermini { get; set; } = false;
    public bool Non_Passaggio_In_Esame { get; set; } = false;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string MOZ_Abbinata { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ODG_Atto_PEM { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ODG_Atto_Oggetto_PEM { get; set; }

    public bool Inviato_Al_Protocollo { get; set; } = false;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? DataInvioAlProtocollo { get; set; }

    public bool CapogruppoNeiTermini { get; set; } = false;
    public bool MOZU_Capigruppo { get; set; } = false;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DettaglioMozioniAbbinate { get; set; }

    public string Display { get; set; }
    public string DisplayExtended { get; set; }

    // Matteo Cattapan #520
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<KeyValueDto> FirmeCartacee { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string FirmeCartacee_string { get; set; }

    public bool IsMOZ()
    {
        return Tipo == (int)TipoAttoEnum.MOZ;
    }

    public bool IsMOZOrdinaria()
    {
        return Tipo == (int)TipoAttoEnum.MOZ && TipoMOZ == (int)TipoMOZEnum.ORDINARIA;
    }

    public bool IsMOZUrgente()
    {
        return Tipo == (int)TipoAttoEnum.MOZ && TipoMOZ == (int)TipoMOZEnum.URGENTE;
    }

    public bool IsMOZAbbinata()
    {
        return Tipo == (int)TipoAttoEnum.MOZ && TipoMOZ == (int)TipoMOZEnum.ABBINATA;
    }

    public bool IsMOZSfiducia()
    {
        return Tipo == (int)TipoAttoEnum.MOZ && TipoMOZ == (int)TipoMOZEnum.SFIDUCIA;
    }

    public bool IsMOZCensura()
    {
        return Tipo == (int)TipoAttoEnum.MOZ && TipoMOZ == (int)TipoMOZEnum.CENSURA;
    }

    public bool IsIQT()
    {
        return Tipo == (int)TipoAttoEnum.IQT;
    }

    public bool IsITL()
    {
        return Tipo == (int)TipoAttoEnum.ITL;
    }

    public bool IsITR()
    {
        return Tipo == (int)TipoAttoEnum.ITR;
    }

    public bool IsODG()
    {
        return Tipo == (int)TipoAttoEnum.ODG;
    }

    // #558

    public bool IsChiuso => IDStato == (int)StatiAttoEnum.COMPLETATO;

    public bool IsBozza => IDStato == (int)StatiAttoEnum.BOZZA
                           || IDStato == (int)StatiAttoEnum.BOZZA_CARTACEA
                           || IDStato == (int)StatiAttoEnum.BOZZA_RISERVATA;


    public List<AttiFirmeDto> FirmeAnte { get; set; }
    public List<AttiFirmeDto> FirmePost { get; set; }

    public string OggettoView()
    {
        if (!string.IsNullOrEmpty(Oggetto_Privacy))
            return Oggetto_Privacy;
        if (!string.IsNullOrEmpty(Oggetto_Modificato))
            return Oggetto_Modificato;
        return Oggetto;
    }

    public string GetLegislatura()
    {
        if (!string.IsNullOrEmpty(Etichetta))
        {
            var parti = Etichetta.Split('_');
            if (parti.Length > 0)
                return parti[parti.Length - 1];
        }

        return string.Empty;
    }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [DisplayName("Data annunzio")]
    public DateTime? DataAnnunzio { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [DisplayName("Codice materia")]
    public string CodiceMateria { get; set; }
    public string BURL { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Protocollo { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? IDTipo_Risposta_Effettiva { get; set; }

    public bool Pubblicato { get; set; }
    public bool Sollecito { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [DisplayName("Tipo chiusura iter")]
    public int? TipoChiusuraIter { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [DisplayName("Data chiusura iter")]
    public DateTime? DataChiusuraIter { get; set; }

    [DisplayName("Emendato")] public bool Emendato { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [DisplayName("Tipo votazione")]
    public int? TipoVotazioneIter { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [DisplayName("Area tematica")]
    public string AreaTematica { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [DisplayName("Altri Soggetti")]
    public string AltriSoggetti { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DisplayTipoRispostaRichiesta { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DisplayStato { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DisplayAreaPolitica { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DisplayTipo { get; set; }

    public List<AttiRisposteDto> Risposte { get; set; } = new List<AttiRisposteDto>();
    public List<AttiMonitoraggioDto> Monitoraggi { get; set; } = new List<AttiMonitoraggioDto>();
    public List<AttiDocumentiDto> Documenti { get; set; } = new List<AttiDocumentiDto>();
    public List<NoteDto> Note { get; set; } = new List<NoteDto>();

    [DisplayName("Abbinamenti")]
    public List<AttiAbbinamentoDto> Abbinamenti { get; set; } = new List<AttiAbbinamentoDto>();

    public string DisplayTipoChiusuraIter { get; set; }
    public string DisplayTipoVotazioneIter { get; set; }
    [DisplayName("DCR/DCCR")] public int? DCR { get; set; }
    public int? DCCR { get; set; }
    public string DCRL { get; set; }

    [DisplayName("Privacy - dati giudiziari")]
    public bool Privacy_Dati_Personali_Giudiziari { get; set; }

    [DisplayName("Privacy - dati sanitari")]
    public bool Privacy_Divieto_Pubblicazione_Salute { get; set; }

    [DisplayName("Privacy - dati di natura sessuale")]
    public bool Privacy_Divieto_Pubblicazione_Vita_Sessuale { get; set; }

    [DisplayName("Privacy - divieto di pubblicazione")]
    public bool Privacy_Divieto_Pubblicazione { get; set; }

    [DisplayName("Privacy - dati sensibili")]
    public bool Privacy_Dati_Personali_Sensibili { get; set; }

    [DisplayName("Privicy - altri motivi")]
    public bool Privacy_Divieto_Pubblicazione_Altri { get; set; }

    [DisplayName("Privacy - dati personali semplici")]
    public bool Privacy_Dati_Personali_Semplici { get; set; }

    [DisplayName("Privacy")] public bool Privacy { get; set; }

    [DisplayName("Data comunicazione assemblea")]public DateTime? DataComunicazioneAssemblea { get; set; }
    [DisplayName("Data risposta")]public DateTime? DataRisposta { get; set; }
    [DisplayName("Data trattazione risposta")]public DateTime? DataTrattazione { get; set; }
    [DisplayName("Iter multiplo")]public bool IterMultiplo { get; set; } = false;

    [DisplayName("Commissioni proponenti")]
    public List<KeyValueDto> CommissioniProponenti { get; set; } = new List<KeyValueDto>();

    [DisplayName("Impegni scadenze")] public string ImpegniScadenze { get; set; }
    [DisplayName("Stato attuazione")] public string StatoAttuazione { get; set; }
    [DisplayName("Competenza monitoraggio")] public string CompetenzaMonitoraggio { get; set; }
    public IEnumerable<PersonaLightDto> Relatori { get; set; } = new List<PersonaLightDto>();
    public string CommissioniProponenti_string { get; set; }
}