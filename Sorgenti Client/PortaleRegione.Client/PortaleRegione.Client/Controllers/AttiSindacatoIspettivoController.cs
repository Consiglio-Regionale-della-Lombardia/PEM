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

using PortaleRegione.Client.Models;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Response;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PortaleRegione.Client.Controllers
{
    /// <summary>
    ///     Controller atti
    /// </summary>
    [Authorize]
    [RoutePrefix("atti-sindacato-ispettivo")]
    public class AttiSindacatoIspettivoController : BaseController
    {
        public async Task<ActionResult> RiepilogoDasi()
        {
            var list = new List<AttiDto>
            {
                new AttiDto
                {
                    PersonaProponente = new PersonaLightDto("Fante", "Lelle"),
                    Oggetto = "Domanda spiccia",
                    Testo = "Perchè sei li seduto e non fai niente?",
                    IDTipoAtto = (int)TipoAttoEnum.IQT,
                    Stato = (int)StatiAttoEnum.APPROVATO
                },
                new AttiDto
                {
                    PersonaProponente = new PersonaLightDto("Insegno", "Pino"),
                    Oggetto = "Domanda spiccia ma non troppo",
                    Testo = "Mai pensato a far qualcosa di diverso?",
                    IDTipoAtto = (int)TipoAttoEnum.ITR,
                    Stato = (int)StatiAttoEnum.BOZZA
                },
                new AttiDto
                {
                    PersonaProponente = new PersonaLightDto("Wario", "Mario"),
                    Oggetto = "Prova di trasmissione",
                    Testo = "Volevo vedere se funzionava tutto.. sembra di no.. o forse si",
                    IDTipoAtto = (int)TipoAttoEnum.ITL,
                    Stato = (int)StatiAttoEnum.PRESENTATO
                },
                new AttiDto
                {
                    PersonaProponente = new PersonaLightDto("Fante", "Lelle"),
                    Oggetto = "Domanda spiccia",
                    Testo = "Perchè sei li seduto e non fai niente?",
                    IDTipoAtto = (int)TipoAttoEnum.MOZ,
                    Stato = (int)StatiAttoEnum.DECADUTO
                },
                new AttiDto
                {
                    PersonaProponente = new PersonaLightDto("Fante", "Lelle"),
                    Oggetto = "Domanda spiccia",
                    Testo = "Perchè sei li seduto e non fai niente?",
                    IDTipoAtto = (int)TipoAttoEnum.ODG,
                    Stato = (int)StatiAttoEnum.INAMMISSIBILE
                },
                new AttiDto
                {
                    PersonaProponente = new PersonaLightDto("Cattapan", "Matteo"),
                    Oggetto = "Domanda spiccia",
                    Testo = "Perchè sei li seduto e non fai niente?",
                    IDTipoAtto = (int)TipoAttoEnum.MOZ_A,
                    Stato = (int)StatiAttoEnum.IN_TRATTAZIONE
                }
            };

            var model = new DASIViewModel
            {
                Data = new BaseResponse<AttiDto>
                {
                    Results = list
                },
                ODG = 21000
            };

            return View("RiepilogoDASI", model);
        }
    }
}