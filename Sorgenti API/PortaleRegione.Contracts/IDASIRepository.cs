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
using System.Threading.Tasks;
using ExpressionBuilder.Generics;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;

namespace PortaleRegione.Contracts
{
    public interface IDASIRepository : IRepository<ATTI_DASI>
    {
        Task<ATTI_DASI> Get(Guid attoUId);

        Task<List<Guid>> GetAll(PersonaDto currentUser, int page, int size, ClientModeEnum mode,
            Filter<ATTI_DASI> filtro, QueryExtendedRequest queryExtended, List<SortingInfo> dettaglioOrdinamento);

        Task<int> Count(Filter<ATTI_DASI> queryFilter);

        Task<ATTI_DASI_CONTATORI> GetContatore(int tipo, int tipo_risposta);
        bool CheckIfPresentabile(AttoDASIDto dto, PersonaDto persona);
        bool CheckIfRitirabile(AttoDASIDto dto, PersonaDto persona);
        bool CheckIfEliminabile(AttoDASIDto dto, PersonaDto persona);
        bool CheckIfModificabile(AttoDASIDto dto, PersonaDto persona);
        Task<int> GetProgressivo(TipoAttoEnum tipo, int gruppoId, int legislatura);
        Task<bool> CheckProgressivo(string etichettaEncrypt);
        void IncrementaContatore(ATTI_DASI_CONTATORI contatore, int salto = 1);
        Task<List<View_cariche_assessori_in_carica>> GetSoggettiInterrogabili();
        Task<List<View_Commissioni_attive>> GetCommissioniAttive();
        Task RimuoviCommissioni(Guid UidAtto);
        Task RimuoviCommissioniProponenti(Guid UidAtto);
        void AggiungiCommissione(Guid UidAtto, int organo);
        Task AggiungiCommissioneProponente(Guid UidAtto, KeyValueDto organo);
        Task<List<View_Commissioni_attive>> GetCommissioni(Guid uidAtto);
        Task RimuoviSoggetti(Guid UidAtto);
        void AggiungiSoggetto(Guid UidAtto, int soggetto);
        Task<List<View_cariche_assessori_in_carica>> GetSoggettiInterrogati(Guid uidAtto);
        Task<IEnumerable<NOTIFICHE_DESTINATARI>> GetInvitati(Guid attoUId);
        Task<int> CountByQuery(ByQueryModel model);
        List<Guid> GetByQuery(ByQueryModel model);
        Task<List<ATTI_DASI>> GetMOZAbbinabili(Guid sedutaUId);
        Task<List<ATTI_DASI>> GetAttiBySeduta(Guid uidSeduta, TipoAttoEnum tipo, TipoMOZEnum tipoMoz);
        Task<List<ATTI_DASI>> GetProposteAtti(int gruppoId, TipoAttoEnum tipo, TipoMOZEnum tipoMoz);
        Task<List<ATTI_DASI>> GetProposteAtti(string dataRichiesta, TipoAttoEnum tipo, TipoMOZEnum tipoMoz);
        Task<int> CountODGByAttoPEM(Guid appoggioUidAtto);
        Task<bool> CheckIscrizioneSedutaIQT(string dataRichiesta, Guid uidPersona);
        Task<bool> CheckMOZUrgente(SEDUTE seduta, string dataSedutaEncrypt, Guid personaUID);
        Task<bool> CheckIfFirmatoDaiCapigruppo(Guid uidAtto);

        Task<string> GetAll_Query(PersonaDto persona, ClientModeEnum mode, Filter<ATTI_DASI> filtro, List<int> soggetti,
            List<int> stati);

        Task<List<Guid>> GetAbbinamentiMozione(Guid uidAtto);
        Task<List<Guid>> GetAllCartacei(int legislatura);
        Task<List<Guid>> GetAttiProponente(Guid personaUid);
        Task<List<AttiRisposteDto>> GetRisposte(Guid uidAtto);
        Task<List<AttiMonitoraggioDto>> GetMonitoraggi(Guid uidAtto);
        Task<List<AttiDocumentiDto>> GetDocumenti(Guid uidAtto);
        Task<List<NoteDto>> GetNote(Guid uidAtto);
        Task<List<AttiAbbinamentoDto>> GetAbbinamenti(Guid uidAtto);
        Task<List<AttoLightDto>> GetAbbinamentiDisponibili(int legislaturaId, int page, int size);
        Task<List<OrganoDto>> GetOrganiDisponibili(int legislaturaId);

        Task<int> CountByTipo(PersonaDto persona, TipoAttoEnum tipo);
        Task<int> CountByStato(PersonaDto persona, List<int> tipo, StatiAttoEnum stato);

        Task<int> Count(PersonaDto persona, TipoAttoEnum tipo, ClientModeEnum clientMode, Filter<ATTI_DASI> queryFilter, QueryExtendedRequest queryExtended);

        Task<int> Count(PersonaDto persona, StatiAttoEnum stato, ClientModeEnum clientMode,
            Filter<ATTI_DASI> queryFilter, QueryExtendedRequest queryExtended);

        Task<int> Count(PersonaDto persona, ClientModeEnum clientMode, Filter<ATTI_DASI> queryFilter, QueryExtendedRequest queryExtended);
        Task<List<GruppiDto>> GetGruppiDisponibili(int legislaturaId, int page, int size);
        Task<List<KeyValueDto>> GetCommissioniProponenti(Guid uidAtto);
        void AggiungiAbbinamento(Guid requestUidAbbinamento, Guid requestUidAttoAbbinato);
        Task<ATTI_ABBINAMENTI> GetAbbinamento(Guid requestUidAbbinamento, Guid requestUidAttoAbbinato);
        void RimuoviAbbinamento(ATTI_ABBINAMENTI abbinamentoInDb);
        void AggiungiRisposta(ATTI_RISPOSTE risposta);
        void RimuoviRisposta(ATTI_RISPOSTE risposta);
        Task<ATTI_RISPOSTE> GetRisposta(Guid requestUid);
        Task<ATTI_MONITORAGGIO> GetMonitoraggio(Guid requestUid);
        void AggiungiMonitoraggio(ATTI_MONITORAGGIO monitoraggio);
        void RimuoviMonitoraggio(ATTI_MONITORAGGIO monitoraggioInDb);
        Task<ATTI_NOTE> GetNota(Guid requestUidAtto, TipoNotaEnum requestTipoEnum);
        void RimuoviNota(ATTI_NOTE notaInDb);
        void AggiungiNota(ATTI_NOTE notaInDb);
        void AggiungiDocumento(ATTI_DOCUMENTI doc);
        Task<ATTI_DOCUMENTI> GetDocumento(Guid requestUid);
        Task<List<ATTI_DOCUMENTI>> GetDocumento(Guid UIdAtto, TipoDocumentoEnum tipoDocumento);
        void RimuoviDocumento(ATTI_DOCUMENTI doc);
        Task<ATTI_DASI> GetByEtichetta(string etichettaProgressiva);
        Task<Guid> GetByQR(Guid id);
        Task<bool> CheckDCR(string dcrl, string dcr, string dccr);
    }
}