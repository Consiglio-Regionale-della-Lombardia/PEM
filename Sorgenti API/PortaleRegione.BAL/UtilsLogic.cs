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

using PortaleRegione.Contracts;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.Logger;
using System;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace PortaleRegione.BAL
{
    public class UtilsLogic : BaseLogic
    {
        //private const string maddy = "xjuy i                                                          mamma  pap fà";
        private readonly IUnitOfWork _unitOfWork;

        public UtilsLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public string GenerateRandomCode()
        {
            var _random = new Random();
            return _random.Next(1101, 9999).ToString();
        }
        
        public async Task InvioMail(MailModel model)
        {
            try
            {
                if (!AppSettingsConfiguration.Invio_Notifiche)
                {
                    return;
                }

                var msg = new MailMessage
                {
                    From = new MailAddress(model.DA), Subject = model.OGGETTO, Body = model.MESSAGGIO,
                    IsBodyHtml = true
                };

                if (!string.IsNullOrEmpty(model.CC))
                {
                    var destinatariCC = model.CC.Split(";".ToCharArray());
                    foreach (var destinatario in destinatariCC)
                    {
                        if (!string.IsNullOrEmpty(destinatario))
                        {
                            msg.CC.Add(new MailAddress(destinatario));
                        }
                    }
                }

                var destinatari = model.A.Split(";".ToCharArray());
                foreach (var destinatario in destinatari)
                {
                    if (!string.IsNullOrEmpty(destinatario))
                    {
                        msg.To.Add(new MailAddress(destinatario));
                    }
                }

                if (!string.IsNullOrEmpty(model.pathAttachment))
                {
                    msg.Attachments.Add(new Attachment(model.pathAttachment));
                }

                var smtp = new SmtpClient(AppSettingsConfiguration.SMTP);

                await smtp.SendMailAsync(msg);
            }
            catch (Exception e)
            {
                Log.Error("InvioMail", e);
                throw e;
            }
        }
        
        public async Task SalvaDocumento(string proprietarioId, TipoAllegatoEnum tipoDoc, string pathFile)
        {
            try
            {
                switch (tipoDoc)
                {
                    case TipoAllegatoEnum.ATTO:
                        var atto = await _unitOfWork.Atti.Get(proprietarioId);
                        atto.Path_Testo_Atto = pathFile;
                        break;
                    case TipoAllegatoEnum.EMENDAMENTO_GENERICO:
                    case TipoAllegatoEnum.EMENDAMENTO_TECNICO:
                        var em = await _unitOfWork.Emendamenti.Get(proprietarioId);

                        switch (tipoDoc)
                        {
                            case TipoAllegatoEnum.EMENDAMENTO_TECNICO:
                                em.PATH_AllegatoTecnico = pathFile;
                                break;
                            case TipoAllegatoEnum.EMENDAMENTO_GENERICO:
                                em.PATH_AllegatoGenerico = pathFile;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("[TipoAllegatoEnum] Emendamento non valido");
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Log.Debug("SALVATAGGIO DOCUMENTO", e);
                throw e;
            }
        }
        
        public string ArchiviaDocumento(HttpPostedFile postedFile)
        {
            try
            {
                var path = HttpContext.Current.Server.MapPath(AppSettingsConfiguration.CartellaDocumentiAtti);
                var nomefile = DateTime.Now.ToString("yyyy") + DateTime.Now.ToString("MM") +
                               DateTime.Now.ToString("dd") + DateTime.Now.ToString("HH")
                               + DateTime.Now.ToString("mm") + DateTime.Now.ToString("ss") + "_" + Guid.NewGuid() +
                               Path.GetExtension(postedFile.FileName);
                var pathFile = Path.Combine(path, nomefile);
                postedFile.SaveAs(pathFile);
                return pathFile;
            }
            catch (Exception e)
            {
                Log.Error("ARCHIVIO", e);
                throw e;
            }
        }
    }
}