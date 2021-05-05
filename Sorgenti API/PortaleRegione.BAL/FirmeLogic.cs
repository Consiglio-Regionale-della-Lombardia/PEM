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
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.Logger;

namespace PortaleRegione.BAL
{
    public class FirmeLogic : BaseLogic
    {
        private readonly IUnitOfWork _unitOfWork;

        public FirmeLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<FIRME>> GetFirme(EmendamentiDto emDto, FirmeTipoEnum tipo)
        {
            var em = await _unitOfWork.Emendamenti.Get(emDto.UIDEM);
            return await GetFirme(em, tipo);
        }

        public async Task<IEnumerable<FIRME>> GetFirme(EM em, FirmeTipoEnum tipo)
        {
            try
            {
                var firmeInDb = await _unitOfWork
                    .Firme
                    .GetFirmatari(em, tipo);

                var firme = firmeInDb.ToList();

                if (!firme.Any()) return firme;
                var result = new List<FIRME>();
                foreach (var firma in firme)
                {
                    firma.FirmaCert = Decrypt(firma.FirmaCert);
                    firma.Data_firma = Decrypt(firma.Data_firma);
                    firma.Data_ritirofirma = string.IsNullOrEmpty(firma.Data_ritirofirma)
                        ? null
                        : Decrypt(firma.Data_ritirofirma);
                    result.Add(firma);
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetFirme", e);
                throw e;
            }
        }

        public async Task<int> CountFirme(Guid emendamentoUId)
        {
            try
            {
                return await _unitOfWork
                    .Firme
                    .CountFirme(emendamentoUId);
            }
            catch (Exception e)
            {
                Log.Error("Logic - CountFirme", e);
                throw e;
            }
        }
    }
}