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

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlToOpenXml;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using PortaleRegione.API.Controllers;
using PortaleRegione.Common;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Document = DocumentFormat.OpenXml.Wordprocessing.Document;

namespace PortaleRegione.BAL
{
    public class EsportaLogic : BaseLogic
    {
        public EsportaLogic(IUnitOfWork unitOfWork, EmendamentiLogic logicEm, DASILogic logicDASI,
            FirmeLogic logicFirme)
        {
            _unitOfWork = unitOfWork;
            _logicEm = logicEm;
            _logicDasi = logicDASI;
            _logicFirme = logicFirme;
        }

        public async Task<HttpResponseMessage> EsportaGrigliaExcel(EmendamentiViewModel model, PersonaDto persona)
        {
            try
            {
                var FilePathComplete = GetLocalPath("xlsx");

                IWorkbook workbook = new XSSFWorkbook();
                var excelSheet = workbook.CreateSheet($"{Utility.GetText_Tipo(model.Atto.IDTipoAtto)} {model.Atto.NAtto.Replace('/', '-')}");

                var row = excelSheet.CreateRow(0);
                SetColumnValue(ref row, "Ordine");
                if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM)
                {
                    SetColumnValue(ref row, "IDEM");
                    SetColumnValue(ref row, "Atto");
                }

                SetColumnValue(ref row, "Numero EM");
                SetColumnValue(ref row, "Data Deposito");
                SetColumnValue(ref row, "Stato");
                SetColumnValue(ref row, "Tipo");
                SetColumnValue(ref row, "Parte");
                SetColumnValue(ref row, "Articolo");
                SetColumnValue(ref row, "Comma");
                SetColumnValue(ref row, "Lettera");
                SetColumnValue(ref row, "Titolo");
                SetColumnValue(ref row, "Capo");

                if (model.Atto.VIS_Mis_Prog)
                {
                    SetColumnValue(ref row, "Missione");
                    SetColumnValue(ref row, "Programma");
                    SetColumnValue(ref row, "TitoloB");
                }

                SetColumnValue(ref row, "Proponente");
                SetColumnValue(ref row, "Area Politica");
                SetColumnValue(ref row, "Firmatari");
                SetColumnValue(ref row, "Firmatari dopo deposito");
                SetColumnValue(ref row, "LinkEM");

                var emList =
                    await _logicEm.ScaricaEmendamenti(model, persona, false, true);
                var totalProcessTime = 0f;
                foreach (var em in emList)
                {
                    var startTimer = DateTime.Now;
                    var rowEm = excelSheet.CreateRow(excelSheet.LastRowNum + 1);

                    if (model.Ordinamento == OrdinamentoEnum.Presentazione)
                        SetColumnValue(ref rowEm, em.OrdinePresentazione.ToString());
                    else if (model.Ordinamento == OrdinamentoEnum.Votazione)
                        SetColumnValue(ref rowEm, em.OrdineVotazione.ToString());

                    if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM)
                    {
                        SetColumnValue(ref rowEm, em.UIDEM.ToString());
                        var legislatura = await _unitOfWork.Legislature.Get(model.Atto.SEDUTE.id_legislatura);
                        SetColumnValue(ref rowEm,
                            $"{Utility.GetText_Tipo(model.Atto.IDTipoAtto)}-{model.Atto.NAtto}-{legislatura.num_legislatura}");
                    }

                    SetColumnValue(ref rowEm, em.N_EM);
                    SetColumnValue(ref rowEm, em.DataDeposito);
                    SetColumnValue(ref rowEm, persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                        ? $"{em.STATI_EM.IDStato}-{em.STATI_EM.Stato}"
                        : em.STATI_EM.Stato);

                    SetColumnValue(ref rowEm, persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                        ? $"{em.TIPI_EM.IDTipo_EM}-{em.TIPI_EM.Tipo_EM}"
                        : em.TIPI_EM.Tipo_EM);

                    SetColumnValue(ref rowEm, persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                        ? $"{em.IDParte}-{em.PARTI_TESTO.Parte}"
                        : em.PARTI_TESTO.Parte);

                    if (em.UIDArticolo.HasValue && em.IDParte == (int)PartiEMEnum.Articolo)
                    {
                        var articolo = await _unitOfWork.Articoli.GetArticolo(em.UIDArticolo.Value);
                        SetColumnValue(ref rowEm, articolo.Articolo);
                    }
                    else
                        SetColumnValue(ref rowEm, "");

                    if (em.UIDComma.HasValue && em.UIDComma != Guid.Empty && em.IDParte == (int)PartiEMEnum.Articolo)
                    {
                        var comma = await _unitOfWork.Commi.GetComma(em.UIDComma.Value);
                        SetColumnValue(ref rowEm, comma.Comma);
                    }
                    else
                        SetColumnValue(ref rowEm, "");

                    if (em.UIDLettera.HasValue && em.UIDLettera != Guid.Empty && em.IDParte == (int)PartiEMEnum.Articolo)
                    {
                        var lettera = await _unitOfWork.Lettere.GetLettera(em.UIDLettera.Value);
                        SetColumnValue(ref rowEm, lettera.Lettera);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(em.NLettera) && em.IDParte == (int)PartiEMEnum.Articolo)
                            SetColumnValue(ref rowEm, em.NLettera);
                        else
                            SetColumnValue(ref rowEm, "");
                    }

