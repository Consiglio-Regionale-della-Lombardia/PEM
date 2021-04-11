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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Request;
using PortaleRegione.Logger;

namespace PortaleRegione.BAL
{
    public class EsportaLogic : BaseLogic
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmendamentiLogic _logicEm;
        private readonly FirmeLogic _logicFirme;
        private readonly PersoneLogic _logicPersone;

        public EsportaLogic(IUnitOfWork unitOfWork, EmendamentiLogic logicEm, FirmeLogic logicFirme,
            PersoneLogic logicPersone)
        {
            _unitOfWork = unitOfWork;
            _logicEm = logicEm;
            _logicFirme = logicFirme;
            _logicPersone = logicPersone;
        }

        #region EsportaGrigliaExcel

        public async Task<HttpResponseMessage> EsportaGrigliaExcel(Guid id, OrdinamentoEnum ordine, PersonaDto persona)
        {
            try
            {
                var _pathTemp = AppSettingsConfiguration.CartellaTemp;
                if (!Directory.Exists(_pathTemp))
                    Directory.CreateDirectory(_pathTemp);

                var nameFileXLS = $"PEM_{DateTime.Now:ddMMyyyy_hhmmss}.xlsx";
                var FilePathComplete = Path.Combine(_pathTemp, nameFileXLS);

                var atto = await _unitOfWork.Atti.Get(id);

                using (var fs = new FileStream(FilePathComplete, FileMode.Create, FileAccess.Write))
                {
                    IWorkbook workbook = new XSSFWorkbook();
                    var excelSheet = workbook.CreateSheet($"{atto.TIPI_ATTO.Tipo_Atto} {atto.NAtto}");

                    var row = excelSheet.CreateRow(0);

                    if (atto.OrdineVotazione.HasValue)
                        if (atto.OrdineVotazione.Value)
                            row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("OrdineVotazione");

                    if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM)
                    {
                        row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("IDEM");
                        row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Atto");
                    }

                    row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Numero EM");
                    row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Data Deposito");
                    row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Stato");
                    row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Tipo");
                    row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Parte");
                    row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Articolo");
                    row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Comma");
                    row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Lettera");
                    row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Titolo");
                    row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Capo");

                    if (atto.VIS_Mis_Prog)
                    {
                        row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Missione");
                        row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Programma");
                        row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("TitoloB");
                    }

                    row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Proponente");
                    row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Area Politica");
                    row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Firmatari");
                    row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("Firmatari dopo deposito");
                    row.CreateCell(GetColumn(row.LastCellNum)).SetCellValue("LinkEM");

                    var emList = await _logicEm.GetEmendamenti(new BaseRequest<EmendamentiDto>
                    {
                        id = id,
                        ordine = ordine,
                        page = 1,
                        size = 50
                    }, persona, (int) ClientModeEnum.GRUPPI);

                    foreach (var em in emList)
                    {
                        var rowEm = excelSheet.CreateRow(excelSheet.LastRowNum + 1);

                        if (atto.OrdineVotazione.HasValue)
                            if (atto.OrdineVotazione.Value)
                                rowEm.CreateCell(GetColumn(rowEm.LastCellNum)).SetCellValue(em.OrdineVotazione);

                        if (persona.CurrentRole == RuoliIntEnum.Amministratore_PEM)
                        {
                            rowEm.CreateCell(GetColumn(rowEm.LastCellNum)).SetCellValue(em.UIDEM.ToString());
                            rowEm.CreateCell(GetColumn(rowEm.LastCellNum))
                                .SetCellValue(
                                    $"{atto.TIPI_ATTO.Tipo_Atto}-{atto.NAtto}-{atto.SEDUTE.legislature.num_legislatura}");
                        }

                        rowEm.CreateCell(GetColumn(rowEm.LastCellNum)).SetCellValue(
                            GetNomeEM(em,
                                em.Rif_UIDEM.HasValue
                                    ? Mapper.Map<EM, EmendamentiDto>(await _logicEm.GetEM(em.Rif_UIDEM.Value))
                                    : null));
                        rowEm.CreateCell(GetColumn(rowEm.LastCellNum)).SetCellValue(
                            !string.IsNullOrEmpty(em.DataDeposito)
                                ? Decrypt(em.DataDeposito)
                                : "--");

                        rowEm.CreateCell(GetColumn(rowEm.LastCellNum))
                            .SetCellValue(persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                                ? $"{em.STATI_EM.IDStato}-{em.STATI_EM.Stato}"
                                : em.STATI_EM.Stato);

                        rowEm.CreateCell(GetColumn(rowEm.LastCellNum))
                            .SetCellValue(persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                                ? $"{em.TIPI_EM.IDTipo_EM}-{em.TIPI_EM.Tipo_EM}"
                                : em.TIPI_EM.Tipo_EM);

                        rowEm.CreateCell(GetColumn(rowEm.LastCellNum))
                            .SetCellValue(persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                                ? $"{em.PARTI_TESTO.IDParte}-{em.PARTI_TESTO.Parte}"
                                : em.PARTI_TESTO.Parte);

                        rowEm.CreateCell(GetColumn(rowEm.LastCellNum))
                            .SetCellValue(em.UIDArticolo.HasValue ? em.ARTICOLI.Articolo : "");
                        rowEm.CreateCell(GetColumn(rowEm.LastCellNum))
                            .SetCellValue(em.UIDComma.HasValue ? em.COMMI.Comma : "");
                        rowEm.CreateCell(GetColumn(rowEm.LastCellNum))
                            .SetCellValue(em.UIDLettera.HasValue ? em.LETTERE.Lettera : em.NLettera);

                        rowEm.CreateCell(GetColumn(rowEm.LastCellNum)).SetCellValue(em.NTitolo);
                        rowEm.CreateCell(GetColumn(rowEm.LastCellNum)).SetCellValue(em.NCapo);

                        if (atto.VIS_Mis_Prog)
                        {
                            rowEm.CreateCell(GetColumn(rowEm.LastCellNum)).SetCellValue(em.NMissione.ToString());
                            rowEm.CreateCell(GetColumn(rowEm.LastCellNum)).SetCellValue(em.NProgramma.ToString());
                            rowEm.CreateCell(GetColumn(rowEm.LastCellNum)).SetCellValue(em.NTitoloB.ToString());
                        }

                        var proponente =
                            Mapper.Map<View_UTENTI, PersonaDto>(await _unitOfWork.Persone.Get(em.UIDPersonaProponente.Value));
                        rowEm.CreateCell(GetColumn(rowEm.LastCellNum))
                            .SetCellValue(persona.CurrentRole == RuoliIntEnum.Amministratore_PEM
                                ? $"{proponente.id_persona}-{proponente.DisplayName}"
                                : proponente.DisplayName);

                        rowEm.CreateCell(GetColumn(rowEm.LastCellNum)).SetCellValue("");

                        if (!string.IsNullOrEmpty(em.DataDeposito))
                        {
                            var firme = await _logicFirme.GetFirme(Mapper.Map<EmendamentiDto, EM>(em),
                                FirmeTipoEnum.TUTTE);
                            var firmeDto = firme.Select(Mapper.Map<FIRME, FirmeDto>)
                                .ToList();

                            var firmeAnte = firmeDto.Where(f =>
                                f.Timestamp < Convert.ToDateTime(Decrypt(em.DataDeposito)));
                            var firmePost = firmeDto.Where(f =>
                                f.Timestamp > Convert.ToDateTime(Decrypt(em.DataDeposito)));

                            rowEm.CreateCell(GetColumn(rowEm.LastCellNum))
                                .SetCellValue(GetFirmatariEM_OPENDATA(firmeAnte,
                                    persona.CurrentRole));
                            rowEm.CreateCell(GetColumn(rowEm.LastCellNum))
                                .SetCellValue(GetFirmatariEM_OPENDATA(firmePost,
                                    persona.CurrentRole));
                        }
                        else
                        {
                            rowEm.CreateCell(GetColumn(rowEm.LastCellNum)).SetCellValue("--");
                            rowEm.CreateCell(GetColumn(rowEm.LastCellNum)).SetCellValue("--");
                        }

                        rowEm.CreateCell(GetColumn(rowEm.LastCellNum))
                            .SetCellValue($"{AppSettingsConfiguration.urlPEM}/{em.UID_QRCode}");
                    }

                    workbook.Write(fs);
                }

                var stream = new MemoryStream();
                using (var fileStream = new FileStream(FilePathComplete, FileMode.Open))
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
                    FileName = nameFileXLS
                };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - EsportaGrigliaXLS", e);
                throw e;
            }
        }

        #endregion

        #region EsportaGrigliaWord

        public async Task<HttpResponseMessage> EsportaGrigliaWord(Guid id, OrdinamentoEnum ordine, PersonaDto persona)
        {
            try
            {
                var _pathTemp = AppSettingsConfiguration.CartellaTemp;
                if (!Directory.Exists(_pathTemp))
                    Directory.CreateDirectory(_pathTemp);

                var nameFileDOC = $"EstrazioneEM_{DateTime.Now:ddMMyyyy_hhmmss}.docx";
                var FilePathComplete = Path.Combine(_pathTemp, nameFileDOC);

                var atto = await _unitOfWork.Atti.Get(id);

                using (var fs = new FileStream(FilePathComplete, FileMode.Create, FileAccess.Write))
                {
                    var doc = new XWPFDocument();
                    var para = doc.CreateParagraph();
                    para.Alignment = ParagraphAlignment.CENTER;
                    var r0 = para.CreateRun();
                    r0.IsBold = true;
                    r0.SetText($"{atto.TIPI_ATTO.Tipo_Atto} {atto.NAtto}");

                    var table = doc.CreateTable(1, 7);

                    #region HEADERS

                    var c0 = table.GetRow(0).GetCell(0);
                    var headerCell0 = c0.AddParagraph();
                    headerCell0.Alignment = ParagraphAlignment.CENTER;
                    var headerCell0_Run = headerCell0.CreateRun();
                    headerCell0_Run.IsBold = true;
                    headerCell0_Run.SetText("Ordine di Votazione");

                    var c1 = table.GetRow(0).GetCell(1);
                    var headerCell1 = c1.AddParagraph();
                    headerCell1.Alignment = ParagraphAlignment.CENTER;
                    var headerCell1_Run = headerCell1.CreateRun();
                    headerCell1_Run.IsBold = true;
                    headerCell1_Run.SetText("N.EM/SUBEM");

                    var c2 = table.GetRow(0).GetCell(2);
                    var headerCell2 = c2.AddParagraph();
                    headerCell2.Alignment = ParagraphAlignment.CENTER;
                    var headerCell2_Run = headerCell2.CreateRun();
                    headerCell2_Run.IsBold = true;
                    headerCell2_Run.SetText("Testo EM/SUBEM");

                    var c3 = table.GetRow(0).GetCell(3);
                    var headerCell3 = c3.AddParagraph();
                    headerCell3.Alignment = ParagraphAlignment.CENTER;
                    var headerCell3_Run = headerCell3.CreateRun();
                    headerCell3_Run.IsBold = true;
                    headerCell3_Run.SetText("Relazione Illustrativa");

                    var c4 = table.GetRow(0).GetCell(4);
                    var headerCell4 = c4.AddParagraph();
                    headerCell4.Alignment = ParagraphAlignment.CENTER;
                    var headerCell4_Run = headerCell4.CreateRun();
                    headerCell4_Run.IsBold = true;
                    headerCell4_Run.SetText("Proponente");

                    var c5 = table.GetRow(0).GetCell(5);
                    var headerCell5 = c5.AddParagraph();
                    headerCell5.Alignment = ParagraphAlignment.CENTER;
                    var headerCell5_Run = headerCell5.CreateRun();
                    headerCell5_Run.IsBold = true;
                    headerCell5_Run.SetText("Firme prima del deposito");

                    var c6 = table.GetRow(0).GetCell(6);
                    var headerCell6 = c6.AddParagraph();
                    headerCell6.Alignment = ParagraphAlignment.CENTER;
                    var headerCell6_Run = headerCell6.CreateRun();
                    headerCell6_Run.IsBold = true;
                    headerCell6_Run.SetText("Stato");

                    #endregion

                    var emList = await _logicEm.GetEmendamenti(new BaseRequest<EmendamentiDto>
                        {
                            id = id,
                            ordine = ordine,
                            page = 1,
                            size = 50
                        }, persona,
                        (int) ClientModeEnum.GRUPPI);

                    foreach (var em in emList)
                    {
                        var row = table.CreateRow();

                        var c0_em = row.GetCell(0);
                        var headerCell0_em = c0_em.AddParagraph();
                        headerCell0_em.Alignment = ParagraphAlignment.CENTER;
                        var headerCell0_Run_em = headerCell0_em.CreateRun();
                        headerCell0_Run_em.SetText(em.OrdineVotazione.ToString());

                        var c1_em = row.GetCell(1);
                        var headerCell1_em = c1_em.AddParagraph();
                        headerCell1_em.Alignment = ParagraphAlignment.CENTER;
                        var headerCell1_Run_em = headerCell1_em.CreateRun();
                        headerCell1_Run_em.SetText(GetNomeEM(em,
                            em.Rif_UIDEM.HasValue
                                ? Mapper.Map<EM, EmendamentiDto>(await _logicEm.GetEM(em.Rif_UIDEM.Value))
                                : null));

                        var c2_em = row.GetCell(2);
                        var headerCell2_em = c2_em.AddParagraph();
                        headerCell2_em.Alignment = ParagraphAlignment.CENTER;
                        var headerCell2_Run_em = headerCell2_em.CreateRun();
                        headerCell2_Run_em.SetText(em.TestoEM_originale);

                        var c3_em = row.GetCell(3);
                        var headerCell3_em = c3_em.AddParagraph();
                        headerCell3_em.Alignment = ParagraphAlignment.CENTER;
                        var headerCell3_Run_em = headerCell3_em.CreateRun();
                        headerCell3_Run_em.SetText(string.IsNullOrEmpty(em.TestoREL_originale)
                            ? ""
                            : em.TestoREL_originale);

                        var proponente =
                            await _logicPersone.GetPersona(em.UIDPersonaProponente.Value,
                                em.id_gruppo >= AppSettingsConfiguration.GIUNTA_REGIONALE_ID);

                        var c4_em = row.GetCell(4);
                        var headerCell4_em = c4_em.AddParagraph();
                        headerCell4_em.Alignment = ParagraphAlignment.CENTER;
                        var headerCell4_Run_em = headerCell4_em.CreateRun();
                        headerCell4_Run_em.SetText(proponente.DisplayName);

                        var firme = await _logicFirme.GetFirme(Mapper.Map<EmendamentiDto, EM>(em), FirmeTipoEnum.TUTTE);
                        var firmeDto = firme.Select(Mapper.Map<FIRME, FirmeDto>).ToList();

                        var firmeAnte = firmeDto.Where(f =>
                            f.Timestamp < Convert.ToDateTime(Decrypt(em.DataDeposito)));

                        var c5_em = row.GetCell(5);
                        var headerCell5_em = c5_em.AddParagraph();
                        headerCell5_em.Alignment = ParagraphAlignment.CENTER;
                        var headerCell5_Run_em = headerCell5_em.CreateRun();
                        headerCell5_Run_em.SetText(GetFirmatariEM_OPENDATA(firmeAnte,
                            persona.CurrentRole));

                        var c6_em = row.GetCell(6);
                        var headerCell6_em = c6_em.AddParagraph();
                        headerCell6_em.Alignment = ParagraphAlignment.CENTER;
                        var headerCell6_Run_em = headerCell6_em.CreateRun();
                        headerCell6_Run_em.SetText(em.STATI_EM.Stato);
                    }

                    doc.Write(fs);
                }

                var stream = new MemoryStream();
                using (var fileStream = new FileStream(FilePathComplete, FileMode.Open))
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
                    FileName = nameFileDOC
                };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                return result;
            }
            catch (Exception e)
            {
                Log.Error("Logic - EsportaGrigliaXLS", e);
                throw e;
            }
        }

        #endregion

        #region GetColumn

        private short GetColumn(short column)
        {
            return column < 0 ? (short) 0 : column;
        }

        #endregion
    }
}