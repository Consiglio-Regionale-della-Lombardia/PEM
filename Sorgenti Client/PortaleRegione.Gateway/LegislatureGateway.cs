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


namespace PortaleRegione.Gateway
{
    public class LegislatureGateway : BaseGateway, ILegislatureGateway
    {
        private readonly string _token;

        protected internal LegislatureGateway(string token)
        {
            _token = token;
        }

        public async Task<LegislaturaDto> GetLegislatura(int idLegislatura)
        {
            try
            {
                var requestUrl = $"{apiUrl}/legislature/{idLegislatura}";

                var lst = JsonConvert.DeserializeObject<LegislaturaDto>(await Get(requestUrl, _token));
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("GetLegislatura", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("GetLegislatura", ex);
                throw ex;
            }
        }

        public async Task<IEnumerable<LegislaturaDto>> GetLegislature()
        {
            try
            {
                var requestUrl = $"{apiUrl}/legislature";

                var lst = JsonConvert.DeserializeObject<IEnumerable<LegislaturaDto>>(await Get(requestUrl, _token));
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("GetLegislature", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("GetLegislature", ex);
                throw ex;
            }
        }
    }
}