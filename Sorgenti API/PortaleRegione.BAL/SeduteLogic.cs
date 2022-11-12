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
using ExpressionBuilder.Generics;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<SEDUTE> GetSeduta(DateTime dataSeduta)
        {
            try
            {
                var result = await _unitOfWork.Sedute.Get(dataSeduta);
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
                Mapper.Map(sedutaDto, sedutaInDb);
                if (sedutaDto.Data_apertura == null)
                {
                    sedutaInDb.Data_apertura = null;
                }
                if (sedutaDto.Data_effettiva_inizio == null)
                {
                    sedutaInDb.Data_effettiva_inizio = null;
                }

                if (sedutaDto.Data_effettiva_fine == null)
                {
                    sedutaInDb.Data_effettiva_fine = null;
                }

                sedutaInDb.UIDPersonaModifica = persona.UID_persona;
                sedutaInDb.DataModifica = DateTime.Now;

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Error("Logic - ModificaSeduta", e);
                throw e;
            }
        }

        public async Task<BaseResponse<SeduteDto>> GetSeduteAttive(PersonaDto persona)
        {
            try
            {
                var sedute_attive = await _unitOfWork.Sedute.GetAttive(!persona.IsSegreteriaAssemblea, false);

                return new BaseResponse<SeduteDto>(
                    1,
                    10,
                    sedute_attive
                        .Select(Mapper.Map<SEDUTE, SeduteDto>),
                    null,
                    sedute_attive.Count());
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetSeduteAttive", e);
                throw e;
            }
        }

        public async Task<BaseResponse<SeduteDto>> GetSeduteAttiveMOZU()
        {
            try
            {
                var sedute_attive = await _unitOfWork.Sedute.GetAttive(false, false);

                return new BaseResponse<SeduteDto>(
                    1,
                    10,
                    sedute_attive
                        .Select(Mapper.Map<SEDUTE, SeduteDto>),
                    null,
                    sedute_attive.Count());
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetSeduteAttiveMOZU", e);
                throw e;
            }
        }

        public async Task<BaseResponse<SeduteDto>> GetSeduteAttiveDashboard()
        {
            try
            {
                var sedute_attive = await _unitOfWork.Sedute.GetAttiveDashboard();

                return new BaseResponse<SeduteDto>(
                    1,
                    10,
                    sedute_attive
                        .Select(Mapper.Map<SEDUTE, SeduteDto>),
                    null,
                    sedute_attive.Count());
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetSeduteAttiveDashboard", e);
                throw e;
            }

        }
    }
}