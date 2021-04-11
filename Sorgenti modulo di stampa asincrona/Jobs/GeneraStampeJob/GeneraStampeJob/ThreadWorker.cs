using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.Gateway;
using PortaleRegione.GestioneStampe;
using PortaleRegione.Logger;

namespace GeneraStampeJob
{
    public class ThreadWorker
    {
        public ThreadWorker(StampaDto stampa, ThreadWorkerModel model)
        {
            this.stampa = stampa;
            this.model = model;
            t = new Thread(DoWork);
            t.Start();
        }

        private async void DoWork()
        {
            var utenteRichiedente = await ApiGateway.GetPersona(stampa.UIDUtenteRichiesta);
            try
            {
                Log.Debug(
                    $"[{stampa.UIDStampa}] Utente Richiedente [{utenteRichiedente.DisplayName}], EMAIL [{utenteRichiedente.email}]");
                if (stampa.Tentativi < Convert.ToInt16(model.NumMaxTentativi))
                {
                    var dirFascicolo = $"Fascicolo_{stampa.UIDStampa}_{DateTime.Now:ddMMyyyy_hhmmss}";
                    var _pathTemp = Path.Combine(model.CartellaLavoroTemporanea, dirFascicolo);
                    if (!Directory.Exists(_pathTemp))
                        Directory.CreateDirectory(_pathTemp);

                    var resultEmendamenti = await ApiGateway.JobGetEmendamenti(stampa.QueryEM, 1);
                    var has_next = resultEmendamenti.Paging.Has_Next;
                    var listaEMendamenti = resultEmendamenti.Results.ToList();
                    while (has_next)
                    {
                        resultEmendamenti =
                            await ApiGateway.JobGetEmendamenti(stampa.QueryEM, resultEmendamenti.Paging.Page + 1);
                        has_next = resultEmendamenti.Paging.Has_Next;
                        listaEMendamenti.AddRange(resultEmendamenti.Results);
                    }

                    Log.Debug($"[{stampa.UIDStampa}] Totale EM [{listaEMendamenti.Count}]");

                    var atto = stampa.ATTI;

                    if (stampa.NotificaDepositoEM)
                    {
                        //STAMPA PDF DEPOSITATO (BACKGROUND MODE)
                        Log.Debug($"[{stampa.UIDStampa}] BACKGROUND MODE - Genera PDF Depositato");

                        var listaPdfEmendamentiGenerati =
                            await GeneraPDFEmendamenti(listaEMendamenti, stampa.Da, stampa.A, _pathTemp);

                        Log.Debug($"[{stampa.UIDStampa}] BACKGROUND MODE - Salva EM nel repository");

                        var em = listaEMendamenti.First();
                        var dirSeduta = $"Seduta_{atto.SEDUTE.Data_seduta:yyyyMMdd}";
                        var dirPDL = Regex.Replace($"{atto.TIPI_ATTO.Tipo_Atto} {atto.NAtto}", @"[^0-9a-zA-Z]+", "_");
                        var pathRepository = $"{model.RootRepository}/{dirSeduta}/{dirPDL}";

                        if (!Directory.Exists(pathRepository))
                            Directory.CreateDirectory(pathRepository);

                        var destinazioneDeposito = Path.Combine(pathRepository,
                            Path.GetFileName(listaPdfEmendamentiGenerati.First().Value.Path));
                        SpostaFascicolo(listaPdfEmendamentiGenerati.First().Value.Path, destinazioneDeposito);
                        stampa.PathFile = Path.Combine($"{dirSeduta}/{dirPDL}",
                            Path.GetFileName(listaPdfEmendamentiGenerati.First().Value.Path));
                        stampa.UIDEM = em.UIDEM;
                        await ApiGateway.JobUpdateFileStampa(stampa);

                        var bodyMail = await ApiGateway.GetBodyEM(em.UIDEM, TemplateTypeEnum.MAIL, true);

                        if (atto.SEDUTE.Data_effettiva_inizio.HasValue)
                        {
                            var ruoloSegreteriaAssempblea =
                                await ApiGateway.GetRuolo(RuoliIntEnum.Segreteria_Assemblea);
                            Log.Debug(
                                $"[{stampa.UIDStampa}] BACKGROUND MODE - EM depositato il {listaEMendamenti.First().DataDeposito}");
                            if (Convert.ToDateTime(listaEMendamenti.First().DataDeposito) >
                                atto.SEDUTE.Data_effettiva_inizio.Value)
                            {
                                Log.Debug($"[{stampa.UIDStampa}] BACKGROUND MODE - Seduta già iniziata");

                                await ApiGateway.SendMail(new MailModel
                                {
                                    DA = model.EmailFrom,
                                    A = $"{ruoloSegreteriaAssempblea.ADGroupShort}@consiglio.regione.lombardia.it",
                                    OGGETTO =
                                        $"[TRATTAZIONE AULA] {atto.TIPI_ATTO.Tipo_Atto} {atto.NAtto}: Depositato {listaEMendamenti.First().DisplayTitle}",
                                    MESSAGGIO = bodyMail
                                });
                            }
                        }
                        else
                        {
                            Log.Debug($"[{stampa.UIDStampa}] BACKGROUND MODE - Seduta non è ancora iniziata");
                        }

                        var email_destinatari = $"{utenteRichiedente.email};pem@consiglio.regione.lombardia.it";
                        var email_destinatariGruppo = string.Empty;
                        var email_destinatariGiunta = string.Empty;

                        if (em.id_gruppo > 10000)
                        {
                            Log.Debug(
                                $"[{stampa.UIDStampa}] BACKGROUND MODE - Invio mail a Capo Gruppo e Segreteria Politica");
                            var capoGruppo = await ApiGateway.GetCapoGruppo(em.id_gruppo);
                            var segreteriaPolitica = await ApiGateway.GetSegreteriaPolitica(em.id_gruppo, false, true);

                            if (segreteriaPolitica.Any())
                                email_destinatariGruppo = segreteriaPolitica.Select(u => u.email)
                                    .Aggregate((i, j) => $"{i};{j}");
                            if (capoGruppo != null)
                                email_destinatariGruppo += $";{capoGruppo.email}";
                        }
                        else
                        {
                            Log.Debug($"[{stampa.UIDStampa}] BACKGROUND MODE - Invio mail a Giunta Regionale");
                            var giuntaRegionale = await ApiGateway.GetGiuntaRegionale();
                            var segreteriaGiuntaRegionale = await ApiGateway.GetSegreteriaGiuntaRegionale(false, true);

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

                        var resultInvio = await ApiGateway.SendMail(new MailModel
                        {
                            DA = model.EmailFrom,
                            A = email_destinatari,
                            OGGETTO =
                                $"{atto.TIPI_ATTO.Tipo_Atto} {atto.NAtto}: Depositato {listaEMendamenti.First().DisplayTitle}",
                            MESSAGGIO = bodyMail,
                            pathAttachment = destinazioneDeposito,
                            IsDeposito = true
                        });

                        if (resultInvio)
                            await ApiGateway.JobSetInvioStampa(stampa);
                    }
                    else
                    {
                        //STAMPA - STANDARD MODE
                        Log.Debug($"[{stampa.UIDStampa}] STANDARD MODE - Genera PDF");

                        var bodyCopertina = await ApiGateway.GetCopertina(new CopertinaModel
                        {
                            Atto = atto,
                            TotaleEM = listaEMendamenti.Count,
                            Ordinamento = stampa.Ordine.HasValue
                                ? (OrdinamentoEnum) stampa.Ordine.Value
                                : OrdinamentoEnum.Presentazione
                        });

                        var nameFilePDFCopertina = $"COPERTINAEM_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                        var DirCopertina = Path.Combine(_pathTemp, nameFilePDFCopertina);
                        PdfStamper.CreaPDFCopertina(bodyCopertina, DirCopertina);

                        var listaPdfEmendamentiGenerati =
                            await GeneraPDFEmendamenti(listaEMendamenti, stampa.Da, stampa.A, _pathTemp);

                        var countNonGenerati = listaPdfEmendamentiGenerati.Count(item => !File.Exists(item.Value.Path));
                        Log.Debug($"PDF NON GENERATI [{countNonGenerati}]");

                        //Funzione che fascicola i PDF creati prima
                        var nameFileTarget = $"Fascicolo_{DateTime.Now:ddMMyyyy_hhmmss}.pdf";
                        var FilePathTarget = Path.Combine(_pathTemp, nameFileTarget);
                        PdfStamper.CreateMergedPDF(FilePathTarget, DirCopertina,
                            listaPdfEmendamentiGenerati.ToDictionary(item => item.Key, item => item.Value.Path));
                        var _pathStampe = Path.Combine(model.CartellaLavoroStampe, nameFileTarget);
                        Log.Debug($"[{stampa.UIDStampa}] Percorso stampe {_pathStampe}");
                        SpostaFascicolo(FilePathTarget, _pathStampe);

                        var LinkFile = Path.Combine(model.CartellaStampeLink, nameFileTarget);
                        //Funzione che genera il LINK per il download del documento appena creato
                        var URLDownload = Path.Combine(model.urlCLIENT, LinkFile);
                        //Funzione che genera il LINK per il download del documento appena creato
                        stampa.PathFile = nameFileTarget;
                        await ApiGateway.JobUpdateFileStampa(stampa);
                        if (stampa.Scadenza.HasValue)
                        {
                            var resultInvio = await ApiGateway.SendMail(new MailModel
                            {
                                DA = model.EmailFrom,
                                A = utenteRichiedente.email,
                                OGGETTO = "Link download fascicolo",
                                MESSAGGIO = URLDownload
                            });
                            if (resultInvio)
                                await ApiGateway.JobSetInvioStampa(stampa);
                        }
                        else
                        {
                            if (stampa.Ordine.HasValue)
                            {
                                if ((OrdinamentoEnum) stampa.Ordine.Value == OrdinamentoEnum.Presentazione)
                                    atto.LinkFascicoloPresentazione = URLDownload;
                                if ((OrdinamentoEnum) stampa.Ordine.Value == OrdinamentoEnum.Votazione)
                                    atto.LinkFascicoloVotazione = URLDownload;
                                await ApiGateway.ModificaFilesAtto(atto);
                            }
                        }
                    }

                    PulisciCartellaLavoroTemporanea(_pathTemp);
                }
                else
                {
                    await ApiGateway.SendMail(new MailModel
                    {
                        DA = model.EmailFrom,
                        A = utenteRichiedente.email,
                        OGGETTO = "Errore generazione stampa",
                        MESSAGGIO = $"ID stampa: [{stampa.UIDStampa}], per l'atto: [{stampa.UIDAtto}]"
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[{stampa.UIDStampa}] ERROR", ex);
                try
                {
                    await ApiGateway.JobErrorStampa(stampa.UIDStampa, ex.Message);
                    await ApiGateway.JobUnLockStampa(stampa.UIDStampa);
                }
                catch (Exception ex2)
                {
                    Log.Error($"[{stampa.UIDStampa}] ERROR", ex2);
                    try
                    {
                        await ApiGateway.SendMail(new MailModel
                        {
                            DA = model.EmailFrom,
                            A = utenteRichiedente.email,
                            OGGETTO = "Errore generazione fascicolo",
                            MESSAGGIO = ex.Message
                        });
                    }
                    catch (Exception exMail)
                    {
                        Log.Error($"[{stampa.UIDStampa}] ERROR", exMail);
                        await ApiGateway.JobErrorStampa(stampa.UIDStampa, exMail.Message);
                    }
                }
            }
        }

        internal async Task<Dictionary<Guid, BodyModel>> GeneraPDFEmendamenti(IEnumerable<EmendamentiDto> Emendamenti,
            int Da, int A,
            string _pathTemp)
        {
            var listaPercorsiEM = new Dictionary<Guid, BodyModel>();
            try
            {
                listaPercorsiEM = Emendamenti.ToDictionary(em => em.UIDEM, em => new BodyModel());
                Action<BodyModel> CreaPDF = item =>
                {
                    Log.Debug($"Percorso EM [{item.Path}]");
                    //Creo PDF EM con allegati
                    PdfStamper.CreaPDF(item.Body, item.Path, item.EM, model.urlCLIENT);
                };

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

                            var bodyPDF = await ApiGateway.GetBodyEM(item.UIDEM, TemplateTypeEnum.PDF);
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
                        }

                        j++;
                    }
                }
                else
                {
                    foreach (var item in Emendamenti)
                    {
                        var bodyPDF = await ApiGateway.GetBodyEM(item.UIDEM, TemplateTypeEnum.PDF);
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
                    }
                }

                listaPercorsiEM.AsParallel().ForAll(item => CreaPDF(item.Value));
            }
            catch (Exception ex)
            {
                Log.Error("GeneraPDFEmendamenti Error-->", ex);
            }

            return listaPercorsiEM;
        }

        internal void SpostaFascicolo(string _pathFascicolo, string _pathDestinazione)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_pathDestinazione)))
                Directory.CreateDirectory(Path.GetDirectoryName(_pathDestinazione));

            File.Move(_pathFascicolo, _pathDestinazione);
        }

        internal void PulisciCartellaLavoroTemporanea(string _pathTemp)
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

        #region FIELDS

        private readonly StampaDto stampa;
        private readonly ThreadWorkerModel model;
        private readonly Thread t;

        #endregion
    }

    internal class BodyModel
    {
        public string Path { get; set; }
        public string Body { get; set; }
        public EmendamentiDto EM { get; set; }
    }
}