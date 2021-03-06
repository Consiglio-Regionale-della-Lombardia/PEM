using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using PortaleRegione.GestioneStampe;
using PortaleRegione.Logger;

namespace GeneraStampeJob
{
    public class Worker
    {
        private readonly ThreadWorkerModel _model;
        private StampaDto _stampa;

        public Worker(ref LoginResponse auth, ref ThreadWorkerModel model)
        {
            _model = model;
            BaseGateway.apiUrl = _model.UrlAPI;
            BaseGateway.access_token = auth.jwt;
        }

        public event EventHandler<bool> OnWorkerFinish;

        public async Task ExecuteAsync(StampaDto stampa)
        {
            _stampa = stampa;
            var utenteRichiedente = await PersoneGate.Get(_stampa.UIDUtenteRichiesta);
            try
            {
                await StampeGate.AddInfo(_stampa.UIDStampa,
                    $"Inizio lavorazione - Tentativo {_stampa.Tentativi} di {_model.NumMaxTentativi}");
                if (_stampa.Tentativi < Convert.ToInt16(_model.NumMaxTentativi))
                {
                    //GetFascicolo
                    var path = string.Empty;
                    GetFascicolo(ref path);
                    //

                    //GetListEM
                    var listaEMendamenti = await GetListaEM();
                    //

                    if (_stampa.NotificaDepositoEM)
                        await DepositoDifferito(listaEMendamenti, path, utenteRichiedente);
                    else
                        await Stampa(listaEMendamenti, path, utenteRichiedente);

                    PulisciCartellaLavoroTemporanea(path);
                }
                else
                {
                    await BaseGateway.SendMail(new MailModel
                    {
                        DA = _model.EmailFrom,
                        A = utenteRichiedente.email,
                        OGGETTO = "Errore generazione stampa",
                        MESSAGGIO = $"ID stampa: [{_stampa.UIDStampa}], per l'atto: [{_stampa.UIDAtto}]"
                    });
                }

                OnWorkerFinish?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                OnWorkerFinish?.Invoke(this, false);

                Log.Error($"[{_stampa.UIDStampa}] ERROR", ex);
                try
                {
                    await StampeGate.JobErrorStampa(_stampa.UIDStampa, ex.Message);
                    await StampeGate.JobUnLockStampa(_stampa.UIDStampa);
                }
                catch (Exception ex2)
                {
                    Log.Error($"[{_stampa.UIDStampa}] ERROR", ex2);
                    try
                    {
                        await BaseGateway.SendMail(new MailModel
                        {
                            DA = _model.EmailFrom,
                            A = utenteRichiedente.email,
                            OGGETTO = "Errore generazione fascicolo",
                            MESSAGGIO = ex.Message
                        });
                    }
                    catch (Exception exMail)
                    {
                        Log.Error($"[{_stampa.UIDStampa}] ERROR", exMail);
                        await StampeGate.JobErrorStampa(_stampa.UIDStampa, exMail.Message);
                    }
                }
            }
        }

