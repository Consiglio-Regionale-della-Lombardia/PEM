using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PortaleRegione.Common;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using PortaleRegione.GestioneStampe;

namespace GeneraStampeJob
{
    public class Worker
    {
        private readonly LoginResponse _auth;
        private readonly ThreadWorkerModel _model;
        private readonly PdfStamper_IronPDF _stamper;
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

        public event EventHandler<bool> OnWorkerFinish;

        public async Task<bool> ExecuteAsync(StampaDto stampa)
        {
            var result = false;
            _stampa = stampa;
            var utenteRichiedente = await apiGateway.Persone.Get(_stampa.UIDUtenteRichiesta);
            try
            {
                await apiGateway.Stampe.AddInfo(_stampa.UIDStampa,
                    $"Inizio lavorazione - Tentativo {_stampa.Tentativi} di {_model.NumMaxTentativi}");
                if (_stampa.Tentativi < Convert.ToInt16(_model.NumMaxTentativi))
                {
                    //GetFascicolo
                    var path = string.Empty;
                    GetFascicolo(ref path);

                    if (_stampa.DASI)
                        await ExecuteStampaDASI(utenteRichiedente, path);
                    else
                        await ExecuteStampaEmendamenti(utenteRichiedente, path);

                    PulisciCartellaLavoroTemporanea(path);
                }
                else
                {
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
                        //Log.Debug($"[{_stampa.UIDStampa}] Invio mail EXCEPTION", e);
                        await apiGateway.Stampe.AddInfo(_stampa.UIDStampa,
                            $"Invio mail EXCEPTION ERRORE. Motivo: {e.Message}");
                    }
                }

                OnWorkerFinish?.Invoke(this, true);
                result = true;
            }
            catch (Exception ex)
            {
                OnWorkerFinish?.Invoke(this, false);

                //Log.Error($"[{_stampa.UIDStampa}] ERROR", ex);
                try
                {
                    await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, ex.StackTrace);
                    await apiGateway.Stampe.JobErrorStampa(_stampa.UIDStampa, ex.Message);
                    await apiGateway.Stampe.JobUnLockStampa(_stampa.UIDStampa);
                }
                catch (Exception)
                {
                    //Log.Error($"[{_stampa.UIDStampa}] ERROR", ex2);
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
                        //Log.Error($"[{_stampa.UIDStampa}] ERROR", exMail);
                        await apiGateway.Stampe.JobErrorStampa(_stampa.UIDStampa, exMail.Message);
                    }
                }
            }

            return result;
        }

        private async Task ExecuteStampaDASI(PersonaDto persona, string path)
        {
            //GetListEM
            var listaAtti = await GetListaAtti();

            if (_stampa.Notifica)
            {
                await PresentazioneDifferita(listaAtti, path);
            }
            else
                await StampaDASI(listaAtti, path, persona);
        }

        private async Task StampaDASI(List<AttoDASIDto> lista, string path, PersonaDto persona)
        {
            var docs = new List<byte[]>();
            var bodyCopertina = await apiGateway.DASI.GetCopertina(new ByQueryModel
            {
                Query = _stampa.Query
            });

            docs.Add(await _stamper.CreaPDFInMemory(bodyCopertina));

            await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, "Copertina generata");

            var attiGenerati = await GeneraPDFAtti(lista, path);

            docs.AddRange(attiGenerati.Where(item => item.Value.Content != null)
                .Select(i => (byte[])i.Value.Content));

            var countNonGenerati = attiGenerati.Count(item => item.Value.Content == null);
            await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, $"PDF NON GENERATI [{countNonGenerati}]");

            //Funzione che fascicola i PDF creati prima
            var nameFileTarget = $"Fascicolo_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
            var FilePathTarget = Path.Combine(path, nameFileTarget);
            _stamper.MergedPDF(FilePathTarget, docs);
            await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, "FASCICOLAZIONE COMPLETATA");
            var _pathStampe = Path.Combine(_model.CartellaLavoroStampe, nameFileTarget);
            //Log.Debug($"[{_stampa.UIDStampa}] Percorso stampe {_pathStampe}");
            SpostaFascicolo(FilePathTarget, _pathStampe);

            var URLDownload = Path.Combine(_model.UrlCLIENT, $"stampe/{_stampa.UIDStampa}");
            _stampa.PathFile = nameFileTarget;
            await apiGateway.Stampe.JobUpdateFileStampa(_stampa);

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
            if (resultInvio)
                await apiGateway.Stampe.JobSetInvioStampa(_stampa);
        }

        //private async Task<Dictionary<Guid, BodyModel>> GeneraPDFAtti(List<AttoDASIDto> lista, string path)
        //{
        //    var listaPercorsi = new Dictionary<Guid, BodyModel>();
        //    var counter = 1;
        //    listaPercorsi = lista.ToDictionary(atto => atto.UIDAtto, atto => new BodyModel());
        //    foreach (var item in lista)
        //    {
        //        var bodyPDF = await apiGateway.DASI.GetBody(item.UIDAtto, TemplateTypeEnum.PDF, true);
        //        var nameFilePDF =
        //            $"{item.Display}_{item.UIDAtto}_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";

        //        var FilePathComplete = string.IsNullOrEmpty(path) ? nameFilePDF : Path.Combine(path, nameFilePDF);

        //        var dettagliCreaPDF = new BodyModel
        //        {
        //            Path = FilePathComplete,
        //            Body = bodyPDF,
        //            Atto = item
        //        };
        //        var listAttachments = new List<string>();
        //        if (!string.IsNullOrEmpty(item.PATH_AllegatoGenerico))
        //        {
        //            var complete_path = string.Empty;

        //            var attachName = Path.GetFileName(item.PATH_AllegatoGenerico);
        //            await apiGateway.Stampe.AddInfo(_stampa.UIDStampa,
        //                $"PercorsoCompatibilitaDocumenti {_model.PercorsoCompatibilitaDocumenti} - {attachName}");
        //            complete_path = Path.Combine(
        //                _model.PercorsoCompatibilitaDocumenti,
        //                attachName);
        //            listAttachments.Add(complete_path);
        //        }

        //        var pdf = await _stamper.CreaPDFInMemory(dettagliCreaPDF.Body, item.Display, listAttachments);
        //        dettagliCreaPDF.Content = pdf;
        //        listaPercorsi[item.UIDAtto] = dettagliCreaPDF;
        //        await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, $"Progresso {counter}/{lista.Count}");
        //        counter++;
        //    }

        //    return listaPercorsi;
        //}

        private async Task<Dictionary<Guid, BodyModel>> GeneraPDFAtti(List<AttoDASIDto> lista, string path)
        {
            var listaPercorsi = new Dictionary<Guid, BodyModel>();

            try
            {
                listaPercorsi = lista.ToDictionary(atto => atto.UIDAtto, atto => new BodyModel());

                foreach (var item in lista)
                {
                    try
                    {
                        var bodyPDF = await apiGateway.DASI.GetBody(item.UIDAtto, TemplateTypeEnum.PDF, true);
                        var nameFilePDF = $"{item.Display}_{item.UIDAtto}_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
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
                            await apiGateway.Stampe.AddInfo(_stampa.UIDStampa,
                                $"PercorsoCompatibilitaDocumenti {_model.PercorsoCompatibilitaDocumenti} - {Path.GetFileName(item.PATH_AllegatoGenerico)}");
                        }

                        // Genera PDF e salva direttamente su disco
                        dettagliCreaPDF.Content = await _stamper.CreaPDFInMemory(dettagliCreaPDF.Body, item.Display,
                            listAttachments);

                        listaPercorsi[item.UIDAtto] = dettagliCreaPDF;
                        counter++;

                        await apiGateway.Stampe.AddInfo(_stampa.UIDStampa,
                            $"Progresso {counter}/{lista.Count}");
                    }
                    catch (Exception e)
                    {
                        await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, $"Errore: {item.Display}");
                        // Log error here if necessary
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error here if necessary
            }

            return listaPercorsi;
        }


        private async Task PresentazioneDifferita(List<AttoDASIDto> listaAtti, string path)
        {
            //STAMPA PDF PRESENTATO (BACKGROUND MODE)
            //Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Genera PDF Atto presentato");
            var item = listaAtti.First();
            var bodyPDF = await apiGateway.DASI.GetBody(item.UIDAtto, TemplateTypeEnum.PDF, true);
            var nameFilePDF = $"{item.Display}_{item.UIDAtto}_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";

            var content = await _stamper.CreaPDFInMemory(bodyPDF, nameFilePDF);
            
            //Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Salva Atti nel repository");

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
            await apiGateway.Stampe.JobUpdateFileStampa(_stampa);
        }

        private async Task<List<AttoDASIDto>> GetListaAtti()
        {
            if (_stampa.UIDAtto.HasValue)
            {
                await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, "Scarica atto..");

                var item = await apiGateway.DASI.Get(_stampa.UIDAtto.Value);
                await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, "Scarica atto.. OK");
                return new List<AttoDASIDto>
                {
                    item
                };
            }

            try
            {
                var resFromJson = new List<AttoDASIDto>();
                var listaAttiFromJson = JsonConvert.DeserializeObject<List<Guid>>(_stampa.Query);
                foreach (var guid in listaAttiFromJson)
                {
                    var item = await apiGateway.DASI.Get(guid);
                    resFromJson.Add(item);
                }

                return resFromJson;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var result = await apiGateway.Stampe.JobGetDASI(_stampa.Query, 1);
            var currentPaging = result.Paging;
            await Scarica_Log(currentPaging);
            var has_next = currentPaging.Has_Next;
            var lista = result.Results.ToList();
            while (has_next)
            {
                result =
                    await apiGateway.Stampe.JobGetDASI(_stampa.Query, result.Paging.Page + 1);
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

        private async Task ExecuteStampaEmendamenti(PersonaDto persona, string path)
        {
            try
            {
                //GetListEM
                var listaEMendamenti = await GetListaEM();

                if (_stampa.Notifica)
                    await DepositoDifferito(listaEMendamenti, path, persona);
                else
                    await Stampa(listaEMendamenti, path, persona);
            }
            catch (Exception e)
            {
                //Log.Error("StampaEmendamenti ERROR", e);
                throw e;
            }
        }

        private async Task Stampa(Dictionary<Guid, string> listaEMendamenti, string path,
            PersonaDto utenteRichiedente)
        {
            try
            {
                var atto = await apiGateway.Atti.Get(_stampa.UIDAtto.Value);
                var nameFileTarget = $"Fascicolo_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                var FilePathTarget = Path.Combine(path, nameFileTarget);
                var bodyCopertina = await apiGateway.Emendamento.GetCopertina(new CopertinaModel
                {
                    Atto = atto,
                    Totale = listaEMendamenti.Count,
                    Ordinamento = _stampa.Ordine.HasValue
                        ? (OrdinamentoEnum)_stampa.Ordine.Value
                        : OrdinamentoEnum.Presentazione
                });

                var cover = await _stamper.CreaPDFInMemory(bodyCopertina);
                _stamper.MergedPDF(FilePathTarget, cover);
                await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, "Copertina generata");

                var listaPdfEmendamentiGenerati =
                    await GeneraPDFEmendamenti(listaEMendamenti, path);

                _stamper.MergedPDFWithRetry(FilePathTarget,
                    listaPdfEmendamentiGenerati.Select(p => p.Value.Path).ToList());

                await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, "FASCICOLAZIONE COMPLETATA");
                var _pathStampe = Path.Combine(_model.CartellaLavoroStampe, nameFileTarget);
                //Log.Debug($"[{_stampa.UIDStampa}] Percorso stampe {_pathStampe}");
                SpostaFascicolo(FilePathTarget, _pathStampe);

                var URLDownload = Path.Combine(_model.UrlCLIENT, $"stampe/{_stampa.UIDStampa}");
                _stampa.PathFile = nameFileTarget;
                await apiGateway.Stampe.JobUpdateFileStampa(_stampa);
                if (_stampa.Scadenza.HasValue)
                {
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
                            await apiGateway.Stampe.JobSetInvioStampa(_stampa);
                    }
                    catch (Exception e)
                    {
                        //Log.Debug($"[{_stampa.UIDStampa}] Invio mail", e);
                        await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, $"Invio mail ERRORE. Motivo: {e.Message}");
                    }
                }
                else
                {
                    if (_stampa.Ordine.HasValue)
                    {
                        if ((OrdinamentoEnum)_stampa.Ordine.Value == OrdinamentoEnum.Presentazione)
                            atto.LinkFascicoloPresentazione = URLDownload;
                        if ((OrdinamentoEnum)_stampa.Ordine.Value == OrdinamentoEnum.Votazione)
                            atto.LinkFascicoloVotazione = URLDownload;
                        await apiGateway.Atti.ModificaFiles(atto);
                    }
                }

                try
                {
                    if (Directory.Exists(path)) Directory.Delete(path, true);
                }
                catch
                {
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }

        private async Task DepositoDifferito(Dictionary<Guid, string> listaEMendamenti, string path,
            PersonaDto utenteRichiedente)
        {
            //STAMPA PDF DEPOSITATO (BACKGROUND MODE)
            //Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Genera PDF Depositato");
            var em = listaEMendamenti.First();
            var emDto = await apiGateway.Emendamento.Get(em.Key);

            var nameFilePDF =
                $"{emDto.N_EM}_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";

            var content = await _stamper.CreaPDFInMemory(em.Value, nameFilePDF);

            //Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Salva EM nel repository");
            
            var atto = await apiGateway.Atti.Get(_stampa.UIDAtto.Value);
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
            await apiGateway.Stampe.JobUpdateFileStampa(_stampa);

            var bodyMail = "E' stato depositato l'EM in oggetto";

            if (atto.SEDUTE.Data_effettiva_inizio.HasValue)
            {
                var ruoloSegreteriaAssempblea =
                    await apiGateway.Persone.GetRuolo(RuoliIntEnum.Segreteria_Assemblea);
                //Log.Debug(
                //$"[{_stampa.UIDStampa}] BACKGROUND MODE - EM depositato il {listaEMendamenti.First().DataDeposito}");
                if (DateTime.Now >
                    atto.SEDUTE.Data_effettiva_inizio.Value)
                    //Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Seduta già iniziata");
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
                        await apiGateway.Stampe.AddInfo(_stampa.UIDStampa,
                            $"Invio mail a segreteria ERRORE. Motivo: {e.Message}");
                    }
            }

            //Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Seduta non è ancora iniziata");
            var email_destinatari = $"{utenteRichiedente.email};pem@consiglio.regione.lombardia.it";
            var email_destinatariGruppo = string.Empty;
            var email_destinatariGiunta = string.Empty;

            if (emDto.id_gruppo < 10000)
            {
                //Log.Debug(
                //$"[{_stampa.UIDStampa}] BACKGROUND MODE - Invio mail a Capo Gruppo e Segreteria Politica");
                var capoGruppo = await apiGateway.Persone.GetCapoGruppo(emDto.id_gruppo);
                var segreteriaPolitica = await apiGateway.Persone
                    .GetSegreteriaPolitica(emDto.id_gruppo, false, true);

                if (segreteriaPolitica.Any())
                    email_destinatariGruppo = segreteriaPolitica.Select(u => u.email)
                        .Aggregate((i, j) => $"{i};{j}");
                if (capoGruppo != null)
                    email_destinatariGruppo += $";{capoGruppo.email}";
            }
            else
            {
                //Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Invio mail a Giunta Regionale");
                var giuntaRegionale = await apiGateway.Persone.GetGiuntaRegionale();
                var segreteriaGiuntaRegionale = await apiGateway.Persone
                    .GetSegreteriaGiuntaRegionale(false, true);

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
                    await apiGateway.Stampe.JobSetInvioStampa(_stampa);
            }
            catch (Exception e)
            {
                //Log.Debug($"[{_stampa.UIDStampa}] Invio mail deposito", e);
                await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, $"Invio mail deposito ERRORE. Motivo: {e.Message}");
            }
        }

        private async Task<Dictionary<Guid, string>> GetListaEM()
        {
            if (_stampa.UIDEM.HasValue)
            {
                await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, "Scarica emendamento..");

                var emBody = await apiGateway.Emendamento.GetBody(_stampa.UIDEM.Value, TemplateTypeEnum.PDF);
                await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, "Scarica emendamento.. OK");
                return new Dictionary<Guid, string>()
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
                Console.WriteLine(e);
            }

            return new Dictionary<Guid, string>();
        }

        private async Task Scarica_Log(Paging currentPaging, Paging paging)
        {
            var partial = paging.Limit * paging.Page - (paging.Limit - paging.Entities);
            await apiGateway.Stampe.AddInfo(_stampa.UIDStampa,
                $"Scaricamento Blocco [{paging.Page}/{paging.Last_Page}] - [{partial}/{currentPaging.Total}]");
        }

        private async Task Scarica_Log(Paging paging)
        {
            var partial = paging.Limit * paging.Page - (paging.Limit - paging.Entities);
            await apiGateway.Stampe.AddInfo(_stampa.UIDStampa,
                $"Scaricamento Blocco [{paging.Page}/{paging.Last_Page}] - [{partial}/{paging.Total}]");
        }

        private void GetFascicolo(ref string path)
        {
            var dirFascicolo = $"Fascicolo_{_stampa.UIDStampa}_{DateTime.Now:ddMMyyyy_hhmmss}";
            path = Path.Combine(_model.CartellaLavoroTemporanea, dirFascicolo);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        //private async Task<Dictionary<Guid, BodyModel>> GeneraPDFEmendamenti(ICollection<EmendamentiDto> lista,
        //    string _pathTemp)
        //{
        //    var listaPercorsi = new Dictionary<Guid, BodyModel>();
        //    try
        //    {
        //        listaPercorsi = lista.ToDictionary(em => em.UIDEM, em => new BodyModel());
        //        var batchSize = 500;
        //        for (var i = 0; i < lista.Count; i += batchSize)
        //        {
        //            var batch = lista.Skip(i).Take(batchSize).ToList();
        //            foreach (var item in batch)
        //            {
        //                try
        //                {
        //                    counter++;
        //                    var dettagliCreaPDF = new BodyModel
        //                    {
        //                        EM = item
        //                    };

        //                    var FilePathComplete = string.Empty;
        //                    var nameFilePDF =
        //                        $"{item.N_EM.Replace(" ", "_").Replace("all'", "")}_{item.UIDEM}_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
        //                    FilePathComplete = Path.Combine(_pathTemp, nameFilePDF);
        //                    dettagliCreaPDF.Body = await apiGateway.Emendamento.GetBody(item.UIDEM, TemplateTypeEnum.PDF);
        //                    dettagliCreaPDF.Path = FilePathComplete;
        //                    var listAttachments = new List<string>();
        //                    if (!string.IsNullOrEmpty(item.PATH_AllegatoGenerico))
        //                    {
        //                        var complete_path = Path.Combine(
        //                            _model.PercorsoCompatibilitaDocumenti,
        //                            Path.GetFileName(item.PATH_AllegatoGenerico));
        //                        listAttachments.Add(complete_path);
        //                    }

        //                    if (!string.IsNullOrEmpty(item.PATH_AllegatoTecnico))
        //                    {
        //                        var complete_path = Path.Combine(
        //                            _model.PercorsoCompatibilitaDocumenti,
        //                            Path.GetFileName(item.PATH_AllegatoTecnico));
        //                        listAttachments.Add(complete_path);
        //                    }

        //                    dettagliCreaPDF.Attachments = listAttachments;

        //                    await _stamper.CreaPDFAsync(dettagliCreaPDF.Path, dettagliCreaPDF.Body, item.N_EM,
        //                        listAttachments);

        //                    listaPercorsi[item.UIDEM] = dettagliCreaPDF;

        //                    await apiGateway.Stampe.AddInfo(_stampa.UIDStampa,
        //                        $"Progresso {counter}/{lista.Count}");
        //                }
        //                catch (Exception e)
        //                {
        //                    await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, $"Errore: {item.N_EM}");
        //                    throw;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //Log.Error("GeneraPDFEmendamenti Error-->", ex);
        //    }

        //    return listaPercorsi;
        //}

        private async Task<Dictionary<Guid, BodyModel>> GeneraPDFEmendamenti(Dictionary<Guid, string> lista,
            string _pathTemp)
        {
            var listaPercorsi = new Dictionary<Guid, BodyModel>();
            var lockObject = new object();
            var counter = 0;
            listaPercorsi = lista.ToDictionary(em => em.Key, em => new BodyModel());
            foreach (var item in lista)
            {
                try
                {
                    var dettagliCreaPDF = new BodyModel();

                    var nameFilePDF =
                        $"{item.Key}_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                    var filePathComplete = Path.Combine(_pathTemp, nameFilePDF);

                    dettagliCreaPDF.Body = item.Value;
                    dettagliCreaPDF.Path = filePathComplete;

                    //var listAttachments = new List<string>();
                    //if (!string.IsNullOrEmpty(item.PATH_AllegatoGenerico))
                    //{
                    //    var completePath = Path.Combine(_model.PercorsoCompatibilitaDocumenti,
                    //        Path.GetFileName(item.PATH_AllegatoGenerico));
                    //    listAttachments.Add(completePath);
                    //}

                    //if (!string.IsNullOrEmpty(item.PATH_AllegatoTecnico))
                    //{
                    //    var completePath = Path.Combine(_model.PercorsoCompatibilitaDocumenti,
                    //        Path.GetFileName(item.PATH_AllegatoTecnico));
                    //    listAttachments.Add(completePath);
                    //}

                    //dettagliCreaPDF.Attachments = listAttachments;

                    // Genera PDF e salva direttamente su disco
                    await _stamper.CreaPDFAsync(filePathComplete, dettagliCreaPDF.Body, "");

                    // Update the dictionary safely
                    lock (lockObject)
                    {
                        listaPercorsi[item.Key] = dettagliCreaPDF;
                        counter++;
                    }

                    await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, $"Progresso {counter}/{lista.Count}");
                }
                catch (Exception)
                {
                    await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, $"Errore: {item.Key}");
                    throw;
                }
            }

            return listaPercorsi;
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