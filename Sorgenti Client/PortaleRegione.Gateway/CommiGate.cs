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
    public sealed class CommiGate : BaseGateway
    {
        static  CommiGate _instance;  
   
        public static CommiGate Instance => _instance ?? (_instance = new CommiGate());

        private CommiGate()
        {
            
        }

        public static async Task<IEnumerable<CommiDto>> Get(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/commi?id={id}";

                var lst = JsonConvert.DeserializeObject<IEnumerable<CommiDto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetCommi", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetCommi", ex);
                throw ex;
            }
        }

        public static async Task Crea(Guid id, string commi)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/crea-commi?id={id}&commi={commi}";

                await Get(requestUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("CreaCommi", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("CreaCommi", ex);
                throw ex;
            }
        }

        public static async Task Elimina(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/elimina-comma?id={id}";

                await Delete(requestUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("EliminaComma", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("EliminaComma", ex);
                throw ex;
            }
        }
    }
}