        private async Task Stampa(IEnumerable<EmendamentiDto> listaEMendamenti, string path,
            PersonaDto utenteRichiedente)
        {
            try
            {
                var bodyCopertina = await EMGate.GetCopertina(new CopertinaModel
                {
                    Atto = _stampa.ATTI,
                    TotaleEM = listaEMendamenti.Count(),
                    Ordinamento = _stampa.Ordine.HasValue
                        ? (OrdinamentoEnum) _stampa.Ordine.Value
                        : OrdinamentoEnum.Presentazione
                });

                var nameFilePDFCopertina = $"COPERTINAEM_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                var DirCopertina = Path.Combine(path, nameFilePDFCopertina);
                PdfStamper.CreaPDFCopertina(bodyCopertina, DirCopertina);
                await StampeGate.AddInfo(_stampa.UIDStampa, "Copertina generata");
                var listaPdfEmendamentiGenerati =
                    await GeneraPDFEmendamenti(listaEMendamenti, _stampa.Da, _stampa.A, path);

                var countNonGenerati = listaPdfEmendamentiGenerati.Count(item => !File.Exists(item.Value.Path));
                await StampeGate.AddInfo(_stampa.UIDStampa, $"PDF NON GENERATI [{countNonGenerati}]");

                //Funzione che fascicola i PDF creati prima
                var nameFileTarget = $"Fascicolo_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                var FilePathTarget = Path.Combine(path, nameFileTarget);
                PdfStamper.CreateMergedPDF(FilePathTarget, DirCopertina,
                    listaPdfEmendamentiGenerati.ToDictionary(item => item.Key, item => item.Value.Path));
                await StampeGate.AddInfo(_stampa.UIDStampa, "FASCICOLAZIONE COMPLETATA");
                var _pathStampe = Path.Combine(_model.CartellaLavoroStampe, nameFileTarget);
                Log.Debug($"[{_stampa.UIDStampa}] Percorso stampe {_pathStampe}");
                SpostaFascicolo(FilePathTarget, _pathStampe);

                var URLDownload = Path.Combine(_model.UrlAPI, $"stampe?id={_stampa.UIDStampa}");
                _stampa.PathFile = nameFileTarget;
                await StampeGate.JobUpdateFileStampa(_stampa);
                if (_stampa.Scadenza.HasValue)
                {
                    var resultInvio = await BaseGateway.SendMail(new MailModel
                    {
                        DA = _model.EmailFrom,
                        A = utenteRichiedente.email,
                        OGGETTO = "Link download fascicolo",
                        MESSAGGIO = URLDownload
                    });
                    if (resultInvio)
                        await StampeGate.JobSetInvioStampa(_stampa);
                }
                else
                {
                    if (_stampa.Ordine.HasValue)
                    {
                        if ((OrdinamentoEnum) _stampa.Ordine.Value == OrdinamentoEnum.Presentazione)
                            _stampa.ATTI.LinkFascicoloPresentazione = URLDownload;
                        if ((OrdinamentoEnum) _stampa.Ordine.Value == OrdinamentoEnum.Votazione)
                            _stampa.ATTI.LinkFascicoloVotazione = URLDownload;
                        await AttiGate.ModificaFiles(_stampa.ATTI);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }

        private async Task DepositoDifferito(IEnumerable<EmendamentiDto> listaEMendamenti, string path,
            PersonaDto utenteRichiedente)
        {
            //STAMPA PDF DEPOSITATO (BACKGROUND MODE)
            Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Genera PDF Depositato");

            var listaPdfEmendamentiGenerati =
                await GeneraPDFEmendamenti(listaEMendamenti, _stampa.Da, _stampa.A, path);

            Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Salva EM nel repository");

            var em = listaEMendamenti.First();
            var dirSeduta = $"Seduta_{_stampa.ATTI.SEDUTE.Data_seduta:yyyyMMdd}";
            var dirPDL = Regex.Replace($"{_stampa.ATTI.TIPI_ATTO.Tipo_Atto} {_stampa.ATTI.NAtto}", @"[^0-9a-zA-Z]+",
                "_");
            var pathRepository = $"{_model.RootRepository}/{dirSeduta}/{dirPDL}";

            if (!Directory.Exists(pathRepository))
                Directory.CreateDirectory(pathRepository);

            var destinazioneDeposito = Path.Combine(pathRepository,
                Path.GetFileName(listaPdfEmendamentiGenerati.First().Value.Path));
            SpostaFascicolo(listaPdfEmendamentiGenerati.First().Value.Path, destinazioneDeposito);
            _stampa.PathFile = Path.Combine($"{dirSeduta}/{dirPDL}",
                Path.GetFileName(listaPdfEmendamentiGenerati.First().Value.Path));
            _stampa.UIDEM = em.UIDEM;
            await StampeGate.JobUpdateFileStampa(_stampa);

            var bodyMail = await EMGate.GetBody(em.UIDEM, TemplateTypeEnum.MAIL, true);

            if (_stampa.ATTI.SEDUTE.Data_effettiva_inizio.HasValue)
            {
                var ruoloSegreteriaAssempblea =
                    await PersoneGate.GetRuolo(RuoliIntEnum.Segreteria_Assemblea);
                Log.Debug(
                    $"[{_stampa.UIDStampa}] BACKGROUND MODE - EM depositato il {listaEMendamenti.First().DataDeposito}");
                if (Convert.ToDateTime(listaEMendamenti.First().DataDeposito) >
                    _stampa.ATTI.SEDUTE.Data_effettiva_inizio.Value)
                {
                    Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Seduta già iniziata");

                    await BaseGateway.SendMail(new MailModel
                    {
                        DA = _model.EmailFrom,
                        A = $"{ruoloSegreteriaAssempblea.ADGroupShort}@consiglio.regione.lombardia.it",
                        OGGETTO =
                            $"[TRATTAZIONE AULA] {_stampa.ATTI.TIPI_ATTO.Tipo_Atto} {_stampa.ATTI.NAtto}: Depositato {listaEMendamenti.First().DisplayTitle}",
                        MESSAGGIO = bodyMail
                    });
                }
            }
            else
            {
                Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Seduta non è ancora iniziata");
            }

            var email_destinatari = $"{utenteRichiedente.email};pem@consiglio.regione.lombardia.it";
            var email_destinatariGruppo = string.Empty;
            var email_destinatariGiunta = string.Empty;

            if (em.id_gruppo > 10000)
            {
                Log.Debug(
                    $"[{_stampa.UIDStampa}] BACKGROUND MODE - Invio mail a Capo Gruppo e Segreteria Politica");
                var capoGruppo = await PersoneGate.GetCapoGruppo(em.id_gruppo);
                var segreteriaPolitica = await PersoneGate.GetSegreteriaPolitica(em.id_gruppo, false, true);

                if (segreteriaPolitica.Any())
                    email_destinatariGruppo = segreteriaPolitica.Select(u => u.email)
                        .Aggregate((i, j) => $"{i};{j}");
                if (capoGruppo != null)
                    email_destinatariGruppo += $";{capoGruppo.email}";
            }
            else
            {
                Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Invio mail a Giunta Regionale");
                var giuntaRegionale = await PersoneGate.GetGiuntaRegionale();
                var segreteriaGiuntaRegionale = await PersoneGate.GetSegreteriaGiuntaRegionale(false, true);

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

            var resultInvio = await BaseGateway.SendMail(new MailModel
            {
                DA = _model.EmailFrom,
                A = email_destinatari,
                OGGETTO =
                    $"{_stampa.ATTI.TIPI_ATTO.Tipo_Atto} {_stampa.ATTI.NAtto}: Depositato {listaEMendamenti.First().DisplayTitle}",
                MESSAGGIO = bodyMail,
                pathAttachment = destinazioneDeposito,
                IsDeposito = true
            });

            if (resultInvio)
                await StampeGate.JobSetInvioStampa(_stampa);
        }

        private async Task<IEnumerable<EmendamentiDto>> GetListaEM()
        {
            if (_stampa.UIDEM.HasValue)
            {
                await StampeGate.AddInfo(_stampa.UIDStampa, "Scarica emendamento..");

                var em = await EMGate.Get(_stampa.UIDEM.Value);
                await StampeGate.AddInfo(_stampa.UIDStampa, "Scarica emendamento.. OK");
                return new List<EmendamentiDto>
                {
                    em
                };
            }

            var resultEmendamenti = await StampeGate.JobGetEmendamenti(_stampa.QueryEM, 1);
            await ScaricaEM_Log(resultEmendamenti.Paging);
            var has_next = resultEmendamenti.Paging.Has_Next;
            var listaEMendamenti = resultEmendamenti.Results.ToList();
            while (has_next)
            {
                resultEmendamenti =
                    await StampeGate.JobGetEmendamenti(_stampa.QueryEM, resultEmendamenti.Paging.Page + 1);
                has_next = resultEmendamenti.Paging.Has_Next;
                await ScaricaEM_Log(resultEmendamenti.Paging);
                listaEMendamenti.AddRange(resultEmendamenti.Results);
            }

            return listaEMendamenti;
        }

        private async Task ScaricaEM_Log(Paging paging)
        {
            var partial_em = paging.Limit * paging.Page - (paging.Limit - paging.Entities);
            await StampeGate.AddInfo(_stampa.UIDStampa,
                $"Scarica emendamenti.. Blocco [{paging.Page}/{paging.Last_Page}] - EM [{partial_em}/{paging.Total}]");
        }

        private void GetFascicolo(ref string path)
        {
            var dirFascicolo = $"Fascicolo_{_stampa.UIDStampa}_{DateTime.Now:ddMMyyyy_hhmmss}";
            path = Path.Combine(_model.CartellaLavoroTemporanea, dirFascicolo);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private async Task<Dictionary<Guid, BodyModel>> GeneraPDFEmendamenti(IEnumerable<EmendamentiDto> Emendamenti,
            int Da, int A,
            string _pathTemp)
        {
            var listaPercorsiEM = new Dictionary<Guid, BodyModel>();
            try
            {
                listaPercorsiEM = Emendamenti.ToDictionary(em => em.UIDEM, em => new BodyModel());

                if (Da != 0 && A != 0)
                {
                    Log.Debug($"Stampo solo DA [{Da}], A [{A}]");
                    var j = 1;
                    var k = Da;
                    var k_end = A;
                    foreach (var item in Emendamenti)
                    {
                        if (j >= k)
                        {
                            if (j > k_end)
                                break;

                            var bodyPDF = await EMGate.GetBody(item.UIDEM, TemplateTypeEnum.PDF);
                            var nameFilePDF =
                                $"{item.DisplayTitle.Replace(" ", "_").Replace("all'", "")}_{item.UIDEM}_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                            var FilePathComplete = Path.Combine(_pathTemp, nameFilePDF);

                            var dettagliCreaPDF = new BodyModel
                            {
                                Path = FilePathComplete,
                                Body = bodyPDF,
                                EM = item
                            };
                            listaPercorsiEM[item.UIDEM] = dettagliCreaPDF;
                            await CreaPDF(dettagliCreaPDF, Emendamenti);
                        }

                        j++;
                    }
                }
                else
                {
                    foreach (var item in Emendamenti)
                    {
                        var bodyPDF = await EMGate.GetBody(item.UIDEM, TemplateTypeEnum.PDF);
                        var nameFilePDF =
                            $"{item.DisplayTitle.Replace(" ", "_").Replace("all'", "")}_{item.UIDEM}_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                        var FilePathComplete = Path.Combine(_pathTemp, nameFilePDF);

                        var dettagliCreaPDF = new BodyModel
                        {
                            Path = FilePathComplete,
                            Body = bodyPDF,
                            EM = item
                        };
                        listaPercorsiEM[item.UIDEM] = dettagliCreaPDF;
                        await CreaPDF(dettagliCreaPDF, Emendamenti);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("GeneraPDFEmendamenti Error-->", ex);
            }

            return listaPercorsiEM;
        }

        private async Task CreaPDF(BodyModel item, IEnumerable<EmendamentiDto> Emendamenti)
        {
            PdfStamper.CreaPDF(item.Body, item.Path, item.EM, _model.UrlCLIENT);
            var dirInfo = new DirectoryInfo(Path.GetDirectoryName(item.Path));
            var files = dirInfo.GetFiles().Where(f => !f.Name.ToLower().Contains("copertina"));
            await StampeGate.AddInfo(_stampa.UIDStampa, $"Progresso {files.Count()}/{Emendamenti.Count()}");
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