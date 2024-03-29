﻿/*
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
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;

namespace PortaleRegione.Gateway
{
    public sealed class SeduteGate : BaseGateway
    {
        static  SeduteGate _instance;  
   
        public static SeduteGate Instance => _instance ?? (_instance = new SeduteGate());

        private SeduteGate()
        {
            
        }

        public static async Task<BaseResponse<SeduteDto>> Get(int page, int size)
        {
            try
            {
                var requestUrl = $"{apiUrl}/sedute/view";

                var model = new BaseRequest<SeduteDto>
                {
                    page = page,
                    size = size
                };
                var body = JsonConvert.SerializeObject(model);

                var lst = JsonConvert.DeserializeObject<BaseResponse<SeduteDto>>(await Post(requestUrl, body));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetSedute", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetSedute", ex);
                throw ex;
            }
        }

        public static async Task<SeduteFormUpdateDto> Get(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/sedute?id={id}";

                var lst = JsonConvert.DeserializeObject<SeduteFormUpdateDto>(await Get(requestUrl));
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetSeduta", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetSeduta", ex);
                throw ex;
            }
        }

        public static async Task<BaseResponse<SeduteDto>> Get(BaseRequest<SeduteDto> model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/sedute/view";
                var body = JsonConvert.SerializeObject(model);

                var lst = JsonConvert.DeserializeObject<BaseResponse<SeduteDto>>(await Post(requestUrl, body));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetSedute", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetSedute", ex);
                throw ex;
            }
        }

        public static async Task Salva(SeduteFormUpdateDto seduta)
        {
            try
            {
                var requestUrl = $"{apiUrl}/sedute";
                var body = JsonConvert.SerializeObject(seduta);

                await Post(requestUrl, body);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("SalvaSeduta", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("SalvaSeduta", ex);
                throw ex;
            }
        }

        public static async Task Modifica(SeduteFormUpdateDto seduta)
        {
            try
            {
                var requestUrl = $"{apiUrl}/sedute";
                var body = JsonConvert.SerializeObject(seduta);

                await Put(requestUrl, body);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("ModificaSeduta", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("ModificaSeduta", ex);
                throw ex;
            }
        }

        public static async Task Elimina(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/sedute?id={id}";

                await Delete(requestUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("EliminaSeduta", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("EliminaSeduta", ex);
                throw ex;
            }
        }
    }
}