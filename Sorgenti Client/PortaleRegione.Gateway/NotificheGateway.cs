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
using PortaleRegione.DTO.Routes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace PortaleRegione.Gateway
{
    public class NotificheGateway : BaseGateway, INotificheGateway
    {
        private readonly string _token;

        protected internal NotificheGateway(string token)
        {
            _token = token;
        }

        public async Task<Dictionary<string, string>> GetListaDestinatari(TipoDestinatarioNotificaEnum tipo)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.DASI.GetAllDestinatari.Replace("{tipo}", tipo.ToString())}";
            var lst = JsonConvert.DeserializeObject<Dictionary<string, string>>(await Get(requestUrl, _token));

            return lst;
        }

        public async Task<RiepilogoNotificheModel> GetNotificheInviate(int page, int size,
            bool archivio = false)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Notifiche.GetInviate}";

            var model = new BaseRequest<NotificaDto>
            {
                param = new Dictionary<string, object>(),
                page = page,
                size = size
            };
            model.param.Add(new KeyValuePair<string, object>("Archivio", archivio));
            var body = JsonConvert.SerializeObject(model);

            var lst = JsonConvert.DeserializeObject<RiepilogoNotificheModel>(await Post(requestUrl, body,
                _token));

            return lst;
        }

        public async Task<RiepilogoNotificheModel> GetNotificheRicevute(int page, int size, bool archivio, bool soloNonViste = false)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Notifiche.GetRicevute}";

            var model = new BaseRequest<NotificaDto>
            {
                param = new Dictionary<string, object>(),
                page = page,
                size = size
            };
            model.param.Add(new KeyValuePair<string, object>("Archivio", archivio));
            model.param.Add(new KeyValuePair<string, object>("Solo_Non_Viste", soloNonViste));
            var body = JsonConvert.SerializeObject(model);

            var lst = JsonConvert.DeserializeObject<RiepilogoNotificheModel>(await Post(requestUrl, body,
                _token));

            return lst;
        }

        public async Task<IEnumerable<DestinatariNotificaDto>> GetDestinatariNotifica(string id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Notifiche.GetDestinatari.Replace("{id}", id)}";

            var lst = JsonConvert.DeserializeObject<IEnumerable<DestinatariNotificaDto>>(await Get(requestUrl,
                _token));

            return lst;
        }

        public async Task<Dictionary<Guid, string>> NotificaEM(ComandiAzioneModel model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Notifiche.InvitoAFirmare}";
            var body = JsonConvert.SerializeObject(model);
            var result =
                JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

            return result;
        }

        public async Task NotificaVista(string notificaId)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Notifiche.NotificaVista.Replace("{id}", notificaId)}";

            await Get(requestUrl, _token);
        }

        public async Task<Dictionary<Guid, string>> NotificaDASI(ComandiAzioneModel model)
        {
            model.IsDASI = true;

            var requestUrl = $"{apiUrl}/{ApiRoutes.Notifiche.InvitoAFirmare}";
            var body = JsonConvert.SerializeObject(model);
            var result =
                JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body, _token));

            return result;
        }

        public async Task AccettaPropostaFirma(string id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Notifiche.AccettaPropostaFirma.Replace("{id}", id)}";
            await Get(requestUrl, _token);
        }

        public async Task AccettaRitiroFirma(string id)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Notifiche.AccettaRitiroFirma.Replace("{id}", id)}";
            await Get(requestUrl, _token);
        }

        public async Task ArchiviaNotifiche(List<string> notifiche)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Notifiche.Archivia}";
            var body = JsonConvert.SerializeObject(notifiche);
            await Post(requestUrl, body, _token);
        }

        public async Task<Dictionary<string, string>> GetListaDestinatari(Guid atto,
            TipoDestinatarioNotificaEnum tipo)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.PEM.GetAllDestinatari.Replace("{atto}", atto.ToString()).Replace("{tipo}", tipo.ToString())}";
            var lst = JsonConvert.DeserializeObject<Dictionary<string, string>>(await Get(requestUrl, _token));

            return lst;
        }
    }
}