                    SetColumnValue(ref rowEm, em.NTitolo);
                    SetColumnValue(ref rowEm, em.NCapo);

                    if (model.Atto.VIS_Mis_Prog)
                    {
                        SetColumnValue(ref rowEm, em.NMissione.ToString());
                        SetColumnValue(ref rowEm, em.NProgramma.ToString());
                        SetColumnValue(ref rowEm, em.NTitoloB.ToString());
                    }

                    SetColumnValue(ref rowEm, persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                        ? $"{em.PersonaProponente.UID_persona}-{em.PersonaProponente.DisplayName}"
                        : em.PersonaProponente.DisplayName);
                    SetColumnValue(ref rowEm, "");

                    if (!string.IsNullOrEmpty(em.DataDeposito))
                    {
                        var firme = await _logicFirme.GetFirme(em, FirmeTipoEnum.TUTTE);
                        var firmeDto = firme.ToList();

                        var firmatari_opendata_ante = "--";
                        try
                        {
                            if (firmeDto.Any(f =>
                                f.Timestamp < Convert.ToDateTime(em.DataDeposito)))
                                firmatari_opendata_ante = _logicEm.GetFirmatariEM_OPENDATA(firmeDto.Where(f =>
                                        f.Timestamp < Convert.ToDateTime(em.DataDeposito)),
                                    persona.CurrentRole);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                        var firmatari_opendata_post = "--";
                        try
                        {
                            if (firmeDto.Any(f =>
                                f.Timestamp > Convert.ToDateTime(em.DataDeposito)))
                                firmatari_opendata_post = _logicEm.GetFirmatariEM_OPENDATA(firmeDto.Where(f =>
                                        f.Timestamp > Convert.ToDateTime(em.DataDeposito)),
                                    persona.CurrentRole);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                        SetColumnValue(ref rowEm, firmatari_opendata_ante);
                        SetColumnValue(ref rowEm, firmatari_opendata_post);
                    }
                    else
                    {
                        SetColumnValue(ref rowEm, "--");
                        SetColumnValue(ref rowEm, "--");
                    }

                    SetColumnValue(ref rowEm, $"{AppSettingsConfiguration.urlPEM_ViewEM}{em.UID_QRCode}");
                    var spentTime = Math.Round((DateTime.Now - startTimer).TotalSeconds, 2);
                    totalProcessTime += (float)spentTime;
                }

                //Log.Debug($"EsportaGrigliaXLS: Compilazione XLS eseguita in {totalProcessTime} s");

                Console.WriteLine($"Excel row count: {excelSheet.LastRowNum}");
                return await Response(FilePathComplete, workbook);
            }
            catch (Exception e)
            {
                //Log.Error("Logic - EsportaGrigliaXLS", e);
                throw e;
            }
        }

