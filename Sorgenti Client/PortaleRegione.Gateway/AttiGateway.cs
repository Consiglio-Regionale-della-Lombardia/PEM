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
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.DTO.Routes;
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
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.GetAll}";
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

        public async Task<BaseResponse<AttiDto>> Get(BaseRequest<AttiDto> model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.GetAll}";
            var body = JsonConvert.SerializeObject(model);
            var lst = JsonConvert.DeserializeObject<BaseResponse<AttiDto>>(await Post(requestUrl, body, _token));
            return lst;
        }

        public async Task<AttiDto> Get(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.Get.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<AttiDto>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<AttiFormUpdateModel> GetFormUpdate(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.Get.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<AttiFormUpdateModel>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task BloccoODG(BloccoModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.BloccoODG}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }

        public async Task BloccoEM(BloccoModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.BloccoEM}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }

        public async Task JollyODG(JollyODGModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.JollyODG}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }

        public async Task<AttiDto> Salva(AttiFormUpdateModel atto)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.Create}";
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

        public async Task SalvaTesto(TestoAttoModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.AggiornaTesto}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }

        public async Task<AttiDto> Modifica(AttiFormUpdateModel atto)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.Edit}";
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

        public async Task ModificaFiles(AttiDto atto)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.ModificaFascicoli}";
            var body = JsonConvert.SerializeObject(atto);
            await Put(requestUrl, body, _token);
        }

        public async Task<FileResponse> Download(string path)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.DownloadDoc}?path={path}";
            var lst = await GetFile(requestUrl, _token);
            return lst;
        }

        public async Task Elimina(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.Delete.Replace("{id}", id.ToString())}";
            await Delete(requestUrl, _token);
        }

        public async Task SalvaRelatori(AttoRelatoriModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.AggiornaRelatori}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }

        public async Task PubblicaFascicolo(PubblicaFascicoloModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.AbilitaFascicolo}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }

        public async Task SPOSTA_DOWN(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.SpostaDown.Replace("{id}", id.ToString())}";
            await Get(requestUrl, _token);
        }

        public async Task SPOSTA_UP(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.SpostaUp.Replace("{id}", id.ToString())}";
            await Get(requestUrl, _token);
        }

        public async Task<IEnumerable<Tipi_AttoDto>> GetTipi(bool dasi = true)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.GetTipi.Replace("{dasi}", dasi.ToString())}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<Tipi_AttoDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<List<ArticoliModel>> GetGrigliaTesto(Guid id, bool viewEm = false)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.GrigliaTesti.Replace("{id}", id.ToString()).Replace("{view}", viewEm.ToString())}";
            var lst = JsonConvert.DeserializeObject<List<ArticoliModel>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<List<EmendamentoExtraLightDto>> GetGrigliaOrdinamento(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.GrigliaOrdinamento.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<List<EmendamentoExtraLightDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task SpostaInAltraSeduta(Guid uidAtto, Guid uidSeduta)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.SpostaInAltraSeduta.Replace("{uidAtto}", uidAtto.ToString()).Replace("{uidSeduta}", uidSeduta.ToString())}";
            await Get(requestUrl, _token);
        }

        public async Task<string> CercaAttiGea(CercaAttiGeaRequest request)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.CercaAttiGea}";
            var body = JsonConvert.SerializeObject(request);
            var lst = await Post(requestUrl, body, _token);
            return lst;
        }

        public async Task<IEnumerable<ArticoliDto>> GetArticoli(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.Articoli.GetAll.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<ArticoliDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task CreaArticolo(Guid id, string articoli)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.Articoli.Create.Replace("{id}", id.ToString()).Replace("{articoli}", articoli)}";
            await Get(requestUrl, _token);
        }

        public async Task EliminaArticolo(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.Articoli.Delete.Replace("{id}", id.ToString())}";
            await Delete(requestUrl, _token);
        }

        public async Task<IEnumerable<CommiDto>> GetComma(Guid id, bool expanded = false)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.Commi.GetAll.Replace("{id}", id.ToString()).Replace("{expanded}", expanded.ToString())}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<CommiDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task CreaComma(Guid id, string commi)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.Commi.Create.Replace("{id}", id.ToString()).Replace("{commi}", commi)}";
            await Get(requestUrl, _token);
        }

        public async Task EliminaComma(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.Commi.Delete.Replace("{id}", id.ToString())}";
            await Delete(requestUrl, _token);
        }

        public async Task<IEnumerable<LettereDto>> GetLettere(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.Lettere.GetAll.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<LettereDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task CreaLettera(Guid id, string lettere)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.Lettere.Create.Replace("{id}", id.ToString()).Replace("{lettere}", lettere)}";
            await Get(requestUrl, _token);
        }

        public async Task EliminaLettera(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Atti.Lettere.Delete.Replace("{id}", id.ToString())}";
            await Delete(requestUrl, _token);
        }
    }
}