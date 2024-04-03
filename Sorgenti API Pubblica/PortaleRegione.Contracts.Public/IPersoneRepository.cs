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

using System.Collections.Generic;
using System.Threading.Tasks;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Model;

namespace PortaleRegione.Contracts.Public
{
    /// <summary>
    ///     Definisce le operazioni di repository per l'accesso e la gestione delle informazioni relative a persone,
    ///     inclusi ruoli, commissioni e gruppi all'interno della legislatura.
    /// </summary>
    public interface IPersoneRepository
    {
        /// <summary>
        ///     Recupera le cariche associate a una specifica legislatura.
        /// </summary>
        /// <param name="idLegislatura">Identificativo della legislatura di interesse.</param>
        /// <returns>Una lista di oggetti KeyValueDto rappresentanti le cariche.</returns>
        Task<List<KeyValueDto>> GetCariche(int idLegislatura);

        /// <summary>
        ///     Recupera le commissioni esistenti in una specifica legislatura.
        /// </summary>
        /// <param name="idLegislatura">Identificativo della legislatura di interesse.</param>
        /// <returns>Una lista di oggetti KeyValueDto rappresentanti le commissioni.</returns>
        Task<List<KeyValueDto>> GetCommissioni(int idLegislatura);

        /// <summary>
        ///     Ottiene i gruppi legislativi presenti in una specifica legislatura.
        /// </summary>
        /// <param name="idLegislatura">Identificativo della legislatura di interesse.</param>
        /// <returns>Una lista di oggetti KeyValueDto rappresentanti i gruppi legislativi.</returns>
        Task<List<KeyValueDto>> GetGruppiByLegislatura(int idLegislatura);

        /// <summary>
        ///     Recupera i firmatari degli atti legislativi per una data legislatura.
        /// </summary>
        /// <param name="idLegislatura">Identificativo della legislatura di interesse.</param>
        /// <returns>Una lista di oggetti PersonaPublicDto rappresentanti i firmatari.</returns>
        Task<List<PersonaPublicDto>> GetFirmatariByLegislatura(int idLegislatura);

        /// <summary>
        ///     Ottiene un elenco completo degli utenti registrati nel sistema.
        /// </summary>
        /// <returns>Una lista di oggetti View_UTENTI rappresentanti tutti gli utenti.</returns>
        Task<List<View_UTENTI>> GetAll();
    }
}