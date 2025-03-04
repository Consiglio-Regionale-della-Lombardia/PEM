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
using System.Web;
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
using Color = System.Drawing.Color;

namespace PortaleRegione.BAL
{
    public class EsportaLogic : BaseLogic
    {
        public EsportaLogic(IUnitOfWork unitOfWork, EmendamentiLogic logicEm, DASILogic logicDASI,
            FirmeLogic logicFirme, AttiLogic logicAtti, AttiFirmeLogic logicFirmeAtti, PersoneLogic logicPersona)
        {
            _unitOfWork = unitOfWork;
            _logicEm = logicEm;
            _logicDasi = logicDASI;
            _logicFirme = logicFirme;
            _logicAtti = logicAtti;
            _logicPersona = logicPersona;
            _logicAttiFirme = logicFirmeAtti;
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
                    var emGroup = emList.Where(em => !em.Rif_UIDEM.HasValue).OrderBy(em => em.OrdinePresentazione);
                    var subemGroup = emList.Where(em => em.Rif_UIDEM.HasValue).OrderBy(em => em.OrdinePresentazione);
                    var groupProgressivo = new List<EmendamentiDto>();
                    groupProgressivo.AddRange(emGroup);
                    groupProgressivo.AddRange(subemGroup);

                    await NewSheet(package, nameof(ReportType.UOLA), model.Atto.UIDAtto, ReportType.UOLA, emList
                        .OrderBy(em => em.OrdineVotazione));
                    await NewSheet(package, nameof(ReportType.PCR), model.Atto.UIDAtto, ReportType.PCR, emList
                        .OrderBy(em => em.OrdineVotazione));
                    await NewSheet(package, nameof(ReportType.PROGRESSIVO), model.Atto.UIDAtto, ReportType.PROGRESSIVO,
                        groupProgressivo);

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
                var firmatariList = new List<AttiFirmeDto>();
                foreach (var uid in data)
                {
                    var dto = await _logicDasi.GetAttoDto(uid);
                    if (dto.IDStato == (int)StatiAttoEnum.BOZZA_CARTACEA) continue;
                    attiList.Add(dto);

                    if (dto.FirmeAnte.Any())
                    {
                        firmatariList.AddRange(dto.FirmeAnte);
                    }

                    if (dto.FirmePost.Any())
                    {
                        firmatariList.AddRange(dto.FirmePost);
                    }
                }

                var dasiSheet = package.Workbook.Worksheets.Add("Atti");
                FillSheetDASI_Atti(dasiSheet, attiList);

                var firmatariSheet = package.Workbook.Worksheets.Add("Firmatari");
                FillSheetDASI_Firmatari(firmatariSheet, firmatariList, attiList);

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

        public async Task<HttpResponseMessage> EsportaXLSConsiglieriDASI(List<Guid> data, PersonaDto currentUser)
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
                FillSheetDASI_AttiConsiglieri(dasiSheet, attiList);

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

        private void FillSheetDASI_Firmatari(ExcelWorksheet sheet, List<AttiFirmeDto> firmatariList,
            List<AttoDASIDto> attiList)
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

                var row = 2;
                foreach (var firma in firmatariList)
                {
                    try
                    {
                        var atto = attiList.First(a => a.UIDAtto.Equals(firma.UIDAtto));
                        sheet.Cells[row, 1].Value = Utility.GetText_Tipo(atto);
                        sheet.Cells[row, 2].Value = Convert.ToInt32(atto.NAtto);
                        sheet.Cells[row, 2].Style.Numberformat.Format = "0";

                        var firmacert = firma.FirmaCert;
                        var indiceParentesiApertura = firmacert.IndexOf('(');
                        firmacert = firmacert.Remove(indiceParentesiApertura - 1);
                        sheet.Cells[row, 3].Value = firmacert;
                        sheet.Cells[row, 4].Value =
                            firma.EstraiGruppo(); // #862 fix: codice gruppo per firmatario, #1074
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
                // #994
                // #1287 Tolta la prima riga dell'excel

                //HEADER
                var headerRow = 1;

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
                sheet.Cells[headerRow, 19].Value = "MOTIVO CHIUSURA";
                sheet.Cells[headerRow, 20].Value = "DATA CHIUSURA ITER";
                sheet.Cells[headerRow, 21].Value = "NOTE CHIUSURA ITER";
                sheet.Cells[headerRow, 22].Value = "RISULTATO VOTAZIONE";
                sheet.Cells[headerRow, 23].Value = "DATA TRASMISSIONE";
                sheet.Cells[headerRow, 24].Value = "TIPO VOTAZIONE";
                sheet.Cells[headerRow, 25].Value = "DCR";
                sheet.Cells[headerRow, 26].Value = "NUMERO DCR";
                sheet.Cells[headerRow, 27].Value = "NUMERO DCRC";
                sheet.Cells[headerRow, 28].Value = "BURL";
                sheet.Cells[headerRow, 29].Value = "EMENDATO";
                sheet.Cells[headerRow, 30].Value = "DATA COMUNICAZIONE ASSEMBLEA";
                sheet.Cells[headerRow, 31].Value = "AREA TEMATICA";
                sheet.Cells[headerRow, 32].Value = "DATA TRASMISSIONE";
                sheet.Cells[headerRow, 33].Value = "ALTRI SOGGETTI";
                sheet.Cells[headerRow, 34].Value = "COMPETENZA";
                sheet.Cells[headerRow, 35].Value = "IMPEGNI E SCADENZE";
                sheet.Cells[headerRow, 36].Value = "STATO DI ATTUAZIONE";
                sheet.Cells[headerRow, 37].Value = "CONCLUSO";

                var row = 2;
                foreach (var atto in attiList)
                {
                    // Popolamento delle celle
                    sheet.Cells[row, 1].Value = atto.DisplayTipo;
                    sheet.Cells[row, 2].Value = atto.IsMOZ() ? atto.DisplayTipoMozione : "";
                    sheet.Cells[row, 3].Value = Convert.ToInt32(atto.NAtto);
                    sheet.Cells[row, 3].Style.Numberformat.Format = "0";
                    sheet.Cells[row, 4].Value = atto.DisplayStato;
                    sheet.Cells[row, 5].Value = atto.Protocollo;
                    sheet.Cells[row, 6].Value = atto.CodiceMateria;
                    sheet.Cells[row, 7].Value = atto.Timestamp?.ToString("dd/MM/yyyy");
                    sheet.Cells[row, 8].Value = atto.OggettoView();
                    sheet.Cells[row, 9].Value = atto.Oggetto; // Presentato
                    sheet.Cells[row, 10].Value = ""; // Assemblea
                    sheet.Cells[row, 11].Value = atto.DisplayTipoRispostaRichiesta;
                    sheet.Cells[row, 12].Value = atto.DisplayAreaPolitica;
                    sheet.Cells[row, 13].Value = atto.DataAnnunzio?.ToString("dd/MM/yyyy");
                    sheet.Cells[row, 14].Value = atto.Pubblicato ? "Sì" : "No";
                    sheet.Cells[row, 15].Value = ""; // Risposta fornita (potrebbe richiedere logica aggiuntiva)
                    sheet.Cells[row, 16].Value = atto.IterMultiplo ? "Sì" : "No";
                    sheet.Cells[row, 17].Value = atto.Note_Pubbliche;
                    sheet.Cells[row, 18].Value = ""; // Annotazioni
                    sheet.Cells[row, 19].Value = atto.DisplayTipoChiusuraIter;
                    sheet.Cells[row, 20].Value = atto.DataChiusuraIter?.ToString("dd/MM/yyyy");
                    sheet.Cells[row, 21].Value = atto.Note_Private;
                    sheet.Cells[row, 22].Value = "";
                    sheet.Cells[row, 23].Value = atto.DataTrasmissione?.ToString("dd/MM/yyyy");
                    sheet.Cells[row, 24].Value = atto.DisplayTipoVotazioneIter;
                    sheet.Cells[row, 25].Value = atto.DCR;
                    sheet.Cells[row, 26].Value = atto.DCRL;
                    sheet.Cells[row, 27].Value = atto.DCCR;
                    sheet.Cells[row, 28].Value = atto.BURL;
                    sheet.Cells[row, 29].Value = atto.Emendato ? "Sì" : "No";
                    sheet.Cells[row, 30].Value = atto.DataComunicazioneAssemblea?.ToString("dd/MM/yyyy");
                    sheet.Cells[row, 31].Value = Utility.StripWordMarkup(atto.AreaTematica);
                    sheet.Cells[row, 32].Value = atto.DataTrasmissione?.ToString("dd/MM/yyyy");
                    sheet.Cells[row, 33].Value = Utility.StripWordMarkup(atto.AltriSoggetti);
                    sheet.Cells[row, 34].Value = Utility.StripWordMarkup(atto.CompetenzaMonitoraggio);
                    sheet.Cells[row, 35].Value = Utility.StripWordMarkup(atto.ImpegniScadenze);
                    sheet.Cells[row, 36].Value = Utility.StripWordMarkup(atto.StatoAttuazione);
                    sheet.Cells[row, 37].Value = atto.IsChiuso ? "Sì" : "No";

                    row++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void FillSheetDASI_AttiConsiglieri(ExcelWorksheet sheet, IEnumerable<AttoDASIDto> attiList)
        {
            try
            {
                // #994
                // #1287

                //HEADER
                var headerRow = 1;

                sheet.Cells[headerRow, 1].Value = nameof(AttoDASIDto.Legislatura).ToUpper();
                sheet.Cells[headerRow, 2].Value = nameof(AttoDASIDto.Etichetta).ToUpper();
                sheet.Cells[headerRow, 3].Value = nameof(AttoDASIDto.Tipo).ToUpper();
                sheet.Cells[headerRow, 4].Value = nameof(AttoDASIDto.NAtto).ToUpper();
                sheet.Cells[headerRow, 5].Value = nameof(AttoDASIDto.TipoMOZ).ToUpper();
                sheet.Cells[headerRow, 6].Value = nameof(AttoDASIDto.Oggetto).ToUpper();
                sheet.Cells[headerRow, 7].Value = nameof(AttoDASIDto.Firme).ToUpper();
                sheet.Cells[headerRow, 8].Value = "DATA PRESENTAZIONE";
                sheet.Cells[headerRow, 9].Value = nameof(AttoDASIDto.DataAnnunzio).ToUpper();
                sheet.Cells[headerRow, 10].Value = nameof(AttoDASIDto.Firme_ritirate).ToUpper();
                sheet.Cells[headerRow, 11].Value = nameof(AttoDASIDto.ConteggioFirme).ToUpper();
                sheet.Cells[headerRow, 12].Value = nameof(AttoDASIDto.AreaPolitica).ToUpper();
                sheet.Cells[headerRow, 13].Value = "STATO";
                sheet.Cells[headerRow, 14].Value = "MOTIVO CHIUSURA";
                sheet.Cells[headerRow, 15].Value = nameof(AttoDASIDto.DCR).ToUpper();
                sheet.Cells[headerRow, 16].Value = nameof(AttoDASIDto.DataChiusuraIter).ToUpper();
                sheet.Cells[headerRow, 17].Value = "SEDUTA";
                sheet.Cells[headerRow, 18].Value = nameof(AttoDASIDto.TipoVotazioneIter).ToUpper();
                sheet.Cells[headerRow, 19].Value = nameof(AttoDASIDto.Emendato).ToUpper();
                sheet.Cells[headerRow, 20].Value = "INFORMAZIONI RISPOSTA";
                sheet.Cells[headerRow, 21].Value = nameof(AttoDASIDto.ImpegniScadenze).ToUpper();

                var row = 2;
                foreach (var atto in attiList)
                {
                    var firme = new List<string>();
                    if (atto.FirmeAnte.Any())
                        firme.AddRange(atto.FirmeAnte.Select(f => f.FirmaCert));
                    if (atto.FirmePost.Any())
                        firme.AddRange(atto.FirmePost.Select(f => f.FirmaCert));

                    var dcrText = string.Empty;
                    if (atto.DCR > 0)
                    {
                        dcrText = $"{atto.DCRL}/{atto.DCR}";

                        if (atto.DCCR > 0)
                        {
                            dcrText += $"/{atto.DCCR}";
                        }
                    }

                    // Popolamento delle celle
                    sheet.Cells[row, 1].Value = atto.GetLegislatura();
                    sheet.Cells[row, 2].Value = atto.Etichetta;
                    sheet.Cells[row, 3].Value = atto.DisplayTipo;
                    sheet.Cells[row, 4].Value = atto.NAtto;
                    sheet.Cells[row, 5].Value = atto.TipoMOZ > 0 ? atto.DisplayTipoMozione : "";
                    sheet.Cells[row, 6].Value = atto.OggettoView();
                    sheet.Cells[row, 7].Value = firme.Any() ? string.Join(", ", firme) : "";
                    sheet.Cells[row, 8].Value = atto.DataPresentazione;
                    sheet.Cells[row, 9].Value = atto.IDStato > (int)StatiAttoEnum.PRESENTATO
                        ? atto.DataAnnunzio?.ToString("dd/MM/yyyy")
                        : "";
                    sheet.Cells[row, 10].Value = atto.Firme_ritirate;
                    sheet.Cells[row, 11].Value = atto.ConteggioFirme;
                    sheet.Cells[row, 12].Value =
                        atto.IDStato > (int)StatiAttoEnum.PRESENTATO ? atto.DisplayAreaPolitica : "";
                    sheet.Cells[row, 13].Value = atto.DisplayStato;
                    sheet.Cells[row, 14].Value =
                        atto.IDStato > (int)StatiAttoEnum.PRESENTATO ? atto.DisplayTipoChiusuraIter : "";
                    sheet.Cells[row, 15].Value = dcrText;
                    sheet.Cells[row, 16].Value = atto.IDStato > (int)StatiAttoEnum.PRESENTATO
                        ? atto.DataChiusuraIter?.ToString("dd/MM/yyyy")
                        : "";
                    sheet.Cells[row, 17].Value = atto.IDStato > (int)StatiAttoEnum.PRESENTATO
                        ? atto.UIDSeduta.HasValue ? atto.Seduta?.Data_seduta.ToString("dd/MM/yyyy") : ""
                        : "";
                    sheet.Cells[row, 18].Value = atto.IDStato > (int)StatiAttoEnum.PRESENTATO
                        ? atto.DisplayTipoVotazioneIter
                        : ""; // Annotazioni
                    sheet.Cells[row, 19].Value = atto.Emendato ? "Si" : "";
                    sheet.Cells[row, 20].Value =
                        atto.IDStato > (int)StatiAttoEnum.PRESENTATO ? ParseRisposte(atto) : "";
                    sheet.Cells[row, 21].Value = Utility.StripWordMarkup(atto.ImpegniScadenze);

                    row++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string ParseRisposte(AttoDASIDto atto)
        {
            if (!atto.Risposte.Any())
                return "";
            var bodyRisposte = string.Empty;
            foreach (var attiRisposteDto in atto.Risposte)
            {
                if (atto.IDTipo_Risposta_Effettiva.HasValue)
                {
                    // Identificazione del tipo di risposta
                    switch ((TipoRispostaEnum)atto.IDTipo_Risposta_Effettiva)
                    {
                        case TipoRispostaEnum.IMMEDIATA:
                        case TipoRispostaEnum.ORALE:
                        {
                            // Risposta orale
                            bodyRisposte += $"Orale: Risposta fornita da {attiRisposteDto.DescrizioneOrgano}";
                            break;
                        }
                        case TipoRispostaEnum.SCRITTA:
                        {
                            // Risposta scritta
                            bodyRisposte += $"Scritta: Risposta fornita da {attiRisposteDto.DescrizioneOrgano}";
                            break;
                        }
                        case TipoRispostaEnum.COMMISSIONE:
                        {
                            // Gestione delle risposte associate
                            if (attiRisposteDto.RisposteAssociate.Any())
                            {
                                var assessoriAssociati = string.Join(", ",
                                    attiRisposteDto.RisposteAssociate.Select(assoc => assoc.DescrizioneOrgano));
                                bodyRisposte +=
                                    $"In commissione: Risposta fornita da {assessoriAssociati} in {attiRisposteDto.DescrizioneOrgano}";
                            }
                            else
                            {
                                // Nessuna risposta associata
                                bodyRisposte +=
                                    $"In commissione: Risposta richiesta in {attiRisposteDto.DescrizioneOrgano} non ancora fornita";
                            }

                            break;
                        }
                        case TipoRispostaEnum.ITER_IN_ASSEMBLEA:
                        {
                            bodyRisposte += "Atto trattato in assemblea";
                            break;
                        }
                        case TipoRispostaEnum.ITER_IN_ASSEMBLEA_COMMISSIONE:
                        {
                            bodyRisposte += $"{attiRisposteDto.DescrizioneOrgano}";
                            break;
                        }
                        default:
                        {
                            bodyRisposte = $"Risposta di tipo non specificato da {attiRisposteDto.DescrizioneOrgano}";
                            break;
                        }
                    }

                    if (attiRisposteDto.Data.HasValue)
                    {
                        var dataRisposta = attiRisposteDto.Data.Value.ToString("dd/MM/yyyy");
                        bodyRisposte += $" - Data risposta: {dataRisposta}";
                    }

                    if (attiRisposteDto.DataTrasmissione.HasValue)
                    {
                        var dataRispostaTrasmissione = attiRisposteDto.DataTrasmissione.Value.ToString("dd/MM/yyyy");
                        bodyRisposte += $" - Trasmessa il: {dataRispostaTrasmissione}";
                    }

                    bodyRisposte += "; ";
                }
            }

            return bodyRisposte.TrimEnd(';', ' ');
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