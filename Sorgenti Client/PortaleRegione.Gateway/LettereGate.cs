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
    public sealed class LettereGate : BaseGateway
    {
        private static LettereGate _instance;

        private LettereGate()
        {
        }

        public static LettereGate Instance => _instance ?? (_instance = new LettereGate());

        public static async Task<IEnumerable<LettereDto>> Get(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/lettere?id={id}";

                var lst = JsonConvert.DeserializeObject<IEnumerable<LettereDto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetLettere", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetLettere", ex);
                throw ex;
            }
        }

        public static async Task Crea(Guid id, string lettere)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/crea-lettere?id={id}&lettere={lettere}";

                await Get(requestUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("CreaLettere", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("CreaLettere", ex);
                throw ex;
            }
        }

        public static async Task Elimina(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/elimina-lettera?id={id}";

                await Delete(requestUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("EliminaLettera", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("EliminaLettera", ex);
                throw ex;
            }
        }
    }
}