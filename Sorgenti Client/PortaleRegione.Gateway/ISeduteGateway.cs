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
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using System;
using System.Threading.Tasks;

namespace PortaleRegione.Gateway
{
    public interface ISeduteGateway
    {
        Task Elimina(Guid id);
        Task<BaseResponse<SeduteDto>> Get(BaseRequest<SeduteDto> model);
        Task<SeduteFormUpdateDto> Get(Guid id);
        Task<BaseResponse<SeduteDto>> Get(int page, int size);
        Task Modifica(SeduteFormUpdateDto seduta);
        Task Salva(SeduteFormUpdateDto seduta);
        Task<BaseResponse<SeduteDto>> GetAttive();
        Task<BaseResponse<SeduteDto>> GetAttiveDashboard();
    }
}