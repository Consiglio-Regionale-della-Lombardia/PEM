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

using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
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
using System.Reflection;
using System.Threading.Tasks;

namespace PortaleRegione.Gateway
{
    public class EMGateway : BaseGateway, IEMGateway
    {
        private readonly string _token;

        protected internal EMGateway(string token)
        {
            _token = token;
        }

        public async Task<EmendamentiViewModel> Get(Guid attoUId, ClientModeEnum mode, OrdinamentoEnum ordine, int page, int size)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetAll}";
            var param = new Dictionary<string, object> { { "CLIENT_MODE", (int)mode } };
            var model = new BaseRequest<EmendamentiDto>
            {
                id = attoUId,
                page = page,
                size = size,
                ordine = ordine,
                param = param,
                filtro = new List<FilterStatement<EmendamentiDto>>()
            };
            var filtro1 = new FilterStatement<EmendamentiDto>
            {
                PropertyId = nameof(EmendamentiDto.UIDAtto),
                Operation = Operation.EqualTo,
                Value = attoUId
            };
            model.filtro.Add(filtro1);
            var body = JsonConvert.SerializeObject(model);
            var lst = JsonConvert.DeserializeObject<EmendamentiViewModel>(await Post(requestUrl, body, _token));
            return lst;
        }

        public async Task<EmendamentiViewModel> Get_RichiestaPropriaFirma(Guid attoUId, ClientModeEnum mode,
            OrdinamentoEnum ordine,
            int page, int size)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetAllRichiestaPropriaFirma}";
            var param = new Dictionary<string, object> { { "CLIENT_MODE", (int)mode } };
            var model = new BaseRequest<AttiDto>
            {
                id = attoUId,
                page = page,
                size = size,
                ordine = ordine,
                param = param
            };
            var body = JsonConvert.SerializeObject(model);
            var lst = JsonConvert.DeserializeObject<EmendamentiViewModel>(await Post(requestUrl, body, _token));
            return lst;
        }

        public async Task<EmendamentiViewModel> Get(BaseRequest<EmendamentiDto> model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetAll}";
            var body = JsonConvert.SerializeObject(model);
            var lst = JsonConvert.DeserializeObject<EmendamentiViewModel>(await Post(requestUrl, body, _token));
            return lst;
        }

        public async Task<List<Guid>> GetSoloIds(BaseRequest<EmendamentiDto> model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetAllSoloIds}";
            var body = JsonConvert.SerializeObject(model);
            var lst = JsonConvert.DeserializeObject<List<Guid>>(await Post(requestUrl, body, _token));
            return lst;
        }

        public async Task<EmendamentiDto> Get(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.Get.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<EmendamentiDto>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<string> GetBody(Guid id, TemplateTypeEnum template, bool IsDeposito = false)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetBody}";
            var model = new GetBodyModel
            {
                Id = id,
                Template = template,
                IsDeposito = IsDeposito
            };
            var body = JsonConvert.SerializeObject(model);
            var result = await Post(requestUrl, body, _token);
            var lst = JsonConvert.DeserializeObject<string>(result);
            return lst;
        }

        public async Task<string> GetCopertina(CopertinaModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetBodyCopertina}";
            var body = JsonConvert.SerializeObject(model);
            var result = await Post(requestUrl, body, _token);
            var lst = JsonConvert.DeserializeObject<string>(result);
            return lst;
        }

        public async Task Elimina(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.Elimina.Replace("{id}", id.ToString())}";
            JsonConvert.DeserializeObject<string>(await Get(requestUrl, _token));
        }

        public async Task Ritira(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.Ritira.Replace("{id}", id.ToString())}";
            JsonConvert.DeserializeObject<string>(await Get(requestUrl, _token));
        }

        public async Task<Dictionary<string, string>> Firma(Guid emendamentoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                Lista = new List<Guid> { emendamentoUId },
                Pin = pin
            };
            return await Firma(model);
        }

        public async Task<Dictionary<string, string>> Firma(ComandiAzioneModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.Firma}";
            var body = JsonConvert.SerializeObject(model);
            var result =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(await Post(requestUrl, body, _token));
            return result;
        }

        public async Task<Dictionary<Guid, string>> Deposita(Guid emendamentoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                Lista = new List<Guid> { emendamentoUId },
                Pin = pin
            };
            return await Deposita(model);
        }

        public async Task<Dictionary<Guid, string>> Deposita(ComandiAzioneModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.Deposita}";
            var body = JsonConvert.SerializeObject(model);
            var result =
                JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));
            return result;
        }

        public async Task<Dictionary<Guid, string>> RitiraFirma(Guid emendamentoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                Lista = new List<Guid> { emendamentoUId },
                Pin = pin
            };
            return await RitiraFirma(model);
        }

        public async Task<Dictionary<Guid, string>> RitiraFirma(ComandiAzioneModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.RitiroFirma}";
            var body = JsonConvert.SerializeObject(model);
            var result =
                JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));
            return result;
        }

        public async Task<Dictionary<Guid, string>> EliminaFirma(Guid emendamentoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                Lista = new List<Guid> { emendamentoUId },
                Pin = pin
            };
            return await EliminaFirma(model);
        }

        public async Task<Dictionary<Guid, string>> EliminaFirma(ComandiAzioneModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.EliminaFirma}";
            var body = JsonConvert.SerializeObject(model);
            var result =
                JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));
            return result;
        }

        public async Task<EmendamentiFormModel> GetNuovoModel(Guid id, Guid? em_riferimentoUId)
        {
            em_riferimentoUId ??= Guid.Empty;
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetNuovoModello.Replace("{id}", id.ToString()).Replace("{sub_id}", em_riferimentoUId.ToString())}";
            var result = await Get(requestUrl, _token);
            var lst = JsonConvert.DeserializeObject<EmendamentiFormModel>(result);
            return lst;
        }

        public async Task<EmendamentiFormModel> GetModificaModel(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetModificaModello.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<EmendamentiFormModel>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<TitoloMissioniDto>> GetTitoliMissioni()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetTitoliMissioni}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<TitoloMissioniDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<EmendamentiFormModel> GetModificaMetaDatiModel(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetModificaModelloMetaDati.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<EmendamentiFormModel>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<EmendamentiDto> Salva(EmendamentiDto model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.Create}";
            if (model.DocAllegatoGenerico != null)
            {
                using var memoryStream = new MemoryStream();
                await model.DocAllegatoGenerico.InputStream.CopyToAsync(memoryStream);
                model.DocAllegatoGenerico_Stream = memoryStream.ToArray();
            }

            if (model.DocEffettiFinanziari != null)
            {
                using var memoryStream = new MemoryStream();
                await model.DocEffettiFinanziari.InputStream.CopyToAsync(memoryStream);
                model.DocEffettiFinanziari_Stream = memoryStream.ToArray();
            }

            var body = JsonConvert.SerializeObject(model);
            var result = JsonConvert.DeserializeObject<EmendamentiDto>(await Post(requestUrl, body, _token));
            return result;
        }

        public async Task Modifica(EmendamentiDto model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.Edit}";
            if (model.DocAllegatoGenerico != null)
            {
                using var memoryStream = new MemoryStream();
                await model.DocAllegatoGenerico.InputStream.CopyToAsync(memoryStream);
                model.DocAllegatoGenerico_Stream = memoryStream.ToArray();
            }

            if (model.DocEffettiFinanziari != null)
            {
                using var memoryStream = new MemoryStream();
                await model.DocEffettiFinanziari.InputStream.CopyToAsync(memoryStream);
                model.DocEffettiFinanziari_Stream = memoryStream.ToArray();
            }

            var body = JsonConvert.SerializeObject(model);
            await Put(requestUrl, body, _token);
        }

        public async Task ModificaMetaDati(EmendamentiDto model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.AggiornaMetaDati}";
            var body = JsonConvert.SerializeObject(model);
            await Put(requestUrl, body, _token);
        }

        public async Task CambioStato(ModificaStatoModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.ModificaStato}";
            var body = JsonConvert.SerializeObject(model);
            await Put(requestUrl, body, _token);
        }

        public async Task<Dictionary<Guid, string>> Raggruppa(RaggruppaEmendamentiModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.Raggruppa}";
            var body = JsonConvert.SerializeObject(model);
            var result =
                JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Put(requestUrl, body, _token));
            return result;
        }

        public async Task<Dictionary<Guid, string>> AssegnaNuovoPorponente(AssegnaProponenteModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.AssegnaNuovoProponente}";
            var body = JsonConvert.SerializeObject(model);
            var result =
                JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Put(requestUrl, body, _token));
            return result;
        }

        public async Task<IEnumerable<FirmeDto>> GetFirmatari(Guid id, FirmeTipoEnum tipo)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetFirmatari.Replace("{id}", id.ToString()).Replace("{tipo}", tipo.ToString())}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<FirmeDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<DestinatariNotificaDto>> GetInvitati(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetInvitati.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<DestinatariNotificaDto>>(await Get(requestUrl,
                _token));
            return lst;
        }

        public async Task<IEnumerable<StatiDto>> GetStati()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetStati}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<StatiDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<PartiTestoDto>> GetParti()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetParti}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<PartiTestoDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<Tipi_EmendamentiDto>> GetTipi()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetTipi}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<Tipi_EmendamentiDto>>(await Get(requestUrl,
                _token));
            return lst;
        }

        public async Task<IEnumerable<MissioniDto>> GetMissioni()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetMissioni}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<MissioniDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task ORDINA_EM_TRATTAZIONE(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.Ordina.Replace("{id}", id.ToString())}";
            await Get(requestUrl, _token);
        }

        public async Task UP_EM_TRATTAZIONE(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.OrdinaUp.Replace("{id}", id.ToString())}";
            await Get(requestUrl, _token);
        }

        public async Task<List<TagDto>> GetTags()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetTags}";
            var lst = JsonConvert.DeserializeObject<List<TagDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<FileResponse> Download(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.StampaImmediata.Replace("{id}", id.ToString())}";
            var lst = await GetFile(requestUrl, _token);
            return lst;
        }

        public async Task<Dictionary<Guid, string>> GetByJson(Guid uidStampa)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.GetByJson}";
            var bodyObject = new BaseRequest<StampaDto>
            {
                id = uidStampa
            };
            var body = JsonConvert.SerializeObject(bodyObject);
            var res = await Post(requestUrl, body, _token);
            return JsonConvert.DeserializeObject<Dictionary<Guid, string>>(res);
        }

        public async Task DOWN_EM_TRATTAZIONE(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.OrdinaDown.Replace("{id}", id.ToString())}";
            await Get(requestUrl, _token);
        }

        public async Task SPOSTA_EM_TRATTAZIONE(Guid id, int pos)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.Sposta.Replace("{id}", id.ToString()).Replace("{pos}", pos.ToString())}";
            await Get(requestUrl, _token);
        }

        public async Task OrdinamentoConcluso(ComandiAzioneModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.Emendamenti.OrdinamentoConcluso}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }
    }
}