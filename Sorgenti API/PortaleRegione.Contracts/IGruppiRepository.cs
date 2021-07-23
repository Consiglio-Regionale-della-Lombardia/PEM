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

using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.Contracts
{
    /// <summary>
    ///     Interfaccia Gruppi
    /// </summary>
    public interface IGruppiRepository : IRepository<gruppi_politici>
    {
        Task<GruppiDto> GetGruppoPersona(List<string> LGruppi, bool IsGiunta = false);
        Task<IEnumerable<KeyValueDto>> GetAll(int id_legislatura);
        Task<View_gruppi_politici_con_giunta> Get(int gruppoId);
        Task<View_UTENTI> GetCapoGruppo(int gruppoId);
        Task<IEnumerable<UTENTI_NoCons>> GetSegreteriaPolitica(int id, bool notifica_firma, bool notifica_deposito);
        Task<IEnumerable<View_UTENTI>> GetConsiglieriGruppo(int id_legislatura, int id_gruppo);
        Task<View_gruppi_politici_con_giunta> GetGruppoAttuale(List<string> lGruppi, RuoliIntEnum personaDtoCurrentRole);
        Task<View_gruppi_politici_con_giunta> GetGruppoAttuale(Guid personaUId, bool IsGiunta);

        Task<IEnumerable<JOIN_GRUPPO_AD>> GetGruppiPoliticiAD(int id_legislatura, bool soloRuoliGiunta);
        Task<View_gruppi_politici_con_giunta> GetGruppoAttuale(List<string> lGruppi);
    }
}