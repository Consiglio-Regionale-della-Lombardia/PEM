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

using ExpressionBuilder.Generics;
using PortaleRegione.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortaleRegione.Contracts
{
    /// <summary>
    ///     Interfaccia Persone
    /// </summary>
    public interface IPersoneRepository : IRepository<View_UTENTI>
    {
        Task<View_UTENTI> Get(string login_windows);
        Task<View_UTENTI> Get(Guid personaUId);
        Task<View_UTENTI> Get(int personaId);
        Task<IEnumerable<View_UTENTI>> GetAll(int page, int size, Filter<View_UTENTI> filtro = null);
        Task<IEnumerable<View_UTENTI>> GetAllByGiunta(int page, int size, Filter<View_UTENTI> filtro = null);
        Task<int> CountAll(Filter<View_UTENTI> filtro = null);
        Task<int> CountAllByGiunta(Filter<View_UTENTI> queryFilter);
        Task<IEnumerable<View_UTENTI>> GetAll();
        Task<IEnumerable<View_UTENTI>> GetAssessoriRiferimento(int id_legislatura);
        Task<IEnumerable<View_UTENTI>> GetRelatori(Guid? attoUId);
        Task<bool> IsRelatore(Guid personaUId, Guid attoUId);
        Task<bool> IsAssessore(Guid personaUId, Guid attoUId);
        Task<string> GetCarica(Guid personaUId);
        Task<View_PINS> GetPin(Guid personaUId);

        Task<IEnumerable<View_UTENTI>> GetConsiglieri(int id_legislatura);

        Task<IEnumerable<View_Composizione_GiuntaRegionale>> GetGiuntaRegionale();


        Task<IEnumerable<UTENTI_NoCons>> GetSegreteriaGiuntaRegionale(bool notifica_firma,
            bool notifica_deposito);

        Task SavePin(Guid personaUId, string nuovo_pin, bool reset);
        Task DeletePersona(int id);
        Task UpdateUtente_NoCons(Guid uid_persona, int id_persona, string userAd);
        void Add(UTENTI_NoCons newUser);
    }
}