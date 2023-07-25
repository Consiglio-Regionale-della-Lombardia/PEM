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

using Newtonsoft.Json;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using PortaleRegione.DTO.Routes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PortaleRegione.Gateway
{
    /// <summary>
    ///     Classe base del Gateway. Qui ci sono tutte le chiamate CRUD verso l'api
    /// </summary>
    public class BaseGateway
    {
        /// <summary>
        ///     Configura url api
        /// </summary>
        public static string apiUrl = "";

        /// <summary>
        ///     Metodo che segue il POST
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="body"></param>
        /// <param name="auth"></param>
        /// <returns></returns>
        protected static async Task<string> Post(string requestUrl, string body, string token)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(token))
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var result = await httpClient.PostAsync(requestUrl, content);
                return await CheckResponseStatusCode(result);
            }
            catch (UnauthorizedAccessException e)
            {
                //Log.Error($"Post [{requestUrl}][{body}]", e);
                throw e;
            }
            catch (Exception e)
            {
                //Log.Error($"Post [{requestUrl}][{body}]", e);
                throw e;
            }
        }

        /// <summary>
        ///     Metodo che esegue il PUT
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="body"></param>
        /// <param name="auth"></param>
        /// <returns></returns>
        protected static async Task<string> Put(string requestUrl, string body, string token)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(token))
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var result = await httpClient.PutAsync(requestUrl, content);
                return await CheckResponseStatusCode(result);
            }
            catch (UnauthorizedAccessException e)
            {
                //Log.Error($"Put  [{requestUrl}][{body}]", e);
                throw e;
            }
            catch (Exception e)
            {
                //Log.Error($"Put [{requestUrl}][{body}]", e);
                throw e;
            }
        }

        /// <summary>
        ///     Metodo che esegue il GET
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="auth"></param>
        /// <returns></returns>
        protected static async Task<string> Get(string requestUrl, string token)
        {
            try
            {
                using var httpClient = new HttpClient();
                if (!string.IsNullOrEmpty(token))
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var result = await httpClient.GetAsync(requestUrl);
                return await CheckResponseStatusCode(result);
            }
            catch (UnauthorizedAccessException e)
            {
                //Log.Error($"Get [{requestUrl}]", e);
                throw e;
            }
            catch (Exception e)
            {
                //Log.Error($"Get [{requestUrl}]", e);
                throw e;
            }
        }

        /// <summary>
        ///     Metodo per avere scaricare il file
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="auth"></param>
        /// <returns></returns>
        protected static async Task<FileResponse> GetFile(string requestUrl, string token)
        {
            try
            {
                using var httpClient = new HttpClient();
                if (!string.IsNullOrEmpty(token))
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var result = await httpClient.GetAsync(requestUrl);
                return await CheckResponseFile(result);
            }
            catch (UnauthorizedAccessException e)
            {
                //Log.Error($"Get [{requestUrl}]", e);
                throw e;
            }
            catch (Exception e)
            {
                //Log.Error($"Get [{requestUrl}]", e);
                throw e;
            }
        }

        /// <summary>
        ///     Metodo per avere scaricare il file
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="auth"></param>
        /// <returns></returns>
        protected static async Task<FileResponse> GetFile(string url, string body, string token)
        {
            try
            {
                using var httpClient = new HttpClient();
                if (!string.IsNullOrEmpty(token))
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync(url, content);
                return await CheckResponseFile(result);
            }
            catch (UnauthorizedAccessException e)
            {
                //Log.Error($"Get [{url}][{body}]", e);
                throw e;
            }
            catch (Exception e)
            {
                //Log.Error($"Get [{url}][{body}]", e);
                throw e;
            }
        }

        /// <summary>
        ///     Metodo che esegue il DELETE
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="auth"></param>
        /// <returns></returns>
        protected static async Task Delete(string requestUrl, string token)
        {
            try
            {
                using var httpClient = new HttpClient();
                if (!string.IsNullOrEmpty(token))
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var result = await httpClient.DeleteAsync(requestUrl);
                await CheckResponseStatusCode(result);
            }
            catch (UnauthorizedAccessException e)
            {
                //Log.Error($"Delete [{requestUrl}]", e);
                throw e;
            }
            catch (Exception e)
            {
                //Log.Error($"Delete [{requestUrl}]", e);
                throw e;
            }
        }

        /// <summary>
        ///     Metodo che controlla la response
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private static async Task<string> CheckResponseStatusCode(HttpResponseMessage result)
        {
            switch (result.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedAccessException();
                case HttpStatusCode.NotFound:
                    throw new KeyNotFoundException();
                case HttpStatusCode.Created:
                case HttpStatusCode.OK:
                    return await result.Content.ReadAsStringAsync();
                default:
                    {
                        throw new Exception(await result.Content.ReadAsStringAsync());
                    }
            }
        }

        /// <summary>
        ///     Metodo che controlla la response dei file
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private static async Task<FileResponse> CheckResponseFile(HttpResponseMessage result)
        {
            switch (result.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedAccessException();
                case HttpStatusCode.NotFound:
                    throw new KeyNotFoundException();
                case HttpStatusCode.Created:
                case HttpStatusCode.OK:
                    {
                        try
                        {
                            return new FileResponse
                            {
                                Url = await result.Content.ReadAsStringAsync(),
                            };
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                        try
                        {
                            return new FileResponse
                            {
                                Content = await result.Content.ReadAsByteArrayAsync(),
                                FileName = result.Content.Headers.ContentDisposition.FileName
                            };
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                default:
                    {
                        throw new Exception(await result.Content.ReadAsStringAsync());
                    }
            }
        }

        public static async Task<bool> SendMail(MailModel model, string token)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Util.InvioMail}";
            var body = JsonConvert.SerializeObject(model);
            return JsonConvert.DeserializeObject<bool>(await Post(requestUrl, body, token));
        }
    }
}