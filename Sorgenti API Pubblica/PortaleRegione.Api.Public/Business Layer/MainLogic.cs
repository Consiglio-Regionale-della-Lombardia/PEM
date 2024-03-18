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
using System.Runtime.Caching;
using System.Threading.Tasks;
using PortaleRegione.Common;
using PortaleRegione.Contracts.Public;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;

namespace PortaleRegione.Api.Public.Business_Layer
{
    public class MainLogic
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MemoryCache memoryCache = MemoryCache.Default;

        private List<Tipi_AttoDto> TipiAtto
        {
            get
            {
                if (memoryCache.Contains(Constants.TIPI_ATTO))
                    return memoryCache.Get(Constants.TIPI_ATTO) as List<Tipi_AttoDto>;

                return new List<Tipi_AttoDto>();
            }
            set => memoryCache.Add(Constants.TIPI_ATTO, value, DateTimeOffset.UtcNow.AddHours(8));
        }

        public MainLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<KeyValueDto> GetTipi()
        {
            var result = new List<KeyValueDto>();
            var tipi = Enum.GetValues(typeof(TipoAttoEnum));
            foreach (var tipo in tipi)
            {
                if (Utility.tipiNonVisibili.Contains((TipoAttoEnum)tipo))
                {
                    continue;
                }

                result.Add(new KeyValueDto
                {
                    id = (int)tipo,
                    descr = Utility.GetText_Tipo((int)tipo)
                });
            }

            return result;
        }

        public async Task<List<KeyValueDto>> GetLegislature()
        {
            var legislature = await _unitOfWork.Legislature.GetLegislature();

            var result = new List<KeyValueDto>();
            foreach (var legislatura in legislature)
            {
                result.Add(new KeyValueDto
                {
                    id = legislatura.id_legislatura,
                    descr = legislatura.num_legislatura
                });
            }

            return result;
        }

        public List<KeyValueDto> GetTipiRisposta()
        {
            var result = new List<KeyValueDto>
            {
                new KeyValueDto
                {
                    id = (int)TipoRispostaEnum.SCRITTA,
                    descr = "Scritta"
                },
                new KeyValueDto
                {
                    id = (int)TipoRispostaEnum.ORALE,
                    descr = "Orale"
                },
                new KeyValueDto
                {
                    id = (int)TipoRispostaEnum.COMMISSIONE,
                    descr = "In commissione"
                }
            };

            return result;
        }

        public List<KeyValueDto> GetStati()
        {
            var result = new List<KeyValueDto>();
            var stati = Enum.GetValues(typeof(StatiAttoEnum));
            foreach (var stato in stati)
            {
                if (Utility.statiNonVisibili_Segreteria.Contains((int)stato))
                {
                    continue;
                }

                result.Add(new KeyValueDto
                {
                    id = (int)stato,
                    descr = Utility.GetText_StatoDASI((int)stato)
                });
            }

            return result;
        }

        public async Task<List<KeyValueDto>> GetGruppiByLegislatura(int idLegislatura)
        {
            var gruppi = await _unitOfWork.Persone.GetGruppiByLegislatura(idLegislatura);
            return gruppi;
        }
        public async Task<List<KeyValueDto>> GetCaricheByLegislatura(int idLegislatura)
        {
            var cariche = await _unitOfWork.Persone.GetCariche(idLegislatura);
            return cariche;
        }
        
        public async Task<List<KeyValueDto>> GetCommissioniByLegislatura(int idLegislatura)
        {
            var commissioni = await _unitOfWork.Persone.GetCommissioni(idLegislatura);
            return commissioni;
        }
    }
}