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
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;

namespace PortaleRegione.DTO.Domain
{
    public class AttoDASIDto
    {
        public AttoDASIDto()
        {
            FirmeCartacee = new List<KeyValueDto>();
            FirmeAnte = new List<AttiFirmeDto>();
            FirmePost = new List<AttiFirmeDto>();
        }

        public Guid UIDAtto { get; set; }

        [DisplayName("Seduta")] public Guid? UIDSeduta { get; set; }
        public Guid? UID_MOZ_Abbinata { get; set; }
        public Guid? UID_Atto_ODG { get; set; }
        public string Etichetta { get; set; }

        public string Oggetto { get; set; }
        public string Oggetto_Modificato { get; set; }
        public string Oggetto_Privacy { get; set; }

        [AllowHtml] public string Premesse { get; set; }

        [AllowHtml] public string Premesse_Modificato { get; set; }

        [AllowHtml] public string Richiesta { get; set; }

        [AllowHtml] public string Richiesta_Modificata { get; set; }


        [DisplayName("Tipo")] public int Tipo { get; set; }

        [DisplayName("Tipo mozione")] public int TipoMOZ { get; set; } = 0;

        [DisplayName("Numero atto")] public string NAtto { get; set; }
        public DateTime DataCreazione { get; set; }
        public Guid UIDPersonaCreazione { get; set; }
        public int idRuoloCreazione { get; set; }
        public DateTime? DataModifica { get; set; }
        public Guid? UIDPersonaModifica { get; set; }

        [DisplayName("Data presentazione")] public string DataPresentazione { get; set; }
        public string DataPresentazione_MOZ { get; set; }
        public string DataPresentazione_MOZ_URGENTE { get; set; }
        public string DataPresentazione_MOZ_ABBINATA { get; set; }
        public string DataRichiestaIscrizioneSeduta { get; set; }
        public Guid? UIDPersonaRichiestaIscrizione { get; set; }

        [DisplayName("Proponente")] public Guid? UIDPersonaProponente { get; set; }
        public Guid? UIDPersonaPrimaFirma { get; set; }
        public DateTime DataPrimaFirma { get; set; }
        public Guid? UIDPersonaPresentazione { get; set; }
        public bool Proietta { get; set; } = false;
        public DateTime? DataProietta { get; set; }
        public Guid? UIDPersonaProietta { get; set; }
        public DateTime? DataRitiro { get; set; }
        public Guid? UIDPersonaRitiro { get; set; }
        public string Hash { get; set; }

        [DisplayName("Tipo risposta")] public int IDTipo_Risposta { get; set; }
        public int OrdineVisualizzazione { get; set; }

        [DisplayName("Allegato")] public string PATH_AllegatoGenerico { get; set; }


        public string Note_Pubbliche { get; set; }
        public string Note_Private { get; set; }

        [DisplayName("Stato")] public int IDStato { get; set; }
        public bool Firma_su_invito { get; set; } = false;
        public Guid UID_QRCode { get; set; }
        public int AreaPolitica { get; set; }
        public bool Firma_da_ufficio { get; set; } = false;
        public bool Firmato_Da_Me { get; set; } = false;
        public bool Firmato_Dal_Proponente { get; set; } = false;
        public bool Presentabile { get; set; } = false;
        public int Progressivo { get; set; }

        [DisplayName("Legislatura")] public int Legislatura { get; set; }

        [JsonIgnore] public HttpPostedFileBase DocAllegatoGenerico { get; set; }

        public byte[] DocAllegatoGenerico_Stream { get; set; }
        public string Atto_Certificato { get; set; } = "";
        public string BodyAtto { get; set; }
        public string Firme { get; set; }
        public DateTime Timestamp { get; set; }
        public string Firme_dopo_deposito { get; set; }
        public string Destinatari { get; set; }
        public PersonaLightDto PersonaModifica { get; set; }
        public PersonaLightDto PersonaProponente { get; set; }
        public PersonaLightDto PersonaCreazione { get; set; }
        public int ConteggioFirme { get; set; }
        public GruppiDto gruppi_politici { get; set; }
        public bool Firmabile { get; set; }
        public bool Eliminabile { get; set; }
        public bool Ritirabile { get; set; }
        public bool Modificabile { get; set; }
        public int id_gruppo { get; set; }
        public List<CommissioneDto> Commissioni { get; set; }
        public string Commissioni_client { get; set; }
        public SeduteDto Seduta { get; set; }

        [DisplayName("Data iscrizione in seduta")]
        public DateTime? DataIscrizioneSeduta { get; set; }

        public bool Invito_Abilitato { get; set; } = false;
        public bool PresentatoOltreITermini { get; set; } = false;
        public bool Non_Passaggio_In_Esame { get; set; } = false;

        public string MOZ_Abbinata { get; set; }
        public string ODG_Atto_PEM { get; set; }
        public string ODG_Atto_Oggetto_PEM { get; set; }

        public bool Inviato_Al_Protocollo { get; set; } = false;
        public DateTime? DataInvioAlProtocollo { get; set; }
        public bool CapogruppoNeiTermini { get; set; } = false;
        public bool MOZU_Capigruppo { get; set; } = false;

        public string DettaglioMozioniAbbinate { get; set; }

        public string Display { get; set; }

        // Matteo Cattapan #520
        public List<KeyValueDto> FirmeCartacee { get; set; }
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

        public bool IsChiuso => IDStato == (int)StatiAttoEnum.CHIUSO
                                || IDStato == (int)StatiAttoEnum.CHIUSO_RITIRATO
                                || IDStato == (int)StatiAttoEnum.CHIUSO_DECADUTO;

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

        public string DisplayTipoRispostaRichiesta { get; set; }
        public string DisplayStato { get; set; }
    }
}