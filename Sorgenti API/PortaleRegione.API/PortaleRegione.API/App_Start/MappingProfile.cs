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

using AutoMapper;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Model;

namespace PortaleRegione.API
{
    /// <summary>
    ///     Classe che indica ad AutoMapper come mappare gli oggetti in uscita/entrata
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        ///     CTOR
        /// </summary>
        public MappingProfile()
        {
            #region SEDUTE

            CreateMap<SEDUTE, SeduteDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SeduteDto, SEDUTE>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SEDUTE, SeduteFormUpdateDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SeduteFormUpdateDto, SEDUTE>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SeduteDto, SeduteFormUpdateDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SeduteFormUpdateDto, SeduteDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<legislature, LegislaturaDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<LegislaturaDto, legislature>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region ATTI

            CreateMap<ATTI, AttiDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AttiDto, ATTI>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ATTI, AttiFormUpdateModel>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AttiFormUpdateModel, ATTI>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TIPI_ATTO, Tipi_AttoDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Tipi_AttoDto, TIPI_ATTO>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region PERSONE / GRUPPI

            CreateMap<View_UTENTI, PersonaDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaDto, View_UTENTI>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<UTENTI_NoCons, PersonaDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaDto, UTENTI_NoCons>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<View_Composizione_GiuntaRegionale, PersonaDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaDto, View_Composizione_GiuntaRegionale>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<View_UTENTI, PersonaLightDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaLightDto, View_UTENTI>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaDto, PersonaLightDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaLightDto, PersonaDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaDto, PersonaExtraLightDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaExtraLightDto, PersonaDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<RUOLI, RuoliDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<RuoliDto, RUOLI>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<gruppi_politici, GruppiDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<GruppiDto, gruppi_politici>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<View_gruppi_politici_con_giunta, GruppiDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<GruppiDto, View_gruppi_politici_con_giunta>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<View_PINS, PinDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PinDto, View_PINS>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<JOIN_GRUPPO_AD, GruppoAD_Dto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<GruppoAD_Dto, JOIN_GRUPPO_AD>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region EMENDAMENTI

            CreateMap<EM, EmendamentiDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentiDto, EM>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EM, EmendamentoLightDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentoLightDto, EM>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EM, EmendamentoExtraLightDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentiDto, EmendamentoExtraLightDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentoExtraLightDto, EM>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentoExtraLightDto, EmendamentiDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentiDto, EmendamentoLightDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentoLightDto, EmendamentiDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentiDto, MetaDatiEMDto>();
            CreateMap<MetaDatiEMDto, EmendamentiDto>();
            CreateMap<EM, MetaDatiEMDto>();
            CreateMap<MetaDatiEMDto, EM>();
            CreateMap<FIRME, FirmeDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<FirmeDto, FIRME>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ARTICOLI, ArticoliDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ArticoliDto, ARTICOLI>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<COMMI, CommiDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<CommiDto, COMMI>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<LETTERE, LettereDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<LettereDto, LETTERE>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PARTI_TESTO, PartiTestoDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PartiTestoDto, PARTI_TESTO>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<STATI_EM, StatiDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<StatiDto, STATI_EM>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TIPI_EM, Tipi_EmendamentiDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Tipi_EmendamentiDto, TIPI_EM>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<MISSIONI, MissioniDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<MissioniDto, MISSIONI>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TITOLI_MISSIONI, TitoloMissioniDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TitoloMissioniDto, TITOLI_MISSIONI>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region STAMPE

            CreateMap<STAMPE, StampaDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<StampaDto, STAMPE>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<STAMPE_INFO, Stampa_InfoDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Stampa_InfoDto, STAMPE_INFO>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region NOTIFICHE

            CreateMap<NOTIFICHE, NotificaDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<NotificaDto, NOTIFICHE>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TIPI_NOTIFICA, TipoNotificaDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TipoNotificaDto, TIPI_NOTIFICA>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<NOTIFICHE_DESTINATARI, DestinatariNotificaDto>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<DestinatariNotificaDto, NOTIFICHE_DESTINATARI>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region DASI

            CreateMap<ATTI_DASI, AttoDASIDto>().ForMember(x => x.FirmeCartacee, opt => opt.Ignore())
                .ForMember(x => x.IsChiuso, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AttoDASIDto, ATTI_DASI>().ForMember(x => x.FirmeCartacee, opt => opt.Ignore())
                .ForMember(x => x.IsChiuso, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ATTI_FIRME, AttiFirmeDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AttiFirmeDto, ATTI_FIRME>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<View_cariche_assessori_in_carica, AssessoreInCaricaDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<View_Commissioni_attive, OrganoDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<ATTI_NOTE, NoteDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<NoteDto, ATTI_NOTE>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            CreateMap<TAGS, TagDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TagDto, TAGS>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<View_Conteggi_EM_Gruppi_Politici, View_Conteggi_EM_Gruppi_PoliticiDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<View_Conteggi_EM_Gruppi_PoliticiDto, View_Conteggi_EM_Gruppi_Politici>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<View_Conteggi_EM_Area_Politica, View_Conteggi_EM_Area_PoliticaDto>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<View_Conteggi_EM_Area_PoliticaDto, View_Conteggi_EM_Area_Politica>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}