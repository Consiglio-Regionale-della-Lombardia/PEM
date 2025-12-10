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
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PortaleRegione.Crypto;
using PortaleRegione.DTO.Domain.Essentials;

namespace PortaleRegione.BAL
{
    public class PersoneLogic
    {
        private readonly IUnitOfWork _unitOfWork;

        public PersoneLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PinDto> GetPin(PersonaDto persona)
        {
            var pinInDb = await _unitOfWork.Persone.GetPin(persona.UID_persona);
            if (pinInDb == null)
            {
                return null;
            }

            var pin = Mapper.Map<View_PINS, PinDto>(pinInDb);
            pin.PIN_Decrypt = BALHelper.Decrypt(pin.PIN);
            return pin;
        }

        public async Task CambioPin(CambioPinModel model)
        {
            await _unitOfWork.Persone.SavePin(model.PersonaUId,
                CryptoHelper.EncryptString(model.nuovo_pin, AppSettingsConfiguration.masterKey),
                false);
            await _unitOfWork.CompleteAsync();
        }

        public async Task ResetPin(ResetPinModel model)
        {
            await _unitOfWork.Persone.SavePin(model.PersonaUId,
                CryptoHelper.EncryptString(model.nuovo_pin, AppSettingsConfiguration.masterKey),
                true);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<PersonaDto>> GetConsiglieri()
        {
            var persone = (await _unitOfWork
                    .Persone
                    .GetConsiglieri(await _unitOfWork.Legislature.Legislatura_Attiva()))
                .Select(Mapper.Map<View_UTENTI, PersonaDto>).ToList();
            var result = new List<PersonaDto>();
            foreach (var persona in persone)
            {
                persona.Gruppo = await GetGruppoAttualePersona(persona.UID_persona, persona.IsGiunta);
                result.Add(persona);
            }

            return result;
        }

        public async Task<IEnumerable<PersonaDto>> GetAssessoriRiferimento()
        {
            var result = (await _unitOfWork
                    .Persone
                    .GetAssessoriRiferimento(await _unitOfWork.Legislature.Legislatura_Attiva()))
                .Select(Mapper.Map<View_UTENTI, PersonaDto>);
            return result;
        }

        public async Task<IEnumerable<PersonaDto>> GetProponenti()
        {
            var legislatura_corrente = await _unitOfWork.Legislature.Legislatura_Attiva();
            var consiglieri =
                await _unitOfWork.Persone.GetConsiglieri(legislatura_corrente);
            var assessori = await _unitOfWork.Persone.GetAssessoriRiferimento(legislatura_corrente);
            var persone = new List<PersonaDto>();
            persone.AddRange(consiglieri.Select(Mapper.Map<View_UTENTI, PersonaDto>));
            persone.AddRange(assessori.Select(Mapper.Map<View_UTENTI, PersonaDto>));

            return persone;
        }

        public async Task<List<PersonaPublicDto>> GetProponentiFirmatari(string legislaturaId)
        {
            var consiglieri =
                await _unitOfWork.Persone.GetProponentiFirmatari(legislaturaId);
            var persone = new List<PersonaPublicDto>();
            foreach (var c in consiglieri)
            {
                try
                {
                    if (c==null)
                        continue;

                    var personaDto = new PersonaPublicDto
                    {
                        DisplayName = c.DisplayName,
                        id = c.id_persona,
                        uid = c.UID_persona
                    };
                    persone.Add(personaDto);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return persone;
        }

        public async Task<IEnumerable<PersonaDto>> GetRelatori(Guid? id)
        {
            var personaDtos = (await _unitOfWork.Persone
                    .GetRelatori(id == null || id == Guid.Empty ? Guid.Empty : id))
                .Select(Mapper.Map<View_UTENTI, PersonaDto>);

            return personaDtos;
        }

        public async Task<string> GetCaricaPersona(Guid personaUId)
        {
            return await _unitOfWork.Persone.GetCarica(personaUId);
        }

        public async Task<PersonaDto> GetPersona(Guid proponenteUId, bool isGiunta)
        {
            var persona = Mapper.Map<View_UTENTI, PersonaDto>(await _unitOfWork.Persone.Get(proponenteUId));
            persona.Gruppo = await GetGruppoAttualePersona(persona.UID_persona, isGiunta);
            return persona;
        }

        public async Task<PersonaDto> GetPersona(SessionManager session)
        {
            if (session._currentUId == Guid.Empty)
            {
                return null;
            }

            var persona = Mapper.Map<View_UTENTI, PersonaDto>(await _unitOfWork.Persone.Get(session._currentUId));
            persona.CurrentRole = session._currentRole;
            persona.Gruppo = await GetGruppo(session._currentGroup);
            if (persona.Gruppo != null)
            {
                var capogruppo = await _unitOfWork.Gruppi.GetCapoGruppo(persona.Gruppo.id_gruppo);
                if (capogruppo != null)
                    if (capogruppo.id_persona == persona.id_persona)
                    {
                        persona.IsCapoGruppo = true;
                    }
            }

            return persona;
        }

        public async Task<PersonaDto> GetPersona(int personaId)
        {
            var persona = Mapper.Map<View_UTENTI, PersonaDto>(await _unitOfWork.Persone.Get(personaId));
            persona.Gruppo = await GetGruppoAttualePersona(new List<string>() { persona.GruppiAD });
            return persona;
        }

        public async Task<PersonaDto> GetPersona(Guid personaUId)
        {
            var persona = Mapper.Map<View_UTENTI, PersonaDto>(await _unitOfWork.Persone.Get(personaUId));
            persona.Gruppo = await GetGruppoAttualePersona(new List<string>() { persona.GruppiAD });
            return persona;
        }

        public async Task<PersonaDto> GetPersona(string userAD)
        {
            var persona = Mapper.Map<View_UTENTI, PersonaDto>(await _unitOfWork.Persone.Get(userAD));
            return persona;
        }

        public async Task<RUOLI> GetRuolo(RuoliIntEnum ruolo)
        {
            return await _unitOfWork.Ruoli.Get((int)ruolo);
        }

        public async Task<IEnumerable<RUOLI>> GetRuoli(List<string> ruoli)
        {
            return (await _unitOfWork.Ruoli.RuoliUtente(ruoli)).ToList();
        }

        public async Task<IEnumerable<KeyValueDto>> GetGruppiAttivi()
        {
            var gruppiDtos = await _unitOfWork
                .Gruppi
                .GetAllAttivi(await _unitOfWork
                    .Legislature
                    .Legislatura_Attiva());

            return gruppiDtos;
        }

        public async Task<IEnumerable<KeyValueDto>> GetGruppiInDb()
        {
            var gruppiDtos = await _unitOfWork
                .Gruppi
                .GetAll();

            return gruppiDtos;
        }

        public async Task<GruppiDto> GetGruppo(int id)
        {
            if (id >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID)
            {
                return new GruppiDto
                {
                    id_gruppo = id,
                    abilita_em_privati = false,
                    giunta = true,
                    codice_gruppo = "GIUNTA",
                    nome_gruppo = "Giunta Regionale"
                };
            }

            return Mapper.Map<View_gruppi_politici_con_giunta, GruppiDto>(await _unitOfWork.Gruppi.Get(id));
        }

        public async Task<GruppiDto> GetGruppoAttualePersona(List<string> gruppi)
        {
            return Mapper.Map<View_gruppi_politici_con_giunta, GruppiDto>(
                await _unitOfWork.Gruppi.GetGruppoAttuale(gruppi));
        }

        public async Task<GruppiDto> GetGruppoAttualePersona(Guid personaUId, bool isGiunta)
        {
            return Mapper.Map<View_gruppi_politici_con_giunta, GruppiDto>(
                await _unitOfWork.Gruppi.GetGruppoAttuale(personaUId, isGiunta));
        }

        public async Task<IEnumerable<PersonaDto>> GetConsiglieriGruppo(int gruppoId)
        {
            var result = (await _unitOfWork
                    .Gruppi
                    .GetConsiglieriGruppo(await _unitOfWork.Legislature.Legislatura_Attiva(),
                        gruppoId))
                .Select(Mapper.Map<View_UTENTI, PersonaDto>);
            return result;
        }

        public async Task<PersonaDto> GetCapoGruppo(int gruppoId)
        {
            var persona = Mapper.Map<View_UTENTI, PersonaDto>(await _unitOfWork.Gruppi.GetCapoGruppo(gruppoId));
            return persona;
        }

        public async Task<IEnumerable<PersonaDto>> GetPersone()
        {
            return (await _unitOfWork
                    .Persone
                    .GetAll())
                .Select(Mapper.Map<View_UTENTI, PersonaDto>);
        }

        public async Task<IEnumerable<PersonaDto>> GetSegreteriaPolitica(int id, bool notifica_firma,
            bool notifica_deposito)
        {
            return (await _unitOfWork.Gruppi.GetSegreteriaPolitica(id, notifica_firma, notifica_deposito))
                .Select(Mapper.Map<UTENTI_NoCons, PersonaDto>);
        }

        public async Task<IEnumerable<PersonaDto>> GetGiuntaRegionale()
        {
            return (await _unitOfWork.Persone.GetGiuntaRegionale())
                .Select(Mapper.Map<View_Composizione_GiuntaRegionale, PersonaDto>);
        }

        public async Task<IEnumerable<PersonaDto>> GetSegreteriaGiuntaRegionale(bool notificaFirma,
            bool notificaDeposito)
        {
            var segreteria_giunta =
                await _unitOfWork.Persone.GetSegreteriaGiuntaRegionale(notificaFirma, notificaDeposito);
            return segreteria_giunta.Select(Mapper.Map<UTENTI_NoCons, PersonaDto>);
        }

        public async Task<IEnumerable<GruppiDto>> GetGruppi(BaseRequest<GruppiDto> model)
        {
            var result = new List<GruppiDto>();
            var queryFilter = new Filter<gruppi_politici>();
            queryFilter.ImportStatements(model.filtro);

            var legislatura_attiva = await _unitOfWork.Legislature.Legislatura_Attiva();
            var gruppi_politici = await _unitOfWork.Gruppi.GetGruppiAdmin(queryFilter);
            var join_gruppi_ad = await _unitOfWork.Gruppi.GetJoinGruppiAdmin(legislatura_attiva);
            var giunta = await _unitOfWork.Gruppi.GetGiunta(legislatura_attiva);
            foreach (var join_gruppo in join_gruppi_ad)
            {
                var gruppo = gruppi_politici.FirstOrDefault(g => g.id_gruppo == join_gruppo.id_gruppo);
                if (gruppo != null)
                {
                    var tmpG = new GruppiDto
                    {
                        id_gruppo = gruppo.id_gruppo,
                        nome_gruppo = gruppo.nome_gruppo,
                        codice_gruppo = gruppo.codice_gruppo,
                        data_inizio = gruppo.data_inizio,
                        abilita_em_privati = join_gruppo.AbilitaEMPrivati,
                        giunta = join_gruppo.GiuntaRegionale,
                        GruppoAD = join_gruppo.GruppoAD,
                        UID_Gruppo = join_gruppo.UID_Gruppo
                    };
                    result.Add(tmpG);
                }
            }

            if (giunta != null)
            {
                result.Add(new GruppiDto
                {
                    id_gruppo = giunta.id_gruppo,
                    codice_gruppo = "GR",
                    nome_gruppo = "GIUNTA REGIONALE",
                    abilita_em_privati = giunta.AbilitaEMPrivati,
                    giunta = giunta.GiuntaRegionale,
                    GruppoAD = giunta.GruppoAD,
                    UID_Gruppo = giunta.UID_Gruppo
                });
            }

            return result.OrderBy(g => g.nome_gruppo);
        }
    }
}