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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortaleRegione.BAL
{
    public class AttiFirmeLogic : BaseLogic
    {
        public AttiFirmeLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AttiFirmeDto>> GetFirme(ATTI_DASI atto, FirmeTipoEnum tipo)
        {
            try
            {
                var firmeInDb = await _unitOfWork
                    .Atti_Firme
                    .GetFirmatari(atto, tipo);

                var firme = firmeInDb.ToList();

                if (!firme.Any()) return new List<AttiFirmeDto>();

                var result = new List<AttiFirmeDto>();
                foreach (var firma in firme)
                {
                    firma.FirmaCert = BALHelper.Decrypt(firma.FirmaCert);
                    firma.Data_firma = BALHelper.Decrypt(firma.Data_firma);
                    if (!string.IsNullOrEmpty(firma.Data_ritirofirma))
                        firma.Data_ritirofirma = BALHelper.Decrypt(firma.Data_ritirofirma);

                    result.Add(firma);
                }

                return result;
            }
            catch (Exception e)
            {
                //Log.Error("Logic - GetFirme - DASI", e);
                throw e;
            }
        }

        public async Task RimuoviFirme(ATTI_DASI atto)
        {
            try
            {
                var firmeInDb = await _unitOfWork
                    .Atti_Firme
                    .GetFirmatari(atto, FirmeTipoEnum.TUTTE);

                var firme = firmeInDb.ToList();

                if (!firme.Any()) return;

                foreach (var firma in firme.Where(firma => !firma.PrimoFirmatario))
                {
                    _unitOfWork.Atti_Firme.Remove(firma);
                }
            }
            catch (Exception e)
            {
                //Log.Error("Logic - GetFirme - DASI", e);
                throw e;
            }
        }

        public async Task<IEnumerable<AttiFirmeDto>> GetFirme(Guid attoUId)
        {
            try
            {
                var firmeInDb = await _unitOfWork
                    .Atti_Firme
                    .GetFirmatari(attoUId);

                var firme = firmeInDb.ToList();

                if (!firme.Any()) return new List<AttiFirmeDto>();

                var result = new List<AttiFirmeDto>();
                foreach (var firma in firme)
                {
                    var firmaDto = new AttiFirmeDto
                    {
                        UIDAtto = firma.UIDAtto,
                        UID_persona = firma.UID_persona,
                        PrimoFirmatario = firma.PrimoFirmatario,
                        id_gruppo = firma.id_gruppo,
                        FirmaCert = BALHelper.Decrypt(firma.FirmaCert),
                        Data_firma = BALHelper.Decrypt(firma.Data_firma),
                        Data_ritirofirma = string.IsNullOrEmpty(firma.Data_ritirofirma)
                            ? null
                            : BALHelper.Decrypt(firma.Data_ritirofirma)
                    };

                    result.Add(firmaDto);
                }

                return result;
            }
            catch (Exception e)
            {
                //Log.Error("Logic - GetFirme - DASI", e);
                throw e;
            }
        }

        public async Task<int> CountFirme(Guid attoUId)
        {
            try
            {
                return await _unitOfWork
                    .Atti_Firme
                    .CountFirme(attoUId);
            }
            catch (Exception e)
            {
                //Log.Error("Logic - CountFirme - DASI", e);
                throw e;
            }
        }
    }
}