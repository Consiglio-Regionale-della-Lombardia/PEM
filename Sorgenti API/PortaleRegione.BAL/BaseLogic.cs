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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using PortaleRegione.Common;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.Logger;

namespace PortaleRegione.BAL
{
    public class BaseLogic
    {
        internal string ByteArrayToFile(byte[] byteArray, DocTypeEnum type)
        {
            try
            {
                var root = string.Empty;
                switch (type)
                {
                    case DocTypeEnum.ATTO:
                        root = AppSettingsConfiguration.CartellaDocumentiAtti;
                        break;
                    case DocTypeEnum.EM:
                        root = AppSettingsConfiguration.CartellaAllegatiEM;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                if (!Directory.Exists(root))
                    Directory.CreateDirectory(root);
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

        internal static string GetNomeEM(EmendamentiDto emendamento)
        {
            try
            {
                var result = string.Empty;
                if (emendamento.Rif_UIDEM.HasValue == false)
                {
                    //EMENDAMENTO
                    if (!string.IsNullOrEmpty(emendamento.N_EM))
                        result = "EM " + DecryptString(emendamento.N_EM, AppSettingsConfiguration.masterKey);
                    else
                        result = "TEMP " + emendamento.Progressivo;
                }
                else
                {
                    //SUB EMENDAMENTO

                    if (!string.IsNullOrEmpty(emendamento.N_SUBEM))
                        result = "SUBEM " + DecryptString(emendamento.N_SUBEM, AppSettingsConfiguration.masterKey);
                    else
                        result = "SUBEM TEMP " + emendamento.SubProgressivo;

                    var n_em_riferimento = GetNomeEM(emendamento.EM2);
                    result = $"{result} all' {n_em_riferimento}";
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error("GetNomeEM", e);
                throw e;
            }
        }

        internal static string GetNomeEM(EM emendamento)
        {
            try
            {
                return GetNomeEM(Mapper.Map<EM, EmendamentiDto>(emendamento));
            }
            catch (Exception e)
            {
                Log.Error("GetNomeEM", e);
                throw e;
            }
        }

        internal static string GetFirmatariEM(IEnumerable<FirmeDto> firme)
        {
            try
            {
                if (firme == null)
                    return string.Empty;
                var firmeDtos = firme.ToList();
                if (!firmeDtos.Any())
                    return string.Empty;

                var result = firmeDtos.Select(item => string.IsNullOrEmpty(item.Data_ritirofirma)
                        ? $"<label style='font-size:12px'>{item.FirmaCert}, {item.Data_firma}</label><br/>"
                        : $"<div style='text-decoration:line-through;'><label style='font-size:12px'>{item.FirmaCert}, {item.Data_firma} ({item.Data_ritirofirma})</label></div><br/>")
                    .ToList();

                return result.Aggregate((i, j) => i + j);
            }
            catch (Exception e)
            {
                Log.Error("GetFirmatariEM", e);
                throw e;
            }
        }

        internal static string EncryptString(string InString, string Key)
        {
            try
            {
                byte[] Results;
                var UTF8 = new UTF8Encoding();

                var HashProvider = new MD5CryptoServiceProvider();
                var TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Key));

                var TDESAlgorithm = new TripleDESCryptoServiceProvider
                {
                    Key = TDESKey, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7
                };

                var DataToEncrypt = UTF8.GetBytes(InString);

                try
                {
                    var Encryptor = TDESAlgorithm.CreateEncryptor();
                    Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
                }
                finally
                {
                    TDESAlgorithm.Clear();
                    HashProvider.Clear();
                }

                return Convert.ToBase64String(Results);
            }
            catch (Exception e)
            {
                Log.Error("EncryptString", e);
                throw e;
            }
        }

        internal static string GetTemplate(TemplateTypeEnum templateType)
        {
            try
            {
                string path;
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
                    case TemplateTypeEnum.HTML_MODIFICABILE:
                        path = HttpContext.Current.Server.MapPath(
                            "~/templates/template_html_testomodificabile.html");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null);
                }

                var result = File.ReadAllText(path);

                return result;
            }
            catch (Exception e)
            {
                Log.Error("GetTemplate", e);
                throw e;
            }
        }

