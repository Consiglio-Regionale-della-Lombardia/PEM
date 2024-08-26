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
using PortaleRegione.DTO.Domain.Essentials;

namespace PortaleRegione.Gateway
{
    public interface IDASIGateway
    {
        Task<AttoDASIDto> Salva(AttoDASIDto request);
        Task Salva_InformazioniGenerali(AttoDASI_InformazioniGeneraliDto request);
        Task<AttoDASIDto> Get(Guid id);
        Task<RiepilogoDASIModel> Get(int page, int size, StatiAttoEnum stato, TipoAttoEnum tipo, RuoliIntEnum ruolo, bool propria_firma = false);
        Task<RiepilogoDASIModel> Get(int page, int size, StatiAttoEnum stato, TipoAttoEnum tipo, RuoliIntEnum ruolo, int legislatura, bool propria_firma = false);
        Task<RiepilogoDASIModel> Get(BaseRequest<AttoDASIDto> model);
        Task<List<Guid>> GetSoloIds(BaseRequest<AttoDASIDto> model);
        Task<List<AttoDASIDto>> GetMOZAbbinabili();
        Task<List<AttiDto>> GetAttiSeduteAttive();
        Task<Dictionary<Guid, string>> Presenta(ComandiAzioneModel model);
        Task<Dictionary<Guid, string>> Presenta(Guid attoUId, string pin);
        Task<Dictionary<Guid, string>> EliminaFirma(ComandiAzioneModel model);
        Task<Dictionary<Guid, string>> EliminaFirma(Guid attoUId, string pin);
        Task<Dictionary<Guid, string>> Firma(ComandiAzioneModel model);
        Task<Dictionary<Guid, string>> Firma(Guid attoUId, string pin);
        Task<RiepilogoDASIModel> GetBySeduta(Guid sedutaUidSeduta);
        Task<RiepilogoDASIModel> GetBySeduta_Trattazione(Guid id, TipoAttoEnum tipoAtto, string uidAtto, int page = 1,
            int size = 50);
        Task<IEnumerable<AttiFirmeDto>> GetFirmatari(Guid id, FirmeTipoEnum primaDeposito);
        Task<string> GetBody(Guid id, TemplateTypeEnum template, bool privacy = false);
        Task Elimina(Guid id);
        Task Ritira(Guid id);
        Task<DASIFormModel> GetNuovoModello(TipoAttoEnum tipo);
        Task<DASIFormModel> GetModificaModello(Guid id);
        Task CambioStato(ModificaStatoAttoModel model);
        Task IscriviSeduta(IscriviSedutaDASIModel model);
        Task RichiediIscrizione(RichiestaIscrizioneDASIModel model);
        Task RimuoviSeduta(IscriviSedutaDASIModel model);
        Task RimuoviRichiestaIscrizione(RichiestaIscrizioneDASIModel model);
        Task ProponiMozioneUrgente(PromuoviMozioneModel model);
        Task ProponiMozioneAbbinata(PromuoviMozioneModel model);
        Task<IEnumerable<DestinatariNotificaDto>> GetInvitati(Guid id);
        Task<string> GetCopertina(ByQueryModel model);
        Task<IEnumerable<StatiDto>> GetStati();
        Task<IEnumerable<Tipi_AttoDto>> GetTipiMOZ();
        Task<IEnumerable<AssessoreInCaricaDto>> GetSoggettiInterrogabili();
        Task ModificaMetaDati(AttoDASIDto model);
        Task<Dictionary<Guid, string>> RitiraFirma(ComandiAzioneModel model);
        Task<Dictionary<Guid, string>> RitiraFirma(Guid attoUId, string pin);
        Task PresentazioneCartacea(PresentazioneCartaceaModel model);
        Task<FileResponse> Download(Guid id);
        Task<FileResponse> DownloadWithPrivacy(Guid id);
        Task InviaAlProtocollo(Guid id);
        Task DeclassaMozione(List<string> data);
        Task<List<AttoDASIDto>> GetCartacei();
        Task SalvaCartaceo(AttoDASIDto atto);
        Task CambiaPrioritaFirma(AttiFirmeDto firma);
        Task SalvaGruppoFiltri(FiltroPreferitoDto model);
        Task<List<FiltroPreferitoDto>> GetGruppoFiltri();
        Task EliminaGruppoFiltri(string nomeFiltro);
        Task<FileResponse> GeneraReport(ReportDto request);
        Task<FileResponse> GeneraZIP(ReportDto request);
        Task SalvaReport(ReportDto report);
        Task<List<ReportDto>> GetReports();
        Task<List<AttoLightDto>> GetAbbinamentiDisponibili(int legislaturaId, int page, int size);
        Task<List<GruppiDto>> GetGruppiDisponibili(int legislaturaId, int page, int size);
        Task<List<OrganoDto>> GetOrganiDisponibili(int legislaturaId);
        Task EliminaReport(string nomeReport);
        Task<List<TemplatesItemDto>> GetReportsCovers();
        Task<List<TemplatesItemDto>> GetReportsCardTemplates();
        Task Salva_NuovoAbbinamento(AttiAbbinamentoDto request);
        Task Salva_RimuoviAbbinamento(AttiAbbinamentoDto request);
    }
}