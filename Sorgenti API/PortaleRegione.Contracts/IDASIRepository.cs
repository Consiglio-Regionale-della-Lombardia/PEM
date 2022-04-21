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

namespace PortaleRegione.Contracts
{
    public interface IDASIRepository : IRepository<ATTI_DASI>
    {
        Task<ATTI_DASI> Get(Guid attoUId);
        Task<List<Guid>> GetAll(PersonaDto persona, int page, int size, Filter<ATTI_DASI> filtro = null);
        Task<int> Count(PersonaDto persona, Filter<ATTI_DASI> queryFilter);
        Task<int> Count(PersonaDto persona, TipoAttoEnum tipo, StatiAttoEnum stato);
        Task<ATTI_DASI_CONTATORI> GetContatore(TipoAttoEnum tipo, int tipo_risposta);
        Task<int> GetOrdine(int tipo);
        Task<bool> CheckIfPresentabile(AttoDASIDto dto, PersonaDto persona);
        bool CheckIfRitirabile(AttoDASIDto dto, PersonaDto persona);
        bool CheckIfEliminabile(AttoDASIDto dto, PersonaDto persona);
        bool CheckIfModificabile(AttoDASIDto dto, PersonaDto persona);
        Task<int> GetProgressivo(TipoAttoEnum tipo, int gruppoId, int legislatura);
        Task<bool> CheckProgressivo(string etichettaEncrypt);
        void IncrementaContatore(ATTI_DASI_CONTATORI contatore);
        Task<List<View_cariche_assessori_in_carica>> GetSoggettiInterrogabili();
        Task RimuoviSoggetti(Guid UidAtto);
        void AggiungiSoggetto(Guid UidAtto, int soggetto);
        Task<List<View_cariche_assessori_in_carica>> GetSoggettiInterrogati(Guid uidAtto);
    }
}