        internal static void GetBodyTemporaneo(EmendamentiDto emendamento, AttiDto atto, ref string body)
        {
            try
            {
                if (!string.IsNullOrEmpty(emendamento.EM_Certificato)) return;
                //EM TEMPORANEO
                body = body.Replace("{lblPDLEMView}",
                    $"{atto.TIPI_ATTO.Tipo_Atto}/{atto.NAtto}");
                body = body.Replace("{lblTitoloPDLEMView}",
                    $"PROGETTO DI LEGGE N.{atto.NAtto}");
                body = body.Replace("{lblSubTitoloPDLEMView}", atto.Oggetto);
                body = body.Replace("{lblTipoParteEMView}",
                    $"Tipo: {emendamento.TIPI_EM.Tipo_EM}<br/>{Utility.GetParteEM(emendamento)}");
                body = body.Replace("{lblEffettiFinanziari}",
                    Utility.EffettiFinanziariEM(emendamento.EffettiFinanziari));

                body = body.Replace("{lblTestoEMView}", emendamento.TestoEM_originale);
                body = !string.IsNullOrEmpty(emendamento.TestoREL_originale)
                    ? body.Replace("{lblTestoRelEMView}",
                        "<b>RELAZIONE ILLUSTRATIVA</b><br />" + emendamento.TestoREL_originale)
                    : body.Replace("{lblTestoRelEMView}", string.Empty);

                var allegato_generico = string.Empty;
                var allegato_tecnico = string.Empty;

                #region Allegato Tecnico

                //Allegato Tecnico
                if (!string.IsNullOrEmpty(emendamento.PATH_AllegatoTecnico))
                    allegato_tecnico =
                        $"<tr><td colspan='2' style='text-align:left;padding-left:10px'><a href='{AppSettingsConfiguration.URL_API}/emendamenti/file?path={emendamento.PATH_AllegatoTecnico}' target='_blank'>SCARICA ALLEGATO TECNICO</a></td></tr>";

                #endregion

                #region Allegato Generico

                //Allegato Generico
                if (!string.IsNullOrEmpty(emendamento.PATH_AllegatoGenerico))
                    allegato_generico =
                        $"<tr><td colspan='2' style='text-align:left;padding-left:10px'><a href='{AppSettingsConfiguration.URL_API}/emendamenti/file?path={emendamento.PATH_AllegatoGenerico}' target='_blank'>SCARICA ALLEGATO GENERICO</a></td></tr>";

                #endregion

                body = body.Replace("{lblAllegati}", allegato_tecnico + allegato_generico);
            }
            catch (Exception e)
            {
                Log.Error("GetBodyTemporaneo", e);
                throw e;
            }
        }

