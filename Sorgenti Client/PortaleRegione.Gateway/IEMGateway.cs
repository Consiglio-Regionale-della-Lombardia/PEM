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
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortaleRegione.Gateway
{
    public interface IEMGateway
    {
        Task<Dictionary<Guid, string>> AssegnaNuovoPorponente(AssegnaProponenteModel model);
        Task CambioStato(ModificaStatoModel model);
        Task<Dictionary<Guid, string>> Deposita(ComandiAzioneModel model);
        Task<Dictionary<Guid, string>> Deposita(Guid emendamentoUId, string pin);
        Task<Dictionary<Guid, string>> EliminaFirma(ComandiAzioneModel model);
        Task<Dictionary<Guid, string>> EliminaFirma(Guid emendamentoUId, string pin);
        Task<Dictionary<string, string>> Firma(ComandiAzioneModel model);
        Task<Dictionary<string, string>> Firma(Guid emendamentoUId, string pin);
        Task DOWN_EM_TRATTAZIONE(Guid id);
        Task Elimina(Guid id);
        Task<EmendamentiViewModel> Get(BaseRequest<EmendamentiDto> model);
        Task<List<Guid>> GetSoloIds(BaseRequest<EmendamentiDto> model);
        Task<EmendamentiDto> Get(Guid id);
        Task<EmendamentiViewModel> Get(Guid attoUId, ClientModeEnum mode, OrdinamentoEnum ordine, int page, int size);
        Task<string> GetBody(Guid id, TemplateTypeEnum template, bool IsDeposito = false);
        Task<string> GetCopertina(CopertinaModel model);
        Task<IEnumerable<FirmeDto>> GetFirmatari(Guid id, FirmeTipoEnum tipo);
        Task<IEnumerable<DestinatariNotificaDto>> GetInvitati(Guid id);
        Task<IEnumerable<MissioniDto>> GetMissioni();
        Task<IEnumerable<TitoloMissioniDto>> GetTitoliMissioni();
        Task<EmendamentiFormModel> GetModificaMetaDatiModel(Guid id);
        Task<EmendamentiFormModel> GetModificaModel(Guid id);
        Task<EmendamentiFormModel> GetNuovoModel(Guid id, Guid? em_riferimentoUId);
        Task<IEnumerable<PartiTestoDto>> GetParti();
        Task<IEnumerable<StatiDto>> GetStati();
        Task<IEnumerable<Tipi_EmendamentiDto>> GetTipi();
        Task<EmendamentiViewModel> Get_RichiestaPropriaFirma(Guid attoUId, ClientModeEnum mode, OrdinamentoEnum ordine, int page, int size);
        Task Modifica(EmendamentiDto model);
        Task ModificaMetaDati(EmendamentiDto model);
        Task OrdinamentoConcluso(ComandiAzioneModel model);
        Task ORDINA_EM_TRATTAZIONE(Guid id);
        Task<Dictionary<Guid, string>> Raggruppa(RaggruppaEmendamentiModel model);
        Task Ritira(Guid id);
        Task<Dictionary<Guid, string>> RitiraFirma(ComandiAzioneModel model);
        Task<Dictionary<Guid, string>> RitiraFirma(Guid emendamentoUId, string pin);
        Task<EmendamentiDto> Salva(EmendamentiDto model);
        Task SPOSTA_EM_TRATTAZIONE(Guid id, int pos);
        Task UP_EM_TRATTAZIONE(Guid id);
        Task<List<TagDto>> GetTags();
        Task<FileResponse> Download(Guid id);
        Task<Dictionary<Guid, string>> GetByJson(Guid uidStampa);
    }
}