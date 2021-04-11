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
    public sealed class NotificheGate : BaseGateway
    {
        static  NotificheGate _instance;  
   
        public static NotificheGate Instance => _instance ?? (_instance = new NotificheGate());

        private NotificheGate()
        {
            
        }

        public static async Task<BaseResponse<NotificaDto>> GetNotificheInviate(int page, int size,
            bool Archivio = false)
        {
            try
            {
                var requestUrl = $"{apiUrl}/notifiche/view-inviate";

                var model = new BaseRequest<NotificaDto>
                {
                    param = new Dictionary<string, object>(),
                    page = page,
                    size = size,
                    filtro = new List<FilterStatement<NotificaDto>>
                    {
                        new FilterStatement<NotificaDto>
                        {
                            PropertyId = nameof(NotificaDto.IDTipo),
                            Operation = Operation.EqualTo,
                            Value = (int) TipoNotificaEnum.INVITO
                        }
                    }
                };
                model.param.Add(new KeyValuePair<string, object>("Archivio", Archivio));
                var body = JsonConvert.SerializeObject(model);

                var lst = JsonConvert.DeserializeObject<BaseResponse<NotificaDto>>(await Post(requestUrl, body));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetNotificheInviate", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetNotificheInviate", ex);
                throw ex;
            }
        }

        public static async Task<BaseResponse<NotificaDto>> GetNotificheRicevute(int page, int size, bool Archivio)
        {
            try
            {
                var requestUrl = $"{apiUrl}/notifiche/view-ricevute";

                var model = new BaseRequest<NotificaDto>
                {
                    param = new Dictionary<string, object>(),
                    page = page,
                    size = size,
                    filtro = new List<FilterStatement<NotificaDto>>
                    {
                        new FilterStatement<NotificaDto>
                        {
                            PropertyId = nameof(NotificaDto.IDTipo),
                            Operation = Operation.EqualTo,
                            Value = (int) TipoNotificaEnum.INVITO
                        }
                    }
                };
                model.param.Add(new KeyValuePair<string, object>("Archivio", Archivio));
                var body = JsonConvert.SerializeObject(model);

                var lst = JsonConvert.DeserializeObject<BaseResponse<NotificaDto>>(await Post(requestUrl, body));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetNotificheRicevute", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetNotificheRicevute", ex);
                throw ex;
            }
        }

        public static async Task<IEnumerable<DestinatariNotificaDto>> GetDestinatariNotifica(int id)
        {
            try
            {
                var requestUrl = $"{apiUrl}/notifiche/{id}/destinatari";

                var lst = JsonConvert.DeserializeObject<IEnumerable<DestinatariNotificaDto>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetDestinatariNotifica", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetDestinatariNotifica", ex);
                throw ex;
            }
        }

        public static async Task<Dictionary<Guid, string>> NotificaEM(ComandiAzioneModel model)
        {
            try
            {
                var requestUrl = $"{apiUrl}/notifiche/invita";
                var body = JsonConvert.SerializeObject(model);
                var result = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(await Post(requestUrl, body));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("NotificaEM", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("NotificaEM", ex);
                throw ex;
            }
        }

        public static async Task NotificaVista(long notificaId)
        {
            try
            {
                var requestUrl = $"{apiUrl}/notifiche/vista/{notificaId}";

                await Get(requestUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("NotificaVista", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("NotificaVista", ex);
                throw ex;
            }
        }

        public static async Task<Dictionary<string, string>> GetListaDestinatari(Guid atto,
            TipoDestinatarioNotificaEnum tipo)
        {
            try
            {
                var requestUrl = $"{apiUrl}/notifiche/destinatari?atto={atto}&tipo={(int) tipo}";
                var lst = JsonConvert.DeserializeObject<Dictionary<string, string>>(await Get(requestUrl));

                return lst;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("GetListaDestinatari", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("GetListaDestinatari", ex);
                throw ex;
            }
        }
    }
}