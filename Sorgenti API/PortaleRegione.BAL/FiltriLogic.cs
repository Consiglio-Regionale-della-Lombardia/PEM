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
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.BAL
{
    /// <summary>
    ///     Logica BAL condivisa per la gestione dei filtri preferiti utente.
    ///     Sostituisce SalvaGruppoFiltri/GetGruppoFiltri/EliminaGruppoFiltri
    ///     prima ospitati in DASILogic. Funziona per entrambi i moduli (PEM,
    ///     DASI) tramite il parametro ModuloEnum, sfruttando la colonna
    ///     Modulo della tabella FILTRI.
    /// </summary>
    public class FiltriLogic : BaseLogic
    {
        public FiltriLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        ///     Salva un nuovo filtro preferito per l'utente sul modulo richiesto.
        ///     Il campo Preferito e' impostato sempre a true: l'interfaccia
        ///     non espone piu' la distinzione "prioritario / non prioritario"
        ///     (richiesta utente v2026.5.1).
        /// </summary>
        public async Task Salva(FiltroPreferitoDto request, PersonaDto currentUser, ModuloEnum modulo)
        {
            if (string.IsNullOrEmpty(request?.name))
                throw new Exception("E' necessario dare un nome al filtro per poterlo salvare.");

            var filtro = new FILTRI
            {
                UId_persona = currentUser.UID_persona,
                Filtri = request.filters,
                Colonne = request.columns,
                DettagliOrdinamento = request.sorting,
                Nome = request.name,
                Preferito = true,
                Modulo = modulo
            };

            _unitOfWork.Filtri.Add(filtro);
            await _unitOfWork.CompleteAsync();
        }

        /// <summary>
        ///     Restituisce i filtri preferiti dell'utente per il modulo richiesto,
        ///     ordinati per data discendente (i piu' recenti in testa).
        /// </summary>
        public async Task<List<FiltroPreferitoDto>> GetByUser(PersonaDto currentUser, ModuloEnum modulo)
        {
            var listFromDb = await _unitOfWork.Filtri.GetByUser(currentUser.UID_persona, modulo);
            var res = new List<FiltroPreferitoDto>();
            foreach (var f in listFromDb)
                res.Add(new FiltroPreferitoDto
                {
                    name = f.Nome,
                    favourite = f.Preferito,
                    filters = f.Filtri,
                    columns = f.Colonne,
                    sorting = f.DettagliOrdinamento,
                    modulo = f.Modulo
                });

            return res;
        }

        /// <summary>
        ///     Elimina il filtro preferito identificato da nome+utente sul
        ///     modulo richiesto.
        /// </summary>
        public async Task Elimina(string nomeFiltro, PersonaDto currentUser, ModuloEnum modulo)
        {
            var filtro = await _unitOfWork.Filtri.Get(nomeFiltro, currentUser.UID_persona, modulo);
            if (filtro == null)
                return;

            _unitOfWork.Filtri.Remove(filtro);
            await _unitOfWork.CompleteAsync();
        }
    }
}
