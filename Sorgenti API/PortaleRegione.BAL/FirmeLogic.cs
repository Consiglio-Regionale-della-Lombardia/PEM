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

using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortaleRegione.BAL
{
    public class FirmeLogic : BaseLogic
    {
        public FirmeLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<FirmeDto>> GetFirme(EM em, FirmeTipoEnum tipo)
        {
            try
            {
                var firmeInDb = await _unitOfWork
                    .Firme
                    .GetFirmatari(em, tipo);

                if (firmeInDb == null) return new List<FirmeDto>();

                var firme = firmeInDb.ToList();

                if (!firme.Any()) return new List<FirmeDto>();

                var result = new List<FirmeDto>();
                foreach (var firma in firme)
                {
                    var firmaDto = new FirmeDto
                    {
                        UIDEM = firma.UIDEM,
                        UID_persona = firma.UID_persona,
                        FirmaCert = BALHelper.Decrypt(firma.FirmaCert),
                        Data_firma = BALHelper.Decrypt(firma.Data_firma),
                        Data_ritirofirma = string.IsNullOrEmpty(firma.Data_ritirofirma)
                            ? null
                            : BALHelper.Decrypt(firma.Data_ritirofirma),
                        Timestamp = firma.Timestamp
                    };

                    result.Add(firmaDto);
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

        public async Task<FirmeDto> GetFirmaUfficio(EmendamentiDto em)
        {
            try
            {
                var firmaInDb = await _unitOfWork
                    .Firme
                    .GetFirmaUfficio(em.UIDEM);

                var firmaDto = new FirmeDto
                {
                    UIDEM = firmaInDb.UIDEM,
                    UID_persona = firmaInDb.UID_persona,
                    FirmaCert = BALHelper.Decrypt(firmaInDb.FirmaCert),
                    Data_firma = BALHelper.Decrypt(firmaInDb.Data_firma),
                    Data_ritirofirma = string.IsNullOrEmpty(firmaInDb.Data_ritirofirma)
                        ? null
                        : BALHelper.Decrypt(firmaInDb.Data_ritirofirma)
                };

                return firmaDto;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetFirmaUfficio", e);
                throw e;
            }
        }

        public async Task<List<FirmeDto>> GetFirmatariAtto(Guid attoUid)
        {
            try
            {
                var firmeInDb = await _unitOfWork
                    .Firme
                    .GetFirmatariAtto(attoUid);

                if (firmeInDb == null) return new List<FirmeDto>();

                var firme = firmeInDb.ToList();

                if (!firme.Any()) return new List<FirmeDto>();

                var result = new List<FirmeDto>();
                foreach (var firma in firme)
                {
                    var firmaDto = new FirmeDto
                    {
                        UIDEM = firma.UIDEM,
                        UID_persona = firma.UID_persona,
                        FirmaCert = BALHelper.Decrypt(firma.FirmaCert),
                        Data_firma = BALHelper.Decrypt(firma.Data_firma),
                        Data_ritirofirma = string.IsNullOrEmpty(firma.Data_ritirofirma)
                            ? null
                            : BALHelper.Decrypt(firma.Data_ritirofirma),
                        Timestamp = firma.Timestamp
                    };

                    result.Add(firmaDto);
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetFirme", e);
                throw e;
            }
        }
    }
}