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
    public class DASIGateway : BaseGateway, IDASIGateway
    {
        private readonly string _token;

        public DASIGateway(string token)
        {
            _token = token;
        }

        public async Task<AttoDASIDto> Salva(AttoDASIDto request)
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

        public Task Modifica(AttoDASIDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<AttoDASIDto> Get(Guid id)
        {
            var requestUrl = $"{apiUrl}/dasi/{id}";

            var result = JsonConvert.DeserializeObject<AttoDASIDto>(await Get(requestUrl, _token));

            return result;
        }

        public async Task<List<AttoDASIDto>> GetMOZAbbinabili()
        {
            var requestUrl = $"{apiUrl}/dasi/moz-abbinabili";
            var str = await Get(requestUrl, _token);
            var result = JsonConvert.DeserializeObject<List<AttoDASIDto>>(str);

            return result;
        }

        public async Task<List<AttiDto>> GetAttiSeduteAttive()
        {
            var requestUrl = $"{apiUrl}/dasi/odg/atti-sedute-attive";

            var result = JsonConvert.DeserializeObject<List<AttiDto>>(await Get(requestUrl, _token));

            return result;
        }

        public async Task<RiepilogoDASIModel> Get(int page, int size, StatiAttoEnum stato, TipoAttoEnum tipo,
            RuoliIntEnum ruolo)
        {
            var requestUrl = $"{apiUrl}/dasi/riepilogo";

            var model = new BaseRequest<AttoDASIDto>
            {
                page = page,
                size = size,
                param = new Dictionary<string, object> { { "CLIENT_MODE", (int)ClientModeEnum.GRUPPI } }
            };
            var operationStato = Operation.EqualTo;
            if ((int)stato > (int)StatiAttoEnum.BOZZA)
            {
                if (ruolo != RuoliIntEnum.Segreteria_Assemblea
                    && ruolo != RuoliIntEnum.Amministratore_PEM)
                {
                    //cosi il consigliere può visualizzare tutti i suoi atti dallo stato presentato in poi
                    operationStato = Operation.GreaterThanOrEqualTo;
                }
            }
            else if ((int)stato <= (int)StatiAttoEnum.BOZZA)
            {
                if (ruolo == RuoliIntEnum.Segreteria_Assemblea
                    || ruolo == RuoliIntEnum.Amministratore_PEM)
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
                Value = (int)stato,
                Connector = FilterStatementConnector.And
            };
            model.filtro.Add(filtroStato);
            if (tipo != TipoAttoEnum.TUTTI)
            {
                var filtroTipo = new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.Tipo),
                    Operation = Operation.EqualTo,
                    Value = (int)tipo,
                    Connector = FilterStatementConnector.And
                };
                model.filtro.Add(filtroTipo);
            }

            var body = JsonConvert.SerializeObject(model);

            var result = JsonConvert.DeserializeObject<RiepilogoDASIModel>(await Post(requestUrl, body, _token));

            return result;
        }

        public async Task<RiepilogoDASIModel> Get(BaseRequest<AttoDASIDto> model)
        {
            var requestUrl = $"{apiUrl}/dasi/riepilogo";
            var body = JsonConvert.SerializeObject(model);

            var lst = JsonConvert.DeserializeObject<RiepilogoDASIModel>(await Post(requestUrl, body, _token));

            return lst;
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

            var body = JsonConvert.SerializeObject(model);

            var result = JsonConvert.DeserializeObject<RiepilogoDASIModel>(await Post(requestUrl, body, _token));

            return result;
        }

        public async Task<RiepilogoDASIModel> GetBySeduta_Trattazione(Guid id, TipoAttoEnum tipoAtto, string uidAtto,
            int page = 1,
            int size = 50)
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
            if (!string.IsNullOrEmpty(uidAtto))
            {
                var tryParse = Guid.TryParse(uidAtto, out var guid);
                if (tryParse)
                {
                    if (guid != Guid.Empty)
                    {
                        var filtroAttoPEM = new FilterStatement<AttoDASIDto>
                        {
                            PropertyId = nameof(AttoDASIDto.UID_Atto_ODG),
                            Operation = Operation.EqualTo,
                            Value = guid,
                            Connector = FilterStatementConnector.And
                        };
                        request.filtro.Add(filtroAttoPEM);
                    }
                }
            }

            var body = JsonConvert.SerializeObject(request);

            var result = JsonConvert.DeserializeObject<RiepilogoDASIModel>(await Post(requestUrl, body, _token));

            return result;
        }

        public async Task<IEnumerable<AttiFirmeDto>> GetFirmatari(Guid id, FirmeTipoEnum tipo)
        {
            var requestUrl = $"{apiUrl}/dasi/firmatari?id={id}&tipo={tipo}";

            var lst = JsonConvert.DeserializeObject<IEnumerable<AttiFirmeDto>>(await Get(requestUrl, _token));

            return lst;
        }

        public async Task<string> GetBody(Guid id, TemplateTypeEnum template)
        {
            var requestUrl = $"{apiUrl}/dasi/template-body";
            var model = new GetBodyModel
            {
                Id = id,
                Template = template
            };
            var body = JsonConvert.SerializeObject(model);
            var result = await Post(requestUrl, body, _token);
            var lst = JsonConvert.DeserializeObject<string>(result);

            return lst;
        }

        public async Task Elimina(Guid id)
        {
            var requestUrl = $"{apiUrl}/dasi/elimina?id={id}";

            JsonConvert.DeserializeObject<string>(await Get(requestUrl, _token));
        }

        public async Task Ritira(Guid id)
        {
            var requestUrl = $"{apiUrl}/dasi/ritira?id={id}";

            JsonConvert.DeserializeObject<string>(await Get(requestUrl, _token));
        }

        public async Task<DASIFormModel> GetNuovoModello(TipoAttoEnum tipo)
        {
            var requestUrl = $"{apiUrl}/dasi/new?tipo={tipo}";

            var result = await Get(requestUrl, _token);
            var lst = JsonConvert.DeserializeObject<DASIFormModel>(result);
            return lst;
        }

        public async Task<DASIFormModel> GetModificaModello(Guid id)
        {
            var requestUrl = $"{apiUrl}/dasi/edit?id={id}";

            var lst = JsonConvert.DeserializeObject<DASIFormModel>(await Get(requestUrl, _token));
            return lst;
        }


        public async Task<Dictionary<Guid, string>> Firma(ComandiAzioneModel model)
        {
            var requestUrl = $"{apiUrl}/dasi/firma";
            var body = JsonConvert.SerializeObject(model);
            var result =
                JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

            return result;
        }

        public async Task<Dictionary<Guid, string>> Presenta(Guid attoUId, string pin)
        {
            var model = new ComandiAzioneModel
            {
                Lista = new List<Guid> { attoUId },
                Pin = pin
            };
            return await Presenta(model);
        }

        public async Task<Dictionary<Guid, string>> Presenta(ComandiAzioneModel model)
        {
            var requestUrl = $"{apiUrl}/dasi/presenta";
            var body = JsonConvert.SerializeObject(model);
            var result =
                JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

            return result;
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
            var requestUrl = $"{apiUrl}/dasi/elimina-firma";
            var body = JsonConvert.SerializeObject(model);
            var result =
                JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

            return result;
        }

        public async Task CambioStato(ModificaStatoAttoModel model)
        {
            var requestUrl = $"{apiUrl}/dasi/modifica-stato";
            var body = JsonConvert.SerializeObject(model);

            await Put(requestUrl, body, _token);
        }

        public async Task IscriviSeduta(IscriviSedutaDASIModel model)
        {
            var requestUrl = $"{apiUrl}/dasi/iscrizione-seduta";
            var body = JsonConvert.SerializeObject(model);

            await Post(requestUrl, body, _token);
        }

        public async Task RichiediIscrizione(RichiestaIscrizioneDASIModel model)
        {
            var requestUrl = $"{apiUrl}/dasi/richiedi-iscrizione";
            var body = JsonConvert.SerializeObject(model);

            await Post(requestUrl, body, _token);
        }

        public async Task RimuoviSeduta(IscriviSedutaDASIModel model)
        {
            var requestUrl = $"{apiUrl}/dasi/rimuovi-seduta";
            var body = JsonConvert.SerializeObject(model);

            await Post(requestUrl, body, _token);
        }

        public async Task RimuoviRichiestaIscrizione(RichiestaIscrizioneDASIModel model)
        {
            var requestUrl = $"{apiUrl}/dasi/rimuovi-richiesta";
            var body = JsonConvert.SerializeObject(model);

            await Post(requestUrl, body, _token);
        }

        public async Task ProponiMozioneUrgente(PromuoviMozioneModel model)
        {
            var requestUrl = $"{apiUrl}/dasi/proponi-urgenza";
            var body = JsonConvert.SerializeObject(model);

            await Post(requestUrl, body, _token);
        }

        public async Task ProponiMozioneAbbinata(PromuoviMozioneModel model)
        {
            var requestUrl = $"{apiUrl}/dasi/proponi-abbinata";
            var body = JsonConvert.SerializeObject(model);

            await Post(requestUrl, body, _token);
        }

        public async Task<IEnumerable<DestinatariNotificaDto>> GetInvitati(Guid guid)
        {
            var requestUrl = $"{apiUrl}/dasi/invitati?id={guid}";

            var lst = JsonConvert.DeserializeObject<IEnumerable<DestinatariNotificaDto>>(await Get(requestUrl,
                _token));

            return lst;
        }

        public async Task<string> GetCopertina(ByQueryModel model)
        {
            var requestUrl = $"{apiUrl}/dasi/template/copertina";
            var body = JsonConvert.SerializeObject(model);

            var result = await Post(requestUrl, body, _token);
            var lst = JsonConvert.DeserializeObject<string>(result);
            return lst;
        }

        public async Task<IEnumerable<StatiDto>> GetStati()
        {
            var requestUrl = $"{apiUrl}/dasi/stati";

            var lst = JsonConvert.DeserializeObject<IEnumerable<StatiDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<Tipi_AttoDto>> GetTipiMOZ()
        {
            var requestUrl = $"{apiUrl}/dasi/tipi-moz";

            var lst = JsonConvert.DeserializeObject<IEnumerable<Tipi_AttoDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<AssessoreInCaricaDto>> GetSoggettiInterrogabili()
        {
            var requestUrl = $"{apiUrl}/dasi/soggetti-interrogabili";

            var lst =
                JsonConvert.DeserializeObject<IEnumerable<AssessoreInCaricaDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task ModificaMetaDati(AttoDASIDto model)
        {
            var requestUrl = $"{apiUrl}/dasi/meta-dati";
            var body = JsonConvert.SerializeObject(model);

            await Put(requestUrl, body, _token);
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

        public async Task PresentazioneCartacea(PresentazioneCartaceaModel model)
        {
            var requestUrl = $"{apiUrl}/dasi/presentazione-cartacea";
            var body = JsonConvert.SerializeObject(model);

            await Post(requestUrl, body, _token);
        }

        public async Task<FileResponse> Download(Guid id)
        {
            var requestUrl = $"{apiUrl}/dasi/file-immediato?id={id}";

            var lst = await GetFile(requestUrl, _token);

            return lst;
        }

        public async Task InviaAlProtocollo(Guid id)
        {
            var requestUrl = $"{apiUrl}/dasi/{id}/invia-al-protocollo";

            await Get(requestUrl, _token);
        }

        public async Task RimuoviUrgenzaMozione(List<string> data)
        {
            var requestUrl = $"{apiUrl}/dasi/rimuovi-urgenza-mozione";
            var body = JsonConvert.SerializeObject(data);
            await Post(requestUrl, body, _token);
        }

        public async Task<Dictionary<Guid, string>> RitiraFirma(ComandiAzioneModel model)
        {
            var requestUrl = $"{apiUrl}/dasi/ritiro-firma";
            var body = JsonConvert.SerializeObject(model);
            var result =
                JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

            return result;
        }
    }
}