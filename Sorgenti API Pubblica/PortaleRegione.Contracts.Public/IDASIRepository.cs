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

namespace PortaleRegione.Contracts.Public
{
    /// <summary>
    ///     Definisce le operazioni di repository per la gestione degli ATTI_DASI,
    ///     consentendo l'accesso e la manipolazione dei dati degli atti.
    /// </summary>
    public interface IDASIRepository
    {
        /// <summary>
        ///     Recupera un singolo atto DASI utilizzando il suo identificativo unico.
        /// </summary>
        /// <param name="attoUId">L'identificativo unico dell'atto da recuperare.</param>
        /// <returns>L'atto DASI corrispondente all'identificativo fornito.</returns>
        Task<ATTI_DASI> Get(Guid attoUId);

        /// <summary>
        ///     Ottiene un elenco paginato di ATTI_DASI in base ai filtri applicati.
        /// </summary>
        /// <param name="page">La pagina di risultati da recuperare.</param>
        /// <param name="size">Il numero di elementi per pagina.</param>
        /// <param name="filtro">Un filtro opzionale per restringere i risultati.</param>
        /// <param name="proponenti"></param>
        /// <param name="firmatari"></param>
        /// <returns>Un elenco paginato di ATTI_DASI che soddisfano i criteri di filtro.</returns>
        Task<List<ATTI_DASI>> GetAll(int page, int size, Filter<ATTI_DASI> filtro = null, List<int> proponenti = null,
            List<int> firmatari = null);

        /// <summary>
        ///     Conta il numero di ATTI_DASI che soddisfano un determinato filtro.
        /// </summary>
        /// <param name="filtro">Un filtro opzionale per restringere i risultati.</param>
        /// <param name="proponenti"></param>
        /// <param name="firmatari"></param>
        /// <returns>Il conteggio degli ATTI_DASI che soddisfano i criteri di filtro.</returns>
        Task<int> Count(Filter<ATTI_DASI> filtro = null, List<int> proponenti = null, List<int> firmatari = null);

        /// <summary>
        ///     Recupera le firme associate a un specifico atto DASI.
        /// </summary>
        /// <param name="atto">L'atto DASI per il quale recuperare le firme.</param>
        /// <param name="tipo">Il tipo di firme da recuperare.</param>
        /// <returns>Un elenco di ATTI_FIRME associate all'atto specificato.</returns>
        Task<List<ATTI_FIRME>> GetFirme(ATTI_DASI atto, FirmeTipoEnum tipo);

        /// <summary>
        ///     Ottiene le commissioni di un atto
        /// </summary>
        /// <param name="uidAtto"></param>
        /// <returns></returns>
        Task<List<KeyValueDto>> GetCommissioniPerAtto(Guid uidAtto);

        /// <summary>
        ///     Ottiene le risposte all'atto
        /// </summary>
        /// <param name="uidAtto"></param>
        /// <returns></returns>
        Task<List<ATTI_RISPOSTE>> GetRisposte(Guid uidAtto);
        
        /// <summary>
        ///     Ottiene i documenti dell'atto
        /// </summary>
        /// <param name="uidAtto"></param>
        /// <returns></returns>
        Task<List<ATTI_DOCUMENTI>> GetDocumenti(Guid uidAtto);

        Task<List<AttiAbbinamentoDto>> GetAbbinamenti(Guid uidAtto);
    }
}