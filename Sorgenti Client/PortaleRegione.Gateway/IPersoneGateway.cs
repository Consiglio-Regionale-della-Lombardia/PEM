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
using PortaleRegione.DTO.Autenticazione;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PortaleRegione.DTO.Domain.Essentials;

namespace PortaleRegione.Gateway
{
    public interface IPersoneGateway
    {
        Task<LoginResponse> CambioGruppo(int gruppo);
        Task<LoginResponse> CambioRuolo(RuoliIntEnum ruolo);
        Task CheckPin(CambioPinModel model);
        Task<IEnumerable<PersonaDto>> Get();
        Task<PersonaDto> Get(Guid id, bool isGiunta = false);
        Task<IEnumerable<PersonaDto>> GetAssessoriRiferimento();
        Task<PersonaDto> GetCapoGruppo(int id);
        Task<IEnumerable<PersonaDto>> GetGiuntaRegionale();
        Task<IEnumerable<KeyValueDto>> GetGruppiAttivi();
        Task<IEnumerable<PersonaDto>> GetRelatori(Guid? attoUId);
        Task<RuoliDto> GetRuolo(RuoliIntEnum ruolo);
        Task<IEnumerable<PersonaDto>> GetSegreteriaGiuntaRegionale(bool notifica_firma, bool notifica_deposito);
        Task<IEnumerable<PersonaDto>> GetSegreteriaPolitica(int id, bool firma, bool deposito);
        Task<LoginResponse> Login(LoginRequest request);
        Task SalvaPin(CambioPinModel model);
        Task<List<PersonaPublicDto>> GetProponentiFirmatari();
    }
}