        public async Task<HttpResponseMessage> EsportaGrigliaExcelDASI(RiepilogoDASIModel model)
        {
            try
            {
                var FilePathComplete = GetLocalPath("xlsx");

                IWorkbook workbook = new XSSFWorkbook();
                var style = workbook.CreateCellStyle();
                style.FillForegroundColor = HSSFColor.Grey25Percent.Index;
                style.FillPattern = FillPattern.SolidForeground;
                var styleReport = workbook.CreateCellStyle();
                styleReport.FillForegroundColor = HSSFColor.LightGreen.Index;
                styleReport.FillPattern = FillPattern.SolidForeground;
                styleReport.Alignment = HorizontalAlignment.Center;

                var attiList = model.Data.Results.ToList();
                var dasiSheet =
                    await NewSheetDASI_Atti(
                        workbook.CreateSheet(
                            "Atti"),
                        attiList);
                var firmatariList = await _logicDasi.ScaricaAtti_Firmatari(attiList);
                var firmatariSheet =
                    await NewSheetDASI_Firmatari(
                        workbook.CreateSheet(
                            "Firmatari"),
                        firmatariList);

                var controlliSheet =
                    NewSheetDASI_Controlli(
                        workbook.CreateSheet(
                            "Controlli"));

                var lookupSheet = workbook.CreateSheet("LOOKUP");

                return await Response(FilePathComplete, workbook);
            }
            catch (Exception e)
            {
                //Log.Error("Logic - EsportaGrigliaXLSDASI", e);
                throw e;
            }
        }

