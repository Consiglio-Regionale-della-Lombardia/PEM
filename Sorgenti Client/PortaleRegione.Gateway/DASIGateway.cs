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
using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using Newtonsoft.Json;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;

namespace PortaleRegione.Gateway
{
    public class DASIGateway : BaseGateway, IDASIGateway
    {
        private readonly string _token;

        public DASIGateway(string token)
        {
            _token = token;
        }

        public async Task<AttoDASIDto> Salva(AttoDASIDto request)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi";
                if (request.DocAllegatoGenerico != null)
                {
                    using var memoryStream = new MemoryStream();
                    await request.DocAllegatoGenerico.InputStream.CopyToAsync(memoryStream);
                    request.DocAllegatoGenerico_Stream = memoryStream.ToArray();
                }

                var body = JsonConvert.SerializeObject(request);

                var result = JsonConvert.DeserializeObject<AttoDASIDto>(await Post(requestUrl, body, _token));
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("SalvaDASI", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("SalvaDASI", ex);
                throw ex;
            }
        }

        public Task Modifica(AttoDASIDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<AttoDASIDto> Get(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/{id}";

                var result = JsonConvert.DeserializeObject<AttoDASIDto>(await Get(requestUrl, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetAttoDasi", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetAttoDasi", ex);
                throw ex;
            }
        }

        public async Task<RiepilogoDASIModel> Get(int page, int size, StatiAttoEnum stato, TipoAttoEnum tipo)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/riepilogo";

                var model = new BaseRequest<AttoDASIDto>
                {
                    page = page,
                    size = size
                };
                var filtroStato = new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.IDStato),
                    Operation = Operation.EqualTo,
                    Value = (int) stato,
                    Connector = FilterStatementConnector.And
                };
                model.filtro.Add(filtroStato);
                if (tipo != TipoAttoEnum.TUTTI)
                {
                    var filtroTipo = new FilterStatement<AttoDASIDto>
                    {
                        PropertyId = nameof(AttoDASIDto.Tipo),
                        Operation = Operation.EqualTo,
                        Value = (int) tipo
                    };
                    model.filtro.Add(filtroTipo);
                }

                var body = JsonConvert.SerializeObject(model);

                var result = JsonConvert.DeserializeObject<RiepilogoDASIModel>(await Post(requestUrl, body, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetRiepilogoAttoDasi", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetRiepilogoAttoDasi", ex);
                throw ex;
            }
        }

        public async Task<Dictionary<Guid, string>> Firma(Guid attoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                Lista = new List<Guid> { attoUId },
                Pin = pin
            };
            return await Firma(model);
        }

        public async Task<RiepilogoDASIModel> GetBySeduta(Guid sedutaUId)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/riepilogo";

                var model = new BaseRequest<AttoDASIDto>();
                var filtroSeduta = new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.UIDSeduta),
                    Operation = Operation.EqualTo,
                    Value = sedutaUId
                };
                model.filtro.Add(filtroSeduta);
                
                var body = JsonConvert.SerializeObject(model);

                var result = JsonConvert.DeserializeObject<RiepilogoDASIModel>(await Post(requestUrl, body, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetRiepilogoAttoDasi -  Per Seduta", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetRiepilogoAttoDasi -  Per Seduta", ex);
                throw ex;
            }
        }

        public async Task<Dictionary<Guid, string>> Firma(ComandiAzioneModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/firma";
                var body = JsonConvert.SerializeObject(model);
                var result = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("Firma - DASI", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("Firma - DASI", ex);
                throw ex;
            }
        }

        public async Task<Dictionary<Guid, string>> Deposita(Guid attoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                Lista = new List<Guid> { attoUId },
                Pin = pin
            };
            return await Deposita(model);
        }

        public async Task<Dictionary<Guid, string>> Deposita(ComandiAzioneModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/deposita";
                var body = JsonConvert.SerializeObject(model);
                var result = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("Deposita - DASI", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("Deposita - DASI", ex);
                throw ex;
            }
        }

        public async Task<Dictionary<Guid, string>> RitiraFirma(Guid attoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                Lista = new List<Guid> { attoUId },
                Pin = pin
            };
            return await RitiraFirma(model);
        }

        public async Task<Dictionary<Guid, string>> RitiraFirma(ComandiAzioneModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/ritiro-firma";
                var body = JsonConvert.SerializeObject(model);
                var result = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("RitiraFirma - DASI", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("RitiraFirma - DASI", ex);
                throw ex;
            }
        }

        public async Task<Dictionary<Guid, string>> EliminaFirma(Guid attoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                Lista = new List<Guid> { attoUId },
                Pin = pin
            };
            return await EliminaFirma(model);
        }

        public async Task<Dictionary<Guid, string>> EliminaFirma(ComandiAzioneModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/elimina-firma";
                var body = JsonConvert.SerializeObject(model);
                var result = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("EliminaFirma - DASI", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("EliminaFirma - DASI", ex);
                throw ex;
            }
        }
    }
}