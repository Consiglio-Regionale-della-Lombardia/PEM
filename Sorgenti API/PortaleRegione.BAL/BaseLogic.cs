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
using PortaleRegione.API.Controllers;
using PortaleRegione.Common;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Routes;
using PortaleRegione.Logger;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;

namespace PortaleRegione.BAL
{
    public class BaseLogic
    {
        private readonly MemoryCache memoryCache = MemoryCache.Default;
        internal AttiLogic _logicAtti;
        internal AttiFirmeLogic _logicAttiFirme;
        internal DASILogic _logicDasi;
        internal EmendamentiLogic _logicEm;
        internal FirmeLogic _logicFirme;
        internal PersoneLogic _logicPersona;
        internal SeduteLogic _logicSedute;
        internal UtilsLogic _logicUtil;
        internal IUnitOfWork _unitOfWork;

        internal List<PersonaLightDto> Users
        {
            get
            {
                if (memoryCache.Contains(BALConstants.USERS_IN_DATABASE))
                    return memoryCache.Get(BALConstants.USERS_IN_DATABASE) as List<PersonaLightDto>;

                return new List<PersonaLightDto>();
            }
            set => memoryCache.Add(BALConstants.USERS_IN_DATABASE, value, DateTimeOffset.UtcNow.AddHours(1));
        }


        internal void GetUsersInDb()
        {
            if (Users.Any())
                return;
            var task_op = Task.Run(async () => await _unitOfWork.Persone.GetAll());
            var personeInDb = task_op.Result;
            var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();

            Users = personeInDbLight;
        }


        internal string ByteArrayToFile(byte[] byteArray)
        {
            try
            {
                var root = AppSettingsConfiguration.PercorsoCompatibilitaDocumenti;
                if (!Directory.Exists(root)) Directory.CreateDirectory(root);

                var fileName = DateTime.Now.Ticks + ".pdf";
                var path = Path.Combine(root, fileName);

                using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                fs.Write(byteArray, 0, byteArray.Length);
                return fileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return string.Empty;
            }
        }

        internal async Task<HttpResponseMessage> ComposeFileResponse(string path)
        {
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
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            return result;
        }

