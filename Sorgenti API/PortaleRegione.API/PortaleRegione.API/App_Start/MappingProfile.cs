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

            Mapper.CreateMap<SEDUTE, SeduteDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<SeduteDto, SEDUTE>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<SEDUTE, SeduteFormUpdateDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<SeduteFormUpdateDto, SEDUTE>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<SeduteDto, SeduteFormUpdateDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<SeduteFormUpdateDto, SeduteDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<legislature, LegislaturaDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<LegislaturaDto, legislature>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));

            #endregion

            #region ATTI

            Mapper.CreateMap<ATTI, AttiDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<AttiDto, ATTI>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<ATTI, AttiFormUpdateModel>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<AttiFormUpdateModel, ATTI>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<TIPI_ATTO, Tipi_AttoDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<Tipi_AttoDto, TIPI_ATTO>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));

            #endregion

            #region PERSONE / GRUPPI

            Mapper.CreateMap<View_UTENTI, PersonaDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<PersonaDto, View_UTENTI>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<UTENTI_NoCons, PersonaDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<PersonaDto, UTENTI_NoCons>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<View_Composizione_GiuntaRegionale, PersonaDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<PersonaDto, View_Composizione_GiuntaRegionale>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<View_UTENTI, PersonaLightDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<PersonaLightDto, View_UTENTI>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<PersonaDto, PersonaLightDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<PersonaLightDto, PersonaDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<PersonaDto, PersonaExtraLightDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<PersonaExtraLightDto, PersonaDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<RUOLI, RuoliDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<RuoliDto, RUOLI>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<gruppi_politici, GruppiDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<GruppiDto, gruppi_politici>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<View_gruppi_politici_con_giunta, GruppiDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<GruppiDto, View_gruppi_politici_con_giunta>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<View_PINS, PinDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<PinDto, View_PINS>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));

            Mapper.CreateMap<JOIN_GRUPPO_AD, GruppoAD_Dto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<GruppoAD_Dto, JOIN_GRUPPO_AD>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            #endregion

            #region EMENDAMENTI

            Mapper.CreateMap<EM, EmendamentiDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<EmendamentiDto, EM>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<EM, EmendamentoLightDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<EmendamentoLightDto, EM>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<EM, EmendamentoExtraLightDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<EmendamentiDto, EmendamentoExtraLightDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<EmendamentoExtraLightDto, EM>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<EmendamentoExtraLightDto, EmendamentiDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<EmendamentiDto, EmendamentoLightDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<EmendamentoLightDto, EmendamentiDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<EmendamentiDto, MetaDatiEMDto>();
            Mapper.CreateMap<MetaDatiEMDto, EmendamentiDto>();
            Mapper.CreateMap<EM, MetaDatiEMDto>();
            Mapper.CreateMap<MetaDatiEMDto, EM>();
            Mapper.CreateMap<FIRME, FirmeDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<FirmeDto, FIRME>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<ARTICOLI, ArticoliDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<ArticoliDto, ARTICOLI>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<COMMI, CommiDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<CommiDto, COMMI>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<LETTERE, LettereDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<LettereDto, LETTERE>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<PARTI_TESTO, PartiTestoDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<PartiTestoDto, PARTI_TESTO>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<STATI_EM, StatiDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<StatiDto, STATI_EM>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<TIPI_EM, Tipi_EmendamentiDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<Tipi_EmendamentiDto, TIPI_EM>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<MISSIONI, MissioniDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<MissioniDto, MISSIONI>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<TITOLI_MISSIONI, TitoloMissioniDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<TitoloMissioniDto, TITOLI_MISSIONI>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));

            #endregion

            #region STAMPE

            Mapper.CreateMap<STAMPE, StampaDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<StampaDto, STAMPE>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<STAMPE_INFO, Stampa_InfoDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<Stampa_InfoDto, STAMPE_INFO>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));

            #endregion

            #region NOTIFICHE

            Mapper.CreateMap<NOTIFICHE, NotificaDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<NotificaDto, NOTIFICHE>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<TIPI_NOTIFICA, TipoNotificaDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<TipoNotificaDto, TIPI_NOTIFICA>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<NOTIFICHE_DESTINATARI, DestinatariNotificaDto>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<DestinatariNotificaDto, NOTIFICHE_DESTINATARI>()
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));

            #endregion

            #region DASI

            Mapper.CreateMap<ATTI_DASI, AttoDASIDto>().ForMember(x => x.FirmeCartacee, opt => opt.Ignore())
                .ForMember(x => x.IsChiuso, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<AttoDASIDto, ATTI_DASI>().ForMember(x => x.FirmeCartacee, opt => opt.Ignore())
                .ForMember(x => x.IsChiuso, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<ATTI_FIRME, AttiFirmeDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<AttiFirmeDto, ATTI_FIRME>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));

            Mapper.CreateMap<View_cariche_assessori_in_carica, AssessoreInCaricaDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<View_Commissioni_attive, OrganoDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));

            Mapper.CreateMap<ATTI_NOTE, NoteDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<NoteDto, ATTI_NOTE>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            #endregion

            Mapper.CreateMap<TAGS, TagDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<TagDto, TAGS>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));

            Mapper.CreateMap<View_Conteggi_EM_Gruppi_Politici, View_Conteggi_EM_Gruppi_PoliticiDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<View_Conteggi_EM_Gruppi_PoliticiDto, View_Conteggi_EM_Gruppi_Politici>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));

            Mapper.CreateMap<View_Conteggi_EM_Area_Politica, View_Conteggi_EM_Area_PoliticaDto>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
            Mapper.CreateMap<View_Conteggi_EM_Area_PoliticaDto, View_Conteggi_EM_Area_Politica>().ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
        }
    }
}