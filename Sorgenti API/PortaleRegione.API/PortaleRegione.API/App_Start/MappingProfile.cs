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

            CreateMap<SEDUTE, SeduteDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SeduteDto, SEDUTE>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SEDUTE, SeduteFormUpdateDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SeduteFormUpdateDto, SEDUTE>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SeduteDto, SeduteFormUpdateDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SeduteFormUpdateDto, SeduteDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<legislature, LegislaturaDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<LegislaturaDto, legislature>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region ATTI

            CreateMap<ATTI, AttiDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AttiDto, ATTI>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ATTI, AttiFormUpdateModel>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AttiFormUpdateModel, ATTI>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TIPI_ATTO, Tipi_AttoDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Tipi_AttoDto, TIPI_ATTO>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region PERSONE / GRUPPI

            CreateMap<View_UTENTI, PersonaDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaDto, View_UTENTI>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<UTENTI_NoCons, PersonaDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaDto, UTENTI_NoCons>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<View_Composizione_GiuntaRegionale, PersonaDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaDto, View_Composizione_GiuntaRegionale>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<View_UTENTI, PersonaLightDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaLightDto, View_UTENTI>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaDto, PersonaLightDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaLightDto, PersonaDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaDto, PersonaExtraLightDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PersonaExtraLightDto, PersonaDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<RUOLI, RuoliDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<RuoliDto, RUOLI>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<gruppi_politici, GruppiDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<GruppiDto, gruppi_politici>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<View_gruppi_politici_con_giunta, GruppiDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<GruppiDto, View_gruppi_politici_con_giunta>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<View_PINS, PinDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PinDto, View_PINS>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<JOIN_GRUPPO_AD, GruppoAD_Dto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<GruppoAD_Dto, JOIN_GRUPPO_AD>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region EMENDAMENTI

            CreateMap<EM, EmendamentiDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentiDto, EM>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EM, EmendamentoLightDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentoLightDto, EM>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EM, EmendamentoExtraLightDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentiDto, EmendamentoExtraLightDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentoExtraLightDto, EM>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentoExtraLightDto, EmendamentiDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentiDto, EmendamentoLightDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentoLightDto, EmendamentiDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmendamentiDto, MetaDatiEMDto>(MemberList.None);
            CreateMap<MetaDatiEMDto, EmendamentiDto>(MemberList.None);
            CreateMap<EM, MetaDatiEMDto>(MemberList.None);
            CreateMap<MetaDatiEMDto, EM>(MemberList.None);
            CreateMap<FIRME, FirmeDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<FirmeDto, FIRME>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ARTICOLI, ArticoliDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ArticoliDto, ARTICOLI>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<COMMI, CommiDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<CommiDto, COMMI>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<LETTERE, LettereDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<LettereDto, LETTERE>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PARTI_TESTO, PartiTestoDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<PartiTestoDto, PARTI_TESTO>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<STATI_EM, StatiDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<StatiDto, STATI_EM>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TIPI_EM, Tipi_EmendamentiDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Tipi_EmendamentiDto, TIPI_EM>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<MISSIONI, MissioniDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<MissioniDto, MISSIONI>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TITOLI_MISSIONI, TitoloMissioniDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TitoloMissioniDto, TITOLI_MISSIONI>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region STAMPE

            CreateMap<STAMPE, StampaDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<StampaDto, STAMPE>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<STAMPE_INFO, Stampa_InfoDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Stampa_InfoDto, STAMPE_INFO>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region NOTIFICHE

            CreateMap<NOTIFICHE, NotificaDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<NotificaDto, NOTIFICHE>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TIPI_NOTIFICA, TipoNotificaDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TipoNotificaDto, TIPI_NOTIFICA>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<NOTIFICHE_DESTINATARI, DestinatariNotificaDto>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<DestinatariNotificaDto, NOTIFICHE_DESTINATARI>(MemberList.None)
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            #endregion

            #region DASI

            CreateMap<ATTI_DASI, AttoDASIDto>(MemberList.None).ForMember(x => x.FirmeCartacee, opt => opt.Ignore())
                .ForMember(x => x.IsChiuso, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AttoDASIDto, ATTI_DASI>(MemberList.None).ForMember(x => x.FirmeCartacee, opt => opt.Ignore())
                .ForMember(x => x.IsChiuso, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ATTI_FIRME, AttiFirmeDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AttiFirmeDto, ATTI_FIRME>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<View_cariche_assessori_in_carica, AssessoreInCaricaDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<View_Commissioni_attive, OrganoDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<ATTI_NOTE, NoteDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<NoteDto, ATTI_NOTE>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            CreateMap<TAGS, TagDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TagDto, TAGS>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<View_Conteggi_EM_Gruppi_Politici, View_Conteggi_EM_Gruppi_PoliticiDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<View_Conteggi_EM_Gruppi_PoliticiDto, View_Conteggi_EM_Gruppi_Politici>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<View_Conteggi_EM_Area_Politica, View_Conteggi_EM_Area_PoliticaDto>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<View_Conteggi_EM_Area_PoliticaDto, View_Conteggi_EM_Area_Politica>(MemberList.None).ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}