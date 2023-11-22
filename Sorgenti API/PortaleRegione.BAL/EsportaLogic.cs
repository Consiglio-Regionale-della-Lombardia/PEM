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
using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using HtmlToOpenXml;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PortaleRegione.API.Controllers;
using PortaleRegione.Common;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Request;
using PortaleRegione.DTO.Response;
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Color = System.Drawing.Color;

namespace PortaleRegione.BAL
{
    public class EsportaLogic : BaseLogic
    {
        public EsportaLogic(IUnitOfWork unitOfWork, EmendamentiLogic logicEm, DASILogic logicDASI,
            FirmeLogic logicFirme, AttiLogic logicAtti, PersoneLogic logicPersona)
        {
            _unitOfWork = unitOfWork;
            _logicEm = logicEm;
            _logicDasi = logicDASI;
            _logicFirme = logicFirme;
            _logicAtti = logicAtti;
            _logicPersona = logicPersona;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<HttpResponseMessage> EsportaGrigliaExcel(EmendamentiViewModel model, PersonaDto persona)
        {
            try
            {
                var excelPackage = new ExcelPackage();
                var excelSheet = excelPackage.Workbook.Worksheets.Add(
                    $"{Utility.GetText_Tipo(model.Atto.IDTipoAtto)} {model.Atto.NAtto.Replace('/', '-')}");

                var row = 1;
                var columnIndex = 1; // Inizia dalla colonna 1

                SetColumnValue(ref row, excelSheet, "Ordine", ref columnIndex);
                if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM)
                {
                    SetColumnValue(ref row, excelSheet, "IDEM", ref columnIndex);
                    SetColumnValue(ref row, excelSheet, "Atto", ref columnIndex);
                }

                SetColumnValue(ref row, excelSheet, "Numero EM", ref columnIndex);
                SetColumnValue(ref row, excelSheet, "Data Deposito", ref columnIndex);
                SetColumnValue(ref row, excelSheet, "Stato", ref columnIndex);
                SetColumnValue(ref row, excelSheet, "Tipo", ref columnIndex);
                SetColumnValue(ref row, excelSheet, "Parte", ref columnIndex);
                SetColumnValue(ref row, excelSheet, "Articolo", ref columnIndex);
                SetColumnValue(ref row, excelSheet, "Comma", ref columnIndex);
                SetColumnValue(ref row, excelSheet, "Lettera", ref columnIndex);
                SetColumnValue(ref row, excelSheet, "Titolo", ref columnIndex);
                SetColumnValue(ref row, excelSheet, "Capo", ref columnIndex);

                if (model.Atto.VIS_Mis_Prog)
                {
                    SetColumnValue(ref row, excelSheet, "Missione", ref columnIndex);
                    SetColumnValue(ref row, excelSheet, "Programma", ref columnIndex);
                    SetColumnValue(ref row, excelSheet, "TitoloB", ref columnIndex);
                }

                SetColumnValue(ref row, excelSheet, "Proponente", ref columnIndex);
                SetColumnValue(ref row, excelSheet, "Area Politica", ref columnIndex);
                SetColumnValue(ref row, excelSheet, "Firmatari", ref columnIndex);
                SetColumnValue(ref row, excelSheet, "Firmatari dopo deposito", ref columnIndex);
                SetColumnValue(ref row, excelSheet, "LinkEM", ref columnIndex);

                var legislatura = await _unitOfWork.Legislature.Get(model.Atto.SEDUTE.id_legislatura);
                var articoli = await _unitOfWork.Articoli.GetArticoli(model.Atto.UIDAtto);
                var commi = new List<COMMI>();
                foreach (var art in articoli)
                {
                    commi.AddRange(await _unitOfWork.Commi.GetCommi(art.UIDArticolo));
                }

                var lettere = new List<LETTERE>();
                foreach (var comm in commi)
                {
                    lettere.AddRange(await _unitOfWork.Lettere.GetLettere(comm.UIDComma));
                }

                var firmeComplete = await _logicFirme.GetFirmatariAtto(model.Atto.UIDAtto);

                var emList = await _logicEm.ScaricaEmendamenti(model, persona, false, true);
                foreach (var em in emList)
                {
                    row++;
                    columnIndex = 1;
                    SetColumnValue(ref row, excelSheet,
                        model.Ordinamento == OrdinamentoEnum.Presentazione
                            ? em.OrdinePresentazione.ToString()
                            : em.OrdineVotazione.ToString()
                        , ref columnIndex);

                    if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM)
                    {
                        SetColumnValue(ref row, excelSheet, em.UIDEM.ToString(), ref columnIndex);
                        SetColumnValue(ref row, excelSheet,
                            $"{Utility.GetText_Tipo(model.Atto.IDTipoAtto)}-{model.Atto.NAtto}-{legislatura.num_legislatura}",
                            ref columnIndex);
                    }
                    SetColumnValue(ref row, excelSheet, em.N_EM, ref columnIndex);
                    SetColumnValue(ref row, excelSheet, em.DataDeposito, ref columnIndex);
                    SetColumnValue(ref row, excelSheet,
                        persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                            ? $"{em.STATI_EM.IDStato}-{em.STATI_EM.Stato}"
                            : em.STATI_EM.Stato, ref columnIndex);
                    SetColumnValue(ref row, excelSheet,
                        persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                            ? $"{em.TIPI_EM.IDTipo_EM}-{em.TIPI_EM.Tipo_EM}"
                            : em.TIPI_EM.Tipo_EM, ref columnIndex);
                    SetColumnValue(ref row, excelSheet,
                        persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                            ? $"{em.IDParte}-{em.PARTI_TESTO.Parte}"
                            : em.PARTI_TESTO.Parte, ref columnIndex);

                    if (em.UIDArticolo.HasValue && em.IDParte == (int)PartiEMEnum.Articolo)
                    {
                        var articolo = articoli.First(i => i.UIDArticolo == em.UIDArticolo.Value);
                        SetColumnValue(ref row, excelSheet, articolo.Articolo, ref columnIndex);
                    }
                    else
                    {
                        SetColumnValue(ref row, excelSheet, "", ref columnIndex);
                    }

                    if (em.UIDComma.HasValue && em.UIDComma != Guid.Empty && em.IDParte == (int)PartiEMEnum.Articolo)
                    {
                        var comma = commi.First(i => i.UIDComma == em.UIDComma.Value);
                        SetColumnValue(ref row, excelSheet, comma.Comma, ref columnIndex);
                    }
                    else
                    {
                        SetColumnValue(ref row, excelSheet, "", ref columnIndex);
                    }

                    if (em.UIDLettera.HasValue && em.UIDLettera != Guid.Empty &&
                        em.IDParte == (int)PartiEMEnum.Articolo)
                    {
                        var lettera = lettere.First(i => i.UIDLettera == em.UIDLettera.Value);
                        SetColumnValue(ref row, excelSheet, lettera.Lettera, ref columnIndex);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(em.NLettera) && em.IDParte == (int)PartiEMEnum.Articolo)
                            SetColumnValue(ref row, excelSheet, em.NLettera, ref columnIndex);
                        else
                            SetColumnValue(ref row, excelSheet, "", ref columnIndex);
                    }

                    SetColumnValue(ref row, excelSheet, em.NTitolo, ref columnIndex);
                    SetColumnValue(ref row, excelSheet, em.NCapo, ref columnIndex);

                    if (model.Atto.VIS_Mis_Prog)
                    {
                        SetColumnValue(ref row, excelSheet, em.NMissione.ToString(), ref columnIndex);
                        SetColumnValue(ref row, excelSheet, em.NProgramma.ToString(), ref columnIndex);
                        SetColumnValue(ref row, excelSheet, em.NTitoloB.ToString(), ref columnIndex);
                    }

                    SetColumnValue(ref row, excelSheet,
                        persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                            ? $"{em.PersonaProponente.id_persona}-{em.PersonaProponente.DisplayName}"
                            : em.PersonaProponente.DisplayName, ref columnIndex);
                    SetColumnValue(ref row, excelSheet, "", ref columnIndex);

                    if (!string.IsNullOrEmpty(em.DataDeposito))
                    {
                        var firmeDto = firmeComplete.Where(f => f.UIDEM == em.UIDEM).ToList();

                        var firmatari_opendata_ante = "--";
                        try
                        {
                            if (firmeDto.Any(f => f.Timestamp < Convert.ToDateTime(em.DataDeposito)))
                                firmatari_opendata_ante =
                                    _logicEm.GetFirmatariEM_OPENDATA(firmeDto.Where(f =>
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
                            if (firmeDto.Any(f => f.Timestamp > Convert.ToDateTime(em.DataDeposito)))
                                firmatari_opendata_post =
                                    _logicEm.GetFirmatariEM_OPENDATA(firmeDto.Where(f =>
                                            f.Timestamp > Convert.ToDateTime(em.DataDeposito)),
                                        persona.CurrentRole);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                        SetColumnValue(ref row, excelSheet, firmatari_opendata_ante, ref columnIndex);
                        SetColumnValue(ref row, excelSheet, firmatari_opendata_post, ref columnIndex);
                    }
                    else
                    {
                        SetColumnValue(ref row, excelSheet, "--", ref columnIndex);
                        SetColumnValue(ref row, excelSheet, "--", ref columnIndex);
                    }

                    SetColumnValue(ref row, excelSheet, $"{AppSettingsConfiguration.urlPEM_ViewEM}{em.UID_QRCode}",
                        ref columnIndex);
                }

                // Imposta il percorso della cartella temporanea sul server
                var tempFolderPath = HttpContext.Current.Server.MapPath("~/esportazioni");

                // Salva il file Excel nella cartella virtuale
                var fileName =
                    $"Esportazione_{excelSheet.Name}_{Guid.NewGuid()}.xlsx"; // Utilizza un nome di file univoco
                var filePath = Path.Combine(tempFolderPath, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    excelPackage.SaveAs(fileStream);
                }

                // Creazione della risposta con il link di download
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent($"{AppSettingsConfiguration.URL_API}/esportazioni/{fileName}");
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

                return response;
            }
            catch (Exception e)
            {
                Log.Error("Logic - EsportaGrigliaXLS", e);
                throw e;
            }
        }

        public async Task<HttpResponseMessage> EsportaGrigliaReportExcel(EmendamentiViewModel model, PersonaDto persona)
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    var emList = await _logicEm.ScaricaEmendamenti(model, persona);

                    await NewSheet(package, nameof(ReportType.UOLA), model.Atto.UIDAtto, ReportType.UOLA, emList
                        .OrderBy(em => em.OrdineVotazione)
                        .ThenBy(em => em.Rif_UIDEM)
                        .ThenBy(em => em.IDStato));
                    await NewSheet(package, nameof(ReportType.PCR), model.Atto.UIDAtto, ReportType.PCR, emList
                        .OrderBy(em => em.OrdineVotazione)
                        .ThenBy(em => em.Rif_UIDEM)
                        .ThenBy(em => em.IDStato));
                    await NewSheet(package, nameof(ReportType.PROGRESSIVO), model.Atto.UIDAtto, ReportType.PROGRESSIVO,
                        emList
                            .OrderBy(em => em.Rif_UIDEM)
                            .ThenBy(em => em.OrdinePresentazione));

                    // Imposta il percorso della cartella temporanea sul server
                    var tempFolderPath = HttpContext.Current.Server.MapPath("~/esportazioni");

                    // Salva il file Excel nella cartella virtuale
                    var fileName =
                        $"Esportazione_{Utility.GetText_Tipo(model.Atto.IDTipoAtto)} {model.Atto.NAtto.Replace('/', '-')}_{Guid.NewGuid()}.xlsx"; // Utilizza un nome di file univoco
                    var filePath = Path.Combine(tempFolderPath, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        package.SaveAs(fileStream);
                    }

                    // Creazione della risposta con il link di download
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent($"{AppSettingsConfiguration.URL_API}/esportazioni/{fileName}");
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

                    return response;
                }
            }
            catch (Exception e)
            {
                Log.Error("Logic - EsportaGrigliaReportXLS", e);
                throw e;
            }
        }

        private async Task NewSheet(ExcelPackage package, string sheetName, Guid attoUId, ReportType reportType,
            IEnumerable<EmendamentiDto> emendamentiDtos)
        {
            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            var atto = await _unitOfWork.Atti.Get(attoUId);

            //HEADER
            var row = 1;
            var columnIndex = 1;
            SetColumnValue(ref row, worksheet, "Numero EM", ref columnIndex);
            SetColumnValue(ref row, worksheet, "Proponente", ref columnIndex);
            SetColumnValue(ref row, worksheet, "Articolo", ref columnIndex);
            SetColumnValue(ref row, worksheet, "Comma", ref columnIndex);
            SetColumnValue(ref row, worksheet, "Lettera", ref columnIndex);
            SetColumnValue(ref row, worksheet, "Titolo", ref columnIndex);
            SetColumnValue(ref row, worksheet, "Capo", ref columnIndex);

            if (atto.VIS_Mis_Prog)
            {
                SetColumnValue(ref row, worksheet, "Missione", ref columnIndex);
                SetColumnValue(ref row, worksheet, "Programma", ref columnIndex);
                SetColumnValue(ref row, worksheet, "TitoloB", ref columnIndex);
            }

            SetColumnValue(ref row, worksheet, "Contenuto", ref columnIndex);
            SetColumnValue(ref row, worksheet, "INAMM.", ref columnIndex);

            if (reportType != ReportType.PCR)
            {
                SetColumnValue(ref row, worksheet, "RITIRATO", ref columnIndex);
                SetColumnValue(ref row, worksheet, "SI", ref columnIndex);
                SetColumnValue(ref row, worksheet, "NO", ref columnIndex);
                SetColumnValue(ref row, worksheet, "DECADE", ref columnIndex);
            }

            SetColumnValue(ref row, worksheet, "NOTE", ref columnIndex);
            SetColumnValue(ref row, worksheet, "NOTE_RISERVATE", ref columnIndex);

            var oldParte = emendamentiDtos.First().IDParte;
            int currentParte_Missione = default, oldParte_Missione = default;
            Guid currentParte_Articolo = default, oldParte_Articolo = default;

            row++;

            foreach (var em in emendamentiDtos)
            {
                var currentParte = em.IDParte;
                if (oldParte != currentParte)
                {
                    SetSeparator(ref worksheet, ref reportType, ref row);
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
                                SetSeparator(ref worksheet, ref reportType, ref row);

                            oldParte_Missione = currentParte_Missione;
                        }
                    }
                    else
                    {
                        if (em.UIDArticolo.HasValue)
                        {
                            currentParte_Articolo = em.UIDArticolo!.Value;
                            if (oldParte_Articolo != currentParte_Articolo && oldParte_Articolo != default)
                                SetSeparator(ref worksheet, ref reportType, ref row);

                            oldParte_Articolo = currentParte_Articolo;
                        }
                    }
                }

                columnIndex = 1; // Reset columnIndex for each row
                SetColumnValue(ref row, worksheet, em.N_EM, ref columnIndex);
                SetColumnValue(ref row, worksheet, em.PersonaProponente.DisplayName, ref columnIndex);
                if (em.UIDArticolo.HasValue && em.UIDArticolo.Value != Guid.Empty)
                    SetColumnValue(ref row, worksheet, em.ARTICOLI.Articolo, ref columnIndex);
                else
                    SetColumnValue(ref row, worksheet, "--", ref columnIndex);

                SetColumnValue(ref row, worksheet, em.COMMI?.Comma ?? "--", ref columnIndex);
                SetColumnValue(ref row, worksheet, em.LETTERE?.Lettera ?? em.NLettera ?? "--", ref columnIndex);
                SetColumnValue(ref row, worksheet, em.NTitolo, ref columnIndex);
                SetColumnValue(ref row, worksheet, em.NCapo, ref columnIndex);

                if (atto.VIS_Mis_Prog)
                {
                    SetColumnValue(ref row, worksheet, em.NMissione?.ToString() ?? "--", ref columnIndex);
                    SetColumnValue(ref row, worksheet, em.NProgramma?.ToString() ?? "--", ref columnIndex);
                    SetColumnValue(ref row, worksheet, em.NTitoloB?.ToString() ?? "--", ref columnIndex);
                }

                SetColumnValue(ref row, worksheet, em.TIPI_EM.Tipo_EM, ref columnIndex);
                SetColumnValue(ref row, worksheet, em.IDStato == (int)StatiEnum.Inammissibile ? "X" : "",
                    ref columnIndex);

                if (reportType != ReportType.PCR)
                {
                    SetColumnValue(ref row, worksheet, em.IDStato == (int)StatiEnum.Ritirato ? "X" : "",
                        ref columnIndex);
                    SetColumnValue(ref row, worksheet,
                        em.IDStato == (int)StatiEnum.Approvato || em.IDStato == (int)StatiEnum.Approvato_Con_Modifiche
                            ? "X"
                            : "", ref columnIndex);
                    SetColumnValue(ref row, worksheet, em.IDStato == (int)StatiEnum.Non_Approvato ? "X" : "",
                        ref columnIndex);
                    SetColumnValue(ref row, worksheet, em.IDStato == (int)StatiEnum.Decaduto ? "X" : "",
                        ref columnIndex);
                }

                SetColumnValue(ref row, worksheet, em.NOTE_Griglia, ref columnIndex);
                SetColumnValue(ref row, worksheet, em.NOTE_EM, ref columnIndex);

                row++;
            }

            // Calculate statistics and set summary row
            var countEM = emendamentiDtos.Count();
            var approvati = emendamentiDtos.Count(em =>
                em.IDStato == (int)StatiEnum.Approvato || em.IDStato == (int)StatiEnum.Approvato_Con_Modifiche);
            var non_approvati = emendamentiDtos.Count(em => em.IDStato == (int)StatiEnum.Non_Approvato);
            var ritirati = emendamentiDtos.Count(em => em.IDStato == (int)StatiEnum.Ritirato);
            var decaduti = emendamentiDtos.Count(em => em.IDStato == (int)StatiEnum.Decaduto);
            var inammissibili = emendamentiDtos.Count(em => em.IDStato == (int)StatiEnum.Inammissibile);

            var rowReport = worksheet.Row(row);
            rowReport.Style.Fill.PatternType = ExcelFillStyle.Solid;
            rowReport.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);

            SetColumnValue(ref row, worksheet, countEM.ToString(), 1);
            var colonna_conteggi = atto.VIS_Mis_Prog ? 11 : 8;
            SetColumnValue(ref row, worksheet, inammissibili.ToString(), colonna_conteggi + 1);

            if (reportType != ReportType.PCR)
            {
                SetColumnValue(ref row, worksheet, ritirati.ToString(), colonna_conteggi + 2);
                SetColumnValue(ref row, worksheet, approvati.ToString(), colonna_conteggi + 3);
                SetColumnValue(ref row, worksheet, non_approvati.ToString(), colonna_conteggi + 4);
                SetColumnValue(ref row, worksheet, decaduti.ToString(), colonna_conteggi + 5);
            }
        }

        private void SetSeparator(ref ExcelWorksheet sheet, ref ReportType reportType, ref int row)
        {
            if (reportType == ReportType.PROGRESSIVO) return;

            sheet.InsertRow(row, 1);

            var rowSep = sheet.Row(row);
            rowSep.Style.Fill.PatternType = ExcelFillStyle.Solid;
            rowSep.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            row++;
        }


        public async Task<HttpResponseMessage> EsportaGrigliaZipDASI(List<Guid> data)
        {
            var attiList = new List<AttoDASIDto>();
            foreach (var uid in data)
            {
                var dto = await _logicDasi.GetAttoDto(uid);
                if (dto.IDStato == (int)StatiAttoEnum.BOZZA_CARTACEA) continue;
                attiList.Add(dto);
            }

            var pdfs = await GetPDF(attiList);

            return ResponseZip(pdfs);
        }

        private async Task<List<FileModel>> GetPDF(List<AttoDASIDto> attiList)
        {
            var pdfs = new List<FileModel>();
            foreach (var dto in attiList)
            {
                var pdf = await _logicDasi.PDFIstantaneo(Mapper.Map<AttoDASIDto, ATTI_DASI>(dto), null);
                pdfs.Add(new FileModel
                {
                    Name = dto.Display + ".pdf",
                    Content = pdf
                });
            }

            return pdfs;
        }

        public async Task<HttpResponseMessage> EsportaGrigliaExcelDASI(List<Guid> data)
        {
            var tempFolderPath = HttpContext.Current.Server.MapPath("~/esportazioni");
            var filename = $"EsportazioneDASI{DateTime.Now.Ticks}.xlsx";
            var FilePathComplete = Path.Combine(tempFolderPath, filename);

            using (var package = new ExcelPackage())
            {
                var attiList = new List<AttoDASIDto>();
                foreach (var uid in data)
                {
                    var dto = await _logicDasi.GetAttoDto(uid);
                    if (dto.IDStato == (int)StatiAttoEnum.BOZZA_CARTACEA) continue;
                    attiList.Add(dto);
                }

                var dasiSheet = package.Workbook.Worksheets.Add("Atti");
                FillSheetDASI_Atti(dasiSheet, attiList);

                var firmatariList = await _logicDasi.ScaricaAtti_Firmatari(attiList);
                var firmatariSheet = package.Workbook.Worksheets.Add("Firmatari");
                await FillSheetDASI_Firmatari(firmatariSheet, firmatariList);

                var controlliSheet = package.Workbook.Worksheets.Add("Controlli");
                FillSheetDASI_Controlli(controlliSheet);

                var lookupSheet = package.Workbook.Worksheets.Add("LOOKUP");

                package.SaveAs(new FileInfo(FilePathComplete));
            }

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"{AppSettingsConfiguration.URL_API}/esportazioni/{filename}")
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

            return result;
        }

        private void FillSheetDASI_Controlli(ExcelWorksheet sheet)
        {
            try
            {
                sheet.Cells[1, 1].Value = "Codice di controllo del template:";
                sheet.Cells[1, 2].Value = "TY86Z5s6VfZtRFF46fi54qskISyv36_v007";

                sheet.Cells[2, 1].Value = "Numero massimo atti da importare:";
                sheet.Cells[2, 2].Value = 1000;
                sheet.Cells[2, 2].Style.Numberformat.Format = "0";

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task FillSheetDASI_Firmatari(ExcelWorksheet sheet, List<AttiFirmeDto> firmatariList)
        {
            try
            {
                //HEADER
                sheet.Cells[1, 1].Value = "TIPO ATTO";
                sheet.Cells[1, 2].Value = "NUMERO ATTO";
                sheet.Cells[1, 3].Value = "FIRMATARIO";
                sheet.Cells[1, 4].Value = "GRUPPO";
                sheet.Cells[1, 5].Value = "DATA FIRMA";
                sheet.Cells[1, 6].Value = "DATA RITIRO FIRMA";
                sheet.Cells[1, 7].Value = "PRIMO FIRMATARIO";

                int row = 2;
                foreach (var firma in firmatariList)
                {
                    try
                    {
                        var gruppo_firmatario = await _logicPersona.GetGruppo(firma.id_gruppo);

                        var atto = await _logicDasi.GetAttoDto(firma.UIDAtto);
                        sheet.Cells[row, 1].Value = Utility.GetText_Tipo(atto);
                        sheet.Cells[row, 2].Value = Convert.ToInt32(atto.NAtto);
                        sheet.Cells[row, 2].Style.Numberformat.Format = "0";

                        var firmacert = firma.FirmaCert;
                        var indiceParentesiApertura = firmacert.IndexOf('(');
                        firmacert = firmacert.Remove(indiceParentesiApertura - 1);
                        sheet.Cells[row, 3].Value = firmacert;
                        sheet.Cells[row, 4].Value = gruppo_firmatario.codice_gruppo; // #862 fix: codice gruppo per firmatario
                        sheet.Cells[row, 5].Value = new DateTime(
                            firma.Timestamp.Year,
                            firma.Timestamp.Month,
                            firma.Timestamp.Day);
                        sheet.Cells[row, 5].Style.Numberformat.Format = "dd/MM/yyyy";

                        if (!string.IsNullOrEmpty(firma.Data_ritirofirma))
                        {
                            var data_ritiro = Convert.ToDateTime(firma.Data_ritirofirma);
                            sheet.Cells[row, 6].Value = new DateTime(
                                data_ritiro.Year,
                                data_ritiro.Month,
                                data_ritiro.Day);
                        }

                        sheet.Cells[row, 6].Style.Numberformat.Format = "dd/MM/yyyy";

                        sheet.Cells[row, 7].Value = firma.PrimoFirmatario ? "SI" : "NO";
                        row++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void FillSheetDASI_Atti(ExcelWorksheet sheet, IEnumerable<AttoDASIDto> attiList)
        {
            try
            {
                //HEADER
                var headerRow = 2;
                sheet.Cells[headerRow, 1].Value = "TIPO ATTO";
                sheet.Cells[headerRow, 2].Value = "TIPO MOZIONE";
                sheet.Cells[headerRow, 3].Value = "NUMERO ATTO";
                sheet.Cells[headerRow, 4].Value = "STATO";
                sheet.Cells[headerRow, 5].Value = "PROTOCOLLO";
                sheet.Cells[headerRow, 6].Value = "CODICE MATERIA";
                sheet.Cells[headerRow, 7].Value = "DATA PRESENTAZIONE";
                sheet.Cells[headerRow, 8].Value = "OGGETTO";
                sheet.Cells[headerRow, 9].Value = "OGGETTO PRESENTATO/OGGETTO COMMISSIONE";
                sheet.Cells[headerRow, 10].Value = "OGGETTO APPROVATO/OGGETTO ASSEMBLEA";
                sheet.Cells[headerRow, 11].Value = "RISPOSTA RICHIESTA";
                sheet.Cells[headerRow, 12].Value = "AREA";
                sheet.Cells[headerRow, 13].Value = "DATA ANNUNZIO";
                sheet.Cells[headerRow, 14].Value = "PUBBLICATO";
                sheet.Cells[headerRow, 15].Value = "RISPOSTA FORNITA";
                sheet.Cells[headerRow, 16].Value = "ITER MULTIPLO";
                sheet.Cells[headerRow, 17].Value = "NOTE RISPOSTA";
                sheet.Cells[headerRow, 18].Value = "ANNOTAZIONI";
                sheet.Cells[headerRow, 19].Value = "TIPO CHIUSURA ITER";
                sheet.Cells[headerRow, 20].Value = "DATA CHIUSURA ITER";
                sheet.Cells[headerRow, 21].Value = "NOTE CHIUSURA ITER";
                sheet.Cells[headerRow, 22].Value = "RISULTATO VOTAZIONE";
                sheet.Cells[headerRow, 23].Value = "DATA TRASMISSIONE";
                sheet.Cells[headerRow, 24].Value = "TIPO CHIUSURA ITER";
                sheet.Cells[headerRow, 25].Value = "DATA CHIUSURA ITER";
                sheet.Cells[headerRow, 26].Value = "NOTE CHIUSURA ITER";
                sheet.Cells[headerRow, 27].Value = "TIPO VOTAZIONE";
                sheet.Cells[headerRow, 28].Value = "DCR";
                sheet.Cells[headerRow, 29].Value = "NUMERO DCR";
                sheet.Cells[headerRow, 30].Value = "NUMERO DCRC";
                sheet.Cells[headerRow, 31].Value = "BURL";
                sheet.Cells[headerRow, 32].Value = "EMENDATO";
                sheet.Cells[headerRow, 33].Value = "DATA COMUNICAZIONE ASSEMBLEA";
                sheet.Cells[headerRow, 34].Value = "AREA TEMATICA";
                sheet.Cells[headerRow, 35].Value = "DATA TRASMISSIONE";
                sheet.Cells[headerRow, 36].Value = "ALTRI SOGGETTI";
                sheet.Cells[headerRow, 37].Value = "COMPETENZA";
                sheet.Cells[headerRow, 38].Value = "IMPEGNI E SCADENZE";
                sheet.Cells[headerRow, 39].Value = "STATO DI ATTUAZIONE";
                sheet.Cells[headerRow, 40].Value = "CONCLUSO";

                int row = 3;
                foreach (var atto in attiList)
                {
                    sheet.Cells[row, 1].Value = Utility.GetText_Tipo(atto);
                    var tipoMozione = "";
                    if (atto.Tipo == (int)TipoAttoEnum.MOZ)
                    {
                        tipoMozione = Utility.GetText_TipoMOZDASI(atto.TipoMOZ);

                    }

                    sheet.Cells[row, 2].Value = tipoMozione;
                    sheet.Cells[row, 3].Value = Convert.ToInt32(atto.NAtto);
                    sheet.Cells[row, 3].Style.Numberformat.Format = "0";

                    sheet.Cells[row, 4].Value = Utility.GetText_StatoDASI(atto.IDStato, true);
                    sheet.Cells[row, 5].Value = "";
                    sheet.Cells[row, 6].Value = "";
                    sheet.Cells[row, 7].Value = new DateTime(
                        atto.Timestamp.Year,
                        atto.Timestamp.Month,
                        atto.Timestamp.Day);
                    sheet.Cells[row, 7].Style.Numberformat.Format = "dd/MM/yyyy";

                    if ((TipoAttoEnum)atto.Tipo == TipoAttoEnum.IQT
                        || (TipoAttoEnum)atto.Tipo == TipoAttoEnum.ITR
                        || (TipoAttoEnum)atto.Tipo == TipoAttoEnum.ITL)
                    {
                        sheet.Cells[row, 8].Value = atto.Oggetto;
                    }
                    else
                    {
                        sheet.Cells[row, 8].Value = "";
                    }

                    //Matteo Cattapan #500 / #676
                    if ((TipoAttoEnum)atto.Tipo == TipoAttoEnum.MOZ
                        || (TipoAttoEnum)atto.Tipo == TipoAttoEnum.ODG)
                    {
                        sheet.Cells[row, 9].Value = atto.Oggetto; // oggetto presentato / oggetto commissione 
                        sheet.Cells[row, 10].Value = ""; // oggetto approvato / oggetto assemblea 
                    }
                    else
                    {
                        sheet.Cells[row, 9].Value = ""; // oggetto presentato / oggetto commissione 
                        sheet.Cells[row, 10].Value = ""; // oggetto approvato / oggetto assemblea 
                    }

                    sheet.Cells[row, 11].Value = Utility.GetText_TipoRispostaDASI(atto.IDTipo_Risposta, true); // risposta
                    row++;
                }
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
                var atto = await _logicAtti.GetAtto(attoUId);
                var tempFolderPath = HttpContext.Current.Server.MapPath("~/esportazioni");
                var fileName =
                    $"Esportazione_{Utility.GetText_Tipo(atto.IDTipoAtto)} {atto.NAtto.Replace('/', '-')}_{Guid.NewGuid()}.docx"; // Utilizza un nome di file univoco
                var filePath = Path.Combine(tempFolderPath, fileName);

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

                File.WriteAllBytes(filePath, generatedDocument.ToArray());

                // Creazione della risposta con il link di download
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent($"{AppSettingsConfiguration.URL_API}/esportazioni/{fileName}");
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

                return response;
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
            var request = new BaseRequest<EmendamentiDto>
            {
                filtro = new List<FilterStatement<EmendamentiDto>>
                {
                    new FilterStatement<EmendamentiDto>
                    {
                        PropertyId = nameof(EmendamentiDto.UIDAtto),
                        Connector = FilterStatementConnector.And,
                        Operation = Operation.EqualTo,
                        Value = attoUID
                    }
                },
                id = attoUID,
                ordine = ordine,
                page = 1,
                size = 1
            };
            var countEM = await _logicEm.CountEM(request, persona, (int)mode);
            request.size = countEM;

            try
            {
                var emList =
                    await _logicEm.GetEmendamenti(request,
                        persona,
                        (int)mode,
                        (int)ViewModeEnum.GRID,
                        null,
                        null);

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
                body = emList.Data.Results.Aggregate(body, (current, em) => current + ComposeBodyRow(em));
                body += "</tbody>";

                body += "</table>";
                body += "</body>";
                body += "</html>";
                return body;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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

        private void SetColumnValue(ref int row, ExcelWorksheet worksheet, string value, ref int columnIndex)
        {
            worksheet.Cells[row, columnIndex].Value = value;
            columnIndex++; // Incrementa l'indice della colonna per la prossima chiamata
        }

        private void SetColumnValue(ref int row, ExcelWorksheet worksheet, string value, int columnIndex)
        {
            worksheet.Cells[row, columnIndex].Value = value;
        }

        private HttpResponseMessage ResponseZip(List<FileModel> pdfs)
        {
            var outputMemoryStream = new MemoryStream();
            var zipStream = new ZipOutputStream(outputMemoryStream);
            zipStream.SetLevel(9);
            foreach (var internalZipFile in pdfs) AddToZip(zipStream, internalZipFile);
            zipStream.IsStreamOwner = false; // to stop the close and underlying stream
            zipStream.Close();

            outputMemoryStream.Position = 0;
            var zipByteArray = outputMemoryStream.ToArray();

            var tempFolderPath = HttpContext.Current.Server.MapPath("~/esportazioni");
            var filename = $"EsportazioneDASI{DateTime.Now.Ticks}.zip";
            var pathZip = Path.Combine(tempFolderPath, filename);

            using (var fileStream = new FileStream(pathZip, FileMode.Create))
            {
                fileStream.Write(zipByteArray, 0, zipByteArray.Length);
            }

            // Creazione della risposta con il link di download
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent($"{AppSettingsConfiguration.URL_API}/esportazioni/{filename}");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

            return response;
        }

        private void AddToZip(ZipOutputStream zipStream, FileModel internalZipFile)
        {
            var inputMemoryStream = new MemoryStream(internalZipFile.Content);

            var newZipEntry = new ZipEntry(internalZipFile.Name);
            newZipEntry.DateTime = DateTime.Now;
            newZipEntry.Size = internalZipFile.Content.Length;

            zipStream.PutNextEntry(newZipEntry);

            StreamUtils.Copy(inputMemoryStream, zipStream, new byte[1024]);
            zipStream.CloseEntry();
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

    internal class FileModel
    {
        public byte[] Content { get; set; }
        public string Name { get; set; }
    }
}