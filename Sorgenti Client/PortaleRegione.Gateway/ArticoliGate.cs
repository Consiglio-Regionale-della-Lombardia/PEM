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
using Newtonsoft.Json;
using PortaleRegione.DTO.Domain;
using PortaleRegione.Logger;

namespace PortaleRegione.Gateway
{
    public sealed class ArticoliGate : BaseGateway
    {
        static  ArticoliGate _instance;  
   
        public static ArticoliGate Instance => _instance ?? (_instance = new ArticoliGate());

        private ArticoliGate()
        {
            
        }

        public static async Task<IEnumerable<ArticoliDto>> Get(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/articoli?id={id}";

                var lst = JsonConvert.DeserializeObject<IEnumerable<ArticoliDto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetArticoli", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetArticoli", ex);
                throw ex;
            }
        }

        public static async Task Crea(Guid id, string articoli)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/crea-articoli?id={id}&articoli={articoli}";

                await Get(requestUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("CreaArticoli", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("CreaArticoli", ex);
                throw ex;
            }
        }

        public static async Task Elimina(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/elimina-articolo?id={id}";

                await Delete(requestUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("EliminaArticolo", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("EliminaArticolo", ex);
                throw ex;
            }
        }
    }
}