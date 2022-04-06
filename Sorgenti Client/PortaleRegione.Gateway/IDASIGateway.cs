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
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;

namespace PortaleRegione.Gateway
{
    public interface IDASIGateway
    {
        Task<AttoDASIDto> Salva(AttoDASIDto request);

        Task Modifica(AttoDASIDto request);
        Task<AttoDASIDto> Get(Guid id);
        Task<RiepilogoDASIModel> Get(int page, int size, StatiAttoEnum stato, TipoAttoEnum tipo);
        Task<Dictionary<Guid, string>> Deposita(ComandiAzioneModel model);
        Task<Dictionary<Guid, string>> Deposita(Guid attoUId, string pin);
        Task<Dictionary<Guid, string>> EliminaFirma(ComandiAzioneModel model);
        Task<Dictionary<Guid, string>> EliminaFirma(Guid attoUId, string pin);
        Task<Dictionary<Guid, string>> Firma(ComandiAzioneModel model);
        Task<Dictionary<Guid, string>> Firma(Guid attoUId, string pin);
        Task<RiepilogoDASIModel> GetBySeduta(Guid sedutaUidSeduta);
    }
}