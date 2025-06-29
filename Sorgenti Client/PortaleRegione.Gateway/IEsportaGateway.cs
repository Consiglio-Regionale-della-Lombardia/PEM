﻿/*
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

using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortaleRegione.Gateway
{
    public interface IEsportaGateway
    {
        Task<FileResponse> EsportaWORD(Guid attoUId, OrdinamentoEnum ordine, ClientModeEnum mode);
        Task<FileResponse> EsportaXLS(EmendamentiViewModel model);
        Task<FileResponse> EsportaXLS_UOLA(EmendamentiViewModel model);
        Task<FileResponse> EsportaXLSDASI(List<Guid> lista);
        Task<FileResponse> EsportaXLSConsiglieriDASI(List<Guid> lista);
        Task<FileResponse> EsportaZipDASI(List<Guid> lista);
    }
}