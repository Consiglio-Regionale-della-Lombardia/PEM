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
using PortaleRegione.DTO.Routes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PortaleRegione.Common;

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
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.Save}";
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

        public async Task<AttoDASIDto> Get(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.Get.Replace("{id}", id.ToString())}";
            var result = JsonConvert.DeserializeObject<AttoDASIDto>(await Get(requestUrl, _token));
            return result;
        }

        public async Task<List<Guid>> GetSoloIds(BaseRequest<AttoDASIDto> model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetAll_SoloIds}";
            var body = JsonConvert.SerializeObject(model);
            var lst = JsonConvert.DeserializeObject<List<Guid>>(await Post(requestUrl, body, _token));
            return lst;
        }

        public async Task<List<AttoDASIDto>> GetMOZAbbinabili()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetMOZAbbinabili}";
            var str = await Get(requestUrl, _token);
            var result = JsonConvert.DeserializeObject<List<AttoDASIDto>>(str);
            return result;
        }

        public async Task<List<AttiDto>> GetAttiSeduteAttive()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetAttiSeduteAttive}";
            var result = JsonConvert.DeserializeObject<List<AttiDto>>(await Get(requestUrl, _token));
            return result;
        }

        public async Task<RiepilogoDASIModel> Get(int page, int size, StatiAttoEnum stato, TipoAttoEnum tipo,
            RuoliIntEnum ruolo, bool propria_firma = false)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetAll}";
            var model = new BaseRequest<AttoDASIDto>
            {
                page = page,
                size = size,
                param = new Dictionary<string, object> { { "CLIENT_MODE", (int)ClientModeEnum.GRUPPI }, { "RequireMySign", propria_firma } }
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
                Connector = FilterStatementConnector.And,
                Label = Utility.GetDisplayName(typeof(AttoDASIDto),nameof(AttoDASIDto.IDStato))
            };
            model.filtro.Add(filtroStato);
            if (tipo != TipoAttoEnum.TUTTI)
            {
                var filtroTipo = new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.Tipo),
                    Operation = Operation.EqualTo,
                    Value = (int)tipo,
                    Connector = FilterStatementConnector.And,
                    Label = Utility.GetDisplayName(typeof(AttoDASIDto),nameof(AttoDASIDto.Tipo))
                };
                model.filtro.Add(filtroTipo);
            }

            var body = JsonConvert.SerializeObject(model);
            var x = await Post(requestUrl, body, _token);
            var result = JsonConvert.DeserializeObject<RiepilogoDASIModel>(x);
            return result;
        }

        public async Task<RiepilogoDASIModel> Get(int page, int size, StatiAttoEnum stato, TipoAttoEnum tipo, RuoliIntEnum ruolo, int legislatura,
            bool propria_firma = false)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetAll}";
            var model = new BaseRequest<AttoDASIDto>
            {
                page = page,
                size = size,
                param = new Dictionary<string, object> { { "CLIENT_MODE", (int)ClientModeEnum.GRUPPI }, { "RequireMySign", propria_firma } }
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
                Connector = FilterStatementConnector.And,
                Label = Utility.GetDisplayName(typeof(AttoDASIDto),nameof(AttoDASIDto.IDStato))
            };
            model.filtro.Add(filtroStato);
            if (tipo != TipoAttoEnum.TUTTI)
            {
                var filtroTipo = new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.Tipo),
                    Operation = Operation.EqualTo,
                    Value = (int)tipo,
                    Connector = FilterStatementConnector.And,
                    Label = Utility.GetDisplayName(typeof(AttoDASIDto),nameof(AttoDASIDto.Tipo))
                };
                model.filtro.Add(filtroTipo);
            }

            var filtroLegislatura = new FilterStatement<AttoDASIDto>
            {
                PropertyId = nameof(AttoDASIDto.Legislatura),
                Operation = Operation.EqualTo,
                Value = legislatura,
                Connector = FilterStatementConnector.And,
                Label = Utility.GetDisplayName(typeof(AttoDASIDto),nameof(AttoDASIDto.Legislatura))
            };
            model.filtro.Add(filtroLegislatura);

            var body = JsonConvert.SerializeObject(model);
            var x = await Post(requestUrl, body, _token);
            var result = JsonConvert.DeserializeObject<RiepilogoDASIModel>(x);
            return result;
        }

        public async Task<RiepilogoDASIModel> Get(BaseRequest<AttoDASIDto> model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetAll}";
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
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetAll}";

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
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetAll}";
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
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetFirmatari.Replace("{id}", id.ToString()).Replace("{tipo}", tipo.ToString())}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<AttiFirmeDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<string> GetBody(Guid id, TemplateTypeEnum template, bool privacy = false)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetBody}";
            var model = new GetBodyModel
            {
                Id = id,
                Template = template,
                privacy = privacy
            };
            var body = JsonConvert.SerializeObject(model);
            var result = await Post(requestUrl, body, _token);
            var lst = JsonConvert.DeserializeObject<string>(result);
            return lst;
        }

        public async Task Elimina(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.Elimina.Replace("{id}", id.ToString())}";
            JsonConvert.DeserializeObject<string>(await Get(requestUrl, _token));
        }

        public async Task Ritira(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.Ritira.Replace("{id}", id.ToString())}";
            JsonConvert.DeserializeObject<string>(await Get(requestUrl, _token));
        }

        public async Task<DASIFormModel> GetNuovoModello(TipoAttoEnum tipo)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetNuovoModello.Replace("{tipo}", tipo.ToString())}";
            var result = await Get(requestUrl, _token);
            var lst = JsonConvert.DeserializeObject<DASIFormModel>(result);
            return lst;
        }

        public async Task<DASIFormModel> GetModificaModello(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetModificaModello.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<DASIFormModel>(await Get(requestUrl, _token));
            return lst;
        }


        public async Task<Dictionary<Guid, string>> Firma(ComandiAzioneModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.Firma}";
            var body = JsonConvert.SerializeObject(model);
            var result = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));
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
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.Presenta}";
            var body = JsonConvert.SerializeObject(model);
            var result = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));
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
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.EliminaFirma}";
            var body = JsonConvert.SerializeObject(model);
            var result = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));
            return result;
        }

        public async Task CambioStato(ModificaStatoAttoModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.ModificaStato}";
            var body = JsonConvert.SerializeObject(model);
            await Put(requestUrl, body, _token);
        }

        public async Task IscriviSeduta(IscriviSedutaDASIModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.IscriviSeduta}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }

        public async Task RichiediIscrizione(RichiestaIscrizioneDASIModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.RichiediIscrizione}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }

        public async Task RimuoviSeduta(IscriviSedutaDASIModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.RimuoviSeduta}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }

        public async Task RimuoviRichiestaIscrizione(RichiestaIscrizioneDASIModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.RimuoviRichiestaIscrizione}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }

        public async Task ProponiMozioneUrgente(PromuoviMozioneModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.ProponiUrgenzaMozione}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }

        public async Task ProponiMozioneAbbinata(PromuoviMozioneModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.ProponiMozioneAbbinata}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }

        public async Task<IEnumerable<DestinatariNotificaDto>> GetInvitati(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetInvitati.Replace("{id}", id.ToString())}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<DestinatariNotificaDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<string> GetCopertina(ByQueryModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetBodyCopertina}";
            var body = JsonConvert.SerializeObject(model);
            var result = await Post(requestUrl, body, _token);
            var lst = JsonConvert.DeserializeObject<string>(result);
            return lst;
        }

        public async Task<IEnumerable<StatiDto>> GetStati()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetStati}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<StatiDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<Tipi_AttoDto>> GetTipiMOZ()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetTipiMOZ}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<Tipi_AttoDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task<IEnumerable<AssessoreInCaricaDto>> GetSoggettiInterrogabili()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetSoggettiInterrogabili}";
            var lst = JsonConvert.DeserializeObject<IEnumerable<AssessoreInCaricaDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task ModificaMetaDati(AttoDASIDto model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.AggiornaMetaDati}";
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
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.PresentazioneCartacea}";
            var body = JsonConvert.SerializeObject(model);

            await Post(requestUrl, body, _token);
        }

        public async Task<FileResponse> Download(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.StampaImmediata.Replace("{id}", id.ToString())}";
            var lst = await GetFile(requestUrl, _token);
            return lst;
        }

        public async Task<FileResponse> DownloadWithPrivacy(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.StampaImmediataPrivacy.Replace("{id}", id.ToString())}";
            var lst = await GetFile(requestUrl, _token);
            return lst;
        }

        public async Task InviaAlProtocollo(Guid id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.InviaAlProtocollo.Replace("{id}", id.ToString())}";
            await Get(requestUrl, _token);
        }

        public async Task DeclassaMozione(List<string> data)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.DeclassaMozione}";
            var body = JsonConvert.SerializeObject(data);
            await Post(requestUrl, body, _token);
        }

        public async Task<List<AttoDASIDto>> GetCartacei()
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetAllCartacei}";
            var lst = JsonConvert.DeserializeObject<List<AttoDASIDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task SalvaCartaceo(AttoDASIDto request)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.SaveCartaceo}";
            if (request.DocAllegatoGenerico != null)
            {
                using var memoryStream = new MemoryStream();
                await request.DocAllegatoGenerico.InputStream.CopyToAsync(memoryStream);
                request.DocAllegatoGenerico_Stream = memoryStream.ToArray();
            }
            var body = JsonConvert.SerializeObject(request);
            await Post(requestUrl, body, _token);
        }

        public async Task CambiaPrioritaFirma(AttiFirmeDto firma)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.CambiaPrioritaFirma}";
            var body = JsonConvert.SerializeObject(firma);
            await Post(requestUrl, body, _token);
        }

        public async Task CambiaOrdineVisualizzazioneFirme(List<AttiFirmeDto> firme)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.CambiaOrdineVisualizzazioneFirme}";
            var body = JsonConvert.SerializeObject(firme);
            await Post(requestUrl, body, _token);
        }

        public async Task<Dictionary<Guid, string>> RitiraFirma(ComandiAzioneModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.RitiroFirma}";
            var body = JsonConvert.SerializeObject(model);
            var result = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));
            return result;
        }
    }
}