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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using PortaleRegione.BAL;
using PortaleRegione.Common;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using PortaleRegione.GestioneStampe;
using PortaleRegione.Persistance;

namespace GeneraStampeJobFramework
{
    public class Worker
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Worker));
        private readonly LoginResponse _auth;
        private readonly ThreadWorkerModel _model;
        private readonly PdfStamper_IronPDF _stamper;
        private readonly UnitOfWork _unitOfWork;
        private readonly ApiGateway apiGateway;
        private StampaDto _stampa;

        private int counter;

        public Worker(LoginResponse auth, ref ThreadWorkerModel model)
        {
            _auth = auth;
            _model = model;
            BaseGateway.apiUrl = _model.UrlAPI;
            apiGateway = new ApiGateway(_auth.jwt);
            _stamper = new PdfStamper_IronPDF(_model.PDF_LICENSE);
        }

        public Worker(string jwt, UnitOfWork unitOfWork, ref ThreadWorkerModel model)
        {
            _model = model;
            BaseGateway.apiUrl = _model.UrlAPI;
            apiGateway = new ApiGateway(jwt);
            _auth = new LoginResponse
            {
                jwt = jwt
            };
            _unitOfWork = unitOfWork;
            _stamper = new PdfStamper_IronPDF(_model.PDF_LICENSE);
        }

        public event EventHandler<bool> OnWorkerFinish;

        public async Task LogStampa(Guid uidStampa, string message)
        {
            _unitOfWork.Stampe.AddInfo(uidStampa, message);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> ExecuteAsync(StampaDto stampa)
        {
            var result = false;
            _stampa = stampa;
            log.Info($"[ExecuteAsync] START Stampa UID={_stampa.UIDStampa} - Tentativo: {_stampa.Tentativi}");
            var utenteRichiedente = await apiGateway.Persone.Get(_stampa.UIDUtenteRichiesta);
            try
            {
                await LogStampa(_stampa.UIDStampa,
                    $"Inizio lavorazione - Tentativo {_stampa.Tentativi} di {_model.NumMaxTentativi}");

                log.Debug($"[ExecuteAsync] Avvio logica stampa (DASI={_stampa.DASI})");

                if (_stampa.Tentativi < Convert.ToInt16(_model.NumMaxTentativi))
                {
                    //GetFascicolo
                    var path = string.Empty;
                    GetFascicolo(ref path);

                    log.Debug($"[ExecuteAsync] Cartella lavoro temp: {path}");

                    if (_stampa.DASI)
                        await ExecuteStampaDASI(utenteRichiedente, path);
                    else
                        await ExecuteStampaEmendamenti(utenteRichiedente, path);

                    PulisciCartellaLavoroTemporanea(path);
                }
                else
                {
                    log.Warn(
                        $"[ExecuteAsync] Raggiunto max tentativi per UID={_stampa.UIDStampa}. Invio notifica errore.");
                    try
                    {
                        await BaseGateway.SendMail(new MailModel
                            {
                                DA = _model.EmailFrom,
                                A = utenteRichiedente.email,
                                OGGETTO = "Errore generazione stampa",
                                MESSAGGIO =
                                    $"ID stampa: [{_stampa.UIDStampa}], per l'atto: [{_stampa.UIDAtto}]. Motivo: [{_stampa.MessaggioErrore}]"
                            },
                            _auth.jwt);
                    }
                    catch (Exception e)
                    {
                        log.Error($"[ExecuteAsync] ERRORE invio mail fallito (UID={_stampa.UIDStampa})", e);
                        await LogStampa(_stampa.UIDStampa,
                            $"Invio mail EXCEPTION ERRORE. Motivo: {e.Message}");
                    }
                }

                log.Info($"[ExecuteAsync] FINE SUCCESSO UID={_stampa.UIDStampa}");
                OnWorkerFinish?.Invoke(this, true);
                result = true;
            }
            catch (Exception ex)
            {
                OnWorkerFinish?.Invoke(this, false);

                log.Error($"[ExecuteAsync] ERRORE generale su UID={_stampa.UIDStampa}", ex);
                try
                {
                    await LogStampa(_stampa.UIDStampa, ex.StackTrace);
                    await apiGateway.Stampe.JobErrorStampa(_stampa.UIDStampa, ex.Message);
                    await apiGateway.Stampe.JobUnLockStampa(_stampa.UIDStampa);
                }
                catch (Exception ex2)
                {
                    log.Error($"[ExecuteAsync] ERRORE gestione errori interni UID={_stampa.UIDStampa}", ex2);
                    try
                    {
                        await BaseGateway.SendMail(new MailModel
                            {
                                DA = _model.EmailFrom,
                                A = utenteRichiedente.email,
                                OGGETTO = "Errore generazione fascicolo",
                                MESSAGGIO = ex.Message
                            },
                            _auth.jwt);
                    }
                    catch (Exception exMail)
                    {
                        log.Error($"[ExecuteAsync] ERRORE invio mail gestione errori UID={_stampa.UIDStampa}", exMail);
                        await apiGateway.Stampe.JobErrorStampa(_stampa.UIDStampa, exMail.Message);
                    }
                }
            }

            return result;
        }

        private async Task ExecuteStampaDASI(PersonaDto persona, string path)
        {
            log.Info($"[ExecuteStampaDASI] INIZIO UIDStampa={_stampa.UIDStampa}");
            try
            {
                var listaAtti = await GetListaAtti();
                log.Debug($"[ExecuteStampaDASI] Numero atti da gestire: {listaAtti.Count}");

                if (_stampa.Notifica)
                {
                    log.Info("[ExecuteStampaDASI] Avvio PresentazioneDifferita");
                    await PresentazioneDifferita(listaAtti);
                    log.Info("[ExecuteStampaDASI] PresentazioneDifferita completata");
                }
                else
                {
                    log.Info("[ExecuteStampaDASI] Avvio StampaDASI normale");
                    await StampaDASI(listaAtti, path, persona);
                    log.Info("[ExecuteStampaDASI] StampaDASI completata");
                }
            }
            catch (Exception e)
            {
                log.Error($"[ExecuteStampaDASI] ERRORE su UIDStampa={_stampa.UIDStampa}", e);
                throw;
            }

            log.Info($"[ExecuteStampaDASI] FINE UIDStampa={_stampa.UIDStampa}");
        }

        private async Task StampaDASI(List<ATTI_DASI> lista, string path, PersonaDto persona)
        {
            log.Info($"[StampaDASI] INIZIO UIDStampa={_stampa.UIDStampa}, N.Att={lista.Count}");
            try
            {
                var docs = new List<byte[]>();
                var bodyCopertina = await apiGateway.DASI.GetCopertina(new ByQueryModel
                {
                    Query = _stampa.Query
                });

                docs.Add(await _stamper.CreaPDFInMemory(bodyCopertina));

                var attiGenerati = await GeneraPDFAtti(lista, path);

                docs.AddRange(attiGenerati.Where(item => item.Value.Content != null)
                    .Select(i => (byte[])i.Value.Content));

                var countNonGenerati = attiGenerati.Count(item => item.Value.Content == null);
                await LogStampa(_stampa.UIDStampa, $"PDF NON GENERATI [{countNonGenerati}]");

                //Funzione che fascicola i PDF creati prima
                var nameFileTarget = $"Fascicolo_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                var FilePathTarget = Path.Combine(path, nameFileTarget);
                _stamper.MergedPDF(FilePathTarget, docs);

                await LogStampa(_stampa.UIDStampa, "FASCICOLAZIONE COMPLETATA");

                var _pathStampe = Path.Combine(_model.CartellaLavoroStampe, nameFileTarget);
                SpostaFascicolo(FilePathTarget, _pathStampe);

                var URLDownload = Path.Combine(_model.UrlCLIENT, $"stampe/{_stampa.UIDStampa}");
                _stampa.PathFile = nameFileTarget;

                var bodyMail =
                    $"Gentile {persona.DisplayName},<br>la stampa richiesta sulla piattaforma è disponibile al seguente link:<br><a href='{URLDownload}' target='_blank'>{URLDownload}</a>";
                var resultInvio = await BaseGateway.SendMail(new MailModel
                    {
                        DA = _model.EmailFrom,
                        A = persona.email,
                        OGGETTO = "Link download fascicolo",
                        MESSAGGIO = bodyMail
                    },
                    _auth.jwt);

                var stampaInDb = await _unitOfWork.Stampe.Get(_stampa.UIDStampa);
                if (resultInvio)
                {
                    stampaInDb.Invio = true;
                    stampaInDb.DataInvio = DateTime.Now;
                }

                stampaInDb.DataFineEsecuzione = DateTime.Now;
                stampaInDb.PathFile = _stampa.PathFile;
                stampaInDb.MessaggioErrore = string.Empty;

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                log.Error($"[StampaDASI] ERRORE UIDStampa={_stampa.UIDStampa}", e);
                throw;
            }

            log.Info($"[StampaDASI] FINE UIDStampa={_stampa.UIDStampa}");
        }

        private async Task<Dictionary<Guid, BodyModel>> GeneraPDFAtti(List<ATTI_DASI> lista, string path)
        {
            var listaPercorsi = new Dictionary<Guid, BodyModel>();
            var counter = 0;
            var totalItems = lista.Count;

            try
            {
                listaPercorsi = lista.ToDictionary(atto => atto.UIDAtto, atto => new BodyModel());

                foreach (var item in lista)
                    try
                    {
                        var bodyPDF = await apiGateway.DASI.GetBody(item.UIDAtto, TemplateTypeEnum.PDF, true);
                        var nameFilePDF = $"{item.Etichetta}_{item.UIDAtto}_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                        var filePathComplete = string.IsNullOrEmpty(path)
                            ? nameFilePDF
                            : Path.Combine(path, nameFilePDF);

                        var dettagliCreaPDF = new BodyModel
                        {
                            Path = filePathComplete,
                            Body = bodyPDF,
                            Atto = item
                        };

                        var listAttachments = new List<string>();
                        if (!string.IsNullOrEmpty(item.PATH_AllegatoGenerico))
                        {
                            var completePath = Path.Combine(_model.PercorsoCompatibilitaDocumenti,
                                Path.GetFileName(item.PATH_AllegatoGenerico));
                            listAttachments.Add(completePath);
                        }

                        // Genera PDF e salva direttamente su disco
                        dettagliCreaPDF.Content = await _stamper.CreaPDFInMemory(dettagliCreaPDF.Body, item.Etichetta,
                            listAttachments);

                        listaPercorsi[item.UIDAtto] = dettagliCreaPDF;
                        counter++;

                        // Update progress every 50 items or at the end
                        if (counter % 50 == 0 || counter == totalItems)
                            await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, $"Progresso {counter}/{totalItems}");
                    }
                    catch (Exception e)
                    {
                        await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, $"Errore: {item.Etichetta}");
                        // Log error here if necessary
                    }
            }
            catch (Exception ex)
            {
                // Log error here if necessary
            }

            return listaPercorsi;
        }

        private async Task PresentazioneDifferita(List<ATTI_DASI> listaAtti)
        {
            log.Info($"[PresentazioneDifferita] INIZIO UIDStampa={_stampa.UIDStampa}, N.Att={listaAtti.Count}");
            try
            {
                var item = listaAtti.First();
                var bodyPDF = await apiGateway.DASI.GetBody(item.UIDAtto, TemplateTypeEnum.PDF, true);
                var nameFilePDF = $"{item.Etichetta}_{item.UIDAtto}_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";

                var content = await _stamper.CreaPDFInMemory(bodyPDF, nameFilePDF);

                var dasiDto = listaAtti.First();
                var legislatura = dasiDto.GetLegislatura();
                //Legislatura/Tipo
                var dir = $"{legislatura}/{Utility.GetText_Tipo(dasiDto.Tipo)}/{dasiDto.Etichetta}";
                var pathRepository = $"{_model.RootRepository}/{dir}";

                if (!Directory.Exists(pathRepository))
                    Directory.CreateDirectory(pathRepository);

                var destinazioneDeposito = Path.Combine(pathRepository, nameFilePDF);
                File.WriteAllBytes(destinazioneDeposito, content);
                _stampa.PathFile = Path.Combine($"{dir}", nameFilePDF);
                _stampa.UIDAtto = dasiDto.UIDAtto;

                var stampaInDb = await _unitOfWork.Stampe.Get(_stampa.UIDStampa);
                stampaInDb.DataFineEsecuzione = DateTime.Now;
                stampaInDb.PathFile = _stampa.PathFile;
                stampaInDb.MessaggioErrore = string.Empty;

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                log.Error($"[PresentazioneDifferita] ERRORE UIDStampa={_stampa.UIDStampa}", e);
                throw;
            }

            log.Info($"[PresentazioneDifferita] FINE UIDStampa={_stampa.UIDStampa}");
        }

        private async Task<List<ATTI_DASI>> GetListaAtti()
        {
            log.Debug($"[GetListaAtti] INIZIO per UIDStampa={_stampa.UIDStampa}");
            try
            {
                if (_stampa.UIDAtto.HasValue)
                {
                    await LogStampa(_stampa.UIDStampa, "Scarica atto..");

                    var item = await _unitOfWork.DASI.Get(_stampa.UIDAtto.Value);
                    await LogStampa(_stampa.UIDStampa, "Scarica atto.. OK");
                    return new List<ATTI_DASI>
                    {
                        item
                    };
                }

                try
                {
                    var resFromJson = new List<ATTI_DASI>();
                    var listaAttiFromJson = JsonConvert.DeserializeObject<List<Guid>>(_stampa.Query);
                    foreach (var guid in listaAttiFromJson)
                    {
                        var item = await _unitOfWork.DASI.Get(guid);
                        resFromJson.Add(item);
                    }

                    return resFromJson;
                }
                catch (Exception e)
                {
                    log.Error($"[GetListaAtti] ERRORE UIDStampa={_stampa.UIDStampa}", e);
                }

                return new List<ATTI_DASI>();
            }
            catch (Exception e)
            {
                log.Error($"[GetListaAtti] ERRORE UIDStampa={_stampa.UIDStampa}", e);
                throw;
            }

            log.Debug($"[GetListaAtti] FINE per UIDStampa={_stampa.UIDStampa}");
        }

        private async Task ExecuteStampaEmendamenti(PersonaDto persona, string path)
        {
            log.Info($"[ExecuteStampaEmendamenti] INIZIO UIDStampa={_stampa.UIDStampa}");

            try
            {
                //GetListEM
                var listaEMendamenti = await GetListaEM();
                log.Debug($"[ExecuteStampaEmendamenti] Numero emendamenti: {listaEMendamenti.Count}");

                if (_stampa.Notifica)
                {
                    log.Info("[ExecuteStampaEmendamenti] Avvio DepositoDifferito");
                    await DepositoDifferito(listaEMendamenti, persona);
                    log.Info("[ExecuteStampaEmendamenti] DepositoDifferito completato");
                }
                else
                {
                    log.Info("[ExecuteStampaEmendamenti] Avvio Stampa");
                    await Stampa(listaEMendamenti, path, persona);
                    log.Info("[ExecuteStampaEmendamenti] Stampa completata");
                }
            }
            catch (Exception e)
            {
                log.Error($"[ExecuteStampaEmendamenti] ERRORE UIDStampa={_stampa.UIDStampa}", e);
                throw e;
            }

            log.Info($"[ExecuteStampaEmendamenti] FINE UIDStampa={_stampa.UIDStampa}");
        }

        private async Task Stampa(Dictionary<Guid, string> listaEMendamenti, string path,
            PersonaDto utenteRichiedente)
        {
            log.Info($"[Stampa] INIZIO UIDStampa={_stampa.UIDStampa}, N.Emendamenti={listaEMendamenti.Count}");
            try
            {
                var atto = await apiGateway.Atti.Get(_stampa.UIDAtto.Value);
                var nameFileTarget = $"Fascicolo_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                var FilePathTarget = string.Empty;
                if (_stampa.UIDFascicolo.HasValue)
                    // Fascicolo
                    nameFileTarget = $"Fascicolo_{_stampa.NumeroFascicolo}_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";

                FilePathTarget = Path.Combine(path, nameFileTarget);

                if (!_stampa.UIDFascicolo.HasValue)
                {
                    // Fascicolo
                    var bodyCopertina = await apiGateway.Emendamento.GetCopertina(new CopertinaModel
                    {
                        Atto = atto,
                        Totale = listaEMendamenti.Count,
                        Ordinamento = _stampa.Ordine.HasValue
                            ? (OrdinamentoEnum)_stampa.Ordine.Value
                            : OrdinamentoEnum.Presentazione
                    });

                    await _stamper.CreaPDFAsync(FilePathTarget, bodyCopertina, "");
                }

                var listaPdfEmendamentiGenerati =
                    await GeneraPDFEmendamenti(listaEMendamenti, path);

                _stamper.MergedPDFWithRetry(FilePathTarget,
                    listaPdfEmendamentiGenerati.Select(p => p.Value.Path).ToList());

                await LogStampa(_stampa.UIDStampa, "FASCICOLAZIONE COMPLETATA");
                var _pathStampe = Path.Combine(_model.CartellaLavoroStampe, nameFileTarget);

                SpostaFascicolo(FilePathTarget, _pathStampe);

                var URLDownload = $"{_model.UrlCLIENT}/stampe/{_stampa.UIDStampa}";
                _stampa.PathFile = nameFileTarget;
                var stampaInDb = await _unitOfWork.Stampe.Get(_stampa.UIDStampa);
                stampaInDb.DataFineEsecuzione = DateTime.Now;
                stampaInDb.PathFile = _stampa.PathFile;
                stampaInDb.MessaggioErrore = string.Empty;

                await _unitOfWork.CompleteAsync();

                if (_stampa.UIDFascicolo.HasValue)
                {
                    // Fascicolo
                    var stampeFascicolo =
                        await _unitOfWork.Stampe.GetStampeFascicolo(_stampa.UIDFascicolo.Value);

                    if (stampeFascicolo.All(s => s.DataFineEsecuzione.HasValue))
                    {
                        // Copertina Fascicolo
                        var prefissoTesto = "Fascicolo";
                        if (_stampa.Da.Equals(0) && _stampa.A.Equals(0))
                        {
                            if (_stampa.Ordine.HasValue)
                                switch ((OrdinamentoEnum)_stampa.Ordine)
                                {
                                    case OrdinamentoEnum.Presentazione:
                                        prefissoTesto += " in ordine di presentazione:";
                                        break;
                                    case OrdinamentoEnum.Votazione:
                                        prefissoTesto += " in ordine di votazione:";
                                        break;
                                }
                        }
                        else
                        {
                            prefissoTesto += ":";
                        }

                        var counter = 1;
                        var totaleElementi = 0;
                        var bodyCopertinaFascicolo = "<ul>";
                        foreach (var fascicolo in stampeFascicolo)
                        {
                            // Deserializza la lista di elementi dal Query
                            var list = JsonConvert.DeserializeObject<List<Guid>>(fascicolo.Query);
                            totaleElementi += list.Count;
                            // Calcola l'inizio e la fine del fascicolo
                            var start_fascicolo = counter;
                            var end_fascicolo = counter + list.Count - 1;

                            var URLDownloadFascicolo = $"{_model.UrlCLIENT}/stampe/{fascicolo.UIDStampa}";

                            // Aggiungi l'elemento alla lista HTML
                            bodyCopertinaFascicolo +=
                                $"<li><a href='{URLDownloadFascicolo}' target='_blank'>{prefissoTesto} EM dal {start_fascicolo} a {end_fascicolo}</a></li>";

                            // Aggiorna il contatore
                            counter = end_fascicolo + 1;
                        }

                        // Chiudi il tag <ul>
                        bodyCopertinaFascicolo += "</ul>";

                        var nomeCopertinaFascicolo = $"Fascicolo_EM_1_{totaleElementi}_{DateTime.Now.Ticks}.pdf";
                        var pathCopertinaFascicolo = Path.Combine(_model.CartellaLavoroStampe, nomeCopertinaFascicolo);

                        await _stamper.CreaPDFAsync(pathCopertinaFascicolo, bodyCopertinaFascicolo, "");

                        var URLDownloadCopertinaFascicolo =
                            $"{_model.UrlCLIENT}/stampe/fascicolo/{Path.GetFileNameWithoutExtension(nomeCopertinaFascicolo)}";

                        if (_stampa.Da.Equals(0) && _stampa.A.Equals(0))
                        {
                            if (_stampa.Ordine.HasValue)
                            {
                                var attoInDb = await _unitOfWork.Atti.Get(atto.UIDAtto);
                                if ((OrdinamentoEnum)_stampa.Ordine.Value == OrdinamentoEnum.Presentazione)
                                    attoInDb.LinkFascicoloPresentazione = URLDownloadCopertinaFascicolo;
                                if ((OrdinamentoEnum)_stampa.Ordine.Value == OrdinamentoEnum.Votazione)
                                    attoInDb.LinkFascicoloVotazione = URLDownloadCopertinaFascicolo;

                                await _unitOfWork.CompleteAsync();
                            }
                        }
                        else
                        {
                            try
                            {
                                var bodyMail =
                                    $"Gentile {utenteRichiedente.DisplayName},<br>la stampa richiesta sulla piattaforma PEM è disponibile al seguente link:<br><a href='{URLDownloadCopertinaFascicolo}' target='_blank'>{URLDownloadCopertinaFascicolo}</a>";
                                var resultInvio = await BaseGateway.SendMail(new MailModel
                                    {
                                        DA = _model.EmailFrom,
                                        A = utenteRichiedente.email,
                                        OGGETTO = "Link download fascicolo",
                                        MESSAGGIO = bodyMail
                                    },
                                    _auth.jwt);
                                if (resultInvio)
                                {
                                    stampaInDb = await _unitOfWork.Stampe.Get(_stampa.UIDStampa);
                                    stampaInDb.DataInvio = DateTime.Now;
                                    stampaInDb.Invio = true;

                                    await _unitOfWork.CompleteAsync();
                                }
                            }
                            catch (Exception e)
                            {
                                //Log.Debug($"[{_stampa.UIDStampa}] Invio mail", e);
                                await LogStampa(_stampa.UIDStampa, $"Invio mail ERRORE. Motivo: {e.Message}");
                            }
                        }
                    }
                }
                else
                {
                    if (_stampa.Da.Equals(0) && _stampa.A.Equals(0))
                        if (_stampa.Ordine.HasValue)
                        {
                            var attoInDb = await _unitOfWork.Atti.Get(atto.UIDAtto);
                            if ((OrdinamentoEnum)_stampa.Ordine.Value == OrdinamentoEnum.Presentazione)
                                attoInDb.LinkFascicoloPresentazione = URLDownload;
                            if ((OrdinamentoEnum)_stampa.Ordine.Value == OrdinamentoEnum.Votazione)
                                attoInDb.LinkFascicoloVotazione = URLDownload;
                            await _unitOfWork.CompleteAsync();
                        }

                    if (_stampa.Scadenza.HasValue)
                        try
                        {
                            var bodyMail =
                                $"Gentile {utenteRichiedente.DisplayName},<br>la stampa richiesta sulla piattaforma PEM è disponibile al seguente link:<br><a href='{URLDownload}' target='_blank'>{URLDownload}</a>";
                            var resultInvio = await BaseGateway.SendMail(new MailModel
                                {
                                    DA = _model.EmailFrom,
                                    A = utenteRichiedente.email,
                                    OGGETTO = "Link download fascicolo",
                                    MESSAGGIO = bodyMail
                                },
                                _auth.jwt);
                            if (resultInvio)
                            {
                                stampaInDb = await _unitOfWork.Stampe.Get(_stampa.UIDStampa);
                                stampaInDb.DataInvio = DateTime.Now;
                                stampaInDb.Invio = true;

                                await _unitOfWork.CompleteAsync();
                            }
                        }
                        catch (Exception e)
                        {
                            //Log.Debug($"[{_stampa.UIDStampa}] Invio mail", e);
                            await LogStampa(_stampa.UIDStampa, $"Invio mail ERRORE. Motivo: {e.Message}");
                        }
                }

                try
                {
                    if (Directory.Exists(path)) Directory.Delete(path, true);
                }
                catch (Exception e)
                {
                    log.Error($"[Stampa] ERRORE UIDStampa={_stampa.UIDStampa}", e);
                }
            }
            catch (Exception e)
            {
                log.Error($"[Stampa] ERRORE UIDStampa={_stampa.UIDStampa}", e);
                throw e;
            }

            log.Info($"[Stampa] FINE UIDStampa={_stampa.UIDStampa}");
        }

        private async Task DepositoDifferito(Dictionary<Guid, string> listaEMendamenti,
            PersonaDto utenteRichiedente)
        {
            log.Info($"[DepositoDifferito] INIZIO UIDStampa={_stampa.UIDStampa}");
            try
            {
                var em = listaEMendamenti.First();
                var emInDb = await _unitOfWork.Emendamenti.Get(em.Key);
                var nEM = "";
                if (emInDb.Rif_UIDEM.HasValue)
                {
                    var emInDbRif = await _unitOfWork.Emendamenti.Get(emInDb.Rif_UIDEM.Value);
                    nEM = GetNomeEM(emInDb, emInDbRif);
                }
                else
                {
                    nEM = GetNomeEM(emInDb, null);
                }

                if (nEM.Contains("Valore Corrotto")) throw new Exception("Errore valore corrotto");

                var nameFilePDF =
                    $"{Utility.CleanFileName(nEM)}_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";

                var content = await _stamper.CreaPDFInMemory(em.Value, nameFilePDF);

                var atto = await _unitOfWork.Atti.Get(_stampa.UIDAtto.Value);
                var dirSeduta = $"Seduta_{atto.SEDUTE.Data_seduta:yyyyMMdd}";
                var dirPDL = Regex.Replace($"{Utility.GetText_Tipo(atto.IDTipoAtto)} {atto.NAtto}", @"[^0-9a-zA-Z]+",
                    "_");
                var pathRepository = $"{_model.RootRepository}/{dirSeduta}/{dirPDL}";

                if (!Directory.Exists(pathRepository))
                    Directory.CreateDirectory(pathRepository);

                var destinazioneDeposito = Path.Combine(pathRepository, nameFilePDF);
                File.WriteAllBytes(destinazioneDeposito, content);

                //SpostaFascicolo(listaPdfEmendamentiGenerati.First().Value.Path, destinazioneDeposito);
                _stampa.PathFile = Path.Combine($"{dirSeduta}/{dirPDL}", nameFilePDF);
                _stampa.UIDEM = em.Key;
                var stampaInDb = await _unitOfWork.Stampe.Get(_stampa.UIDStampa);
                stampaInDb.DataFineEsecuzione = DateTime.Now;
                stampaInDb.PathFile = _stampa.PathFile;
                stampaInDb.MessaggioErrore = string.Empty;

                await _unitOfWork.CompleteAsync();

                var bodyMail = "E' stato depositato l'EM in oggetto";

                if (atto.SEDUTE.Data_effettiva_inizio.HasValue)
                {
                    var ruoloSegreteriaAssempblea =
                        await _unitOfWork.Ruoli.Get((int)RuoliIntEnum.Segreteria_Assemblea);

                    if (DateTime.Now >
                        atto.SEDUTE.Data_effettiva_inizio.Value)
                        try
                        {
                            await BaseGateway.SendMail(new MailModel
                                {
                                    DA = _model.EmailFrom,
                                    A = $"{ruoloSegreteriaAssempblea.ADGroup}@consiglio.regione.lombardia.it",
                                    OGGETTO =
                                        $"[TRATTAZIONE AULA] {Utility.GetText_Tipo(atto.IDTipoAtto)} {atto.NAtto}: Deposito durante l'aula",
                                    MESSAGGIO = bodyMail,
                                    pathAttachment = destinazioneDeposito
                                },
                                _auth.jwt);
                        }
                        catch (Exception e)
                        {
                            //Log.Debug($"[{_stampa.UIDStampa}] Invio mail segreteria", e);
                            await LogStampa(_stampa.UIDStampa,
                                $"Invio mail a segreteria ERRORE. Motivo: {e.Message}");
                        }
                }

                //Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Seduta non è ancora iniziata");
                var email_destinatari = $"{utenteRichiedente.email};pem@consiglio.regione.lombardia.it";
                var email_destinatariGruppo = string.Empty;
                var email_destinatariGiunta = string.Empty;

                if (emInDb.id_gruppo < 10000)
                {
                    var capoGruppo = await _unitOfWork.Gruppi.GetCapoGruppo(emInDb.id_gruppo);
                    var segreteriaPolitica =
                        await _unitOfWork.Gruppi.GetSegreteriaPolitica(emInDb.id_gruppo, false, true);

                    if (segreteriaPolitica.Any())
                        email_destinatariGruppo = segreteriaPolitica.Select(u => u.email)
                            .Aggregate((i, j) => $"{i};{j}");
                    if (capoGruppo != null)
                        email_destinatariGruppo += $";{capoGruppo.email}";
                }
                else
                {
                    //Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Invio mail a Giunta Regionale");
                    var giuntaRegionale = await _unitOfWork.Persone.GetGiuntaRegionale();
                    var segreteriaGiuntaRegionale = await _unitOfWork.Persone.GetSegreteriaGiuntaRegionale(false, true);

                    if (segreteriaGiuntaRegionale.Any())
                        email_destinatariGiunta += segreteriaGiuntaRegionale.Select(u => u.email)
                            .Aggregate((i, j) => $"{i};{j}");
                    if (giuntaRegionale.Any())
                        email_destinatariGiunta +=
                            giuntaRegionale.Select(u => u.email).Aggregate((i, j) => $"{i};{j}");
                }

                if (!string.IsNullOrEmpty(email_destinatariGruppo))
                    email_destinatari += ";" + email_destinatariGruppo;
                if (!string.IsNullOrEmpty(email_destinatariGiunta))
                    email_destinatari += ";" + email_destinatariGiunta;

                try
                {
                    var resultInvio = await BaseGateway.SendMail(new MailModel
                        {
                            DA = _model.EmailFrom,
                            A = email_destinatari,
                            OGGETTO =
                                $"{Utility.GetText_Tipo(atto.IDTipoAtto)} {atto.NAtto}: Depositato",
                            MESSAGGIO = bodyMail,
                            pathAttachment = destinazioneDeposito,
                            IsDeposito = true
                        },
                        _auth.jwt);

                    if (resultInvio)
                    {
                        stampaInDb = await _unitOfWork.Stampe.Get(_stampa.UIDStampa);
                        stampaInDb.Invio = true;
                        stampaInDb.DataInvio = DateTime.Now;
                        await _unitOfWork.CompleteAsync();
                    }
                }
                catch (Exception e)
                {
                    log.Error($"[DepositoDifferito] ERRORE Invio mail UIDStampa={_stampa.UIDStampa}", e);
                    await LogStampa(_stampa.UIDStampa, $"Invio mail deposito ERRORE. Motivo: {e.Message}");
                }
            }
            catch (Exception ex)
            {
                log.Error($"[DepositoDifferito] ERRORE UIDStampa={_stampa.UIDStampa}", ex);
                throw;
            }

            log.Info($"[DepositoDifferito] FINE UIDStampa={_stampa.UIDStampa}");
        }

        private async Task<Dictionary<Guid, string>> GetListaEM()
        {
            log.Debug($"[GetListaEM] INIZIO UIDStampa={_stampa.UIDStampa}");
            try
            {
                if (_stampa.UIDEM.HasValue)
                {
                    await LogStampa(_stampa.UIDStampa, "Scarica emendamento..");

                    var emBody = await apiGateway.Emendamento.GetBody(_stampa.UIDEM.Value, TemplateTypeEnum.PDF);
                    await LogStampa(_stampa.UIDStampa, "Scarica emendamento.. OK");
                    return new Dictionary<Guid, string>
                    {
                        { _stampa.UIDEM.Value, emBody }
                    };
                }

                try
                {
                    return await apiGateway.Emendamento.GetByJson(_stampa.UIDStampa);
                }
                catch (Exception e)
                {
                    log.Error($"[GetListaEM] ERRORE UIDStampa={_stampa.UIDStampa}", e);
                }

                return new Dictionary<Guid, string>();
            }
            catch (Exception e)
            {
                log.Error($"[GetListaEM] ERRORE UIDStampa={_stampa.UIDStampa}", e);
                throw;
            }
            finally
            {
                log.Debug($"[GetListaEM] FINE UIDStampa={_stampa.UIDStampa}");
            }
        }

        private string GetNomeEM(EM emendamento, EM riferimento)
        {
            try
            {
                var riferimentoText = "";
                if (riferimento != null)
                {
                    if (!string.IsNullOrEmpty(riferimento.N_EM))
                        riferimentoText = "EM " + BALHelper.DecryptString(riferimento.N_EM, _model.masterKey);
                    else
                        riferimentoText = "TEMP " + riferimento.Progressivo;
                }

                if (emendamento.Rif_UIDEM.HasValue)
                {
                    //SUB EMENDAMENTO
                    var subRes = "";
                    if (!string.IsNullOrEmpty(emendamento.N_SUBEM))
                        subRes = "SUBEM " +
                                 BALHelper.DecryptString(emendamento.N_SUBEM, _model.masterKey);
                    else
                        subRes = "SUBEM TEMP " + emendamento.SubProgressivo;

                    subRes += $" all' {riferimentoText}";

                    return subRes;
                }

                if (!string.IsNullOrEmpty(emendamento.N_EM))
                    return "EM " +
                           BALHelper.DecryptString(emendamento.N_EM, _model.masterKey);

                return "TEMP " + emendamento.Progressivo;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void GetFascicolo(ref string path)
        {
            var dirFascicolo = $"{_stampa.UIDStampa}";
            path = Path.Combine(_model.CartellaLavoroTemporanea, dirFascicolo);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private async Task<Dictionary<Guid, BodyModel>> GeneraPDFEmendamenti(Dictionary<Guid, string> lista,
            string _pathTemp)
        {
            log.Info($"[GeneraPDFEmendamenti] INIZIO UIDStampa={_stampa.UIDStampa}, N.Emendamenti={lista.Count}");
            try
            {
                var listaPercorsi = new Dictionary<Guid, BodyModel>();
                var lockObject = new object();
                var counter = 0;

                listaPercorsi = lista.ToDictionary(em => em.Key, em => new BodyModel());
                var totalItems = lista.Count;

                foreach (var item in lista)
                    try
                    {
                        var dettagliCreaPDF = new BodyModel();

                        var nameFilePDF = $"{item.Key}.pdf";
                        var filePathComplete = Path.Combine(_pathTemp, nameFilePDF);

                        dettagliCreaPDF.Body = item.Value;
                        dettagliCreaPDF.Path = filePathComplete;

                        // Genera PDF e salva direttamente su disco
                        await _stamper.CreaPDFAsync(filePathComplete, dettagliCreaPDF.Body, "");

                        // Update the dictionary safely
                        lock (lockObject)
                        {
                            listaPercorsi[item.Key] = dettagliCreaPDF;
                            counter++;
                        }

                        // Update progress every 50 items or at the end
                        if (counter % 50 == 0 || counter == totalItems)
                            await LogStampa(_stampa.UIDStampa, $"Progresso {counter}/{totalItems}");
                    }
                    catch (Exception)
                    {
                        await LogStampa(_stampa.UIDStampa, $"Errore: {item.Key}");
                        throw;
                    }

                return listaPercorsi;
            }
            catch (Exception ex)
            {
                log.Error($"[GeneraPDFEmendamenti] ERRORE UIDStampa={_stampa.UIDStampa}", ex);
                throw;
            }
            finally
            {
                log.Info($"[GeneraPDFEmendamenti] FINE UIDStampa={_stampa.UIDStampa}");
            }
        }

        private void SpostaFascicolo(string _pathFascicolo, string _pathDestinazione)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(_pathDestinazione)))
                    Directory.CreateDirectory(Path.GetDirectoryName(_pathDestinazione));

                File.Move(_pathFascicolo, _pathDestinazione);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }

        private void PulisciCartellaLavoroTemporanea(string _pathTemp)
        {
            try
            {
                Directory.Delete(_pathTemp, true);
            }
            catch (Exception e)
            {
                //Log.Error($"ERRORE PULIZIA CARTELLA TEMPORANEA [{_pathTemp}]", e);
            }
        }
    }
}