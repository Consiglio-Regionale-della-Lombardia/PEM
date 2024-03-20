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
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using PortaleRegione.Common;
using PortaleRegione.Contracts.Public;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request.Public;
using PortaleRegione.DTO.Response;

namespace PortaleRegione.Api.Public.Business_Layer
{
    /// <summary>
    ///     Logica per gestire l'elaborazione delle richieste
    /// </summary>
    public class MainLogic
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        ///     Costruttore
        /// </summary>
        /// <param name="unitOfWork"></param>
        public MainLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        ///     Ritorna i tipi di atto disponibili per il modulo DASI
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        ///     Ritorna la lista delle legislature disponibili
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        ///     Ritorna la lista di risposte disponibili
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        ///     Ritorna la lista di stati disponibili
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        ///     Ritorna la lista dei gruppi per legislatura
        /// </summary>
        /// <param name="idLegislatura"></param>
        /// <returns></returns>
        public async Task<List<KeyValueDto>> GetGruppiByLegislatura(int idLegislatura)
        {
            var gruppi = await _unitOfWork.Persone.GetGruppiByLegislatura(idLegislatura);
            return gruppi;
        }

        /// <summary>
        ///     Ritorna la lista delle cariche per legislatura
        /// </summary>
        /// <param name="idLegislatura"></param>
        /// <returns></returns>
        public async Task<List<KeyValueDto>> GetCaricheByLegislatura(int idLegislatura)
        {
            var cariche = await _unitOfWork.Persone.GetCariche(idLegislatura);
            return cariche;
        }

        /// <summary>
        ///     Ritorna la lista di commissioni per legislatura
        /// </summary>
        /// <param name="idLegislatura"></param>
        /// <returns></returns>
        public async Task<List<KeyValueDto>> GetCommissioniByLegislatura(int idLegislatura)
        {
            var commissioni = await _unitOfWork.Persone.GetCommissioni(idLegislatura);
            return commissioni;
        }

        /// <summary>
        ///     Ritorna la lista di firmatari per legislatura
        /// </summary>
        /// <param name="idLegislatura"></param>
        /// <returns></returns>
        public async Task<List<PersonaPublicDto>> GetFirmatariByLegislatura(int idLegislatura)
        {
            var firmatari = await _unitOfWork.Persone.GetFirmatariByLegislatura(idLegislatura);
            return firmatari;
        }

        /// <summary>
        ///     Ritorna la lista di atti in base a dei parametri di ricerca
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BaseResponse<AttoDASILightDto>> Cerca(CercaRequest request)
        {
            var filtroFromRequest = GetFiltroCercaFromRequest(request);
            var filtroBase = new Filter<ATTI_DASI>();
            filtroBase.ImportStatements(filtroFromRequest);
            var res = await _unitOfWork.DASI.GetAll(
                request.page,
                request.size,
                filtroBase);
            var tot = await _unitOfWork.DASI.Count(filtroBase);
            return new BaseResponse<AttoDASILightDto>(
                request.page,
                request.size,
                res.Select(a => new AttoDASILightDto
                {
                    uidAtto = a.UIDAtto,
                    oggetto = a.Oggetto,
                    display = GetDisplayFromEtichetta(a.Etichetta)
                }).ToList(),
                filtroFromRequest,
                tot);
        }

        private List<FilterStatement<AttoDASILightDto>> GetFiltroCercaFromRequest(CercaRequest request)
        {
            var res = new List<FilterStatement<AttoDASILightDto>>();
            if (request.id_tipo.HasValue && request.id_tipo > 0)
            {
                res.Add(new FilterStatement<AttoDASILightDto>
                {
                    PropertyId = nameof(ATTI_DASI.Tipo),
                    Value = request.id_tipo.Value,
                    Operation = Operation.EqualTo,
                    Connector = FilterStatementConnector.And
                });
            }

            if (request.id_legislatura.HasValue && request.id_legislatura > 0)
            {
                res.Add(new FilterStatement<AttoDASILightDto>
                {
                    PropertyId = nameof(ATTI_DASI.Legislatura),
                    Value = request.id_legislatura.Value,
                    Operation = Operation.EqualTo,
                    Connector = FilterStatementConnector.And
                });
            }

            if (request.id_stato.HasValue && request.id_stato > 0)
            {
                res.Add(new FilterStatement<AttoDASILightDto>
                {
                    PropertyId = nameof(ATTI_DASI.IDStato),
                    Value = request.id_stato.Value,
                    Operation = Operation.EqualTo,
                    Connector = FilterStatementConnector.And
                });
            }
            
            if (request.id_tipo_risposta.HasValue && request.id_tipo_risposta > 0)
            {
                res.Add(new FilterStatement<AttoDASILightDto>
                {
                    PropertyId = nameof(ATTI_DASI.IDTipo_Risposta),
                    Value = request.id_tipo_risposta.Value,
                    Operation = Operation.EqualTo,
                    Connector = FilterStatementConnector.And
                });
            }
            
            if (request.id_gruppo.HasValue && request.id_gruppo > 0)
            {
                res.Add(new FilterStatement<AttoDASILightDto>
                {
                    PropertyId = nameof(ATTI_DASI.id_gruppo),
                    Value = request.id_gruppo.Value,
                    Operation = Operation.EqualTo,
                    Connector = FilterStatementConnector.And
                });
            }
            
            if (!string.IsNullOrEmpty(request.n_atto))
            {
                res.Add(new FilterStatement<AttoDASILightDto>
                {
                    PropertyId = nameof(ATTI_DASI.NAtto_search),
                    Value = int.Parse(request.n_atto),
                    Operation = Operation.EqualTo,
                    Connector = FilterStatementConnector.And
                });
            }
            
            if (request.data_presentazione_da.HasValue)
            {
                res.Add(new FilterStatement<AttoDASILightDto>
                {
                    PropertyId = nameof(ATTI_DASI.Timestamp),
                    Value = request.data_presentazione_da.Value,
                    Operation = Operation.GreaterThanOrEqualTo,
                    Connector = FilterStatementConnector.And
                });
            }
            
            if (request.data_presentazione_a.HasValue)
            {
                res.Add(new FilterStatement<AttoDASILightDto>
                {
                    PropertyId = nameof(ATTI_DASI.Timestamp),
                    Value = request.data_presentazione_a.Value,
                    Operation = Operation.LessThanOrEqualTo,
                    Connector = FilterStatementConnector.And
                });
            }

            return res;
        }

        private string GetDisplayFromEtichetta(string etichetta)
        {
            var split = etichetta.Split('_');
            return $"{split[0]} {split[1]}";
        }
    }
}