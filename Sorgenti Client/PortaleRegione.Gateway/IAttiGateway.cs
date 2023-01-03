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

using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortaleRegione.Gateway
{
    public interface IAttiGateway
    {
        Task CreaArticolo(Guid id, string articoli);
        Task CreaComma(Guid id, string commi);
        Task CreaLettera(Guid id, string lettere);
        Task<FileResponse> Download(string path);
        Task Elimina(Guid id);
        Task EliminaArticolo(Guid id);
        Task EliminaComma(Guid id);
        Task EliminaLettera(Guid id);
        Task<BaseResponse<AttiDto>> Get(BaseRequest<AttiDto> model);
        Task<AttiDto> Get(Guid id);
        Task<BaseResponse<AttiDto>> Get(Guid sedutaUId, ClientModeEnum mode, int page, int size);
        Task<IEnumerable<ArticoliDto>> GetArticoli(Guid id);
        Task<IEnumerable<CommiDto>> GetComma(Guid id, bool expanded);
        Task<AttiFormUpdateModel> GetFormUpdate(Guid id);
        Task<IEnumerable<LettereDto>> GetLettere(Guid id);
        Task<AttiDto> Modifica(AttiFormUpdateModel atto);
        Task ModificaFiles(AttiDto atto);
        Task PubblicaFascicolo(PubblicaFascicoloModel model);
        Task BloccoODG(BloccoODGModel model);
        Task JollyODG(JollyODGModel model);
        Task<AttiDto> Salva(AttiFormUpdateModel atto);
        Task SalvaTesto(TestoAttoModel model);
        Task SalvaRelatori(AttoRelatoriModel model);
        Task SPOSTA_DOWN(Guid id);
        Task SPOSTA_UP(Guid id);


        Task<IEnumerable<Tipi_AttoDto>> GetTipi(bool dasi = true);
        Task<List<ArticoliModel>> GetGrigliaTesto(Guid id, bool viewEm);

        Task<List<EmendamentoExtraLightDto>> GetGrigliaOrdinamento(Guid id);
    }
}