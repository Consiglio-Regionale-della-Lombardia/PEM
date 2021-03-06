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
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ExpressionBuilder.Generics;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;

namespace PortaleRegione.BAL
{
    public class SeduteLogic : BaseLogic
    {
        private readonly IUnitOfWork _unitOfWork;

        public SeduteLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<SeduteDto>> GetSedute(BaseRequest<SeduteDto> model, Uri url)
        {
            try
            {
                Log.Debug($"Logic - GetSedute - page[{model.page}], pageSize[{model.size}]");
                var queryFilter = new Filter<SEDUTE>();
                queryFilter.ImportStatements(model.filtro);

                var legislatura_attiva = await _unitOfWork.Legislature.Legislatura_Attiva();
                var listaSedute = await _unitOfWork.Sedute
                    .GetAll(legislatura_attiva, model.page, model.size, queryFilter);
                var countSedute = await _unitOfWork.Sedute.Count(legislatura_attiva, queryFilter);

                return new BaseResponse<SeduteDto>(
                    model.page,
                    model.size,
                    listaSedute.Select(Mapper.Map<SEDUTE, SeduteDto>),
                    model.filtro,
                    countSedute,
                    url);
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetSedute", e);
                throw e;
            }
        }

        public async Task<SEDUTE> GetSeduta(Guid id)
        {
            try
            {
                var result = await _unitOfWork.Sedute.Get(id);
                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetSeduta", e);
                throw e;
            }
        }

        public async Task DeleteSeduta(SeduteDto sedutaDto, PersonaDto persona)
        {
            try
            {
                var sedutaInDb = await _unitOfWork.Sedute.Get(sedutaDto.UIDSeduta);
                sedutaInDb.Eliminato = true;
                sedutaInDb.UIDPersonaModifica = persona.UID_persona;
                sedutaInDb.DataModifica = DateTime.Now;
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - DeleteSeduta", e);
                throw e;
            }
        }

        public async Task<SEDUTE> NuovaSeduta(SEDUTE seduta, PersonaDto persona)
        {
            try
            {
                seduta.UIDSeduta = Guid.NewGuid();
                seduta.Eliminato = false;
                seduta.UIDPersonaCreazione = persona.UID_persona;
                seduta.DataCreazione = DateTime.Now;
                seduta.id_legislatura = await _unitOfWork.Legislature.Legislatura_Attiva();
                _unitOfWork.Sedute.Add(seduta);
                await _unitOfWork.CompleteAsync();
                return await GetSeduta(seduta.UIDSeduta);
            }
            catch (Exception e)
            {
                Log.Error("Logic - NuovaSeduta", e);
                throw e;
            }
        }

        public async Task ModificaSeduta(SeduteFormUpdateDto sedutaDto, PersonaDto persona)
        {
            try
            {
                var sedutaInDb = await _unitOfWork.Sedute.Get(sedutaDto.UIDSeduta);
                sedutaInDb.UIDPersonaModifica = persona.UID_persona;
                sedutaInDb.DataModifica = DateTime.Now;
                Mapper.Map(sedutaDto, sedutaInDb);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - ModificaSeduta", e);
                throw e;
            }
        }

        public async Task<IEnumerable<LegislaturaDto>> GetLegislature()
        {
            return (await _unitOfWork.Legislature.GetLegislature()).Select(Mapper.Map<legislature, LegislaturaDto>);
        }
    }
}