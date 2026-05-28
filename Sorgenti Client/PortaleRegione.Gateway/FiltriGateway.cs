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

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Routes;

namespace PortaleRegione.Gateway
{
    /// <summary>
    ///     Gateway HTTP per gli endpoint /api/filtri/*. Il modulo (PEM o DASI)
    ///     viene veicolato dal DTO per Salva e come parametro di route per Get/Elimina.
    /// </summary>
    public class FiltriGateway : BaseGateway, IFiltriGateway
    {
        private readonly string _token;

        public FiltriGateway(string token)
        {
            _token = token;
        }

        public async Task Salva(FiltroPreferitoDto model)
        {
            var requestUrl = $"{apiUrl}/{ApiRoutes.Filtri.Salva}";
            var body = JsonConvert.SerializeObject(model);
            await Post(requestUrl, body, _token);
        }

        public async Task<List<FiltroPreferitoDto>> Get(ModuloEnum modulo)
        {
            var route = ApiRoutes.Filtri.Get.Replace("{modulo}", ((int)modulo).ToString());
            var requestUrl = $"{apiUrl}/{route}";
            var lst = JsonConvert.DeserializeObject<List<FiltroPreferitoDto>>(await Get(requestUrl, _token));
            return lst;
        }

        public async Task Elimina(string nomeFiltro, ModuloEnum modulo)
        {
            var route = ApiRoutes.Filtri.Elimina
                .Replace("{modulo}", ((int)modulo).ToString())
                .Replace("{nome}", System.Uri.EscapeDataString(nomeFiltro ?? string.Empty));
            var requestUrl = $"{apiUrl}/{route}";
            await Delete(requestUrl, _token);
        }
    }
}