        private ISheet NewSheetDASI_Controlli(ISheet sheet)
        {
            try
            {
                var row = sheet.CreateRow(0);
                SetColumnValue(ref row, "Codice di controllo del template:");
                SetColumnValue(ref row, "TY86Z5s6VfZtRFF46fi54qskISyv36_v007");

                var row1 = sheet.CreateRow(1);
                SetColumnValue(ref row1, "Numero massimo atti da importare:");
                SetColumnValue(ref row1, "1000");

                return sheet;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<ISheet> NewSheetDASI_Firmatari(ISheet sheet, List<AttiFirmeDto> firmatariList)
        {
            //HEADER
            try
            {
                var row = sheet.CreateRow(0);
                SetColumnValue(ref row, "TIPO ATTO");
                SetColumnValue(ref row, "NUMERO ATTO");
                SetColumnValue(ref row, "FIRMATARIO");
                SetColumnValue(ref row, "GRUPPO");
                SetColumnValue(ref row, "DATA FIRMA");
                SetColumnValue(ref row, "DATA RITIRO FIRMA");
                SetColumnValue(ref row, "PRIMO FIRMATARIO");

                View_gruppi_politici_con_giunta gruppo = new View_gruppi_politici_con_giunta();
                foreach (var firma in firmatariList)
                {
                    var atto = await _logicDasi.GetAttoDto(firma.UIDAtto);
                    if (gruppo.id_gruppo != firma.id_gruppo)
                        gruppo = await _unitOfWork.Gruppi.Get(firma.id_gruppo);
                    var rowBody = sheet.CreateRow(sheet.LastRowNum + 1);
                    SetColumnValue(ref rowBody, Utility.GetText_Tipo(atto)); // tipo atto
                    SetColumnValue(ref rowBody, atto.NAtto, CellType.Numeric); // numero atto
                    var firmacert = firma.FirmaCert;
                    var indiceParentesiApertura = firmacert.IndexOf('(');
                    firmacert = firmacert.Remove(indiceParentesiApertura - 1);
                    SetColumnValue(ref rowBody, firmacert); // firmatario
                    SetColumnValue(ref rowBody, gruppo != null ? gruppo.codice_gruppo : ""); // gruppo
                    SetColumnValue(ref rowBody, firma.Data_firma.Substring(0, 10)); // data firma
                    var data_ritiro_firma = firma.Data_ritirofirma;
                    if (!string.IsNullOrEmpty(data_ritiro_firma))
                    {
                        data_ritiro_firma = data_ritiro_firma.Substring(0, 10);
                    }

                    SetColumnValue(ref rowBody, data_ritiro_firma); // data ritiro firma
                    SetColumnValue(ref rowBody, firma.PrimoFirmatario ? "SI" : "NO"); // primo firmatario
                }

                return sheet;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<HttpResponseMessage> HTMLtoWORD(Guid attoUId, OrdinamentoEnum ordine, ClientModeEnum mode,
            PersonaDto persona)
        {
            try
            {
                var FilePathComplete = GetLocalPath("docx");

                using var generatedDocument = new MemoryStream();
                using (var package =
                       WordprocessingDocument.Create(generatedDocument, WordprocessingDocumentType.Document))
                {
                    var mainPart = package.MainDocumentPart;
                    if (mainPart == null)
                    {
                        mainPart = package.AddMainDocumentPart();
                        new Document(new Body()).Save(mainPart);
                    }

                    var converter = new HtmlConverter(mainPart);
                    converter.ParseHtml(await ComposeWordTable(attoUId, ordine, mode, persona));

                    mainPart.Document.Save();
                }

                File.WriteAllBytes(FilePathComplete, generatedDocument.ToArray());
                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(generatedDocument.ToArray())
                };
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = Path.GetFileName(FilePathComplete)
                };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/doc");

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<string> ComposeWordTable(Guid attoUID, OrdinamentoEnum ordine, ClientModeEnum mode,
            PersonaDto persona)
        {
            var emList =
                await _logicEm.ScaricaEmendamenti(attoUID, ordine, mode, persona, false, true);
            var list = emList.Where(em => em.IDStato >= (int)StatiEnum.Depositato);

            var body = "<html>";
            body += "<body style='page-orientation: landscape'>";
            body += "<table>";

            body += "<thead>";
            body += "<tr>";
            body += ComposeHeaderColumn("EM/SUB");
            body += ComposeHeaderColumn("Testo");
            body += ComposeHeaderColumn("Relazione");
            body += ComposeHeaderColumn("Proponente");
            body += ComposeHeaderColumn("Stato");
            body += "</tr>";
            body += "</thead>";

            body += "<tbody>";
            body = list.Aggregate(body, (current, em) => current + ComposeBodyRow(em));
            body += "</tbody>";

            body += "</table>";
            body += "</body>";
            body += "</html>";
            return body;
        }

        private string ComposeHeaderColumn(string column_title)
        {
            return $"<th style='text-align:center;'>{column_title}</th>";
        }

        private string ComposeBodyRow(EmendamentiDto em)
        {
            var row = string.Empty;
            row += "<tr>";
            row += ComposeBodyColumn(em.N_EM);
            row += ComposeBodyColumn(em.TestoEM_originale);
            row += ComposeBodyColumn(em.TestoREL_originale);
            row += ComposeBodyColumn(em.PersonaProponente.DisplayName);
            row += ComposeBodyColumn(em.STATI_EM.Stato);

            row += "</tr>";
            return row;
        }

        private string ComposeBodyColumn(string column_body)
        {
            return $"<td>{column_body}</td>";
        }

        public async Task<HttpResponseMessage> EsportaGrigliaReportExcel(EmendamentiViewModel model, PersonaDto persona)
        {
            try
            {
                var FilePathComplete = GetLocalPath("xlsx");

                IWorkbook workbook = new XSSFWorkbook();
                var style = workbook.CreateCellStyle();
                style.FillForegroundColor = HSSFColor.Grey25Percent.Index;
                style.FillPattern = FillPattern.SolidForeground;
                var styleReport = workbook.CreateCellStyle();
                styleReport.FillForegroundColor = HSSFColor.LightGreen.Index;
                styleReport.FillPattern = FillPattern.SolidForeground;
                styleReport.Alignment = HorizontalAlignment.Center;
                var emList = await _logicEm.ScaricaEmendamenti(model, persona);

                var uolaSheet =
                    await NewSheet(
                        workbook.CreateSheet(
                            nameof(ReportType.UOLA)),
                        model.Atto.UIDAtto,
                        ReportType.UOLA,
                        emList
                            .OrderBy(em => em.OrdineVotazione)
                            .ThenBy(em => em.Rif_UIDEM)
                            .ThenBy(em => em.IDStato),
                        style,
                        styleReport);
                var pcrSheet = await NewSheet(workbook.CreateSheet(nameof(ReportType.PCR)), model.Atto.UIDAtto, ReportType.PCR, emList
                        .OrderBy(em => em.OrdineVotazione)
                        .ThenBy(em => em.Rif_UIDEM)
                        .ThenBy(em => em.IDStato),
                    style,
                    styleReport);
                var progSheet = await NewSheet(workbook.CreateSheet(nameof(ReportType.PROGRESSIVO)), model.Atto.UIDAtto,
                    ReportType.PROGRESSIVO, emList.OrderBy(em => em.Rif_UIDEM)
                        .ThenBy(em => em.OrdinePresentazione),
                    style,
                    styleReport);

                return await Response(FilePathComplete, workbook);
            }
            catch (Exception e)
            {
                //Log.Error("Logic - EsportaGrigliaReportXLS", e);
                throw e;
            }
        }

        private async Task<ISheet> NewSheet(ISheet sheet, Guid attoUId, ReportType reportType,
            IEnumerable<EmendamentiDto> emendamentiDtos, ICellStyle style, ICellStyle styleR)
        {
            var atto = await _unitOfWork.Atti.Get(attoUId);

            //HEADER
            var row = sheet.CreateRow(0);
            SetColumnValue(ref row, "Numero EM");
            SetColumnValue(ref row, "Proponente");
            SetColumnValue(ref row, "Articolo");
            SetColumnValue(ref row, "Comma");
            SetColumnValue(ref row, "Lettera");
            SetColumnValue(ref row, "Titolo");
            SetColumnValue(ref row, "Capo");
            if (atto.VIS_Mis_Prog)
            {
                SetColumnValue(ref row, "Missione");
                SetColumnValue(ref row, "Programma");
                SetColumnValue(ref row, "TitoloB");
            }

            SetColumnValue(ref row, "Contenuto");
            SetColumnValue(ref row, "INAMM.");
            if (reportType != ReportType.PCR)
            {
                SetColumnValue(ref row, "RITIRATO");
                SetColumnValue(ref row, "SI");
                SetColumnValue(ref row, "NO");
                SetColumnValue(ref row, "DECADE");
            }

            SetColumnValue(ref row, "NOTE");
            SetColumnValue(ref row, "NOTE_RISERVATE");

            var oldParte = emendamentiDtos.First().IDParte;
            int currentParte_Missione = default, oldParte_Missione = default;
            Guid currentParte_Articolo = default, oldParte_Articolo = default;

            foreach (var em in emendamentiDtos)
            {
                var currentParte = em.IDParte;
                if (oldParte != currentParte)
                {
                    SetSeparator(ref sheet, ref style, ref reportType);
                    oldParte = em.IDParte;
                }
                else
                {
                    if (em.IDParte == (int)PartiEMEnum.Missione)
                    {
                        if (em.NMissione.HasValue)
                        {
                            currentParte_Missione = em.NMissione!.Value;
                            if (oldParte_Missione != currentParte_Missione && oldParte_Missione != default)
                                SetSeparator(ref sheet, ref style, ref reportType);

                            oldParte_Missione = currentParte_Missione;
                        }
                    }
                    else
                    {
                        if (em.UIDArticolo.HasValue)
                        {
                            currentParte_Articolo = em.UIDArticolo!.Value;
                            if (oldParte_Articolo != currentParte_Articolo && oldParte_Articolo != default)
                                SetSeparator(ref sheet, ref style, ref reportType);

                            oldParte_Articolo = currentParte_Articolo;
                        }
                    }
                }

                var rowEm = sheet.CreateRow(sheet.LastRowNum + 1);
                SetColumnValue(ref rowEm, em.N_EM);
                SetColumnValue(ref rowEm, em.PersonaProponente.DisplayName);
                if (em.UIDArticolo.HasValue && em.UIDArticolo.Value != Guid.Empty)
                    SetColumnValue(ref rowEm, em.ARTICOLI.Articolo);
                else
                    SetColumnValue(ref rowEm, "--");

                if (em.UIDComma.HasValue && em.UIDComma.Value != Guid.Empty)
                    SetColumnValue(ref rowEm, em.COMMI.Comma);
                else
                    SetColumnValue(ref rowEm, "--");

                if (em.UIDLettera.HasValue && em.UIDLettera.Value != Guid.Empty)
                {
                    SetColumnValue(ref rowEm, em.LETTERE.Lettera);
                }
                else
                {
                    if (!string.IsNullOrEmpty(em.NLettera))
                        SetColumnValue(ref rowEm, em.NLettera);
                    else
                        SetColumnValue(ref rowEm, "--");
                }

                SetColumnValue(ref rowEm, em.NTitolo);
                SetColumnValue(ref rowEm, em.NCapo);

                if (atto.VIS_Mis_Prog)
                {
                    if (em.NMissione.HasValue && em.NMissione.Value != 0)
                        SetColumnValue(ref rowEm, em.NMissione.Value.ToString());
                    else
                        SetColumnValue(ref rowEm, "--");

                    if (em.NProgramma.HasValue && em.NProgramma.Value != 0)
                        SetColumnValue(ref rowEm, em.NProgramma.Value.ToString());
                    else
                        SetColumnValue(ref rowEm, "--");

                    if (em.NTitoloB.HasValue && em.NTitoloB.Value != 0)
                        SetColumnValue(ref rowEm, em.NTitoloB.Value.ToString());
                    else
                        SetColumnValue(ref rowEm, "--");
                }

                SetColumnValue(ref rowEm, em.TIPI_EM.Tipo_EM);
                SetColumnValue(ref rowEm, em.IDStato == (int)StatiEnum.Inammissibile ? "X" : "");

                if (reportType != ReportType.PCR)
                {
                    SetColumnValue(ref rowEm, em.IDStato == (int)StatiEnum.Ritirato ? "X" : "");
                    SetColumnValue(ref rowEm,
                        em.IDStato == (int)StatiEnum.Approvato || em.IDStato == (int)StatiEnum.Approvato_Con_Modifiche
                            ? "X"
                            : "");
                    SetColumnValue(ref rowEm, em.IDStato == (int)StatiEnum.Non_Approvato ? "X" : "");
                    SetColumnValue(ref rowEm, em.IDStato == (int)StatiEnum.Decaduto ? "X" : "");
                }

                SetColumnValue(ref rowEm, em.NOTE_EM);
                SetColumnValue(ref rowEm, em.NOTE_Griglia);
            }

            var countEM = emendamentiDtos.Count();
            var approvati = emendamentiDtos.Count(em =>
                em.IDStato == (int)StatiEnum.Approvato || em.IDStato == (int)StatiEnum.Approvato_Con_Modifiche);
            var non_approvati = emendamentiDtos.Count(em => em.IDStato == (int)StatiEnum.Non_Approvato);
            var ritirati = emendamentiDtos.Count(em => em.IDStato == (int)StatiEnum.Ritirato);
            var decaduti = emendamentiDtos.Count(em => em.IDStato == (int)StatiEnum.Decaduto);
            var inammissibili = emendamentiDtos.Count(em => em.IDStato == (int)StatiEnum.Inammissibile);

            var rowReport = sheet.CreateRow(sheet.LastRowNum + 1);
            rowReport.RowStyle = styleR;
            var cellCount = rowReport.CreateCell(0);
            cellCount.CellStyle = styleR;
            cellCount.SetCellValue(countEM);
            var colonna_conteggi = atto.VIS_Mis_Prog ? 11 : 8;
            var cellInamm = rowReport.CreateCell(colonna_conteggi);
            cellInamm.CellStyle = styleR;
            cellInamm.SetCellValue(inammissibili);

            if (reportType == ReportType.PCR) return sheet;

            var cellRit = rowReport.CreateCell(colonna_conteggi + 1);
            cellRit.CellStyle = styleR;
            cellRit.SetCellValue(ritirati);
            var cellApp = rowReport.CreateCell(colonna_conteggi + 2);
            cellApp.CellStyle = styleR;
            cellApp.SetCellValue(approvati);
            var cellNonApp = rowReport.CreateCell(colonna_conteggi + 3);
            cellNonApp.CellStyle = styleR;
            cellNonApp.SetCellValue(non_approvati);
            var cellDeca = rowReport.CreateCell(colonna_conteggi + 4);
            cellDeca.CellStyle = styleR;
            cellDeca.SetCellValue(decaduti);

            return sheet;
        }

        private Task<ISheet> NewSheetDASI_Atti(ISheet sheet, IEnumerable<AttoDASIDto> attiList)
        {
            //HEADER
            try
            {
                var rowH = sheet.CreateRow(0);
                var row = sheet.CreateRow(1);
                SetColumnValue(ref row, "TIPO ATTO");
                SetColumnValue(ref row, "TIPO MOZIONE");
                SetColumnValue(ref row, "NUMERO ATTO");
                SetColumnValue(ref row, "STATO");
                SetColumnValue(ref row, "PROTOCOLLO");
                SetColumnValue(ref row, "CODICE MATERIA");
                SetColumnValue(ref row, "DATA PRESENTAZIONE");
                SetColumnValue(ref row, "OGGETTO");
                SetColumnValue(ref row, "OGGETTO PRESENTATO/OGGETTO COMMISSIONE");
                SetColumnValue(ref row, "OGGETTO APPROVATO/OGGETTO ASSEMBLEA");
                SetColumnValue(ref row, "RISPOSTA RICHIESTA");
                SetColumnValue(ref row, "AREA");
                SetColumnValue(ref row, "DATA ANNUNZIO");
                SetColumnValue(ref row, "PUBBLICATO");
                SetColumnValue(ref row, "RISPOSTA FORNITA");
                SetColumnValue(ref row, "ITER MULTIPLO");
                SetColumnValue(ref row, "NOTE RISPOSTA");
                SetColumnValue(ref row, "ANNOTAZIONI");
                SetColumnValue(ref row, "TIPO CHIUSURA ITER");
                SetColumnValue(ref row, "DATA CHIUSURA ITER");
                SetColumnValue(ref row, "NOTE CHIUSURA ITER");
                SetColumnValue(ref row, "RISULTATO VOTAZIONE");
                SetColumnValue(ref row, "DATA TRASMISSIONE");
                SetColumnValue(ref row, "TIPO CHIUSURA ITER");
                SetColumnValue(ref row, "DATA CHIUSURA ITER");
                SetColumnValue(ref row, "NOTE CHIUSURA ITER");
                SetColumnValue(ref row, "TIPO VOTAZIONE");
                SetColumnValue(ref row, "DCR");
                SetColumnValue(ref row, "NUMERO DCR");
                SetColumnValue(ref row, "NUMERO DCRC");
                SetColumnValue(ref row, "BURL");
                SetColumnValue(ref row, "EMENDATO");
                SetColumnValue(ref row, "DATA COMUNICAZIONE ASSEMBLEA");
                SetColumnValue(ref row, "AREA TEMATICA");
                SetColumnValue(ref row, "DATA TRASMISSIONE");
                SetColumnValue(ref row, "ALTRI SOGGETTI");
                SetColumnValue(ref row, "COMPETENZA");
                SetColumnValue(ref row, "IMPEGNI E SCADENZE");
                SetColumnValue(ref row, "STATO DI ATTUAZIONE");
                SetColumnValue(ref row, "CONCLUSO");

                foreach (var atto in attiList)
                {
                    var rowBody = sheet.CreateRow(sheet.LastRowNum + 1);
                    SetColumnValue(ref rowBody, Utility.GetText_Tipo(atto)); // tipo atto
                    SetColumnValue(ref rowBody, ""); // tipo mozione
                    SetColumnValue(ref rowBody, atto.NAtto, CellType.Numeric); // numero atto
                    SetColumnValue(ref rowBody, Utility.GetText_StatoDASI(atto.IDStato)); // stato atto
                    SetColumnValue(ref rowBody, ""); // protocollo
                    SetColumnValue(ref rowBody, ""); // codice materia
                    SetColumnValue(ref rowBody, atto.DataPresentazione.Substring(0, 10)); // data presentazione
                    SetColumnValue(ref rowBody, ""); // oggetto

                    //Matteo Cattapan #500
                    switch ((TipoAttoEnum)atto.Tipo)
                    {
                        case TipoAttoEnum.MOZ:
                        case TipoAttoEnum.ODG:
                            {
                                SetColumnValue(ref rowBody, ""); // oggetto approvato / oggetto assemblea 
                                SetColumnValue(ref rowBody, atto.Oggetto); // oggetto presentato / oggetto commissione 
                                break;
                            }
                        default:
                            {
                                SetColumnValue(ref rowBody, atto.Oggetto); // oggetto approvato / oggetto assemblea 
                                SetColumnValue(ref rowBody, ""); // oggetto presentato / oggetto commissione 
                                break;
                            }
                    }
                    SetColumnValue(ref rowBody, Utility.GetText_TipoRispostaDASI(atto.IDTipo_Risposta)); // risposta
                }

                return Task.FromResult(sheet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void SetColumnValue(ref IRow row, string val, CellType type = CellType.String)
        {
            row.CreateCell(GetColumn(row.LastCellNum), type).SetCellValue(val);
        }

        private void SetSeparator(ref ISheet sheet, ref ICellStyle style, ref ReportType reportType)
        {
            if (reportType == ReportType.PROGRESSIVO) return;

            var rowSep = sheet.CreateRow(sheet.LastRowNum + 1);
            rowSep.RowStyle = style;
        }

        private short GetColumn(short column)
        {
            return column < 0 ? (short)0 : column;
        }

        private string GetLocalPath(string extension)
        {
            var _pathTemp = AppSettingsConfiguration.CartellaTemp;
            if (!Directory.Exists(_pathTemp)) Directory.CreateDirectory(_pathTemp);

            var nameFile = $"PEM_{DateTime.Now:ddMMyyyy_hhmmss}.{extension}";
            var FilePathComplete = Path.Combine(_pathTemp, nameFile);

            return FilePathComplete;
        }

        private async Task<HttpResponseMessage> Response<T>(string path, T book)
        {
            var contentType = "";
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                var bookType = book.GetType();
                if (typeof(XSSFWorkbook) == bookType)
                {
                    var t = book as IWorkbook;
                    t?.Write(fs);
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }

                if (typeof(XWPFDocument) == bookType)
                {
                    var t = book as XWPFDocument;
                    t?.Write(fs);
                    contentType = "application/doc";
                }
            }

            var stream = new MemoryStream();
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                await fileStream.CopyToAsync(stream);
            }

            stream.Position = 0;
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = Path.GetFileName(path)
            };
            return result;
        }

        private enum ReportType
        {
            UOLA = 1,
            PCR = 2,
            PROGRESSIVO = 3,
            DASI = 4,
            DASI_FIRMATARI = 5
        }
    }
}