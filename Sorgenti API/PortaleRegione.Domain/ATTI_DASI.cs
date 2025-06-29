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
using PortaleRegione.DTO.Enum;

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
        public Guid? UID_MOZ_Abbinata { get; set; }
        public Guid? UID_Atto_ODG { get; set; }
        public int Tipo { get; set; }
        public int TipoMOZ { get; set; } = 0;
        public int? Progressivo { get; set; }
        public string Etichetta { get; set; }

        public string Oggetto { get; set; }
        public string Oggetto_Modificato { get; set; }
        public string Oggetto_Approvato { get; set; }
        public string Premesse { get; set; }
        public string Premesse_Modificato { get; set; }
        public string Richiesta { get; set; }
        public string Richiesta_Modificata { get; set; }
        public string NAtto { get; set; }
        public DateTime DataCreazione { get; set; }
        public Guid UIDPersonaCreazione { get; set; }
        public int idRuoloCreazione { get; set; }
        public DateTime? DataModifica { get; set; }
        public Guid? UIDPersonaModifica { get; set; }
        public string DataPresentazione { get; set; }
        public string DataPresentazione_MOZ { get; set; }
        public string DataPresentazione_MOZ_URGENTE { get; set; }
        public string DataPresentazione_MOZ_ABBINATA { get; set; }
        public string DataRichiestaIscrizioneSeduta { get; set; }
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

        public DateTime? DataIscrizioneSeduta { get; set; }
        public Guid? UIDPersonaIscrizioneSeduta { get; set; }
        public Guid? UIDPersonaRichiestaIscrizione { get; set; }
        public bool Non_Passaggio_In_Esame { get; set; } = false;
        public int NAtto_search { get; set; } = 0;
        public bool Inviato_Al_Protocollo { get; set; } = false;
        public DateTime? DataInvioAlProtocollo { get; set; }
        public bool CapogruppoNeiTermini { get; set; } = false;
        public bool MOZU_Capigruppo { get; set; }

        // Matteo Cattapan #520
        public string FirmeCartacee { get; set; }

        // #558

        public bool IsChiuso => IDStato == (int)StatiAttoEnum.COMPLETATO;


        public DateTime? DataAnnunzio { get; set; }
        public string CodiceMateria { get; set; }
        public string Protocollo { get; set; } = string.Empty;
        public int? IDTipo_Risposta_Effettiva { get; set; }
        public bool Pubblicato { get; set; }
        public bool Sollecito { get; set; }
        public int? TipoChiusuraIter { get; set; }
        public DateTime? DataChiusuraIter { get; set; }
        public string NoteChiusuraIter { get; set; }
        public bool Emendato { get; set; }
        public int? TipoVotazioneIter { get; set; }
        public string AreaTematica { get; set; }
        public string AltriSoggetti { get; set; }

        public int? DCR { get; set; } = 0;
        public int? DCCR { get; set; } = 0;
        public string DCRL { get; set; }
        public string BURL { get; set; }

        public bool Privacy_Dati_Personali_Giudiziari { get; set; }
        public bool Privacy_Divieto_Pubblicazione_Salute { get; set; }
        public bool Privacy_Divieto_Pubblicazione_Vita_Sessuale { get; set; }
        public bool Privacy_Divieto_Pubblicazione { get; set; }
        public bool Privacy_Dati_Personali_Sensibili { get; set; }
        public bool Privacy_Divieto_Pubblicazione_Altri { get; set; }
        public bool Privacy_Dati_Personali_Semplici { get; set; }
        public bool Privacy { get; set; }

        public DateTime? DataComunicazioneAssemblea { get; set; }

        public bool IterMultiplo { get; set; } = false;
        public string ImpegniScadenze { get; set; }
        public string StatoAttuazione { get; set; }
        public string CompetenzaMonitoraggio { get; set; }
        public bool MonitoraggioConcluso { get; set; }
        public DateTime? DataTrasmissioneMonitoraggio { get; set; }

        public Guid? UIDPersonaRelatore1 { get; set; }
        public Guid? UIDPersonaRelatore2 { get; set; }
        public Guid? UIDPersonaRelatoreMinoranza { get; set; }
        public DateTime? DataSedutaRisposta { get; set; }
        public DateTime? DataComunicazioneAssembleaRisposta { get; set; }
        public DateTime? DataProposta { get; set; }
        public DateTime? DataTrasmissione { get; set; }
        public int? TipoChiusuraIterCommissione { get; set; }
        public DateTime? DataChiusuraIterCommissione { get; set; }
        public int? TipoVotazioneIterCommissione { get; set; }
        public int? RisultatoVotazioneIterCommissione { get; set; }
        public bool FlussoRespingi { get; set; } = false;

        public int Ritardo { get; set; } = 0;
        public Guid? UIDPersonaFlussoRespingi { get; set; }
        public DateTime? DataFlussoRespingi { get; set; }

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

        public string OggettoView()
        {
            if (!string.IsNullOrEmpty(Oggetto_Approvato))
                return Oggetto_Approvato;
            if (!string.IsNullOrEmpty(Oggetto_Modificato))
                return Oggetto_Modificato;
            return Oggetto;
        }
    }
}