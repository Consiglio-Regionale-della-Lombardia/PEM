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

using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.Gateway;
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortaleRegione.Client.Helpers
{
    /// <summary>
    ///     Classe per elaborazioni dati per le pagine web
    /// </summary>
    public static class Utility
    {
        /// <summary>
        ///     Template chip
        /// </summary>
        private static readonly string _chipTemplate = @"
            <div class='chip' style='margin: 5px'>
                <img src='http://intranet.consiglio.regione.lombardia.it/GC/foto/{{foto}}'>
                {{DisplayName}}
                {{OPZIONALE}}
            </div>";

        #region GetFirmatariEM

        /// <summary>
        ///     Ritorna la lista dei firmatari per visualizzazione client
        /// </summary>
        /// <param name="firme"></param>
        /// <param name="currentUId"></param>
        /// <param name="tipo"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static async Task<string> GetFirmatariEM(IEnumerable<FirmeDto> firme, Guid currentUId,
            FirmeTipoEnum tipo,
            string token,
            bool tag = false)
        {
            try
            {
                if (firme == null)
                    return string.Empty;
                var firmeDtos = firme.ToList();
                if (!firmeDtos.Any())
                    return string.Empty;

                var apiGateway = new ApiGateway(token);

                if (tag)
                {
                    var result = new List<string>();

                    var firmaProponente = firmeDtos.First();
                    var proponente = await apiGateway.Persone.Get(firmaProponente.UID_persona);
                    if (string.IsNullOrEmpty(firmaProponente.Data_ritirofirma))
                        result.Add(_chipTemplate.Replace("{{foto}}", proponente.foto)
                        .Replace("{{DisplayName}}", $"<b>{firmaProponente.FirmaCert}</b>")
                        .Replace("{{OPZIONALE}}", ""));
                    else
                    {
                        result.Add(_chipTemplate.Replace("{{foto}}", proponente.foto)
                            .Replace("{{DisplayName}}", $"<span style='text-decoration:line-through;color:grey'>{firmaProponente.FirmaCert}</span>")
                            .Replace("{{OPZIONALE}}", ""));
                    }
                    firmeDtos.Remove(firmaProponente);

                    foreach (var firmeDto in firmeDtos)
                    {
                        var persona = await apiGateway.Persone.Get(firmeDto.UID_persona);
                        if (string.IsNullOrEmpty(firmeDto.Data_ritirofirma))
                            result.Add(_chipTemplate.Replace("{{foto}}", persona.foto)
                                .Replace("{{DisplayName}}", $"{firmeDto.FirmaCert}").Replace("{{OPZIONALE}}", ""));
                        else
                            result.Add(
                                $"<span style='text-decoration:line-through;color:grey'>{firmeDto.FirmaCert}</span>");
                    }

                    return result.Aggregate((i, j) => i + j);
                }

                var titoloColonna = tipo == FirmeTipoEnum.DOPO_DEPOSITO
                    ? "Firme aggiunte dopo il deposito"
                    : "Firmatari dell'emendamento";
                var table = "<ul class=\"collection\">{{BODY_FIRME}}</ul>";
                var body = "<li class=\"collection-header\"><h4 style=\"margin-left:10px\">Firmatari</h4></li>";
                var em = await apiGateway.Emendamento.Get(firmeDtos.Select(f => f.UIDEM).First());
                foreach (var firmeDto in firmeDtos)
                {
                    body += "<li class=\"collection-item with-header\">";

                    if (!string.IsNullOrEmpty(firmeDto.Data_ritirofirma))
                    {
                        body += $"<div><del>{firmeDto.FirmaCert}</del>";
                        body += $"<br/><label>firmato il </label><del>{firmeDto.Data_firma}</del>";
                        body += $"<br/><label>ritirato il </label>{firmeDto.Data_ritirofirma}</div>";
                    }
                    else
                    {
                        body += $"<div>{firmeDto.FirmaCert}";
                        body += $"<br/><label>firmato il </label>{firmeDto.Data_firma}";
                        if (currentUId == firmeDto.UID_persona)
                        {
                            if (em.IDStato >= (int)StatiEnum.Depositato)
                                body +=
                                    $"<a class='chip red center white-text secondary-content' style=\"min-width:unset;margin-top:-16px\" onclick=\"RitiraFirma('{firmeDto.UIDEM}')\"><i class='icon material-icons'>delete</i> Ritira</a>";
                            else
                                body +=
                                    $"<a class='chip red center white-text secondary-content' style=\"min-width:unset;margin-top:-16px\" onclick=\"EliminaFirma('{firmeDto.UIDEM}')\"><i class='icon material-icons'>delete</i> Elimina</a>";
                        }
                    }

                    body += "</li>";
                }

                return table.Replace("{{titoloColonna}}", titoloColonna).Replace("{{BODY_FIRME}}", body);
            }
            catch (Exception e)
            {
                Log.Error("GetFirmatariEM", e);
                throw e;
            }
        }

        #endregion

        #region GetDestinatariNotifica

        /// <summary>
        ///     Ritorna lista destinatari per la visualizzazione client
        /// </summary>
        /// <param name="destinatari"></param>
        /// <returns></returns>
        public static async Task<string> GetDestinatariNotifica(IEnumerable<DestinatariNotificaDto> destinatari, string token)
        {
            try
            {
                if (destinatari == null)
                    return string.Empty;
                var destinatariList = destinatari.ToList();
                if (!destinatariList.Any())
                    return string.Empty;

                var result = new List<string>();
                var templateAttesa = "<div title='Non ancora visualizzato' class='notifica-check amber'></div>";
                var templateFirmato = "<div title='Firmato' class='notifica-check green'></div>";
                var apiGateway = new ApiGateway(token);
                foreach (var destinatario in destinatariList)
                {
                    var templateOpzionale = templateAttesa;
                    if (destinatario.Visto)
                        templateOpzionale =
                            $"<div title='Visto il {destinatario.DataVisto.Value:dd/MM/yyyy HH:mm}' class='notifica-check blue'></div>";
                    if (destinatario.Firmato)
                        templateOpzionale = templateFirmato;

                    var persona = await apiGateway.Persone.Get(destinatario.UIDPersona,
                        destinatario.IdGruppo >= 10000);
                    result.Add(_chipTemplate.Replace("{{foto}}", persona.foto)
                        .Replace("{{DisplayName}}", $"{persona.DisplayName_GruppoCode}")
                        .Replace("{{OPZIONALE}}", templateOpzionale));
                }

                return result.Aggregate((i, j) => i + j);
            }
            catch (Exception e)
            {
                Log.Error("GetFirmatariEM", e);
                throw e;
            }
        }

        #endregion
    }
}