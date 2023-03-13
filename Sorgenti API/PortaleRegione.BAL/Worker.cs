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
using PortaleRegione.API.Controllers;
using PortaleRegione.Common;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using PortaleRegione.GestioneStampe;
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PortaleRegione.BAL
{
    public class Worker : BaseLogic
    {
        private readonly AttiLogic _logicAtti;
        private readonly AttiFirmeLogic _logicAttiFirme;
        private readonly DASILogic _logicDasi;
        private readonly EmendamentiLogic _logicEm;
        private readonly FirmeLogic _logicFirme;
        private readonly PdfStamper_IronPDF _stamper;
        private readonly IUnitOfWork _unitOfWork;
        private ThreadWorkerModel _model;
        private StampaDto _stampa;

        public Worker(IUnitOfWork unitOfWork, DASILogic logicDASI, EmendamentiLogic logicEm,
            AttiFirmeLogic logicAttiFirme, FirmeLogic logicFirme, AttiLogic logicAtti)
        {
            _unitOfWork = unitOfWork;
            _logicDasi = logicDASI;
            _logicEm = logicEm;
            _logicAttiFirme = logicAttiFirme;
            _logicFirme = logicFirme;
            _logicAtti = logicAtti;
            _stamper = new PdfStamper_IronPDF(AppSettingsConfiguration.PDF_LICENSE);
        }

        public event EventHandler<bool> OnWorkerFinish;

        public async Task<byte[]> ExecuteAsync(StampaDto stampa, ThreadWorkerModel model)
        {
            byte[] result = null;
            _stampa = stampa;
            _model = model;
            try
            {
                _unitOfWork.Stampe.AddInfo(_stampa.UIDStampa,
                    $"Inizio lavorazione - Tentativo {_stampa.Tentativi} di {_model.NumMaxTentativi}");
                if (_stampa.Tentativi < Convert.ToInt16(_model.NumMaxTentativi))
                {
                    //GetFascicolo
                    var path = string.Empty;
                    GetFascicolo(ref path);

                    if (_stampa.DASI)
                        result = await ExecuteStampaDASI(path);
                    else
                        result = await ExecuteStampaEmendamenti(path);

                    PulisciCartellaLavoroTemporanea(path);
                }

                OnWorkerFinish?.Invoke(this, true);
                return result;
            }
            catch (Exception ex)
            {
                OnWorkerFinish?.Invoke(this, false);
                return null;
            }
        }

        private async Task<byte[]> ExecuteStampaDASI(string path)
        {
            //GetListEM
            var listaAtti = await GetListaAtti();
            return await StampaDASI(listaAtti, path);
        }

        private async Task<byte[]> StampaDASI(List<AttoDASIDto> lista, string path)
        {
            try
            {
                var docs = new List<object>();
                if (_stampa.Da > 0 && _stampa.A > 0)
                    lista = lista.GetRange(_stampa.Da - 1, _stampa.A - (_stampa.Da - 1));

                var bodyCopertina = await _logicDasi.GetCopertina(lista);

                var cover = await _stamper.CreaPDFObject(bodyCopertina);
                docs.Add(cover);
                _unitOfWork.Stampe.AddInfo(_stampa.UIDStampa, "Copertina generata");

                var attiGenerati = await GeneraPDFAtti(lista, path);

                docs.AddRange(attiGenerati.Where(item => item.Content != null).Select(i => i.Content));

                var countNonGenerati = attiGenerati.Count(item => item.Content == null);
                _unitOfWork.Stampe.AddInfo(_stampa.UIDStampa, $"PDF NON GENERATI [{countNonGenerati}]");

                //Funzione che fascicola i PDF creati prima
                var nameFileTarget = $"Fascicolo_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                var FilePathTarget = Path.Combine(path, nameFileTarget);
                var merge_pdf = _stamper.MergedPDFInMemory(FilePathTarget, docs);
                _unitOfWork.Stampe.AddInfo(_stampa.UIDStampa, "FASCICOLAZIONE COMPLETATA");

                return merge_pdf;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }

        private async Task<List<BodyModel>> GeneraPDFAtti(List<AttoDASIDto> lista, string path)
        {
            var listaPercorsi = new List<BodyModel>();
            var counter = 1;
            try
            {
                var appoggio = Utility.Split(lista);

                foreach (var group in appoggio)
                    foreach (var item in group)
                        try
                        {
                            var firme = await _logicAttiFirme.GetFirme(item.UIDAtto);
                            var bodyPDF = await _logicDasi.GetBodyDASI(
                                Mapper.Map<AttoDASIDto, ATTI_DASI>(item),
                                firme,
                                null
                                , TemplateTypeEnum.PDF,
                                true);

                            var nameFilePDF =
                                $"{item.Display}_{item.UIDAtto}_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                            var FilePathComplete = Path.Combine(path, nameFilePDF);

                            var dettagliCreaPDF = new BodyModel
                            {
                                Path = FilePathComplete,
                                Body = bodyPDF,
                                Atto = item
                            };
                            var listAttachments = new List<string>();
                            if (!string.IsNullOrEmpty(item.PATH_AllegatoGenerico))
                            {
                                var complete_path = Path.Combine(
                                    _model.PercorsoCompatibilitaDocumenti,
                                    Path.GetFileName(item.PATH_AllegatoGenerico));
                                listAttachments.Add(complete_path);
                            }

                            var pdf = await _stamper.CreaPDFObject(dettagliCreaPDF.Body, true, listAttachments);
                            dettagliCreaPDF.Content = pdf;
                            listaPercorsi.Add(dettagliCreaPDF);
                            _unitOfWork.Stampe.AddInfo(_stampa.UIDStampa, $"Progresso {counter}/{lista.Count}");
                            counter++;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
            }
            catch (Exception ex)
            {
                Log.Error("GeneraPDFAtti Error-->", ex);
            }

            return listaPercorsi;
        }

        private async Task<List<AttoDASIDto>> GetListaAtti()
        {
            if (_stampa.UIDAtto.HasValue)
            {
                _unitOfWork.Stampe.AddInfo(_stampa.UIDStampa, "Scarica atto..");

                var item = await _logicDasi.GetAttoDto(_stampa.UIDAtto.Value);
                _unitOfWork.Stampe.AddInfo(_stampa.UIDStampa, "Scarica atto.. OK");
                return new List<AttoDASIDto>
                {
                    item
                };
            }

            var queryModel = new ByQueryModel(_stampa.Query);
            var results = await _logicDasi.GetByQuery(queryModel);
            var count = await _logicDasi.CountByQuery(queryModel);
            var result = new BaseResponse<AttoDASIDto>(
                queryModel.page,
                100,
                results,
                null,
                count);
            var currentPaging = result.Paging;
            await Scarica_Log(currentPaging);
            var has_next = currentPaging.Has_Next;
            var lista = result.Results.ToList();
            while (has_next)
            {
                queryModel.page++;
                results = await _logicDasi.GetByQuery(queryModel);
                count = await _logicDasi.CountByQuery(queryModel);
                result = new BaseResponse<AttoDASIDto>(
                    queryModel.page,
                    100,
                    results,
                    null,
                    count);
                await Scarica_Log(currentPaging, result.Paging);
                foreach (var item in result.Results)
                {
                    lista.Add(item);
                    if (lista.Count >= currentPaging.Total)
                    {
                        has_next = false;
                        break;
                    }
                }
            }

            return lista;
        }

        private async Task<byte[]> ExecuteStampaEmendamenti(string path)
        {
            try
            {
                //GetListEM
                var listaEMendamenti = await GetListaEM();

                return await Stampa(listaEMendamenti, path);
            }
            catch (Exception e)
            {
                Log.Error("StampaEmendamenti ERROR", e);
                throw e;
            }
        }

        private async Task<byte[]> Stampa(List<EmendamentiDto> listaEMendamenti, string path)
        {
            try
            {
                var docs = new List<object>();
                var atto = await _unitOfWork.Atti.Get(_stampa.UIDAtto.Value);
                if (_stampa.Da > 0 && _stampa.A > 0)
                    listaEMendamenti = listaEMendamenti.GetRange(_stampa.Da - 1, _stampa.A - (_stampa.Da - 1));

                var bodyCopertina = await _logicEm.GetCopertina(new CopertinaModel
                {
                    Atto = Mapper.Map<ATTI, AttiDto>(atto),
                    Totale = listaEMendamenti.Count,
                    Ordinamento = _stampa.Ordine.HasValue
                        ? (OrdinamentoEnum)_stampa.Ordine.Value
                        : OrdinamentoEnum.Presentazione
                });

                var cover = await _stamper.CreaPDFObject(bodyCopertina);
                docs.Add(cover);

                _unitOfWork.Stampe.AddInfo(_stampa.UIDStampa, "Copertina generata");
                var listaPdfEmendamentiGenerati =
                    await GeneraPDFEmendamenti(listaEMendamenti, path);

                docs.AddRange(listaPdfEmendamentiGenerati.Where(item => item.Content != null)
                    .Select(i => i.Content));

                var countNonGenerati = listaPdfEmendamentiGenerati.Count(item => item.Content == null);
                _unitOfWork.Stampe.AddInfo(_stampa.UIDStampa, $"PDF NON GENERATI [{countNonGenerati}]");

                //Funzione che fascicola i PDF creati prima
                var nameFileTarget = $"Fascicolo_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                var FilePathTarget = Path.Combine(path, nameFileTarget);
                var merge_pdf = _stamper.MergedPDFInMemory(FilePathTarget, docs);
                _unitOfWork.Stampe.AddInfo(_stampa.UIDStampa, "FASCICOLAZIONE COMPLETATA");

                var URLDownload = Path.Combine(_model.UrlCLIENT, $"stampe/{_stampa.UIDStampa}");
                _stampa.PathFile = nameFileTarget;
                //await apiGateway.Stampe.JobUpdateFileStampa(_stampa);
                if (_stampa.Ordine.HasValue)
                {
                    if ((OrdinamentoEnum)_stampa.Ordine.Value == OrdinamentoEnum.Presentazione)
                        atto.LinkFascicoloPresentazione = URLDownload;
                    if ((OrdinamentoEnum)_stampa.Ordine.Value == OrdinamentoEnum.Votazione)
                        atto.LinkFascicoloVotazione = URLDownload;

                    await _unitOfWork.CompleteAsync();
                }

                return merge_pdf;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }

        private async Task<List<EmendamentiDto>> GetListaEM()
        {
            if (_stampa.UIDEM.HasValue)
            {
                _unitOfWork.Stampe.AddInfo(_stampa.UIDStampa, "Scarica emendamento..");

                var em = await _logicEm.GetEM_DTO(_stampa.UIDEM.Value);
                _unitOfWork.Stampe.AddInfo(_stampa.UIDStampa, "Scarica emendamento.. OK");
                return new List<EmendamentiDto>
                {
                    em
                };
            }

            var model = new ByQueryModel(_stampa.Query);
            var results = await _logicEm.GetEmendamenti(model);
            var countEM = await _logicEm.CountEM(_stampa.Query);
            var resultEmendamenti = new BaseResponse<EmendamentiDto>(
                model.page,
                100,
                results,
                null,
                countEM);
            var currentPaging = resultEmendamenti.Paging;
            await Scarica_Log(currentPaging);
            var has_next = currentPaging.Has_Next;
            var listaEMendamenti = resultEmendamenti.Results.ToList();
            while (has_next)
            {
                model.page++;
                results = await _logicEm.GetEmendamenti(model);
                countEM = await _logicEm.CountEM(_stampa.Query);
                resultEmendamenti = new BaseResponse<EmendamentiDto>(
                    model.page,
                    100,
                    results,
                    null,
                    countEM);
                await Scarica_Log(currentPaging, resultEmendamenti.Paging);
                foreach (var item in resultEmendamenti.Results)
                {
                    listaEMendamenti.Add(item);
                    if (listaEMendamenti.Count >= currentPaging.Total)
                    {
                        has_next = false;
                        break;
                    }
                }
            }

            return listaEMendamenti;
        }

        private async Task Scarica_Log(Paging currentPaging, Paging paging)
        {
            var partial = paging.Limit * paging.Page - (paging.Limit - paging.Entities);
            _unitOfWork.Stampe.AddInfo(_stampa.UIDStampa,
                $"Scaricamento Blocco [{paging.Page}/{paging.Last_Page}] - [{partial}/{currentPaging.Total}]");
        }

        private async Task Scarica_Log(Paging paging)
        {
            var partial = paging.Limit * paging.Page - (paging.Limit - paging.Entities);
            _unitOfWork.Stampe.AddInfo(_stampa.UIDStampa,
                $"Scaricamento Blocco [{paging.Page}/{paging.Last_Page}] - [{partial}/{paging.Total}]");
        }

        private void GetFascicolo(ref string path)
        {
            var dirFascicolo = $"Fascicolo_{_stampa.UIDStampa}_{DateTime.Now:ddMMyyyy_hhmmss}";
            path = Path.Combine(_model.CartellaLavoroTemporanea, dirFascicolo);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private async Task<List<BodyModel>> GeneraPDFEmendamenti(List<EmendamentiDto> lista,
            string _pathTemp)
        {
            var counter = 1;
            var listaPercorsi = new List<BodyModel>();
            try
            {
                var appoggio = Utility.Split(lista);

                foreach (var group in appoggio)
                    foreach (var item in group)
                    {
                        var firme = await _logicFirme.GetFirme(new EM
                        {
                            UIDEM = item.UIDEM,
                            UIDPersonaProponente = item.UIDPersonaProponente,
                            IDStato = item.IDStato,
                            Timestamp = item.Timestamp
                        }, FirmeTipoEnum.TUTTE);
                        var bodyPDF = await _logicEm.GetBodyEM(item,
                            firme.ToList(),
                            null,
                            TemplateTypeEnum.PDF);
                        var nameFilePDF =
                            $"{item.N_EM.Replace(" ", "_").Replace("all'", "")}_{item.UIDEM}_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                        var FilePathComplete = Path.Combine(_pathTemp, nameFilePDF);

                        var dettagliCreaPDF = new BodyModel
                        {
                            Path = FilePathComplete,
                            Body = bodyPDF,
                            EM = item
                        };
                        var listAttachments = new List<string>();
                        if (!string.IsNullOrEmpty(item.PATH_AllegatoGenerico))
                        {
                            var complete_path = Path.Combine(
                                _model.PercorsoCompatibilitaDocumenti,
                                Path.GetFileName(item.PATH_AllegatoGenerico));
                            listAttachments.Add(complete_path);
                        }

                        if (!string.IsNullOrEmpty(item.PATH_AllegatoTecnico))
                        {
                            var complete_path = Path.Combine(
                                _model.PercorsoCompatibilitaDocumenti,
                                Path.GetFileName(item.PATH_AllegatoTecnico));
                            listAttachments.Add(complete_path);
                        }

                        var pdf = await _stamper.CreaPDFObject(dettagliCreaPDF.Body, true, listAttachments);
                        dettagliCreaPDF.Content = pdf;
                        listaPercorsi.Add(dettagliCreaPDF);
                        _unitOfWork.Stampe.AddInfo(_stampa.UIDStampa, $"Progresso {counter}/{lista.Count}");
                        counter++;
                    }
            }
            catch (Exception ex)
            {
                Log.Error("GeneraPDFEmendamenti Error-->", ex);
            }

            return listaPercorsi;
        }

        private void SpostaFascicolo(string _pathFascicolo, string _pathDestinazione)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_pathDestinazione)))
                Directory.CreateDirectory(Path.GetDirectoryName(_pathDestinazione));

            File.Move(_pathFascicolo, _pathDestinazione);
        }

        private void PulisciCartellaLavoroTemporanea(string _pathTemp)
        {
            try
            {
                Directory.Delete(_pathTemp, true);
            }
            catch (Exception e)
            {
                Log.Error($"ERRORE PULIZIA CARTELLA TEMPORANEA [{_pathTemp}]", e);
            }
        }
    }
}