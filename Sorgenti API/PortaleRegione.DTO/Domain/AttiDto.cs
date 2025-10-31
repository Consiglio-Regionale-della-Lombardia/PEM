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

using PortaleRegione.DTO.Domain.Essentials;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PortaleRegione.DTO.Domain
{
    public class AttiDto
    {
        public AttiDto(Guid uidAtto, string nAtto, int idTipoAtto, string oggetto, string note, string pathTestoAtto, Guid? uidSeduta, int? legislatura, DateTime? dataApertura, DateTime? dataChiusura, bool visMisProg, Guid? uidAssessoreRiferimento, bool notificaDepositoDifferita, bool invioNotificheDepositoSoloUola, bool? ordinePresentazione, bool? ordineVotazione, int? priorita, DateTime? dataCreazione, Guid? uidPersonaCreazione, DateTime? dataModifica, Guid? uidPersonaModifica, bool? eliminato, string linkFascicoloPresentazione, DateTime? dataCreazionePresentazione, string linkFascicoloVotazione, DateTime? dataCreazioneVotazione, DateTime? dataUltimaModificaEm, Tipi_AttoDto tipiAtto, SeduteDto sedute, int conteggioEm, int conteggioSubEm, IEnumerable<PersonaLightDto> relatori, bool informazioniMancanti, bool canMoveDown, bool canMoveUp, int stato, PersonaLightDto personaAssessore, bool bloccoOdg, bool bloccoEm, bool jolly, bool emendabile, int counterOdg, bool fascicoliDaAggiornare)
        {
            UIDAtto = uidAtto;
            NAtto = nAtto;
            IDTipoAtto = idTipoAtto;
            Oggetto = oggetto;
            Note = note;
            Path_Testo_Atto = pathTestoAtto;
            UIDSeduta = uidSeduta;
            Legislatura = legislatura;
            Data_apertura = dataApertura;
            Data_chiusura = dataChiusura;
            VIS_Mis_Prog = visMisProg;
            UIDAssessoreRiferimento = uidAssessoreRiferimento;
            Notifica_deposito_differita = notificaDepositoDifferita;
            Invio_Notifiche_Deposito_Solo_UOLA = invioNotificheDepositoSoloUola;
            OrdinePresentazione = ordinePresentazione;
            OrdineVotazione = ordineVotazione;
            Priorita = priorita;
            DataCreazione = dataCreazione;
            UIDPersonaCreazione = uidPersonaCreazione;
            DataModifica = dataModifica;
            UIDPersonaModifica = uidPersonaModifica;
            Eliminato = eliminato;
            LinkFascicoloPresentazione = linkFascicoloPresentazione;
            DataCreazionePresentazione = dataCreazionePresentazione;
            LinkFascicoloVotazione = linkFascicoloVotazione;
            DataCreazioneVotazione = dataCreazioneVotazione;
            DataUltimaModificaEM = dataUltimaModificaEm;
            TIPI_ATTO = tipiAtto;
            SEDUTE = sedute;
            Conteggio_EM = conteggioEm;
            Conteggio_SubEM = conteggioSubEm;
            Relatori = relatori;
            Informazioni_Mancanti = informazioniMancanti;
            CanMoveDown = canMoveDown;
            CanMoveUp = canMoveUp;
            Stato = stato;
            PersonaAssessore = personaAssessore;
            BloccoODG = bloccoOdg;
            BloccoEM = bloccoEm;
            Jolly = jolly;
            Emendabile = emendabile;
            CounterODG = counterOdg;
            Fascicoli_Da_Aggiornare = fascicoliDaAggiornare;
        }

        public bool Chiuso => Data_chiusura.HasValue && Data_chiusura.Value < DateTime.Now;

        [Key] public Guid UIDAtto { get; set; }

        [Required] [StringLength(50)] public string NAtto { get; set; }

        public int IDTipoAtto { get; set; }

        [AllowHtml] [StringLength(500)] public string Oggetto { get; set; }

        [AllowHtml] [StringLength(255)] public string Note { get; set; }

        public string Path_Testo_Atto { get; set; }

        public Guid? UIDSeduta { get; set; }
        
        public int? Legislatura { get; set; }

        public DateTime? Data_apertura { get; set; }

        public DateTime? Data_chiusura { get; set; }

        public bool VIS_Mis_Prog { get; set; }

        public Guid? UIDAssessoreRiferimento { get; set; }

        public bool Notifica_deposito_differita { get; set; }

        public bool Invio_Notifiche_Deposito_Solo_UOLA { get; set; } = false;

        public bool? OrdinePresentazione { get; set; } = false;

        public bool? OrdineVotazione { get; set; } = false;

        public int? Priorita { get; set; }

        public DateTime? DataCreazione { get; set; }

        public Guid? UIDPersonaCreazione { get; set; }

        public DateTime? DataModifica { get; set; }

        public Guid? UIDPersonaModifica { get; set; }

        public bool? Eliminato { get; set; }

        public string LinkFascicoloPresentazione { get; set; }

        public DateTime? DataCreazionePresentazione { get; set; }

        public string LinkFascicoloVotazione { get; set; }

        public DateTime? DataCreazioneVotazione { get; set; }

        public DateTime? DataUltimaModificaEM { get; set; }

        public virtual Tipi_AttoDto TIPI_ATTO { get; set; }
        public virtual SeduteDto SEDUTE { get; set; }

        public int Conteggio_EM { get; set; }
        public int Conteggio_SubEM { get; set; }
        public IEnumerable<PersonaLightDto> Relatori { get; set; }
        public bool Informazioni_Mancanti { get; set; }
        public bool CanMoveDown { get; set; } = false;
        public bool CanMoveUp { get; set; } = false;
        public int Stato { get; set; }
        public PersonaLightDto PersonaAssessore { get; set; }
        public bool BloccoODG { get; set; } = false;
        public bool BloccoEM { get; set; } = false;
        public bool Jolly { get; set; } = false;
        public bool Emendabile { get; set; } = false;
        public int CounterODG { get; set; } = 0;
        public bool Fascicoli_Da_Aggiornare { get; set; } = false;

        public bool IsAperto()
        {
            //Atto chiuso
            if (Data_chiusura.HasValue)
                return false;
            //Atto non aperto
            if (!Data_apertura.HasValue)
                return false;
            //Atto non ancora aperto
            if (Data_apertura > DateTime.Now)
                return false;

            //Atto aperto
            return true;
        }
    }
}