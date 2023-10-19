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
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortaleRegione.BAL
{
    public class SeduteLogic : BaseLogic
    {
        public SeduteLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<SeduteDto>> GetSedute(BaseRequest<SeduteDto> model, Uri url)
        {
            var legislatura_attiva = await _unitOfWork.Legislature.Legislatura_Attiva();
            var queryFilter = new Filter<SEDUTE>();
            queryFilter.ImportStatements(model.filtro);

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

        public async Task<SEDUTE> GetSeduta(Guid id)
        {
            var result = await _unitOfWork.Sedute.Get(id);
            return result;
        }

        public async Task<SEDUTE> GetSeduta(DateTime dataSeduta)
        {
            var result = await _unitOfWork.Sedute.Get(dataSeduta);
            return result;
        }

        public async Task DeleteSeduta(SeduteDto sedutaDto, PersonaDto persona)
        {
            var sedutaInDb = await _unitOfWork.Sedute.Get(sedutaDto.UIDSeduta);
            sedutaInDb.Eliminato = true;
            sedutaInDb.UIDPersonaModifica = persona.UID_persona;
            sedutaInDb.DataModifica = DateTime.Now;
            await _unitOfWork.CompleteAsync();
        }

        public async Task<SEDUTE> NuovaSeduta(SEDUTE seduta, PersonaDto persona)
        {
            //#712
            if (seduta.Data_seduta <= DateTime.MinValue)
                throw new InvalidOperationException("Data seduta non valida");
            seduta.UIDSeduta = Guid.NewGuid();
            seduta.Eliminato = false;
            seduta.UIDPersonaCreazione = persona.UID_persona;
            seduta.DataCreazione = DateTime.Now;
            seduta.id_legislatura = await _unitOfWork.Legislature.Legislatura_Attiva();
            _unitOfWork.Sedute.Add(seduta);
            await _unitOfWork.CompleteAsync();
            return await GetSeduta(seduta.UIDSeduta);
        }

        public async Task ModificaSeduta(SeduteFormUpdateDto sedutaDto, PersonaDto persona)
        {
            //#712
            if (sedutaDto.Data_seduta <= DateTime.MinValue)
                throw new InvalidOperationException("Data seduta non valida");

            var sedutaInDb = await _unitOfWork.Sedute.Get(sedutaDto.UIDSeduta);
            Mapper.Map(sedutaDto, sedutaInDb);
            CleanSeduta(sedutaDto, sedutaInDb);

            sedutaInDb.UIDPersonaModifica = persona.UID_persona;
            sedutaInDb.DataModifica = DateTime.Now;

            await _unitOfWork.CompleteAsync();

            if (sedutaDto.Data_effettiva_fine.HasValue)
            {
                //Matteo Cattapan #486
                //Quando viene chiusa la seduta, vengono 'declassate' tutte le mozioni depositate e iscritte in seduta da UOLA

                var mozioni_abbinate =
                    await _unitOfWork.DASI.GetAttiBySeduta(sedutaDto.UIDSeduta, TipoAttoEnum.MOZ, TipoMOZEnum.ABBINATA);
                var mozioni_urgenti =
                    await _unitOfWork.DASI.GetAttiBySeduta(sedutaDto.UIDSeduta, TipoAttoEnum.MOZ, TipoMOZEnum.URGENTE);
                var mozioni_da_declassare = new List<ATTI_DASI>();
                mozioni_da_declassare.AddRange(mozioni_abbinate.Where(a=>a.IDStato == (int)StatiAttoEnum.PRESENTATO || a.IDStato == (int)StatiAttoEnum.IN_TRATTAZIONE));
                mozioni_da_declassare.AddRange(mozioni_urgenti.Where(a=>a.IDStato == (int)StatiAttoEnum.PRESENTATO || a.IDStato == (int)StatiAttoEnum.IN_TRATTAZIONE));

                mozioni_da_declassare.ForEach(moz => { moz.TipoMOZ = (int)TipoMOZEnum.ORDINARIA; });

                await _unitOfWork.CompleteAsync();
            }
        }

        private void CleanSeduta(SeduteFormUpdateDto sedutaDto, SEDUTE sedutaInDb)
        {
            if (sedutaDto.Data_apertura == null) sedutaInDb.Data_apertura = null;

            if (sedutaDto.Data_effettiva_inizio == null) sedutaInDb.Data_effettiva_inizio = null;

            if (sedutaDto.Data_effettiva_fine == null) sedutaInDb.Data_effettiva_fine = null;

            if (sedutaDto.Scadenza_presentazione == null) sedutaInDb.Scadenza_presentazione = null;

            if (sedutaDto.DataScadenzaPresentazioneIQT == null) sedutaInDb.DataScadenzaPresentazioneIQT = null;

            if (sedutaDto.DataScadenzaPresentazioneMOZ == null) sedutaInDb.DataScadenzaPresentazioneMOZ = null;

            if (sedutaDto.DataScadenzaPresentazioneMOZA == null) sedutaInDb.DataScadenzaPresentazioneMOZA = null;

            if (sedutaDto.DataScadenzaPresentazioneMOZU == null) sedutaInDb.DataScadenzaPresentazioneMOZU = null;

            if (sedutaDto.DataScadenzaPresentazioneODG == null) sedutaInDb.DataScadenzaPresentazioneODG = null;

            if (string.IsNullOrEmpty(sedutaDto.Note)) sedutaInDb.Note = "";
        }

        public async Task<BaseResponse<SeduteDto>> GetSeduteAttive(PersonaDto persona)
        {
            var sedute_attive = await _unitOfWork.Sedute.GetAttive(!persona.IsSegreteriaAssemblea, false);

            var seduteAttive = sedute_attive.ToList();
            return new BaseResponse<SeduteDto>(
                1,
                10,
                seduteAttive
                    .Select(Mapper.Map<SEDUTE, SeduteDto>),
                null,
                seduteAttive.Count);
        }

        public async Task<BaseResponse<SeduteDto>> GetSeduteAttiveMOZU()
        {
            var sedute_attive = await _unitOfWork.Sedute.GetAttive(false, false);

            var seduteAttive = sedute_attive.ToList();
            return new BaseResponse<SeduteDto>(
                1,
                10,
                seduteAttive
                    .Select(Mapper.Map<SEDUTE, SeduteDto>),
                null,
                seduteAttive.Count);
        }

        public async Task<BaseResponse<SeduteDto>> GetSeduteAttiveDashboard()
        {
            var sedute_attive = await _unitOfWork.Sedute.GetAttiveDashboard();

            var seduteAttive = sedute_attive.ToList();
            return new BaseResponse<SeduteDto>(
                1,
                10,
                seduteAttive
                    .Select(Mapper.Map<SEDUTE, SeduteDto>),
                null,
                seduteAttive.Count());
        }
    }
}