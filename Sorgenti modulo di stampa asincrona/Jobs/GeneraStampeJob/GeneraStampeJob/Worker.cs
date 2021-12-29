using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using PortaleRegione.Gateway;
using PortaleRegione.GestioneStampe;
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GeneraStampeJob
{
    public class Worker
    {
        private readonly LoginResponse _auth;
        private readonly ThreadWorkerModel _model;
        private StampaDto _stampa;
        private ApiGateway apiGateway;

        public Worker(LoginResponse auth, ref ThreadWorkerModel model)
        {
            _auth = auth;
            _model = model;
            BaseGateway.apiUrl = _model.UrlAPI;
            apiGateway = new ApiGateway(_auth.jwt);
        }

        public event EventHandler<bool> OnWorkerFinish;

        public async Task ExecuteAsync(StampaDto stampa)
        {
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
                    try
                    {
                        await BaseGateway.SendMail(new MailModel
                        {
                            DA = _model.EmailFrom,
                            A = utenteRichiedente.email,
                            OGGETTO = "Errore generazione stampa",
                            MESSAGGIO = $"ID stampa: [{_stampa.UIDStampa}], per l'atto: [{_stampa.UIDAtto}]"
                        },
                            _auth.jwt);
                    }
                    catch (Exception e)
                    {
                        Log.Debug($"[{_stampa.UIDStampa}] Invio mail EXCEPTION", e);
                        await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, $"Invio mail EXCEPTION ERRORE. Motivo: {e.Message}");
                    }
                }

                OnWorkerFinish?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                OnWorkerFinish?.Invoke(this, false);

                Log.Error($"[{_stampa.UIDStampa}] ERROR", ex);
                try
                {
                    await apiGateway.Stampe.JobErrorStampa(_stampa.UIDStampa, ex.Message);
                    await apiGateway.Stampe.JobUnLockStampa(_stampa.UIDStampa);
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
                        },
                            _auth.jwt);
                    }
                    catch (Exception exMail)
                    {
                        Log.Error($"[{_stampa.UIDStampa}] ERROR", exMail);
                        await apiGateway.Stampe.JobErrorStampa(_stampa.UIDStampa, exMail.Message);
                    }
                }
            }
        }

        private async Task Stampa(List<EmendamentiDto> listaEMendamenti, string path,
            PersonaDto utenteRichiedente)
        {
            try
            {
                var atto = await apiGateway.Atti.Get(_stampa.UIDAtto);
                if (_stampa.Da > 0 && _stampa.A > 0)
                {
                    listaEMendamenti = listaEMendamenti.GetRange(_stampa.Da - 1, _stampa.A - (_stampa.Da - 1));
                }

                var bodyCopertina = await apiGateway.Emendamento.GetCopertina(new CopertinaModel
                {
                    Atto = atto,
                    TotaleEM = listaEMendamenti.Count,
                    Ordinamento = _stampa.Ordine.HasValue
                        ? (OrdinamentoEnum)_stampa.Ordine.Value
                        : OrdinamentoEnum.Presentazione
                });

                var nameFilePDFCopertina = $"COPERTINAEM_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                var DirCopertina = Path.Combine(path, nameFilePDFCopertina);
                PdfStamper.CreaPDFCopertina(bodyCopertina, DirCopertina);
                await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, "Copertina generata");
                var listaPdfEmendamentiGenerati =
                    await GeneraPDFEmendamenti(listaEMendamenti, path);

                var countNonGenerati = listaPdfEmendamentiGenerati.Count(item => !File.Exists(item.Value.Path));
                await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, $"PDF NON GENERATI [{countNonGenerati}]");

                //Funzione che fascicola i PDF creati prima
                var nameFileTarget = $"Fascicolo_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                var FilePathTarget = Path.Combine(path, nameFileTarget);
                PdfStamper.CreateMergedPDF(FilePathTarget, DirCopertina,
                    listaPdfEmendamentiGenerati.ToDictionary(item => item.Key, item => item.Value.Path));
                await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, "FASCICOLAZIONE COMPLETATA");
                var _pathStampe = Path.Combine(_model.CartellaLavoroStampe, nameFileTarget);
                Log.Debug($"[{_stampa.UIDStampa}] Percorso stampe {_pathStampe}");
                SpostaFascicolo(FilePathTarget, _pathStampe);

                var URLDownload = Path.Combine(_model.UrlCLIENT, $"stampe/{_stampa.UIDStampa}");
                _stampa.PathFile = nameFileTarget;
                await apiGateway.Stampe.JobUpdateFileStampa(_stampa);
                if (_stampa.Scadenza.HasValue)
                {

                    try
                    {
                        var bodyMail = $"Gentile {utenteRichiedente.DisplayName},<br>la stampa richiesta sulla piattaforma PEM è disponibile al seguente link:<br><a href='{URLDownload}' target='_blank'>{URLDownload}</a>";
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
                        Log.Debug($"[{_stampa.UIDStampa}] Invio mail", e);
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }

        private async Task DepositoDifferito(List<EmendamentiDto> listaEMendamenti, string path,
            PersonaDto utenteRichiedente)
        {
            //STAMPA PDF DEPOSITATO (BACKGROUND MODE)
            Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Genera PDF Depositato");

            var listaPdfEmendamentiGenerati =
                await GeneraPDFEmendamenti(listaEMendamenti, path);

            Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Salva EM nel repository");

            var em = listaEMendamenti.First();
            var atto = await apiGateway.Atti.Get(_stampa.UIDAtto);
            var dirSeduta = $"Seduta_{atto.SEDUTE.Data_seduta:yyyyMMdd}";
            var dirPDL = Regex.Replace($"{atto.TIPI_ATTO.Tipo_Atto} {atto.NAtto}", @"[^0-9a-zA-Z]+",
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
            await apiGateway.Stampe.JobUpdateFileStampa(_stampa);

            var bodyMail = await apiGateway.Emendamento.GetBody(em.UIDEM, TemplateTypeEnum.PDF, true);

            if (atto.SEDUTE.Data_effettiva_inizio.HasValue)
            {
                var ruoloSegreteriaAssempblea =
                    await apiGateway.Persone.GetRuolo(RuoliIntEnum.Segreteria_Assemblea);
                Log.Debug(
                    $"[{_stampa.UIDStampa}] BACKGROUND MODE - EM depositato il {listaEMendamenti.First().DataDeposito}");
                if (Convert.ToDateTime(listaEMendamenti.First().DataDeposito) >
                    atto.SEDUTE.Data_effettiva_inizio.Value)
                {
                    Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Seduta già iniziata");

                    try
                    {
                        await BaseGateway.SendMail(new MailModel
                        {
                            DA = _model.EmailFrom,
                            A = $"{ruoloSegreteriaAssempblea.ADGroupShort}@consiglio.regione.lombardia.it",
                            OGGETTO =
                                    $"[TRATTAZIONE AULA] {atto.TIPI_ATTO.Tipo_Atto} {atto.NAtto}: Depositato {listaEMendamenti.First().N_EM}",
                            MESSAGGIO = bodyMail
                        },
                            _auth.jwt);
                    }
                    catch (Exception e)
                    {
                        Log.Debug($"[{_stampa.UIDStampa}] Invio mail segreteria", e);
                        await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, $"Invio mail a segreteria ERRORE. Motivo: {e.Message}");
                    }
                }
            }
            else
            {
                Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Seduta non è ancora iniziata");
            }

            var email_destinatari = $"{utenteRichiedente.email};pem@consiglio.regione.lombardia.it";
            var email_destinatariGruppo = string.Empty;
            var email_destinatariGiunta = string.Empty;

            if (em.id_gruppo < 10000)
            {
                Log.Debug(
                    $"[{_stampa.UIDStampa}] BACKGROUND MODE - Invio mail a Capo Gruppo e Segreteria Politica");
                var capoGruppo = await apiGateway.Persone.GetCapoGruppo(em.id_gruppo);
                var segreteriaPolitica = await apiGateway.Persone
                    .GetSegreteriaPolitica(em.id_gruppo, false, true);

                if (segreteriaPolitica.Any())
                    email_destinatariGruppo = segreteriaPolitica.Select(u => u.email)
                        .Aggregate((i, j) => $"{i};{j}");
                if (capoGruppo != null)
                    email_destinatariGruppo += $";{capoGruppo.email}";
            }
            else
            {
                Log.Debug($"[{_stampa.UIDStampa}] BACKGROUND MODE - Invio mail a Giunta Regionale");
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
                            $"{atto.TIPI_ATTO.Tipo_Atto} {atto.NAtto}: Depositato {listaEMendamenti.First().N_EM}",
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
                Log.Debug($"[{_stampa.UIDStampa}] Invio mail deposito", e);
                await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, $"Invio mail deposito ERRORE. Motivo: {e.Message}");
            }
        }

        private async Task<List<EmendamentiDto>> GetListaEM()
        {
            if (_stampa.UIDEM.HasValue)
            {
                await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, "Scarica emendamento..");

                var em = await apiGateway.Emendamento.Get(_stampa.UIDEM.Value);
                await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, "Scarica emendamento.. OK");
                return new List<EmendamentiDto>
                {
                    em
                };
            }

            var resultEmendamenti = await apiGateway.Stampe.JobGetEmendamenti(_stampa.QueryEM, 1);
            var currentPaging = resultEmendamenti.Paging;
            await ScaricaEM_Log(currentPaging);
            var has_next = currentPaging.Has_Next;
            var listaEMendamenti = resultEmendamenti.Results.ToList();
            while (has_next)
            {
                resultEmendamenti =
                    await apiGateway.Stampe.JobGetEmendamenti(_stampa.QueryEM, resultEmendamenti.Paging.Page + 1);
                await ScaricaEM_Log(currentPaging, resultEmendamenti.Paging);
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

        private async Task ScaricaEM_Log(Paging currentPaging, Paging paging)
        {
            var partial_em = paging.Limit * paging.Page - (paging.Limit - paging.Entities);
            await apiGateway.Stampe.AddInfo(_stampa.UIDStampa,
                $"Scarica emendamenti.. Blocco [{paging.Page}/{paging.Last_Page}] - EM [{partial_em}/{currentPaging.Total}]");

        }

        private async Task ScaricaEM_Log(Paging paging)
        {
            var partial_em = paging.Limit * paging.Page - (paging.Limit - paging.Entities);
            await apiGateway.Stampe.AddInfo(_stampa.UIDStampa,
                $"Scarica emendamenti.. Blocco [{paging.Page}/{paging.Last_Page}] - EM [{partial_em}/{paging.Total}]");
        }

        private void GetFascicolo(ref string path)
        {
            var dirFascicolo = $"Fascicolo_{_stampa.UIDStampa}_{DateTime.Now:ddMMyyyy_hhmmss}";
            path = Path.Combine(_model.CartellaLavoroTemporanea, dirFascicolo);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private async Task<Dictionary<Guid, BodyModel>> GeneraPDFEmendamenti(IEnumerable<EmendamentiDto> Emendamenti, string _pathTemp)
        {
            var listaPercorsiEM = new Dictionary<Guid, BodyModel>();
            try
            {
                var listaEmendamenti = Emendamenti.ToList();

                listaPercorsiEM = listaEmendamenti.ToDictionary(em => em.UIDEM, em => new BodyModel());
                foreach (var item in listaEmendamenti)
                {
                    var bodyPDF = await apiGateway.Emendamento.GetBody(item.UIDEM, TemplateTypeEnum.PDF);
                    var nameFilePDF =
                        $"{item.N_EM.Replace(" ", "_").Replace("all'", "")}_{item.UIDEM}_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                    var FilePathComplete = Path.Combine(_pathTemp, nameFilePDF);

                    var dettagliCreaPDF = new BodyModel
                    {
                        Path = FilePathComplete,
                        Body = bodyPDF,
                        EM = item
                    };
                    listaPercorsiEM[item.UIDEM] = dettagliCreaPDF;
                    await CreaPDF(dettagliCreaPDF, listaPercorsiEM.Count);
                }
            }
            catch (Exception ex)
            {
                Log.Error("GeneraPDFEmendamenti Error-->", ex);
            }

            return listaPercorsiEM;
        }

        private async Task CreaPDF(BodyModel item, long total)
        {
            PdfStamper.CreaPDF(item.Body, item.Path, item.EM, Path.Combine(_model.UrlCLIENT, $"public/em?id={item.EM.UID_QRCode}"));
            var dirInfo = new DirectoryInfo(Path.GetDirectoryName(item.Path));
            var files = dirInfo.GetFiles().Where(f => !f.Name.ToLower().Contains("copertina"));
            await apiGateway.Stampe.AddInfo(_stampa.UIDStampa, $"Progresso {files.Count()}/{total}");
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