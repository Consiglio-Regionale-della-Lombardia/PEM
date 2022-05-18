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

        public async Task<RiepilogoDASIModel> Get(int page, int size, StatiAttoEnum stato, TipoAttoEnum tipo, PersonaDto persona)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/riepilogo";

                var model = new BaseRequest<AttoDASIDto>
                {
                    page = page,
                    size = size,
                    param = new Dictionary<string, object> { { "CLIENT_MODE", (int)ClientModeEnum.GRUPPI } }
                };
                var operationStato = Operation.EqualTo;
                if ((int)stato>(int)StatiAttoEnum.BOZZA)
                {
                    if (persona.CurrentRole != RuoliIntEnum.Segreteria_Assemblea
                        && persona.CurrentRole != RuoliIntEnum.Amministratore_PEM)
                    {
                        //cosi il consigliere può visualizzare tutti i suoi atti dallo stato presentato in poi
                        operationStato = Operation.GreaterThanOrEqualTo;
                    }
                }
                else if ((int) stato <= (int)StatiAttoEnum.BOZZA)
                {
                    if (persona.CurrentRole == RuoliIntEnum.Segreteria_Assemblea
                        || persona.CurrentRole == RuoliIntEnum.Amministratore_PEM)
                    {
                        //Imposto stato di default per le segreteria e amministratori
                        operationStato = Operation.EqualTo;
                        stato = StatiAttoEnum.PRESENTATO;
                    }
                }

                var filtroStato = new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.IDStato),
                    Operation = operationStato,
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
                Lista = new List<Guid> {attoUId},
                Pin = pin
            };
            return await Firma(model);
        }

        public async Task<RiepilogoDASIModel> GetBySeduta(Guid sedutaUId)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/riepilogo";

                var model = new BaseRequest<AttoDASIDto>
                {
                    page = 1,
                    size = 50,
                    param = new Dictionary<string, object> { { "CLIENT_MODE", (int)ClientModeEnum.TRATTAZIONE } }
                };
                var filtroSeduta = new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.UIDSeduta),
                    Operation = Operation.EqualTo,
                    Value = sedutaUId,
                    Connector = FilterStatementConnector.And
                };
                model.filtro.Add(filtroSeduta);
                var filtroStato = new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.IDStato),
                    Operation = Operation.GreaterThanOrEqualTo,
                    Value = (int) StatiAttoEnum.IN_TRATTAZIONE
                };
                model.filtro.Add(filtroStato);

                var body = JsonConvert.SerializeObject(model);

                var result = JsonConvert.DeserializeObject<RiepilogoDASIModel>(await Post(requestUrl, body, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetRiepilogoAttoDasi - Per Seduta", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetRiepilogoAttoDasi - Per Seduta", ex);
                throw ex;
            }
        }

        public async Task<RiepilogoDASIModel> GetBySeduta_Trattazione(Guid id, TipoAttoEnum tipoAtto, int page = 1,
            int size = 50)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/riepilogo";
                var request = new BaseRequest<AttoDASIDto>
                {
                    page = page,
                    size = size,
                    param = new Dictionary<string, object> { { "CLIENT_MODE", (int)ClientModeEnum.TRATTAZIONE } }
                };
                var filtroSeduta = new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.UIDSeduta),
                    Operation = Operation.EqualTo,
                    Value = id,
                    Connector = FilterStatementConnector.And
                };
                request.filtro.Add(filtroSeduta);
                var filtroTipo = new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.Tipo),
                    Operation = Operation.EqualTo,
                    Value = (int)tipoAtto,
                    Connector = FilterStatementConnector.And
                };
                request.filtro.Add(filtroTipo);
                var filtroStato = new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.IDStato),
                    Operation = Operation.GreaterThanOrEqualTo,
                    Value = (int)StatiAttoEnum.IN_TRATTAZIONE
                };
                request.filtro.Add(filtroStato);

                var body = JsonConvert.SerializeObject(request);

                var result = JsonConvert.DeserializeObject<RiepilogoDASIModel>(await Post(requestUrl, body, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetBySeduta_Trattazione - Per Seduta", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetBySeduta_Trattazione - Per Seduta", ex);
                throw ex;
            }
        }

        public async Task<IEnumerable<AttiFirmeDto>> GetFirmatari(Guid id, FirmeTipoEnum tipo)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/firmatari?id={id}&tipo={tipo}";

                var lst = JsonConvert.DeserializeObject<IEnumerable<AttiFirmeDto>>(await Get(requestUrl, _token));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetFirmatari - DASI", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetFirmatari - DASI", ex);
                throw ex;
            }
        }

        public async Task<string> GetBody(Guid id, TemplateTypeEnum template)
        {
            var result = string.Empty;
            try
            {
                var requestUrl = $"{apiUrl}/dasi/template-body";
                var model = new GetBodyModel
                {
                    Id = id,
                    Template = template
                };
                var body = JsonConvert.SerializeObject(model);
                result = await Post(requestUrl, body, _token);
                var lst = JsonConvert.DeserializeObject<string>(result);

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetBodyAtto - DASI", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error($"GetBodyAtto - DASI - PARAMS: GUID ATTO [{id}], TEMPLATE [{template}]");
                Log.Error($"GetBodyAtto - DASI - RESULT: [{result}]");
                Log.Error("GetBodyAtto - DASI", ex);
                throw ex;
            }
        }

        public async Task Elimina(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/elimina?id={id}";

                JsonConvert.DeserializeObject<string>(await Get(requestUrl, _token));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("Elimina Atto - DASI", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("Elimina Atto - DASI", ex);
                throw ex;
            }
        }

        public async Task Ritira(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/ritira?id={id}";

                JsonConvert.DeserializeObject<string>(await Get(requestUrl, _token));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("Ritira Atto - DASI", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("Ritira Atto - DASI", ex);
                throw ex;
            }
        }

        public async Task<DASIFormModel> GetNuovoModello(TipoAttoEnum tipo)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/new?tipo={tipo}";

                var result = await Get(requestUrl, _token);
                var lst = JsonConvert.DeserializeObject<DASIFormModel>(result);
                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetNuovoModello", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetNuovoModello", ex);
                throw ex;
            }
        }

        public async Task<DASIFormModel> GetModificaModello(Guid id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/edit?id={id}";

                var lst = JsonConvert.DeserializeObject<DASIFormModel>(await Get(requestUrl, _token));
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


        public async Task<Dictionary<Guid, string>> Firma(ComandiAzioneModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/firma";
                var body = JsonConvert.SerializeObject(model);
                var result =
                    JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

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

        public async Task<Dictionary<Guid, string>> Presenta(Guid attoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                Lista = new List<Guid> {attoUId},
                Pin = pin
            };
            return await Presenta(model);
        }

        public async Task<Dictionary<Guid, string>> Presenta(ComandiAzioneModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/presenta";
                var body = JsonConvert.SerializeObject(model);
                var result =
                    JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("Presenta - DASI", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("Presenta - DASI", ex);
                throw ex;
            }
        }

        public async Task<Dictionary<Guid, string>> EliminaFirma(Guid attoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                Lista = new List<Guid> {attoUId},
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
                var result =
                    JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

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

        public async Task<Dictionary<Guid, string>> RitiraFirma(Guid attoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                Lista = new List<Guid> {attoUId},
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
                var result =
                    JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

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

        public async Task CambioStato(ModificaStatoAttoModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/modifica-stato";
                var body = JsonConvert.SerializeObject(model);

                await Put(requestUrl, body, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("CambioStato - DASI", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("CambioStato - DASI", ex);
                throw ex;
            }
        }

        public async Task IscriviSeduta(IscriviSedutaDASIModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/iscrizione-seduta";
                var body = JsonConvert.SerializeObject(model);

                await Post(requestUrl, body, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("IscriviSeduta - DASI", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("IscriviSeduta - DASI", ex);
                throw ex;
            }
        }

        public async Task RimuoviSeduta(IscriviSedutaDASIModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/rimuovi-seduta";
                var body = JsonConvert.SerializeObject(model);

                await Post(requestUrl, body, _token);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("RimuoviSeduta - DASI", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("RimuoviSeduta - DASI", ex);
                throw ex;
            }
        }

        public async Task<IEnumerable<DestinatariNotificaDto>> GetInvitati(Guid emendamentoUId)
        {
            try
            {
                var requestUrl = $"{apiUrl}/dasi/invitati?id={emendamentoUId}";

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
    }
}