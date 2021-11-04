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
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortaleRegione.Gateway
{
    public interface IAdminGateway
    {
        Task<List<AdminGruppiModel>> GetGruppiAdmin(BaseRequest<GruppiDto> request);
        Task<IEnumerable<KeyValueDto>> GetGruppiInDb();
        Task<IEnumerable<GruppoAD_Dto>> GetGruppiPoliticiAD();
        Task<PersonaDto> GetPersona(Guid id);
        Task<BaseResponse<PersonaDto>> GetPersone(BaseRequest<PersonaDto> request);
        Task<IEnumerable<RuoliDto>> GetRuoliAD();
        Task ResetPassword(ResetRequest request);
        Task ResetPin(ResetRequest request);
        Task SalvaGruppo(SalvaGruppoRequest request);
        Task<Guid> SalvaPersona(PersonaUpdateRequest request);
        Task EliminaPersona(Guid uid_persona);
    }
}