        internal static void GetBodyPDF(EmendamentiDto emendamento, AttiDto atto, IEnumerable<FirmeDto> firme, PersonaDto currentUser,
            ref string body)
        {
            try
            {
                var firmeDtos = firme.ToList();

                body = body.Replace("{lblTitoloEMView}", emendamento.N_EM);

                if (string.IsNullOrEmpty(emendamento.EM_Certificato))
                {
                    //EM TEMPORANEO
                    var bodyEMView = string.Empty;
                    GetBodyTemporaneo(emendamento, atto, ref bodyEMView);
                    body = body.Replace("{ltEMView}", bodyEMView);
                    body = body.Replace("{ltTestoModificabile}", "").Replace("{TESTOMOD_COMMENTO_START}", "<!--")
                        .Replace("{TESTOMOD_COMMENTO_END}", "-->");
                    body = body.Replace("{lblFattoProprioDa}", "").Replace("{FATTOPROPRIO_COMMENTO_START}", "<!--")
                        .Replace("{FATTOPROPRIO_COMMENTO_END}", "-->");
                }
                else
                {
                    body = body.Replace("{ltEMView}", emendamento.EM_Certificato);

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

                if (emendamento.STATI_EM.IDStato >= (int) StatiEnum.Depositato)
                {
                    //DEPOSITATO
                    body = body.Replace("{lblDepositoEMView}",
                        firmeDtos.Any(s => s.ufficio)
                            ? "Emendamento Depositato d'ufficio"
                            : $"Emendamento Depositato il {Convert.ToDateTime(emendamento.DataDeposito):dd/MM/yyyy HH:mm}");

                    var firmeAnte = firmeDtos.Where(f => f.Timestamp < Convert.ToDateTime(emendamento.DataDeposito));
                    var firmePost = firmeDtos.Where(f => f.Timestamp > Convert.ToDateTime(emendamento.DataDeposito));

                    body = body.Replace("{radGridFirmeView}", GetFirmatariEM(firmeAnte))
                        .Replace("{FIRMEANTE_COMMENTO_START}", string.Empty)
                        .Replace("{FIRMEANTE_COMMENTO_END}", string.Empty);
                    var TemplatefirmePOST = @"<div>
                             <div style='width:100%;'>
                                      <h5>Firme dopo il deposito</h5>
                              </div>
                              <div style='text-align:left'>
                                {firme}
                            </div>
                        </div>";
                    if (firmePost.Any())
                        body = body.Replace("{radGridFirmePostView}",
                                TemplatefirmePOST.Replace("{firme}", GetFirmatariEM(firmePost)))
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
                    body = body.Replace("{radGridFirmeView}", GetFirmatariEM(firmeDtos))
                        .Replace("{FIRMEANTE_COMMENTO_START}", string.Empty)
                        .Replace("{FIRMEANTE_COMMENTO_END}", string.Empty);
                    body = body.Replace("{radGridFirmePostView}", string.Empty)
                        .Replace("{FIRME_COMMENTO_START}", "<!--")
                        .Replace("{FIRME_COMMENTO_END}", "-->");
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

                if (currentUser != null && currentUser.CurrentRole == RuoliIntEnum.Segreteria_Assemblea &&
                    !string.IsNullOrEmpty(emendamento.NOTE_EM))
                    body = body.Replace("{lblNotePrivateEMView}",
                        $"Note Riservate: {emendamento.NOTE_EM.Replace("{NOTEPRIV_COMMENTO_START}", string.Empty).Replace("{NOTEPRIV_COMMENTO_END}", string.Empty)}");
                else
                    body = body.Replace("{lblNotePrivateEMView}", string.Empty)
                        .Replace("{NOTEPRIV_COMMENTO_START}", "<!--")
                        .Replace("{NOTEPRIV_COMMENTO_END}", "-->");

                body = body.Replace("{QRCode}", string.Empty);
            }
            catch (Exception e)
            {
                Log.Error("GetBodyPDF", e);
                throw e;
            }
        }

        internal static void GetBodyMail(EmendamentiDto emendamento, IEnumerable<FirmeDto> firme, bool isDeposito,
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

                if (emendamento.STATI_EM.IDStato >= (int) StatiEnum.Depositato)
                {
                    //DEPOSITATO
                    body = body.Replace("{lblDepositoEMView}",
                        firmeDtos.Any(s => s.ufficio)
                            ? "Emendamento Depositato d'ufficio"
                            : $"Emendamento Depositato il {Convert.ToDateTime(emendamento.DataDeposito):dd/MM/yyyy HH:mm}");

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
                    "<img src='" + Path.Combine(AppSettingsConfiguration.urlPEM, "/images/LogoCRL120px.gif") +
                    " style='120px;'/>");

                body = body.Replace("{LINKPEM}",
                    $"{string.Format(AppSettingsConfiguration.urlPEM_ViewEM, emendamento.UIDEM)}");

                body = body.Replace("{LINKPEMRIEPILOGO}", string.Empty);
            }
            catch (Exception e)
            {
                Log.Error("GetBodyMail", e);
                throw e;
            }
        }

        internal static string Decrypt(string strData, string key = "")
        {
            try
            {
                key = !string.IsNullOrEmpty(key)
                    ? DecryptString(key, AppSettingsConfiguration.masterKey)
                    : AppSettingsConfiguration.masterKey;

                return DecryptString(strData, key);
            }
            catch (Exception e)
            {
                Log.Error("DecryptString", e);
                throw e;
            }
        }

        private static string DecryptString(string EncryptedString, string Key)
        {
            try
            {
                byte[] Results;
                var UTF8 = new UTF8Encoding();

                var HashProvider = new MD5CryptoServiceProvider();
                var TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Key));

                var TDESAlgorithm = new TripleDESCryptoServiceProvider
                {
                    Key = TDESKey, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7
                };

                var DataToDecrypt = Convert.FromBase64String(EncryptedString);

                try
                {
                    var Decryptor = TDESAlgorithm.CreateDecryptor();
                    Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
                }
                finally
                {
                    TDESAlgorithm.Clear();
                    HashProvider.Clear();
                }

                return UTF8.GetString(Results);
            }
            catch (Exception e)
            {
                Log.Error("DecryptString", e);
                Console.WriteLine("EM CORROTTO");
                return "<font style='color:red'>Valore Corrotto</font>";
            }
        }
    }
}