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
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlToOpenXml;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.Logger;
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
        private readonly EmendamentiLogic _logicEm;
        private readonly FirmeLogic _logicFirme;
        private readonly PersoneLogic _logicPersone;
        private readonly IUnitOfWork _unitOfWork;

        public EsportaLogic(IUnitOfWork unitOfWork, EmendamentiLogic logicEm, FirmeLogic logicFirme,
            PersoneLogic logicPersone)
        {
            _unitOfWork = unitOfWork;
            _logicEm = logicEm;
            _logicFirme = logicFirme;
            _logicPersone = logicPersone;
        }

        public async Task<HttpResponseMessage> EsportaGrigliaExcel(Guid id, OrdinamentoEnum ordine, ClientModeEnum mode, PersonaDto persona)
        {
            try
            {
                var FilePathComplete = GetLocalPath("xlsx");

                var atto = await _unitOfWork.Atti.Get(id);


                IWorkbook workbook = new XSSFWorkbook();
                var excelSheet = workbook.CreateSheet($"{atto.TIPI_ATTO.Tipo_Atto} {atto.NAtto}");

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

                if (atto.VIS_Mis_Prog)
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

                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                var emList = await _logicEm.ScaricaEmendamenti(id, ordine, mode, persona, personeInDbLight, false, true);
                var totalProcessTime = 0f;
                foreach (var em in emList)
                {
                    var startTimer = DateTime.Now;
                    var rowEm = excelSheet.CreateRow(excelSheet.LastRowNum + 1);

                    if (ordine == OrdinamentoEnum.Presentazione)
                        SetColumnValue(ref rowEm, em.OrdinePresentazione.ToString());
                    else if (ordine == OrdinamentoEnum.Votazione)
                        SetColumnValue(ref rowEm, em.OrdineVotazione.ToString());

                    if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM)
                    {
                        SetColumnValue(ref rowEm, em.UIDEM.ToString());
                        var legislatura = await _unitOfWork.Legislature.Get(atto.SEDUTE.id_legislatura);
                        SetColumnValue(ref rowEm,
                            $"{atto.TIPI_ATTO.Tipo_Atto}-{atto.NAtto}-{legislatura.num_legislatura}");
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

                    SetColumnValue(ref rowEm,
                        em.UIDArticolo.HasValue && em.UIDArticolo.Value != Guid.Empty ? em.ARTICOLI.Articolo : "");
                    SetColumnValue(ref rowEm,
                        em.UIDComma.HasValue && em.UIDComma.Value != Guid.Empty ? em.COMMI.Comma : "");
                    SetColumnValue(ref rowEm,
                        em.UIDLettera.HasValue && em.UIDLettera.Value != Guid.Empty ? em.LETTERE.Lettera : em.NLettera);
                    SetColumnValue(ref rowEm, em.NTitolo);
                    SetColumnValue(ref rowEm, em.NCapo);

                    if (atto.VIS_Mis_Prog)
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
                        var firme = await _logicFirme.GetFirme((EM)em, FirmeTipoEnum.TUTTE);
                        var firmeDto = firme.ToList();

                        var firmatari_opendata_ante = "--";
                        try
                        {
                            if (firmeDto.Any(f =>
                                f.Timestamp < Convert.ToDateTime(em.DataDeposito)))
                            {
                                firmatari_opendata_ante = await _logicEm.GetFirmatariEM_OPENDATA(firmeDto.Where(f =>
                                        f.Timestamp < Convert.ToDateTime(em.DataDeposito)),
                                    persona.CurrentRole, personeInDbLight);
                            }
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
                            {
                                firmatari_opendata_post = await _logicEm.GetFirmatariEM_OPENDATA(firmeDto.Where(f =>
                                        f.Timestamp > Convert.ToDateTime(em.DataDeposito)),
                                    persona.CurrentRole, personeInDbLight);
                            }
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
                Log.Debug($"EsportaGrigliaXLS: Compilazione XLS eseguita in {totalProcessTime} s");

                Console.WriteLine($"Excel row count: {excelSheet.LastRowNum}");
                return await Response(FilePathComplete, workbook);
            }
            catch (Exception e)
            {
                Log.Error("Logic - EsportaGrigliaXLS", e);
                throw e;
            }
        }

        public async Task<HttpResponseMessage> HTMLtoWORD(Guid attoUId, OrdinamentoEnum ordine, ClientModeEnum mode, PersonaDto persona)
        {
            try
            {
                var FilePathComplete = GetLocalPath("docx");

                using (var generatedDocument = new MemoryStream())
                {
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<string> ComposeWordTable(Guid attoUID, OrdinamentoEnum ordine, ClientModeEnum mode, PersonaDto persona)
        {
            var personeInDb = await _unitOfWork.Persone.GetAll();
            var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
            var emList = await _logicEm.ScaricaEmendamenti(attoUID, ordine, mode, persona, personeInDbLight, open_data_enabled: false, true);
            var list = emList.Where(em => em.IDStato >= (int)StatiEnum.Depositato);

            var body = "<html>";
            body += "<body style='page-orientation: landscape'>";
            body += "<table>";

            body += "<thead>";
            body += "<tr>";
            body += ComposeHeaderColumn("Ordine");
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
            row += ComposeBodyColumn(em.OrdineVotazione.ToString());
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

        public async Task<HttpResponseMessage> EsportaGrigliaReportExcel(Guid id, OrdinamentoEnum ordine, ClientModeEnum mode, PersonaDto persona)
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
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();
                var emList = await _logicEm.ScaricaEmendamenti(id, ordine, mode, persona, personeInDbLight);

                var uolaSheet =
                    await NewSheet(
                        workbook.CreateSheet(
                            nameof(ReportType.UOLA)),
                        id,
                        ReportType.UOLA,
                        emList
                            .OrderBy(em => em.OrdineVotazione)
                            .ThenBy(em => em.Rif_UIDEM)
                            .ThenBy(em => em.IDStato),
                        style,
                        styleReport);
                var pcrSheet = await NewSheet(workbook.CreateSheet(nameof(ReportType.PCR)), id, ReportType.PCR, emList
                        .OrderBy(em => em.OrdineVotazione)
                        .ThenBy(em => em.Rif_UIDEM)
                        .ThenBy(em => em.IDStato),
                    style,
                    styleReport);
                var progSheet = await NewSheet(workbook.CreateSheet(nameof(ReportType.PROGRESSIVO)), id,
                    ReportType.PROGRESSIVO, emList.OrderBy(em => em.Rif_UIDEM)
                        .ThenBy(em => em.OrdinePresentazione),
                    style,
                    styleReport);

                return await Response(FilePathComplete, workbook);
            }
            catch (Exception e)
            {
                Log.Error("Logic - EsportaGrigliaReportXLS", e);
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
                            {
                                SetSeparator(ref sheet, ref style, ref reportType);
                            }

                            oldParte_Missione = currentParte_Missione;
                        }
                    }
                    else
                    {
                        if (em.UIDArticolo.HasValue)
                        {
                            currentParte_Articolo = em.UIDArticolo!.Value;
                            if (oldParte_Articolo != currentParte_Articolo && oldParte_Articolo != default)
                            {
                                SetSeparator(ref sheet, ref style, ref reportType);
                            }

                            oldParte_Articolo = currentParte_Articolo;
                        }
                    }
                }

                var rowEm = sheet.CreateRow(sheet.LastRowNum + 1);
                SetColumnValue(ref rowEm, em.N_EM);
                SetColumnValue(ref rowEm, em.PersonaProponente.DisplayName);
                if (em.UIDArticolo.HasValue && em.UIDArticolo.Value != Guid.Empty)
                {
                    SetColumnValue(ref rowEm, em.ARTICOLI.Articolo);
                }
                else
                {
                    SetColumnValue(ref rowEm, "--");
                }

                if (em.UIDComma.HasValue && em.UIDComma.Value != Guid.Empty)
                {
                    SetColumnValue(ref rowEm, em.COMMI.Comma);
                }
                else
                {
                    SetColumnValue(ref rowEm, "--");
                }

                if (em.UIDLettera.HasValue && em.UIDLettera.Value != Guid.Empty)
                {
                    SetColumnValue(ref rowEm, em.LETTERE.Lettera);
                }
                else
                {
                    if (!string.IsNullOrEmpty(em.NLettera))
                    {
                        SetColumnValue(ref rowEm, em.NLettera);
                    }
                    else
                    {
                        SetColumnValue(ref rowEm, "--");
                    }
                }

                SetColumnValue(ref rowEm, em.NTitolo);
                SetColumnValue(ref rowEm, em.NCapo);

                if (atto.VIS_Mis_Prog)
                {
                    if (em.NMissione.HasValue && em.NMissione.Value != 0)
                    {
                        SetColumnValue(ref rowEm, em.NMissione.Value.ToString());
                    }
                    else
                    {
                        SetColumnValue(ref rowEm, "--");
                    }

                    if (em.NProgramma.HasValue && em.NProgramma.Value != 0)
                    {
                        SetColumnValue(ref rowEm, em.NProgramma.Value.ToString());
                    }
                    else
                    {
                        SetColumnValue(ref rowEm, "--");
                    }

                    if (em.NTitoloB.HasValue && em.NTitoloB.Value != 0)
                    {
                        SetColumnValue(ref rowEm, em.NTitoloB.Value.ToString());
                    }
                    else
                    {
                        SetColumnValue(ref rowEm, "--");
                    }
                }

                SetColumnValue(ref rowEm, em.TIPI_EM.Tipo_EM);
                SetColumnValue(ref rowEm, em.IDStato == (int)StatiEnum.Inammissibile ? "X" : "");

                if (reportType != ReportType.PCR)
                {
                    SetColumnValue(ref rowEm, em.IDStato == (int)StatiEnum.Ritirato ? "X" : "");
                    SetColumnValue(ref rowEm, em.IDStato == (int)StatiEnum.Approvato || em.IDStato == (int)StatiEnum.Approvato_Con_Modifiche ? "X" : "");
                    SetColumnValue(ref rowEm, em.IDStato == (int)StatiEnum.Non_Approvato ? "X" : "");
                    SetColumnValue(ref rowEm, em.IDStato == (int)StatiEnum.Decaduto ? "X" : "");
                }

                SetColumnValue(ref rowEm, em.NOTE_EM);
                SetColumnValue(ref rowEm, em.NOTE_Griglia);
            }

            var countEM = emendamentiDtos.Count();
            var approvati = emendamentiDtos.Count(em => em.IDStato == (int)StatiEnum.Approvato || em.IDStato == (int)StatiEnum.Approvato_Con_Modifiche);
            var non_approvati = emendamentiDtos.Count(em => em.IDStato == (int)StatiEnum.Non_Approvato);
            var ritirati = emendamentiDtos.Count(em => em.IDStato == (int)StatiEnum.Ritirato);
            var decaduti = emendamentiDtos.Count(em => em.IDStato == (int)StatiEnum.Decaduto);
            var inammissibili = emendamentiDtos.Count(em => em.IDStato == (int)StatiEnum.Inammissibile);

            var rowReport = sheet.CreateRow(sheet.LastRowNum + 1);
            rowReport.RowStyle = styleR;
            var cellCount = rowReport.CreateCell(0);
            cellCount.CellStyle = styleR;
            cellCount.SetCellValue(countEM);
            var cellInamm = rowReport.CreateCell(8);
            cellInamm.CellStyle = styleR;
            cellInamm.SetCellValue(inammissibili);

            if (reportType == ReportType.PCR)
            {
                return sheet;
            }

            var cellRit = rowReport.CreateCell(9);
            cellRit.CellStyle = styleR;
            cellRit.SetCellValue(ritirati);
            var cellApp = rowReport.CreateCell(10);
            cellApp.CellStyle = styleR;
            cellApp.SetCellValue(approvati);
            var cellNonApp = rowReport.CreateCell(11);
            cellNonApp.CellStyle = styleR;
            cellNonApp.SetCellValue(non_approvati);
            var cellDeca = rowReport.CreateCell(12);
            cellDeca.CellStyle = styleR;
            cellDeca.SetCellValue(decaduti);

            return sheet;
        }

        private void SetColumnValue(ref IRow row, string val)
        {
            row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue(val);
        }

        private void SetSeparator(ref ISheet sheet, ref ICellStyle style, ref ReportType reportType)
        {
            if (reportType == ReportType.PROGRESSIVO)
            {
                return;
            }

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
            if (!Directory.Exists(_pathTemp))
            {
                Directory.CreateDirectory(_pathTemp);
            }

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
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(stream);
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
            PROGRESSIVO = 3
        }
    }
}