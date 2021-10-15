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
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.IO;
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

        public async Task<EmendamentiViewModel> Get(Guid attoUId, ClientModeEnum mode,
            OrdinamentoEnum ordine,
            int page, int size)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/view";

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
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetEmendamenti", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetEmendamenti", ex);
                throw ex;
            }
        }

        public async Task<EmendamentiViewModel> Get_RichiestaPropriaFirma(Guid attoUId, ClientModeEnum mode,
            OrdinamentoEnum ordine,
            int page, int size)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/view-richiesta-propria-firma";

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
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetEmendamenti_RichiestaPropriaFirma", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetEmendamenti_RichiestaPropriaFirma", ex);
                throw ex;
            }
        }

        public async Task<EmendamentiViewModel> Get(BaseRequest<EmendamentiDto> model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/view";
                var body = JsonConvert.SerializeObject(model);

                var lst = JsonConvert.DeserializeObject<EmendamentiViewModel>(await Post(requestUrl, body, _token));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetEmendamenti", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetEmendamenti", ex);
                throw ex;
            }
        }

        public async Task<EmendamentiDto> Get(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti?id={id}";

                var lst = JsonConvert.DeserializeObject<EmendamentiDto>(await Get(requestUrl, _token));
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetEmendamento", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetEmendamento", ex);
                throw ex;
            }
        }

        public async Task<string> GetBody(Guid id, TemplateTypeEnum template, bool IsDeposito = false)
        {
            var result = string.Empty;
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/template-body";
                var model = new GetBodyEmendamentoModel
                {
                    Id = id,
                    Template = template,
                    IsDeposito = IsDeposito
                };
                var body = JsonConvert.SerializeObject(model);
                result = await Post(requestUrl, body, _token);
                var lst = JsonConvert.DeserializeObject<string>(result);

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetBodyEM", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error($"GetBodyEM PARAMS: GUID EM [{id}], TEMPLATE [{template}]");
                Log.Error($"GetBodyEM RESULT: [{result}]");
                Log.Error("GetBodyEM", ex);
                throw ex;
            }
        }

        public async Task<string> GetCopertina(CopertinaModel model)
        {
            var result = string.Empty;
            var body = string.Empty;

            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/template/copertina";
                body = JsonConvert.SerializeObject(model);

                result = await Post(requestUrl, body, _token);
                var lst = JsonConvert.DeserializeObject<string>(result);
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetCopertina", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error($"GetCopertina PARAMS: [{body}]");
                Log.Error($"GetCopertina RESULT: [{result}]");
                Log.Error("GetCopertina", ex);
                throw ex;
            }
        }

        public async Task Elimina(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/elimina?id={id}";

                JsonConvert.DeserializeObject<string>(await Get(requestUrl, _token));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("EliminaEM", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("EliminaEM", ex);
                throw ex;
            }
        }

        public async Task Ritira(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/ritira?id={id}";

                JsonConvert.DeserializeObject<string>(await Get(requestUrl, _token));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("RitiraEM", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("RitiraEM", ex);
                throw ex;
            }
        }

        public async Task<Dictionary<Guid, string>> Firma(Guid emendamentoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                ListaEmendamenti = new List<Guid> { emendamentoUId },
                Pin = pin
            };
            return await Firma(model);
        }

        public async Task<Dictionary<Guid, string>> Firma(ComandiAzioneModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/firma";
                var body = JsonConvert.SerializeObject(model);
                var result = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("FirmaEM", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("FirmaEM", ex);
                throw ex;
            }
        }

        public async Task<Dictionary<Guid, string>> Deposita(Guid emendamentoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                ListaEmendamenti = new List<Guid> { emendamentoUId },
                Pin = pin
            };
            return await Deposita(model);
        }

        public async Task<Dictionary<Guid, string>> Deposita(ComandiAzioneModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/deposita";
                var body = JsonConvert.SerializeObject(model);
                var result = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("DepositaEM", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("DepositaEM", ex);
                throw ex;
            }
        }

        public async Task<Dictionary<Guid, string>> RitiraFirma(Guid emendamentoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                ListaEmendamenti = new List<Guid> { emendamentoUId },
                Pin = pin
            };
            return await RitiraFirma(model);
        }

        public async Task<Dictionary<Guid, string>> RitiraFirma(ComandiAzioneModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/ritiro-firma";
                var body = JsonConvert.SerializeObject(model);
                var result = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("RitiraFirma", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("RitiraFirma", ex);
                throw ex;
            }
        }

        public async Task<Dictionary<Guid, string>> EliminaFirma(Guid emendamentoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                ListaEmendamenti = new List<Guid> { emendamentoUId },
                Pin = pin
            };
            return await EliminaFirma(model);
        }

        public async Task<Dictionary<Guid, string>> EliminaFirma(ComandiAzioneModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/elimina-firma";
                var body = JsonConvert.SerializeObject(model);
                var result = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("EliminaFirma", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("EliminaFirma", ex);
                throw ex;
            }
        }

        public async Task<int> GetProgressivoTemporaneo(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/progressivo?id={id}";

                var lst = JsonConvert.DeserializeObject<int>(await Get(requestUrl, _token));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetProgressivoTemporaneo", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetProgressivoTemporaneo", ex);
                throw ex;
            }
        }

        public async Task<EmendamentiFormModel> GetNuovoModel(Guid id, Guid? em_riferimentoUId)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/new?id={id}";
                if (em_riferimentoUId.HasValue)
                    requestUrl += $"&em_riferimentoUId={em_riferimentoUId}";
                var result = await Get(requestUrl, _token);
                var lst = JsonConvert.DeserializeObject<EmendamentiFormModel>(result);
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetNuovoEmendamentoModel", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetNuovoEmendamentoModel", ex);
                throw ex;
            }
        }

        public async Task<EmendamentiFormModel> GetModificaModel(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/edit?id={id}";

                var lst = JsonConvert.DeserializeObject<EmendamentiFormModel>(await Get(requestUrl, _token));
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetModificaEmendamentoModel", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetModificaEmendamentoModel", ex);
                throw ex;
            }
        }

        public async Task<EmendamentiFormModel> GetModificaMetaDatiModel(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/edit-meta-dati?id={id}";

                var lst = JsonConvert.DeserializeObject<EmendamentiFormModel>(await Get(requestUrl, _token));
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetModificaMetaDatiEmendamentoModel", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetModificaMetaDatiEmendamentoModel", ex);
                throw ex;
            }
        }

        public async Task<EmendamentiDto> Salva(EmendamentiDto model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti";
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
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("SalvaEmendamento", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("SalvaEmendamento", ex);
                throw ex;
            }
        }

        public async Task Modifica(EmendamentiDto model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti";
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
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("ModificaEmendamento", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("ModificaEmendamento", ex);
                throw ex;
            }
        }

        public async Task ModificaMetaDati(EmendamentiDto model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/meta-dati";
                var body = JsonConvert.SerializeObject(model);

                await Put(requestUrl, body, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("ModificaMetaDatiEmendamento", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("ModificaMetaDatiEmendamento", ex);
                throw ex;
            }
        }

        public async Task CambioStato(ModificaStatoModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/modifica-stato";
                var body = JsonConvert.SerializeObject(model);

                await Put(requestUrl, body, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("CambioStato", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("CambioStato", ex);
                throw ex;
            }
        }

        public async Task<Dictionary<Guid, string>> Raggruppa(RaggruppaEmendamentiModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/raggruppa";
                var body = JsonConvert.SerializeObject(model);

                var result = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Put(requestUrl, body, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("RaggruppaEmendamenti", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("RaggruppaEmendamenti", ex);
                throw ex;
            }
        }

        public async Task<Dictionary<Guid, string>> AssegnaNuovoPorponente(AssegnaProponenteModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/assegna-nuovo-proponente";
                var body = JsonConvert.SerializeObject(model);

                var result = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Put(requestUrl, body, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("AssegnaNuovoPorponente", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("AssegnaNuovoPorponente", ex);
                throw ex;
            }
        }

        public async Task<IEnumerable<FirmeDto>> GetFirmatari(Guid emendamentoUId, FirmeTipoEnum tipo)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/firmatari?id={emendamentoUId}&tipo={tipo}";

                var lst = JsonConvert.DeserializeObject<IEnumerable<FirmeDto>>(await Get(requestUrl, _token));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetFirmatari", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetFirmatari", ex);
                throw ex;
            }
        }

        public async Task<IEnumerable<DestinatariNotificaDto>> GetInvitati(Guid emendamentoUId)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/invitati?id={emendamentoUId}";

                var lst = JsonConvert.DeserializeObject<IEnumerable<DestinatariNotificaDto>>(await Get(requestUrl, _token));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetInvitati", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetInvitati", ex);
                throw ex;
            }
        }

        public async Task Proietta(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/proietta?id={id}";
                await Get(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("Proietta", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("Proietta", ex);
                throw ex;
            }
        }

        public async Task<ProiettaResponse> Proietta_View(Guid id, int ordineVotazione)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/proietta-view?id={id}&ordine={ordineVotazione}";
                var result = JsonConvert.DeserializeObject<ProiettaResponse>(await Get(requestUrl, _token));
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("Proietta", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("Proietta", ex);
                throw ex;
            }
        }

        public async Task<ProiettaResponse> Proietta_ViewLive(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/proietta-view-live?id={id}";
                var result = JsonConvert.DeserializeObject<ProiettaResponse>(await Get(requestUrl, _token));
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("Proietta - Live", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("Proietta", ex);
                throw ex;
            }
        }

        public async Task<IEnumerable<StatiDto>> GetStati()
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/stati-em";

                var lst = JsonConvert.DeserializeObject<IEnumerable<StatiDto>>(await Get(requestUrl, _token));
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetStatiEM", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetStatiEM", ex);
                throw ex;
            }
        }

        public async Task<IEnumerable<PartiTestoDto>> GetParti()
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/parti-em";

                var lst = JsonConvert.DeserializeObject<IEnumerable<PartiTestoDto>>(await Get(requestUrl, _token));
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetPartiEM", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetPartiEM", ex);
                throw ex;
            }
        }

        public async Task<IEnumerable<Tipi_EmendamentiDto>> GetTipi()
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/tipi-em";

                var lst = JsonConvert.DeserializeObject<IEnumerable<Tipi_EmendamentiDto>>(await Get(requestUrl, _token));
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetTipiEM", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetTipiEM", ex);
                throw ex;
            }
        }

        public async Task<IEnumerable<MissioniDto>> GetMissioni()
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/missioni-em";

                var lst = JsonConvert.DeserializeObject<IEnumerable<MissioniDto>>(await Get(requestUrl, _token));
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetMissioniEM", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetMissioniEM", ex);
                throw ex;
            }
        }

        public async Task ORDINA_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/ordina?id={id}";
                await Get(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("ORDINA_EM_TRATTAZIONE", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("ORDINA_EM_TRATTAZIONE", ex);
                throw ex;
            }
        }

        public async Task UP_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/ordina-up?id={id}";
                await Get(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("UP_EM_TRATTAZIONE", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("UP_EM_TRATTAZIONE", ex);
                throw ex;
            }
        }

        public async Task DOWN_EM_TRATTAZIONE(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/ordina-down?id={id}";
                await Get(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("DOWN_EM_TRATTAZIONE", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("DOWN_EM_TRATTAZIONE", ex);
                throw ex;
            }
        }

        public async Task SPOSTA_EM_TRATTAZIONE(Guid id, int pos)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/sposta?id={id}&pos={pos}";
                await Get(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("SPOSTA_EM_TRATTAZIONE", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("SPOSTA_EM_TRATTAZIONE", ex);
                throw ex;
            }
        }

        public async Task ORDINAMENTO_EM_TRATTAZIONE_CONCLUSO(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/emendamenti/ordinamento-concluso?id={id}";
                await Get(requestUrl, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("ORDINAMENTO_EM_TRATTAZIONE_CONCLUSO", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("ORDINAMENTO_EM_TRATTAZIONE_CONCLUSO", ex);
                throw ex;
            }
        }
    }
}