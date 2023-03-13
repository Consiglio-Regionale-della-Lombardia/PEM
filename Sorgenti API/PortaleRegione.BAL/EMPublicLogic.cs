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
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortaleRegione.BAL
{
    public class EMPublicLogic : BaseLogic
    {
        public EMPublicLogic(IUnitOfWork unitOfWork, EmendamentiLogic logicEm)
        {
            _unitOfWork = unitOfWork;
            _logicEm = logicEm;
        }

        public async Task<string> GetBody(EM em, IEnumerable<FirmeDto> firme)
        {
            try
            {
                var atto = await _unitOfWork.Atti.Get(em.UIDAtto);

                var persona = Users.First(p => p.UID_persona == em.UIDPersonaProponente);
                var personaDto = Mapper.Map<PersonaLightDto, PersonaDto>(persona);
                var emendamentoDto = await _logicEm.GetEM_DTO(em.UIDEM, atto, personaDto);
                var attoDto = Mapper.Map<ATTI, AttiDto>(atto);

                try
                {
                    var body = GetTemplate(TemplateTypeEnum.PDF);
                    GetBody(emendamentoDto, attoDto, firme.ToList(), personaDto, false, ref body);
                    return body;
                }
                catch (Exception e)
                {
                    Log.Error("GetBodyEM", e);
                    throw e;
                }
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetBodyEM", e);
                throw e;
            }
        }
    }
}