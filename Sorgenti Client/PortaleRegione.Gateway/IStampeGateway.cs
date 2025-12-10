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
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;

namespace PortaleRegione.Gateway
{
    public interface IStampeGateway
    {
        Task AddInfo(Guid id, string message);
        Task<FileResponse> DownloadStampa(Guid stampaUId);
        Task<FileResponse> DownloadStampa(string nomeFile);
        Task EliminaStampa(Guid id);
        Task<StampaDto> Get(Guid id);
        Task<BaseResponse<StampaDto>> Get(int page, int size);
        Task<IEnumerable<Stampa_InfoDto>> GetInfo();
        Task<IEnumerable<Stampa_InfoDto>> GetInfo(Guid id);
        Task<FileResponse> Stampa(string uid);
        Task<List<StampaDto>> InserisciStampa(NuovaStampaRequest request);
        Task JobErrorStampa(Guid stampaUId, string errorMessage);
        Task<BaseResponse<EmendamentiDto>> JobGetEmendamenti(string query, int page, int size = 20);
        Task<BaseResponse<AttoDASIDto>> JobGetDASI(string query, int page, int size = 20);
        Task<BaseResponse<StampaDto>> JobGetStampe(int page, int size);
        Task JobSetInvioStampa(StampaDto stampa);
        Task JobUnLockStampa(Guid stampaUId);
        Task JobUpdateFileStampa(StampaDto stampa);
        Task ResetStampa(Guid id);
    }
}