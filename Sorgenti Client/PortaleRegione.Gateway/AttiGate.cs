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
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;

namespace PortaleRegione.Gateway
{
    public sealed class AttiGate : BaseGateway
    {
        static  AttiGate _instance;  
   
        public static AttiGate Instance => _instance ?? (_instance = new AttiGate());

        private AttiGate()
        {
            
        }

        public static async Task<BaseResponse<AttiDto>> Get(Guid sedutaUId, ClientModeEnum mode, int page, int size)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/view";
                var param = new Dictionary<string, object> {{"CLIENT_MODE", (int) mode}};
                var model = new BaseRequest<AttiDto>
                {
                    id = sedutaUId,
                    page = page,
                    size = size,
                    param = param
                };
                var body = JsonConvert.SerializeObject(model);

                var lst = JsonConvert.DeserializeObject<BaseResponse<AttiDto>>(await Post(requestUrl, body));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetAtti", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetAtti", ex);
                throw ex;
            }
        }

        public static async Task<BaseResponse<AttiDto>> Get(BaseRequest<AttiDto> model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/view";
                var body = JsonConvert.SerializeObject(model);

                var lst = JsonConvert.DeserializeObject<BaseResponse<AttiDto>>(await Post(requestUrl, body));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetAtti", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetAtti", ex);
                throw ex;
            }
        }
        
        public static async Task<AttiDto> Get(Guid attoUId)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti?id={attoUId}";

                var lst = JsonConvert.DeserializeObject<AttiDto>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetAtto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetAtto", ex);
                throw ex;
            }
        }

        public static async Task<AttiFormUpdateModel> GetFormUpdate(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti?id={id}";

                var lst = JsonConvert.DeserializeObject<AttiFormUpdateModel>(await Get(requestUrl));
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetAttoFormUpdate", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetAttoFormUpdate", ex);
                throw ex;
            }
        }

        public static async Task<AttiDto> Salva(AttiFormUpdateModel atto)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti";
                if (atto.DocAtto != null)
                {
                    using var memoryStream = new MemoryStream();
                    await atto.DocAtto.InputStream.CopyToAsync(memoryStream);
                    atto.DocAtto_Stream = memoryStream.ToArray();
                }

                var body = JsonConvert.SerializeObject(atto);

                var result = JsonConvert.DeserializeObject<AttiDto>(await Post(requestUrl, body));
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("SalvaAtto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("SalvaAtto", ex);
                throw ex;
            }
        }

        public static async Task<AttiDto> Modifica(AttiFormUpdateModel atto)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/modifica";
                if (atto.DocAtto != null)
                {
                    using var memoryStream = new MemoryStream();
                    await atto.DocAtto.InputStream.CopyToAsync(memoryStream);
                    atto.DocAtto_Stream = memoryStream.ToArray();
                }

                var body = JsonConvert.SerializeObject(atto);

                var result = JsonConvert.DeserializeObject<AttiDto>(await Put(requestUrl, body));
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("ModificaAtto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("ModificaAtto", ex);
                throw ex;
            }
        }

        public static async Task ModificaFiles(AttiDto atto)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/fascicoli";
                var body = JsonConvert.SerializeObject(atto);

                await Put(requestUrl, body);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("ModificaFilesAtto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("ModificaFilesAtto", ex);
                throw ex;
            }
        }

        public static async Task<FileResponse> Download(string path)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/file?path={path}";

                var lst = await GetFile(requestUrl);

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("DownloadAtto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("DownloadAtto", ex);
                throw ex;
            }
        }

        public static async Task Elimina(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti?id={id}";

                await Delete(requestUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("EliminaAtto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("EliminaAtto", ex);
                throw ex;
            }
        }

        public static async Task SalvaRelatori(AttoRelatoriModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/relatori";
                var body = JsonConvert.SerializeObject(model);

                await Post(requestUrl, body);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("SalvaRelatoriAtto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("SalvaRelatoriAtto", ex);
                throw ex;
            }
        }

        public static async Task PubblicaFascicolo(PubblicaFascicoloModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/abilita-fascicolazione";
                var body = JsonConvert.SerializeObject(model);

                await Post(requestUrl, body);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("PubblicaFascicolo", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("PubblicaFascicolo", ex);
                throw ex;
            }
        }
        
        public static async Task SPOSTA_DOWN(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/sposta-down?id={id}";
                await Get(requestUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("SPOSTA_DOWN", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("SPOSTA_DOWN", ex);
                throw ex;
            }
        }

        public static async Task SPOSTA_UP(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/sposta-up?id={id}";
                await Get(requestUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("SPOSTA_UP", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("SPOSTA_UP", ex);
                throw ex;
            }
        }
    }
}