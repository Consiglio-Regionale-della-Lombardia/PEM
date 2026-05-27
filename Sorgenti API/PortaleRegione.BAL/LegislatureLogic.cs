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

using AutoMapper;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortaleRegione.BAL
{
    public class LegislatureLogic : BaseLogic
    {
        public LegislatureLogic(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LegislaturaDto>> GetLegislature()
        {
            var result = await _unitOfWork.Legislature.GetLegislature();
            return (result).Select(_mapper.Map<legislature, LegislaturaDto>);
        }

        public async Task<LegislaturaDto> GetLegislatura(int id)
        {
            var legislatura = await _unitOfWork.Legislature.Get(id);
            return _mapper.Map<legislature, LegislaturaDto>(legislatura);
        }

        public async Task<int> GetLegislaturaAttuale()
        {
            return await _unitOfWork.Legislature.Legislatura_Attiva();
        }
    }
}