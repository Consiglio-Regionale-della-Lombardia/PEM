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
using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;

namespace PortaleRegione.BAL
{
    public class AttiFirmeLogic : BaseLogic
    {
        public AttiFirmeLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AttiFirmeDto>> GetFirme(AttoDASIDto atto, FirmeTipoEnum tipo)
        {
            var attoInDb = Mapper.Map<AttoDASIDto, ATTI_DASI>(atto);
            return await GetFirme(attoInDb, tipo);
        }

        public async Task<List<AttiFirmeDto>> GetFirme(ATTI_DASI atto, FirmeTipoEnum tipo)
        {
            try
            {
                var firmeInDb = await _unitOfWork
                    .Atti_Firme
                    .GetFirmatari(atto, tipo);

                var firme = firmeInDb.ToList();

                if (!firme.Any()) return new List<AttiFirmeDto>();

                var result = new List<AttiFirmeDto>();
                var ordineDefault = 0;

                foreach (var firma in firme)
                {
                    var dto = new AttiFirmeDto
                    {
                        UIDAtto = firma.UIDAtto,
                        UID_persona = firma.UID_persona,
                        FirmaCert = BALHelper.Decrypt(firma.FirmaCert),
                        PrimoFirmatario = firma.PrimoFirmatario,
                        id_gruppo = firma.id_gruppo,
                        ufficio = firma.ufficio,
                        Data_ritirofirma = string.IsNullOrEmpty(firma.Data_ritirofirma)
                            ? null
                            : BALHelper.Decrypt(firma.Data_ritirofirma),
                        Timestamp = firma.Timestamp,
                        Capogruppo = firma.Capogruppo,
                        id_AreaPolitica = firma.id_AreaPolitica,
                        Data_firma = firma.Timestamp.ToString("dd/MM/yyyy"),
                        Prioritario = firma.Prioritario,
                        OrdineVisualizzazione = firma.OrdineVisualizzazione
                    };

                    if (firma.OrdineVisualizzazione == 0 && firma.UID_persona != atto.UIDPersonaProponente)
                    {
                        dto.OrdineVisualizzazione = ordineDefault;
                    }

                    result.Add(dto);
                    ordineDefault++;
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"Logic - GetFirme - DASI - {atto.Etichetta}", e);
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
                Log.Error("Logic - GetFirme - DASI", e);
                throw e;
            }
        }

        public async Task<IEnumerable<AttiFirmeDto>> GetFirme(Guid attoUId)
        {
            try
            {
                var atto = await _unitOfWork.DASI.Get(attoUId);
                var firmeInDb = await _unitOfWork
                    .Atti_Firme
                    .GetFirmatari(attoUId);

                var firme = firmeInDb.ToList();

                if (!firme.Any()) return new List<AttiFirmeDto>();

                var result = new List<AttiFirmeDto>();
                var ordineDefault = 0;
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
                            : BALHelper.Decrypt(firma.Data_ritirofirma),
                        Timestamp = firma.Timestamp,
                        OrdineVisualizzazione = firma.OrdineVisualizzazione
                    };

                    if (firma.OrdineVisualizzazione == 0 && firma.UID_persona != atto.UIDPersonaProponente)
                    {
                        firmaDto.OrdineVisualizzazione = ordineDefault;
                    }

                    result.Add(firmaDto);
                    ordineDefault++;
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetFirme - DASI", e);
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
                Log.Error("Logic - CountFirme - DASI", e);
                throw e;
            }
        }
    }
}