        internal HttpResponseMessage ComposeFileResponse(byte[] content, string filename)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content)
            };
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = filename.Replace(' ', '_').Replace("'", "_")
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            return result;
        }

        public async Task<HttpResponseMessage> Download(string path)
        {
            var complete_path = Path.Combine(
                AppSettingsConfiguration.PercorsoCompatibilitaDocumenti,
                Path.GetFileName(path));
            var result = await ComposeFileResponse(complete_path);
            return result;
        }

        internal string GetNomeEM(EmendamentiDto emendamento, EmendamentiDto riferimento)
        {
            try
            {
                var result = string.Empty;
                if (emendamento.Rif_UIDEM.HasValue == false)
                {
                    //EMENDAMENTO
                    if (!string.IsNullOrEmpty(emendamento.N_EM))
                        result = "EM " + BALHelper.DecryptString(emendamento.N_EM, AppSettingsConfiguration.masterKey);
                    else
                        result = "TEMP " + emendamento.Progressivo;
                }
                else
                {
                    //SUB EMENDAMENTO

                    if (!string.IsNullOrEmpty(emendamento.N_SUBEM))
                        result = "SUBEM " +
                                 BALHelper.DecryptString(emendamento.N_SUBEM, AppSettingsConfiguration.masterKey);
                    else
                        result = "SUBEM TEMP " + emendamento.SubProgressivo;

                    result = $"{result} all' {riferimento.N_EM}";
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("GetNomeEM", e);
                throw e;
            }
        }

        internal string GetNomeEM(EM emendamento, EM riferimento)
        {
            return GetNomeEM(Mapper.Map<EM, EmendamentiDto>(emendamento),
                Mapper.Map<EM, EmendamentiDto>(riferimento));
        }

        internal string GetNome(string nAtto, int? progressivo)
        {
            progressivo ??= 0;
            var result = string.Empty;

            if (!string.IsNullOrEmpty(nAtto))
            {
                result = BALHelper.DecryptString(nAtto, AppSettingsConfiguration.masterKey);
                if (result.Contains("_")) result = result.Split('_')[1];
            }
            else
            {
                result = "TEMP " + progressivo;
            }

            return result;
        }

        internal string GetFirmatariEM(IEnumerable<FirmeDto> firme)
        {
            if (firme == null) return string.Empty;

            var firmeDtos = firme.ToList();
            if (!firmeDtos.Any()) return string.Empty;

            var result = firmeDtos.Select(item => string.IsNullOrEmpty(item.Data_ritirofirma)
                    ? $"<label style='font-size:12px'>{item.FirmaCert}, {item.Data_firma}</label><br/>"
                    : $"<div style='text-decoration:line-through;'><label style='font-size:12px'>{item.FirmaCert}, {item.Data_firma} ({item.Data_ritirofirma})</label></div><br/>")
                .ToList();

            return result.Aggregate((i, j) => i + j);
        }

        internal string GetTemplate(TemplateTypeEnum templateType, bool dasi = false)
        {
            var path = "";
            if (dasi == false)
                switch (templateType)
                {
                    case TemplateTypeEnum.PDF:
                        path = HttpContext.Current.Server.MapPath("~/templates/template_pdf.html");
                        break;
                    case TemplateTypeEnum.PDF_COPERTINA:
                        path = HttpContext.Current.Server.MapPath("~/templates/template_pdf_copertina.html");
                        break;
                    case TemplateTypeEnum.MAIL:
                        path = HttpContext.Current.Server.MapPath("~/templates/template_mail.html");
                        break;
                    case TemplateTypeEnum.HTML:
                        path = HttpContext.Current.Server.MapPath("~/templates/template_html.html");
                        break;
                    case TemplateTypeEnum.FIRMA:
                        path = HttpContext.Current.Server.MapPath("~/templates/template_firma.html");
                        break;
                    case TemplateTypeEnum.HTML_MODIFICABILE:
                        path = HttpContext.Current.Server.MapPath(
                            "~/templates/template_html_testomodificabile.html");
                        break;
                    case TemplateTypeEnum.INDICE_DASI:
                        path = HttpContext.Current.Server.MapPath(
                            "~/templates/dasi/template_indice.html");
                        break;
                }
            else
                switch (templateType)
                {
                    case TemplateTypeEnum.PDF:
                        path = HttpContext.Current.Server.MapPath("~/templates/dasi/template_pdf.html");
                        break;
                    case TemplateTypeEnum.PDF_COPERTINA:
                        path = HttpContext.Current.Server.MapPath("~/templates/dasi/template_pdf_copertina.html");
                        break;
                    case TemplateTypeEnum.MAIL:
                        path = HttpContext.Current.Server.MapPath("~/templates/dasi/template_mail.html");
                        break;
                    case TemplateTypeEnum.HTML:
                        path = HttpContext.Current.Server.MapPath("~/templates/dasi/template_html.html");
                        break;
                    case TemplateTypeEnum.FIRMA:
                        path = HttpContext.Current.Server.MapPath("~/templates/dasi/template_firma.html");
                        break;
                    case TemplateTypeEnum.HTML_MODIFICABILE:
                        path = HttpContext.Current.Server.MapPath(
                            "~/templates/dasi/template_html_testomodificabile.html");
                        break;
                    case TemplateTypeEnum.INDICE_DASI:
                        path = HttpContext.Current.Server.MapPath(
                            "~/templates/dasi/template_indice.html");
                        break;
                }

            var result = File.ReadAllText(path);

            return result;
        }

        internal void GetBodyTemporaneo(EmendamentiDto emendamento, AttiDto atto, ref string body)
        {
            if (!string.IsNullOrEmpty(emendamento.EM_Certificato)) return;
            //EM TEMPORANEO
            body = body.Replace("{lblTitoloPDLEMView}",
                $"PROGETTO DI LEGGE N.{atto.NAtto}");
            body = body.Replace("{lblSubTitoloPDLEMView}", atto.Oggetto);
            body = body.Replace("{lblTipoParteEMView}",
                $"Tipo: {emendamento.TIPI_EM.Tipo_EM}<br/>{Utility.GetParteEM(emendamento)}");
            body = body.Replace("{lblEffettiFinanziari}",
                Utility.EffettiFinanziariEM(emendamento.EffettiFinanziari));

            var body_orig = Utility.CleanWordText(emendamento.TestoEM_originale);
            var body_rel_orig = Utility.CleanWordText(emendamento.TestoREL_originale);

            body = body.Replace("{lblTestoEMView}", body_orig);
            body = !string.IsNullOrEmpty(emendamento.TestoREL_originale)
                ? body.Replace("{lblTestoRelEMView}",
                    "<b>RELAZIONE ILLUSTRATIVA</b><br />" + body_rel_orig)
                : body.Replace("{lblTestoRelEMView}", string.Empty);

            var allegato_generico = string.Empty;
            var allegato_tecnico = string.Empty;

            #region Allegato Tecnico

            //Allegato Tecnico
            if (!string.IsNullOrEmpty(emendamento.PATH_AllegatoTecnico))
                allegato_tecnico =
                    $"<div class=\"chip white black-text\"><a href='{AppSettingsConfiguration.URL_API}/{ApiRoutes.PEM.Emendamenti.DownloadDoc}?path={emendamento.PATH_AllegatoTecnico}' target='_blank'>SCARICA ALLEGATO TECNICO</a></div>";

            #endregion

            #region Allegato Generico

            //Allegato Generico
            if (!string.IsNullOrEmpty(emendamento.PATH_AllegatoGenerico))
                allegato_generico =
                    $"<div class=\"chip white black-text\"><a href='{AppSettingsConfiguration.URL_API}/{ApiRoutes.PEM.Emendamenti.DownloadDoc}?path={emendamento.PATH_AllegatoGenerico}' target='_blank'>SCARICA ALLEGATO GENERICO</a></div>";

            #endregion

            body = body.Replace("{lblAllegati}", allegato_tecnico + allegato_generico);
        }

        internal void GetBodyTemporaneo(AttoDASIDto atto, bool privacy, ref string body)
        {
            if (atto.Tipo == (int)TipoAttoEnum.ODG)
            {
                body = body.Replace("{ODG_RIFERIMENTO_COMMENTO_START}", "");
                body = body.Replace("{ODG_RIFERIMENTO_COMMENTO_END}", "");

                body = body.Replace("{lblTitoloPDLEMView}", atto.ODG_Atto_PEM);
                body = body.Replace("{lblSubTitoloPDLEMView}", atto.ODG_Atto_Oggetto_PEM);
            }
            else
            {
                body = body.Replace("{ODG_RIFERIMENTO_COMMENTO_START}", "<!--");
                body = body.Replace("{ODG_RIFERIMENTO_COMMENTO_END}", "-->");
            }

            if (atto.Tipo == (int)TipoAttoEnum.MOZ
                || atto.Tipo == (int)TipoAttoEnum.ODG)
            {
                body = body.Replace("{TIPO_RISPOSTA_COMMENTO_START}", "<!--");
                body = body.Replace("{TIPO_RISPOSTA_COMMENTO_END}", "-->");
            }
            else
            {
                body = body.Replace("{TIPO_RISPOSTA_COMMENTO_START}", "");
                body = body.Replace("{TIPO_RISPOSTA_COMMENTO_END}", "");
                body = body.Replace("{lblTipoRispostaATTOView}",
                    DASIHelper.GetDescrizioneRisposta((TipoRispostaEnum)atto.IDTipo_Risposta, atto.Commissioni));
            }

            var oggetto = atto.Oggetto;
            var premesse = atto.Premesse;
            var richieste = atto.Richiesta;

            if (!string.IsNullOrEmpty(atto.Oggetto_Privacy) && privacy) oggetto = atto.Oggetto_Privacy; //#631
            if (!string.IsNullOrEmpty(atto.Premesse_Modificato) && privacy) premesse = atto.Premesse_Modificato;
            if (!string.IsNullOrEmpty(atto.Richiesta_Modificata) && privacy) richieste = atto.Richiesta_Modificata;

            body = body.Replace("{lblSubTitoloATTOView}", oggetto);
            body = body.Replace("{lblPremesseATTOView}", premesse);
            body = body.Replace("{lblRichiestaATTOView}", richieste);

            var allegato_generico = string.Empty;

            #region Allegato Generico

            //Allegato Generico
            if (!string.IsNullOrEmpty(atto.PATH_AllegatoGenerico))
                allegato_generico =
                    $"<tr class=\"left-border\" style=\"border-bottom: 1px solid !important\"><td colspan='2' style='text-align:left;padding-left:10px'><a href='{AppSettingsConfiguration.URL_API}/{ApiRoutes.DASI.DownloadDoc}?path={atto.PATH_AllegatoGenerico}' target='_blank'>SCARICA ALLEGATO</a></td></tr>";

            #endregion

            body = body.Replace("{lblAllegati}", allegato_generico);
        }

        public void GetBody(EmendamentiDto emendamento, AttiDto atto, List<FirmeDto> firme,
            PersonaDto currentUser,
            bool enableQrCode,
            ref string body)
        {
            body = body.Replace("{lblTitoloEMView}", emendamento.N_EM);
            body = body.Replace("{StatoEMView}", emendamento.STATI_EM.Stato);
            var testo_deposito = string.Empty;
            if (emendamento.IDStato >= (int)StatiEnum.Depositato)
                testo_deposito = $"Presentato il {emendamento.DataDeposito}";
            body = body.Replace("{DepositatoEMView}", testo_deposito);

            body = body.Replace("{STATO}", emendamento.STATI_EM.Stato.ToUpper());
            body = body.Replace("{GRUPPO_POLITICO}", emendamento.gruppi_politici.nome_gruppo);
            body = body.Replace("{nomePiattaforma}", AppSettingsConfiguration.Titolo);
            body = body.Replace("{urlLogo}", AppSettingsConfiguration.Logo);

            if (string.IsNullOrEmpty(emendamento.EM_Certificato))
            {
                //EM TEMPORANEO
                var bodyTemp = GetTemplate(TemplateTypeEnum.FIRMA);
                GetBodyTemporaneo(emendamento, atto, ref bodyTemp);
                body = body.Replace("{ltEMView}", bodyTemp);
                body = body.Replace("{ltTestoModificabile}", "").Replace("{TESTOMOD_COMMENTO_START}", "<!--")
                    .Replace("{TESTOMOD_COMMENTO_END}", "-->");
                body = body.Replace("{lblFattoProprioDa}", "").Replace("{FATTOPROPRIO_COMMENTO_START}", "<!--")
                    .Replace("{FATTOPROPRIO_COMMENTO_END}", "-->");
            }
            else
            {
                var body_cert = Utility.CleanWordText(emendamento.EM_Certificato);
                body = body.Replace("{ltEMView}", body_cert);

                #region Emendamento Fatto Proprio Da

                body = emendamento.UIDPersonaProponenteOLD.HasValue
                    ? body.Replace("{lblFattoProprioDa}",
                            $"L'emendamento ritirato è stato fatto proprio da {emendamento.PersonaProponente.DisplayName}")
                        .Replace("{FATTOPROPRIO_COMMENTO_START}", string.Empty)
                        .Replace("{FATTOPROPRIO_COMMENTO_END}", string.Empty)
                    : body.Replace("{lblFattoProprioDa}", string.Empty)
                        .Replace("{FATTOPROPRIO_COMMENTO_START}", "<!--")
                        .Replace("{FATTOPROPRIO_COMMENTO_END}", "-->");

                #endregion

                #region Testo Modificabile

                body = !string.IsNullOrEmpty(emendamento.TestoEM_Modificabile)
                    ? body.Replace("{ltTestoModificabile}", emendamento.TestoEM_Modificabile)
                        .Replace("{TESTOMOD_COMMENTO_START}", string.Empty)
                        .Replace("{TESTOMOD_COMMENTO_END}", string.Empty)
                    : body.Replace("{ltTestoModificabile}", string.Empty)
                        .Replace("{TESTOMOD_COMMENTO_START}", "<!--")
                        .Replace("{TESTOMOD_COMMENTO_END}", "-->");

                #endregion
            }

            #region Firme

            var TemplatefirmeANTE = @"<div>
                             <div style='width:100%;'>
                                      <h6>Firmatari</h6>
                              </div>
                              <div style='text-align:left'>
                                {firme}
                            </div>
                        </div>";
            var TemplatefirmePOST = @"<div>
                             <div style='width:100%;'>
                                      <h6>Firmatari dopo il deposito</h6>
                              </div>
                              <div style='text-align:left'>
                                {firme}
                            </div>
                        </div>";

            if (emendamento.IDStato >= (int)StatiEnum.Depositato)
            {
                //DEPOSITATO
                body = body.Replace("{lblDepositoEMView}",
                    firme.Any(s => s.ufficio)
                        ? "Emendamento Presentato d'ufficio"
                        : $"Emendamento Presentato il {emendamento.Timestamp:dd/MM/yyyy HH:mm}");

                var firmeAnte = firme.Where(f => f.Timestamp <= emendamento.Timestamp).Select(i => (AttiFirmeDto)i);
                var firmePost = firme.Where(f => f.Timestamp > emendamento.Timestamp).Select(i => (AttiFirmeDto)i);

                if (firmeAnte.Any())
                    body = body.Replace("{radGridFirmeView}", TemplatefirmeANTE.Replace("{firme}", GetFirmatari(firmeAnte, "dd/MM/yyyy HH:mm")))
                        .Replace("{FIRMEANTE_COMMENTO_START}", string.Empty)
                        .Replace("{FIRMEANTE_COMMENTO_END}", string.Empty);
                else
                    body = body.Replace("{radGridFirmePostView}", string.Empty)
                        .Replace("{FIRMEANTE_COMMENTO_START}", "<!--").Replace("{FIRMEANTE_COMMENTO_END}", "-->");

                if (firmePost.Any())
                    body = body.Replace("{radGridFirmePostView}",
                            TemplatefirmePOST.Replace("{firme}", GetFirmatari(firmePost, "dd/MM/yyyy HH:mm")))
                        .Replace("{FIRME_COMMENTO_START}", string.Empty)
                        .Replace("{FIRME_COMMENTO_END}", string.Empty);
                else
                    body = body.Replace("{radGridFirmePostView}", string.Empty)
                        .Replace("{FIRME_COMMENTO_START}", "<!--").Replace("{FIRME_COMMENTO_END}", "-->");
            }
            else
            {
                //FIRMATO MA NON DEPOSITATO
                body = body.Replace("{lblDepositoEMView}", string.Empty);
                var firmeAnte = firme.Select(i => (AttiFirmeDto)i);
                var firmatari = GetFirmatari(firmeAnte, "dd/MM/yyyy HH:mm");
                if (!string.IsNullOrEmpty(firmatari))
                {
                    body = body.Replace("{radGridFirmeView}", TemplatefirmeANTE.Replace("{firme}", firmatari))
                        .Replace("{FIRMEANTE_COMMENTO_START}", string.Empty)
                        .Replace("{FIRMEANTE_COMMENTO_END}", string.Empty);
                    body = body.Replace("{radGridFirmePostView}", string.Empty)
                        .Replace("{FIRME_COMMENTO_START}", "<!--")
                        .Replace("{FIRME_COMMENTO_END}", "-->");
                }
                else
                {
                    body = body.Replace("{FIRMEANTE_COMMENTO_START}", "<!--")
                        .Replace("{FIRMEANTE_COMMENTO_END}", "-->");
                    body = body.Replace("{radGridFirmePostView}", string.Empty)
                        .Replace("{FIRME_COMMENTO_START}", "<!--")
                        .Replace("{FIRME_COMMENTO_END}", "-->");
                }
            }

            #endregion

            body = body.Replace("{lblNotePubblicheEMView}",
                    !string.IsNullOrEmpty(emendamento.NOTE_Griglia)
                        ? $"Note: {emendamento.NOTE_Griglia}"
                        : string.Empty)
                .Replace("{NOTE_PUBBLICHE_COMMENTO_START}",
                    !string.IsNullOrEmpty(emendamento.NOTE_Griglia) ? string.Empty : "<!--").Replace(
                    "{NOTE_PUBBLICHE_COMMENTO_END}",
                    !string.IsNullOrEmpty(emendamento.NOTE_Griglia) ? string.Empty : "-->");

            if (currentUser != null)
            {
                if (currentUser.IsSegreteriaAssemblea &&
                    !string.IsNullOrEmpty(emendamento.NOTE_EM))
                    body = body.Replace("{lblNotePrivateEMView}",
                            $"Note Riservate: {emendamento.NOTE_EM}")
                        .Replace("{NOTEPRIV_COMMENTO_START}", string.Empty)
                        .Replace("{NOTEPRIV_COMMENTO_END}", string.Empty);
                else
                    body = body.Replace("{lblNotePrivateEMView}", string.Empty)
                        .Replace("{NOTEPRIV_COMMENTO_START}", "<!--")
                        .Replace("{NOTEPRIV_COMMENTO_END}", "-->");
            }
            else
            {
                body = body.Replace("{lblNotePrivateEMView}", string.Empty)
                    .Replace("{NOTEPRIV_COMMENTO_START}", "<!--")
                    .Replace("{NOTEPRIV_COMMENTO_END}", "-->");
            }

            var textQr = string.Empty;
            if (enableQrCode)
            {
                var qr_contentString = "data:image/png;base64,{{DATA}}";
                var qrLink = $"{AppSettingsConfiguration.urlPEM_ViewEM}{emendamento.UID_QRCode}";
                var qrGenerator = new QRCodeGenerator();
                var urlPayload = new PayloadGenerator.Url(qrLink);
                var qrData = qrGenerator.CreateQrCode(urlPayload, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrData);
                using (var qrCodeImage = qrCode.GetGraphic(20))
                {
                    var ms = new MemoryStream();
                    qrCodeImage.Save(ms, ImageFormat.Png);
                    var byteImage = ms.ToArray();
                    qr_contentString =
                        qr_contentString.Replace("{{DATA}}", Convert.ToBase64String(byteImage));
                }

                textQr =
                    $"<img src=\"{qr_contentString}\" style=\"height:100px; width:100px; border=0;\" /><br><label>Collegamento alla piattaforma</label>";
            }

            body = body.Replace("{QRCode}", textQr);
        }

        public void GetBody(AttoDASIDto atto, string tipoAtto, IEnumerable<AttiFirmeDto> firme,
            PersonaDto currentUser,
            bool enableQrCode,
            bool privacy,
            ref string body)
        {
            var firmeDtos = firme.ToList();
            var title = $"{tipoAtto} {atto.NAtto}";
            if (atto.Non_Passaggio_In_Esame) title += "<br><h6>ODG DI NON PASSAGGIO ALL’ESAME</h6>";

            body = body.Replace("{lblTitoloATTOView}", title);
            body = body.Replace("{GRUPPO_POLITICO}", atto.gruppi_politici.nome_gruppo);
            body = body.Replace("{nomePiattaforma}", AppSettingsConfiguration.Titolo);
            body = body.Replace("{urlLogo}", AppSettingsConfiguration.Logo);

            if (privacy || string.IsNullOrEmpty(atto.Atto_Certificato))
            {
                //ATTO NON CERTIFICATO
                var bodyTemp = GetTemplate(TemplateTypeEnum.FIRMA, true);
                GetBodyTemporaneo(atto, privacy, ref bodyTemp);
                body = body.Replace("{ltATTOView}", bodyTemp);
            }
            else
            {
                body = body.Replace("{ltATTOView}", atto.Atto_Certificato);
            }

            #region Firme

            var TemplatefirmeANTE = @"<div>
                             <div style='width:100%;'>
                                      <h6>Firmatari</h6>
                              </div>
                              <div style='text-align:left'>
                                {firme}
                            </div>
                        </div>";
            var TemplatefirmePOST = @"<div>
                             <div style='width:100%;'>
                                      <h6>Firmatari dopo la presentazione</h6>
                              </div>
                              <div style='text-align:left'>
                                {firme}
                            </div>
                        </div>";

            if (atto.IDStato >= (int)StatiAttoEnum.PRESENTATO)
            {
                //DEPOSITATO
                body = body.Replace("{lblDepositoATTOView}", $"Atto presentato il {atto.DataPresentazione}");

                var firmeAnte = firmeDtos.Where(f => f.Timestamp <= atto.Timestamp);
                var firmePost = firmeDtos.Where(f => f.Timestamp > atto.Timestamp);

                if (firmeAnte.Any())
                    body = body.Replace("{radGridFirmeView}",
                        TemplatefirmeANTE.Replace("{firme}", GetFirmatari(firmeAnte)));

                if (firmePost.Any())
                    body = body.Replace("{radGridFirmePostView}",
                        TemplatefirmePOST.Replace("{firme}", GetFirmatari(firmePost)));
                else
                    body = body.Replace("{radGridFirmePostView}", string.Empty);
            }
            else
            {
                //FIRMATO MA NON DEPOSITATO
                var firmatari = GetFirmatari(firmeDtos);
                if (!string.IsNullOrEmpty(firmatari))
                {
                    body = body.Replace("{lblDepositoATTOView}", string.Empty);
                    body = body.Replace("{radGridFirmeView}", TemplatefirmeANTE.Replace("{firme}", firmatari));
                    body = body.Replace("{radGridFirmePostView}", string.Empty);
                }
                else
                {
                    body = body.Replace("{lblDepositoATTOView}", string.Empty);
                    body = body.Replace("{radGridFirmeView}", string.Empty);
                    body = body.Replace("{radGridFirmePostView}", string.Empty);
                }
            }

            #endregion

            body = body.Replace("{lblNotePubblicheATTOView}",
                    !string.IsNullOrEmpty(atto.Note_Pubbliche)
                        ? $"Note: {atto.Note_Pubbliche}"
                        : string.Empty)
                .Replace("{NOTE_PUBBLICHE_COMMENTO_START}",
                    !string.IsNullOrEmpty(atto.Note_Pubbliche) ? string.Empty : "<!--").Replace(
                    "{NOTE_PUBBLICHE_COMMENTO_END}",
                    !string.IsNullOrEmpty(atto.Note_Pubbliche) ? string.Empty : "-->");

            if (currentUser != null)
            {
                if (currentUser.IsSegreteriaAssemblea &&
                    !string.IsNullOrEmpty(atto.Note_Private))
                    body = body.Replace("{lblNotePrivateATTOView}",
                            $"Note Riservate: {atto.Note_Private}")
                        .Replace("{NOTEPRIV_COMMENTO_START}", string.Empty)
                        .Replace("{NOTEPRIV_COMMENTO_END}", string.Empty);
                else
                    body = body.Replace("{lblNotePrivateATTOView}", string.Empty)
                        .Replace("{NOTEPRIV_COMMENTO_START}", "<!--")
                        .Replace("{NOTEPRIV_COMMENTO_END}", "-->");
            }
            else
            {
                body = body.Replace("{lblNotePrivateATTOView}", string.Empty)
                    .Replace("{NOTEPRIV_COMMENTO_START}", "<!--")
                    .Replace("{NOTEPRIV_COMMENTO_END}", "-->");
            }

            var textQr = string.Empty;
            if (enableQrCode)
            {
                var qr_contentString = "data:image/png;base64,{{DATA}}";
                var qrLink =
                    $"{AppSettingsConfiguration.urlDASI_ViewATTO.Replace("{{UIDATTO}}", atto.UIDAtto.ToString())}";
                var qrGenerator = new QRCodeGenerator();
                var urlPayload = new PayloadGenerator.Url(qrLink);
                var qrData = qrGenerator.CreateQrCode(urlPayload, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrData);
                using (var qrCodeImage = qrCode.GetGraphic(20))
                {
                    var ms = new MemoryStream();
                    qrCodeImage.Save(ms, ImageFormat.Png);
                    var byteImage = ms.ToArray();
                    qr_contentString =
                        qr_contentString.Replace("{{DATA}}", Convert.ToBase64String(byteImage));
                }

                textQr =
                    $"<img src=\"{qr_contentString}\" style=\"height:100px; width:100px; border=0;\" /><br><label>Collegamento alla piattaforma</label>";
            }

            body = body.Replace("{QRCode}", textQr);
        }

        private string GetFirmatari(IEnumerable<AttiFirmeDto> firme, string format = "dd/MM/yyyy")
        {
            try
            {
                if (firme == null) return string.Empty;

                var firmeDtos = firme.ToList();
                if (!firmeDtos.Any()) return string.Empty;

                var result = new List<string>();
                foreach (var attiFirmeDto in firmeDtos)
                {
                    var body = attiFirmeDto.FirmaCert;
                    if (string.IsNullOrEmpty(attiFirmeDto.Data_ritirofirma))
                    {
                        if (!attiFirmeDto.ufficio)
                            body = $"{body}, {Convert.ToDateTime(attiFirmeDto.Data_firma).ToString(format)}";

                        body = $"<div class='chip white black-text'>{body}</div></br>";
                    }
                    else
                    {
                        if (!attiFirmeDto.ufficio)
                            body = $"{body}, {Convert.ToDateTime(attiFirmeDto.Data_firma).ToString(format)}";
                        body =
                            $"<div style='text-decoration:line-through;'><div class='chip white black-text'>{body} ({attiFirmeDto.Data_ritirofirma})</div></div></br>";
                    }

                    result.Add(body);
                }

                return result.Aggregate((i, j) => i + j);
            }
            catch (Exception e)
            {
                Log.Error("GetFirmatari - DASI", e);
                throw e;
            }
        }

        internal void GetBodyMail(EmendamentiDto emendamento, IEnumerable<FirmeDto> firme, bool isDeposito,
            ref string body)
        {
            try
            {
                var firmeDtos = firme.ToList();

                if (isDeposito)
                {
                    body = body.Replace("{MESSAGGIOINIZIALE}",
                        AppSettingsConfiguration.MessaggioInizialeDeposito.Replace("{br}", "<br/>"));
                    body = body.Replace("{azione}", "visualizzare");
                    body = body.Replace("{LINKPEMRIEPILOGO_FIRME}", string.Empty);
                }
                else
                {
                    body = body.Replace("{MESSAGGIOINIZIALE}",
                        AppSettingsConfiguration.MessaggioInizialeInvito.Replace("{br}", "<br/>"));
                    body = body.Replace("{azione}", "firmare");
                    body = body.Replace("{LINKPEMRIEPILOGO_FIRME}",
                        "<a href='"
                        + string.Format(AppSettingsConfiguration.urlPEM_RiepilogoEM, emendamento.UIDAtto) +
                        "'>Clicca qui</a> per visualizzare gli em in cui sei indicato come firmatario.");
                }

                body = body.Replace("{lblTitoloEMView}", emendamento.N_EM);
                body = body.Replace("{ltEMView}", emendamento.EM_Certificato);

                #region Firme

                if (emendamento.STATI_EM.IDStato >= (int)StatiEnum.Depositato)
                {
                    //DEPOSITATO
                    body = body.Replace("{lblDepositoEMView}",
                        firmeDtos.Any(s => s.ufficio)
                            ? "Emendamento Depositato d'ufficio"
                            : $"Emendamento Presentato il {Convert.ToDateTime(emendamento.DataDeposito):dd/MM/yyyy HH:mm}");

                    var firmeAnte = firmeDtos.Where(f => f.Timestamp < Convert.ToDateTime(emendamento.DataDeposito));
                    var firmePost = firmeDtos.Where(f => f.Timestamp > Convert.ToDateTime(emendamento.DataDeposito));

                    body = body.Replace("{radGridFirmeView}", GetFirmatariEM(firmeAnte));
                    var TemplatefirmePOST = @"<div>
                             <div style='width:100%;'>
                                      <h5>Firme dopo il deposito</h5>
                              </div>
                              <div style='text-align:left'>
                                {firme}
                            </div>
                        </div>";
                    body = body.Replace("{radGridFirmePostView}",
                        firmePost.Any()
                            ? TemplatefirmePOST.Replace("{firme}", GetFirmatariEM(firmePost))
                            : string.Empty);
                }

                #endregion

                body = body.Replace("{IMGLOGO}",
                    "<img src='" + Path.Combine(AppSettingsConfiguration.url_CLIENT, "/images/LogoCRL120px.gif") +
                    " style='120px;'/>");

                body = body.Replace("{LINKPEM}",
                    $"{AppSettingsConfiguration.urlPEM_ViewEM}{emendamento.UID_QRCode}");

                body = body.Replace("{LINKPEMRIEPILOGO}", string.Empty);
            }
            catch (Exception e)
            {
                Log.Error("GetBodyMail", e);
                throw e;
            }
        }
    }
}