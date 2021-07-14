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
using System.Threading.Tasks;
using ExpressionBuilder.Generics;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;

namespace PortaleRegione.Contracts
{
    /// <summary>
    ///     Interfaccia Emendamenti
    /// </summary>
    public interface IEmendamentiRepository : IRepository<EM>
    {
        Task<int> Count(Guid attoUId, PersonaDto persona, CounterEmendamentiEnum counter_emendamenti, int CLIENT_MODE,
            Filter<EM> filtro = null, List<Guid> firmatari = null);

        Task<int> Count(string query);

        Task<IEnumerable<Guid>> GetAll(Guid attoUId, PersonaDto persona, OrdinamentoEnum ordine, int? page, int? size,
            int CLIENT_MODE, Filter<EM> filtro = null, List<Guid> firmatari = null);

        IEnumerable<EM> GetAll(EmendamentiByQueryModel model);

        string GetAll_Query(Guid attoUId, PersonaDto persona, OrdinamentoEnum ordine, Filter<EM> filtro = null);

        Task<EM> Get(Guid emendamentoUId);
        Task<EM> Get(string emendamentoUId);
        Task<int> GetProgressivo(Guid attoUId, int gruppo, bool sub);
        Task<int> GetEtichetta(Guid attoUId, bool sub);
        Task<IEnumerable<NOTIFICHE_DESTINATARI>> GetInvitati(Guid emendamentoUId);
        Task<IEnumerable<PARTI_TESTO>> GetPartiEmendabili();
        Task<IEnumerable<TIPI_EM>> GetTipiEmendamento();
        Task<IEnumerable<MISSIONI>> GetMissioniEmendamento();
        Task<IEnumerable<TITOLI_MISSIONI>> GetTitoliMissioneEmendamento();
        Task<IEnumerable<STATI_EM>> GetStatiEmendamento();

        bool CheckIfEliminabile(EmendamentiDto em, PersonaDto persona);
        bool CheckIfRitirabile(EmendamentiDto em, PersonaDto persona);
        Task<bool> CheckIfDepositabile(EmendamentiDto em, PersonaDto persona);
        Task<bool> CheckIfModificabile(EmendamentiDto em, PersonaDto persona);

        Task<bool> CheckProgressivo(Guid attoUId, string encrypt_progressivo,
            CounterEmendamentiEnum counter_emendamenti);

        Task ORDINA_EM_TRATTAZIONE(Guid attoUId);
        Task UP_EM_TRATTAZIONE(Guid emendamentoUId);
        Task DOWN_EM_TRATTAZIONE(Guid emendamentoUId);
        Task SPOSTA_EM_TRATTAZIONE(Guid emendamentoUId, int pos);
        
        Task<EM> GetEMInProiezione(Guid emUidAtto, int ordine);

        Task<IEnumerable<EM>> GetAll_RichiestaPropriaFirma(Guid id, PersonaDto persona, OrdinamentoEnum ordine,
            int page, int size, int mode);

        Task<EM> GetCurrentEMInProiezione(Guid attoUId);
        Task<EM> GetByQR(Guid id);
    }
}