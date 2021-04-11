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
using AutoMapper;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;

namespace PortaleRegione.BAL
{
    public class ReportLogic : BaseLogic
    {
        private readonly EmendamentiLogic _logicEm;
        private readonly IUnitOfWork _unitOfWork;

        public ReportLogic(IUnitOfWork unitOfWork, EmendamentiLogic logicEm)
        {
            _unitOfWork = unitOfWork;
            _logicEm = logicEm;
        }


        public async Task<ReportResponse> GetReport(ReportRequest req, Uri url)
        {
            var result = new ReportResponse();
            var lista_em = await _unitOfWork
                .Emendamenti
                .GetReport(req.id, req.type, req.page, req.size);
            var lista_em_dto = new List<EmendamentiDto>();
            foreach (var em in lista_em)
            {
                var newItem = Mapper.Map<EM, EmendamentiDto>(em);
                newItem.DisplayTitle = GetNomeEM(em,
                    em.Rif_UIDEM.HasValue
                        ? await _logicEm.GetEM(em.Rif_UIDEM.Value)
                        : null);
                newItem.PersonaProponente = Mapper.Map<View_UTENTI, PersonaLightDto>(
                    await _unitOfWork.Persone.Get(em.UIDPersonaProponente.Value));
                lista_em_dto.Add(newItem);
            }

            result.Data = new BaseResponse<EmendamentiDto>(
                req.page,
                req.size,
                lista_em_dto,
                null,
                await _unitOfWork.Emendamenti.CountReport(req.id),
                url);

            result.Approvati = await _unitOfWork.Emendamenti.CountReport(req.id, StatiEnum.Approvato);
            result.Non_Approvati = await _unitOfWork.Emendamenti.CountReport(req.id, StatiEnum.Non_Approvato);
            result.Inammissibili = await _unitOfWork.Emendamenti.CountReport(req.id, StatiEnum.Inammissibile);
            result.Decaduti = await _unitOfWork.Emendamenti.CountReport(req.id, StatiEnum.Decaduto);
            result.Ritirati = await _unitOfWork.Emendamenti.CountReport(req.id, StatiEnum.Ritirato);

            result.Atto = Mapper.Map<ATTI,AttiDto>(await _unitOfWork.Atti.Get(req.id));
            return result;
        }
    }
}