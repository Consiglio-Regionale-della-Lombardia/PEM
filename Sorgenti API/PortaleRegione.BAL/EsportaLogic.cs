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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlToOpenXml;
using NPOI.HSSF.Util;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using PortaleRegione.Common;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.Logger;
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

        public async Task<HttpResponseMessage> EsportaGrigliaExcel(Guid id, OrdinamentoEnum ordine, PersonaDto persona)
        {
            try
            {
                var FilePathComplete = GetLocalPath("xlsx");

                var atto = await _unitOfWork.Atti.Get(id);


                IWorkbook workbook = new XSSFWorkbook();
                var excelSheet = workbook.CreateSheet($"{atto.TIPI_ATTO.Tipo_Atto} {atto.NAtto}");

                var row = excelSheet.CreateRow(0);

                if (atto.OrdineVotazione.HasValue)
                    if (atto.OrdineVotazione.Value)
                        SetColumnValue(ref row, "OrdineVotazione");

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

                var emList = await _logicEm.ScaricaEmendamenti(id, ordine, persona);

                foreach (var em in emList)
                {
                    var rowEm = excelSheet.CreateRow(excelSheet.LastRowNum + 1);

                    if (atto.OrdineVotazione.HasValue)
                        if (atto.OrdineVotazione.Value)
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
                        var firme = await _logicFirme.GetFirme(em, FirmeTipoEnum.TUTTE);
                        var firmeDto = firme.Select(Mapper.Map<FIRME, FirmeDto>).ToList();

                        var firmatari_opendata_ante = "--";
                        try
                        {
                            if (firmeDto.Any(f =>
                                f.Timestamp < Convert.ToDateTime(em.DataDeposito)))
                                firmatari_opendata_ante = await _logicEm.GetFirmatariEM_OPENDATA(firmeDto.Where(f =>
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
                                firmatari_opendata_post = await _logicEm.GetFirmatariEM_OPENDATA(firmeDto.Where(f =>
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
                }

                Console.WriteLine($"Excel row count: {excelSheet.LastRowNum}");
                return await Response(FilePathComplete, workbook);
            }
            catch (Exception e)
            {
                Log.Error("Logic - EsportaGrigliaXLS", e);
                throw e;
            }
        }

        public async Task<HttpResponseMessage> EsportaGrigliaWord(Guid id, OrdinamentoEnum ordine, PersonaDto persona)
        {
            try
            {
                var FilePathComplete = GetLocalPath("docx");

                var atto = await _unitOfWork.Atti.Get(id);

                var doc = new XWPFDocument();
                var section = new CT_SectPr
                {
                    pgSz =
                    {
                        orient = ST_PageOrientation.landscape,
                        w = 1680 * 20,
                        h = 1188 * 20
                    }
                };
                section.pgMar.top = "500";
                section.pgMar.left = 500;
                section.pgMar.right = 500;
                section.pgMar.bottom = "500";
                doc.Document.body.sectPr = section;

                var table = doc.CreateTable();

                table.Width = 5000;
                var rowHeader = table.CreateRow();
                rowHeader.RemoveCell(0);

                var cPdl = table.Rows[0].GetCell(0);
                var headerCellcPdl = cPdl.AddParagraph();
                headerCellcPdl.Alignment = ParagraphAlignment.CENTER;
                var headerCellcPdl_Run = headerCellcPdl.CreateRun();
                headerCellcPdl_Run.IsBold = true;
                headerCellcPdl_Run.SetText($"{atto.TIPI_ATTO.Tipo_Atto} {atto.NAtto}");

                var c0 = rowHeader.CreateCell();
                var headerCell0 = c0.AddParagraph();
                headerCell0.Alignment = ParagraphAlignment.CENTER;
                var headerCell0_Run = headerCell0.CreateRun();
                headerCell0_Run.IsBold = true;
                headerCell0_Run.SetText("Ordine di Votazione");

                var c1 = rowHeader.CreateCell();
                var headerCell1 = c1.AddParagraph();
                headerCell1.Alignment = ParagraphAlignment.CENTER;
                var headerCell1_Run = headerCell1.CreateRun();
                headerCell1_Run.IsBold = true;
                headerCell1_Run.SetText("N.EM/SUBEM");

                var c2 = rowHeader.CreateCell();
                var headerCell2 = c2.AddParagraph();
                headerCell2.Alignment = ParagraphAlignment.CENTER;
                var headerCell2_Run = headerCell2.CreateRun();
                headerCell2_Run.IsBold = true;
                headerCell2_Run.SetText("Testo EM/SUBEM");

                var c3 = rowHeader.CreateCell();
                var headerCell3 = c3.AddParagraph();
                headerCell3.Alignment = ParagraphAlignment.CENTER;
                var headerCell3_Run = headerCell3.CreateRun();
                headerCell3_Run.IsBold = true;
                headerCell3_Run.SetText("Relazione Illustrativa");

                var c4 = rowHeader.CreateCell();
                var headerCell4 = c4.AddParagraph();
                headerCell4.Alignment = ParagraphAlignment.CENTER;
                var headerCell4_Run = headerCell4.CreateRun();
                headerCell4_Run.IsBold = true;
                headerCell4_Run.SetText("Proponente");

                var c5 = rowHeader.CreateCell();
                var headerCell5 = c5.AddParagraph();
                headerCell5.Alignment = ParagraphAlignment.CENTER;
                var headerCell5_Run = headerCell5.CreateRun();
                headerCell5_Run.IsBold = true;
                headerCell5_Run.SetText("Firme prima del deposito");

                var c6 = rowHeader.CreateCell();
                var headerCell6 = c6.AddParagraph();
                headerCell6.Alignment = ParagraphAlignment.CENTER;
                var headerCell6_Run = headerCell6.CreateRun();
                headerCell6_Run.IsBold = true;
                headerCell6_Run.SetText("Stato");

                var emList = await _logicEm.ScaricaEmendamenti(id, ordine, persona);

                foreach (var em in emList)
                {
                    var row = table.CreateRow();
                    row.RemoveCell(0);

                    var c0_em = row.CreateCell();
                    var headerCell0_em = c0_em.AddParagraph();
                    headerCell0_em.Alignment = ParagraphAlignment.CENTER;
                    var headerCell0_Run_em = headerCell0_em.CreateRun();
                    headerCell0_Run_em.SetText(em.OrdineVotazione.ToString());

                    var c1_em = row.CreateCell();
                    var headerCell1_em = c1_em.AddParagraph();
                    headerCell1_em.Alignment = ParagraphAlignment.CENTER;
                    var headerCell1_Run_em = headerCell1_em.CreateRun();
                    headerCell1_Run_em.SetText(em.N_EM);

                    var c2_em = row.CreateCell();
                    var headerCell2_em = c2_em.AddParagraph();
                    headerCell2_em.Alignment = ParagraphAlignment.CENTER;
                    var headerCell2_Run_em = headerCell2_em.CreateRun();
                    headerCell2_Run_em.SetText(Utility.StripHTML(em.TestoEM_originale));

                    var c3_em = row.CreateCell();
                    var headerCell3_em = c3_em.AddParagraph();
                    headerCell3_em.Alignment = ParagraphAlignment.CENTER;
                    var headerCell3_Run_em = headerCell3_em.CreateRun();
                    headerCell3_Run_em.SetText(string.IsNullOrEmpty(em.TestoREL_originale)
                        ? ""
                        : Utility.StripHTML(em.TestoREL_originale));

                    var proponente =
                        await _logicPersone.GetPersona(em.UIDPersonaProponente.Value,
                            em.id_gruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID);

                    var c4_em = row.CreateCell();
                    var headerCell4_em = c4_em.AddParagraph();
                    headerCell4_em.Alignment = ParagraphAlignment.CENTER;
                    var headerCell4_Run_em = headerCell4_em.CreateRun();
                    headerCell4_Run_em.SetText(proponente.DisplayName);

                    var firme = await _logicFirme.GetFirme(em, FirmeTipoEnum.TUTTE);
                    var firmeDto = firme.Select(Mapper.Map<FIRME, FirmeDto>).ToList();

                    var firmatari_opendata = "--";
                    try
                    {
                        if (firmeDto.Any(f =>
                            f.Timestamp < Convert.ToDateTime(em.DataDeposito)))
                            firmatari_opendata = await _logicEm.GetFirmatariEM_OPENDATA(firmeDto.Where(f =>
                                    f.Timestamp < Convert.ToDateTime(em.DataDeposito)),
                                persona.CurrentRole);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    var c5_em = row.CreateCell();
                    var headerCell5_em = c5_em.AddParagraph();
                    headerCell5_em.Alignment = ParagraphAlignment.CENTER;
                    var headerCell5_Run_em = headerCell5_em.CreateRun();
                    headerCell5_Run_em.SetText(firmatari_opendata);

                    var c6_em = row.CreateCell();
                    var headerCell6_em = c6_em.AddParagraph();
                    headerCell6_em.Alignment = ParagraphAlignment.CENTER;
                    var headerCell6_Run_em = headerCell6_em.CreateRun();
                    headerCell6_Run_em.SetText(em.STATI_EM.Stato);
                }

                return await Response(FilePathComplete, doc);
            }
            catch (Exception e)
            {
                Log.Error("Logic - EsportaGrigliaWord", e);
                throw e;
            }
        }

        public async Task<HttpResponseMessage> HTMLtoWORD(Guid attoUId, PersonaDto persona)
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
                        converter.ParseHtml(await ComposeWordTable(attoUId, persona));

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
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    return result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<string> ComposeWordTable(Guid attoUID, PersonaDto persona)
        {
            var emList = await _logicEm.ScaricaEmendamenti(attoUID, OrdinamentoEnum.Votazione, persona);

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
            body += ComposeHeaderColumn("Firme");
            body += ComposeHeaderColumn("Stato");
            body += "</tr>";
            body += "</thead>";
            
            body += "<tbody>";
            body = emList.Aggregate(body, (current, em) => current + ComposeBodyRow(em));
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
            row += ComposeBodyColumn(em.Firme_OPENDATA);
            row += ComposeBodyColumn(em.STATI_EM.Stato);

            row += "</tr>";
            return row;
        }
        private string ComposeBodyColumn(string column_body)
        {
            return $"<td>{column_body}</td>";
        }

        public async Task<HttpResponseMessage> EsportaGrigliaReportExcel(Guid id, PersonaDto persona)
        {
            try
            {
                var FilePathComplete = GetLocalPath("xlsx");

                var atto = await _unitOfWork.Atti.Get(id);

                IWorkbook workbook = new XSSFWorkbook();
                var style = workbook.CreateCellStyle();
                style.FillForegroundColor = HSSFColor.Grey25Percent.Index;
                style.FillPattern = FillPattern.SolidForeground;
                var styleReport = workbook.CreateCellStyle();
                styleReport.FillForegroundColor = HSSFColor.LightGreen.Index;
                styleReport.FillPattern = FillPattern.SolidForeground;
                styleReport.Alignment = HorizontalAlignment.Center;

                var emList = await _logicEm.ScaricaEmendamenti(id, OrdinamentoEnum.Default, persona);

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
                    if (em.IDParte == (int) PartiEMEnum.Missione)
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
                SetColumnValue(ref rowEm, em.IDStato == (int) StatiEnum.Inammissibile ? "X" : "");

                if (reportType != ReportType.PCR)
                {
                    SetColumnValue(ref rowEm, em.IDStato == (int) StatiEnum.Ritirato ? "X" : "");
                    SetColumnValue(ref rowEm, em.IDStato == (int) StatiEnum.Approvato ? "X" : "");
                    SetColumnValue(ref rowEm, em.IDStato == (int) StatiEnum.Non_Approvato ? "X" : "");
                    SetColumnValue(ref rowEm, em.IDStato == (int) StatiEnum.Decaduto ? "X" : "");
                }

                SetColumnValue(ref rowEm, em.NOTE_EM);
                SetColumnValue(ref rowEm, em.NOTE_Griglia);
            }

            var countEM = emendamentiDtos.Count();
            var approvati = emendamentiDtos.Count(em => em.IDStato == (int) StatiEnum.Approvato);
            var non_approvati = emendamentiDtos.Count(em => em.IDStato == (int) StatiEnum.Non_Approvato);
            var ritirati = emendamentiDtos.Count(em => em.IDStato == (int) StatiEnum.Ritirato);
            var decaduti = emendamentiDtos.Count(em => em.IDStato == (int) StatiEnum.Decaduto);
            var inammissibili = emendamentiDtos.Count(em => em.IDStato == (int) StatiEnum.Inammissibile);

            var rowReport = sheet.CreateRow(sheet.LastRowNum + 1);
            rowReport.RowStyle = styleR;
            var cellCount = rowReport.CreateCell(0);
            cellCount.CellStyle = styleR;
            cellCount.SetCellValue(countEM);
            var cellInamm = rowReport.CreateCell(8);
            cellInamm.CellStyle = styleR;
            cellInamm.SetCellValue(inammissibili);

            if (reportType == ReportType.PCR) return sheet;

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
            if (reportType == ReportType.PROGRESSIVO) return;
            var rowSep = sheet.CreateRow(sheet.LastRowNum + 1);
            rowSep.RowStyle = style;
        }

        private short GetColumn(short column)
        {
            return column < 0 ? (short) 0 : column;
        }

        private string GetLocalPath(string extension)
        {
            var _pathTemp = AppSettingsConfiguration.CartellaTemp;
            if (!Directory.Exists(_pathTemp))
                Directory.CreateDirectory(_pathTemp);
            var nameFile = $"PEM_{DateTime.Now:ddMMyyyy_hhmmss}.{extension}";
            var FilePathComplete = Path.Combine(_pathTemp, nameFile);

            return FilePathComplete;
        }

        private async Task<HttpResponseMessage> Response<T>(string path, T book)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                var bookType = book.GetType();
                if (typeof(XSSFWorkbook) == bookType)
                {
                    var t = book as IWorkbook;
                    t?.Write(fs);
                }

                if (typeof(XWPFDocument) == bookType)
                {
                    var t = book as XWPFDocument;
                    t?.Write(fs);
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
                Content = new ByteArrayContent(stream.GetBuffer())
            };
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = Path.GetFileName(path)
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

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