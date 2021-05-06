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

using System.Collections.Generic;
using PortaleRegione.DTO.Domain;

namespace PortaleRegione.DTO.Model
{
    public class EmendamentiFormModel
    {
        public IEnumerable<TitoloMissioniDto> ListaTitoli_Missioni;
        public EmendamentiDto Emendamento { get; set; }
        public AttiDto Atto { get; set; }
        public IEnumerable<PersonaDto> ListaGruppo { get; set; }
        public IEnumerable<PersonaDto> ListaAssessori { get; set; }
        public IEnumerable<PersonaDto> ListaConsiglieri { get; set; }
        public IEnumerable<PartiTestoDto> ListaPartiEmendabili { get; set; }
        public IEnumerable<Tipi_EmendamentiDto> ListaTipiEmendamento { get; set; }
        public IEnumerable<MissioniDto> ListaMissioni { get; set; }
        public IEnumerable<ArticoliDto> ListaArticoli { get; set; }
        public List<KeyValueDto> ListaAreaPolitica { get; set; }

        public bool IsGiunta { get; set; } = false;
    }
}