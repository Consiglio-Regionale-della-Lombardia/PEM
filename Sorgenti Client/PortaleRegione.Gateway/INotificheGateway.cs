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

using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortaleRegione.Gateway
{
    public interface INotificheGateway
    {
        Task<IEnumerable<DestinatariNotificaDto>> GetDestinatariNotifica(string id);
        Task<Dictionary<string, string>> GetListaDestinatari(Guid atto, TipoDestinatarioNotificaEnum tipo);
        Task<Dictionary<string, string>> GetListaDestinatari(TipoDestinatarioNotificaEnum tipo);
        Task<RiepilogoNotificheModel> GetNotificheInviate(int page, int size, bool Archivio = false);
        Task<RiepilogoNotificheModel> GetNotificheRicevute(int page, int size, bool Archivio, bool Solo_Non_Viste = false);
        Task<int> GetCounterNotificheRicevute();
        Task<Dictionary<Guid, string>> NotificaEM(ComandiAzioneModel model);
        Task NotificaVista(string notificaId);
        Task<Dictionary<Guid, string>> NotificaDASI(ComandiAzioneModel model);
        Task AccettaPropostaFirma(string id);
        Task AccettaRitiroFirma(string id);
        Task ArchiviaNotifiche(List<string> notifiche);
    }
}