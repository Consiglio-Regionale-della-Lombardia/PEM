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

using ExpressionBuilder.Generics;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortaleRegione.Contracts
{
    public interface IDASIRepository : IRepository<ATTI_DASI>
    {
        Task<ATTI_DASI> Get(Guid attoUId);
        Task<List<Guid>> GetAll(PersonaDto persona, int page, int size, OrdinamentoEnum ordinamentoEnum,
            ClientModeEnum mode,
            Filter<ATTI_DASI> filtro = null, List<int> soggetti = null, List<Guid> proponenti = null,
            List<Guid> provvedimenti = null, List<int> stati = null, List<Guid> atti_da_firmare = null);
        Task<int> Count(Filter<ATTI_DASI> queryFilter);
        Task<int> Count(PersonaDto persona, ClientModeEnum mode, Filter<ATTI_DASI> queryFilter, List<int> soggetti, List<int> stati = null, List<Guid> atti_da_firmare = null);
        Task<int> Count(PersonaDto persona, TipoAttoEnum tipo, StatiAttoEnum stato, Guid? sedutaId,
            ClientModeEnum clientMode, Filter<ATTI_DASI> filtro, List<int> soggetti, List<Guid> proponenti, List<Guid> atti_da_firmare);
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
        void AggiungiCommissione(Guid UidAtto, int organo);
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
    }
}