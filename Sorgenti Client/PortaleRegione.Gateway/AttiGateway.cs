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
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PortaleRegione.Gateway
{
    public class AttiGateway : BaseGateway, IAttiGateway
    {
        private readonly string _token;

        protected internal AttiGateway(string token)
        {
            _token = token;
        }

        public async Task<BaseResponse<AttiDto>> Get(Guid sedutaUId, ClientModeEnum mode, int page, int size)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/view";
                var param = new Dictionary<string, object> { { "CLIENT_MODE", (int)mode } };
                var model = new BaseRequest<AttiDto>
                {
                    id = sedutaUId,
                    page = page,
                    size = size,
                    param = param
                };
                var body = JsonConvert.SerializeObject(model);

                var lst = JsonConvert.DeserializeObject<BaseResponse<AttiDto>>(await Post(requestUrl, body, _token));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("GetAtti", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("GetAtti", ex);
                throw ex;
            }
        }

        public async Task<BaseResponse<AttiDto>> Get(BaseRequest<AttiDto> model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/view";
                var body = JsonConvert.SerializeObject(model);

                var lst = JsonConvert.DeserializeObject<BaseResponse<AttiDto>>(await Post(requestUrl, body, _token));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("GetAtti", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("GetAtti", ex);
                throw ex;
            }
        }

        public async Task<AttiDto> Get(Guid attoUId)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti?id={attoUId}";

                var lst = JsonConvert.DeserializeObject<AttiDto>(await Get(requestUrl, _token));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("GetAtto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("GetAtto", ex);
                throw ex;
            }
        }

        public async Task<AttiFormUpdateModel> GetFormUpdate(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti?id={id}";

                var lst = JsonConvert.DeserializeObject<AttiFormUpdateModel>(await Get(requestUrl, _token));
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("GetAttoFormUpdate", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("GetAttoFormUpdate", ex);
                throw ex;
            }
        }

        public async Task BloccoODG(BloccoODGModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/bloccoODG";
                var body = JsonConvert.SerializeObject(model);

                await Post(requestUrl, body, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("BloccoODG", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("BloccoODG", ex);
                throw ex;
            }
        }

        public async Task JollyODG(JollyODGModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/jollyODG";
                var body = JsonConvert.SerializeObject(model);

                await Post(requestUrl, body, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("JollyODG", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("JollyODG", ex);
                throw ex;
            }

        }

        public async Task<AttiDto> Salva(AttiFormUpdateModel atto)
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

                var result = JsonConvert.DeserializeObject<AttiDto>(await Post(requestUrl, body, _token));
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("SalvaAtto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("SalvaAtto", ex);
                throw ex;
            }
        }

        public async Task SalvaTesto(TestoAttoModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/salva-testo";

                var body = JsonConvert.SerializeObject(model);
                await Post(requestUrl, body, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("SalvaTesto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("SalvaTesto", ex);
                throw ex;
            }
        }

        public async Task<AttiDto> Modifica(AttiFormUpdateModel atto)
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

                var result = JsonConvert.DeserializeObject<AttiDto>(await Put(requestUrl, body, _token));
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("ModificaAtto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("ModificaAtto", ex);
                throw ex;
            }
        }

        public async Task ModificaFiles(AttiDto atto)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/fascicoli";
                var body = JsonConvert.SerializeObject(atto);

                await Put(requestUrl, body, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("ModificaFilesAtto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("ModificaFilesAtto", ex);
                throw ex;
            }
        }

        public async Task<FileResponse> Download(string path)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/file?path={path}";

                var lst = await GetFile(requestUrl, _token);

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("DownloadAtto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("DownloadAtto", ex);
                throw ex;
            }
        }

        public async Task Elimina(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti?id={id}";

                await Delete(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("EliminaAtto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("EliminaAtto", ex);
                throw ex;
            }
        }

        public async Task SalvaRelatori(AttoRelatoriModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/relatori";
                var body = JsonConvert.SerializeObject(model);

                await Post(requestUrl, body, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("SalvaRelatoriAtto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("SalvaRelatoriAtto", ex);
                throw ex;
            }
        }

        public async Task PubblicaFascicolo(PubblicaFascicoloModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/abilita-fascicolazione";
                var body = JsonConvert.SerializeObject(model);

                await Post(requestUrl, body, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("PubblicaFascicolo", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("PubblicaFascicolo", ex);
                throw ex;
            }
        }

        public async Task SPOSTA_DOWN(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/sposta-down?id={id}";
                await Get(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("SPOSTA_DOWN", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("SPOSTA_DOWN", ex);
                throw ex;
            }
        }

        public async Task SPOSTA_UP(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/sposta-up?id={id}";
                await Get(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("SPOSTA_UP", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("SPOSTA_UP", ex);
                throw ex;
            }
        }

        public async Task<IEnumerable<Tipi_AttoDto>> GetTipi(bool dasi = true)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/tipi";

                if (dasi == false)
                {
                    requestUrl += "?dasi=false";
                }

                var lst = JsonConvert.DeserializeObject<IEnumerable<Tipi_AttoDto>>(await Get(requestUrl, _token));
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("GetTipi", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("GetTipi", ex);
                throw ex;
            }
        }

        public async Task<List<ArticoliModel>> GetGrigliaTesto(Guid id, bool viewEm = false)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/griglia-testi?id={id}&viewEm={viewEm}";

                var lst = JsonConvert.DeserializeObject<List<ArticoliModel>>(await Get(requestUrl, _token));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("GetGrigliaTesto", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("GetGrigliaTesto", ex);
                throw ex;
            }
        }

        #region ARTICOLI

        public async Task<IEnumerable<ArticoliDto>> GetArticoli(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/articoli?id={id}";

                var lst = JsonConvert.DeserializeObject<IEnumerable<ArticoliDto>>(await Get(requestUrl, _token));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("GetArticoli", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("GetArticoli", ex);
                throw ex;
            }
        }

        public async Task CreaArticolo(Guid id, string articoli)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/crea-articoli?id={id}&articoli={articoli}";

                await Get(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("CreaArticoli", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("CreaArticoli", ex);
                throw ex;
            }
        }

        public async Task EliminaArticolo(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/elimina-articolo?id={id}";

                await Delete(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("EliminaArticolo", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("EliminaArticolo", ex);
                throw ex;
            }
        }

        #endregion

        #region COMMI

        public async Task<IEnumerable<CommiDto>> GetComma(Guid id, bool expanded = false)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/commi?id={id}&expanded={expanded}";

                var lst = JsonConvert.DeserializeObject<IEnumerable<CommiDto>>(await Get(requestUrl, _token));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("GetCommi", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("GetCommi", ex);
                throw ex;
            }
        }

        public async Task CreaComma(Guid id, string commi)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/crea-commi?id={id}&commi={commi}";

                await Get(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("CreaCommi", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("CreaCommi", ex);
                throw ex;
            }
        }

        public async Task EliminaComma(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/elimina-comma?id={id}";

                await Delete(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("EliminaComma", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("EliminaComma", ex);
                throw ex;
            }
        }

        #endregion

        #region LETTERE

        public async Task<IEnumerable<LettereDto>> GetLettere(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/lettere?id={id}";

                var lst = JsonConvert.DeserializeObject<IEnumerable<LettereDto>>(await Get(requestUrl, _token));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("GetLettere", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("GetLettere", ex);
                throw ex;
            }
        }

        public async Task CreaLettera(Guid id, string lettere)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/crea-lettere?id={id}&lettere={lettere}";

                await Get(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("CreaLettere", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("CreaLettere", ex);
                throw ex;
            }
        }

        public async Task EliminaLettera(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/atti/elimina-lettera?id={id}";

                await Delete(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                //Log.Error("EliminaLettera", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //Log.Error("EliminaLettera", ex);
                throw ex;
            }
        }

        #endregion
    }
}