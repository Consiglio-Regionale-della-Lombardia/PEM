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

using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Request;
using PortaleRegione.Gateway;
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
            <div class='chip' style='margin: 5px; min-width: unset!important'>
                <img src='http://intranet.consiglio.regione.lombardia.it/GC/foto/{{foto}}'>
                {{DisplayName}}
                {{OPZIONALE}}
            </div>";

        public static string GetRelatori(IEnumerable<PersonaLightDto> persone)
        {
            if (!persone.Any()) return default;

            var result = new List<string>();

            foreach (var persona in persone)
                result.Add(_chipTemplate.Replace("{{foto}}", persona.foto)
                    .Replace("{{DisplayName}}", $"{persona.DisplayName}")
                    .Replace("{{OPZIONALE}}", ""));

            return result.Aggregate((i, j) => i + j);
        }

        #region GetDestinatariNotifica

        /// <summary>
        ///     Ritorna lista destinatari per la visualizzazione client
        /// </summary>
        /// <param name="destinatari"></param>
        /// <returns></returns>
        public static async Task<string> GetDestinatariNotifica(IEnumerable<DestinatariNotificaDto> destinatari,
            string token)
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

        #endregion

        public static string GetCSS_TipoDASI(AttoDASIDto atto)
        {
            var result = GetCSS_TipoDASI(atto.Tipo);
            if (atto.IDStato == (int)StatiAttoEnum.BOZZA_CARTACEA)
            {
                result = TipoAttoCSSConst.CARTACEO;
            }

            return result;
        }

        public static string GetCSS_TipoDASI(int tipoAtto)
        {
            switch ((TipoAttoEnum)tipoAtto)
            {
                case TipoAttoEnum.ITR:
                    return TipoAttoCSSConst.ITR;
                case TipoAttoEnum.IQT:
                    return TipoAttoCSSConst.IQT;
                case TipoAttoEnum.ITL:
                    return TipoAttoCSSConst.ITL;
                case TipoAttoEnum.MOZ:
                    return TipoAttoCSSConst.MOZ;
                case TipoAttoEnum.ODG:
                    return TipoAttoCSSConst.ODG;
                case TipoAttoEnum.PDL:
                    return string.Empty;
                case TipoAttoEnum.PDA:
                    return string.Empty;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipoAtto), tipoAtto, null);
            }
        }

        public static string GetTooltip_TipoDASI(int tipoAtto)
        {
            switch ((TipoAttoEnum)tipoAtto)
            {
                case TipoAttoEnum.ITR:
                    return "Interrogazione";
                case TipoAttoEnum.IQT:
                    return "Interrogazione a risposta immediata";
                case TipoAttoEnum.ITL:
                    return "Interpellanza";
                case TipoAttoEnum.MOZ:
                    return "Mozione Ordinaria";
                case TipoAttoEnum.ODG:
                    return "Ordine del Giorno";
                case TipoAttoEnum.PDL:
                    return "Progetto di Legge";
                case TipoAttoEnum.PDA:
                    return TipoAttoEnum.PDA.ToString();
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipoAtto), tipoAtto, null);
            }
        }

        public static string GetCSS_StatoDASI(int stato)
        {
            switch ((StatiAttoEnum)stato)
            {
                case StatiAttoEnum.BOZZA:
                    return StatiAttoCSSConst.BOZZA;
                case StatiAttoEnum.PRESENTATO:
                    return StatiAttoCSSConst.PRESENTATO;
                case StatiAttoEnum.IN_TRATTAZIONE:
                    return StatiAttoCSSConst.IN_TRATTAZIONE;
                case StatiAttoEnum.CHIUSO:
                case StatiAttoEnum.CHIUSO_RITIRATO:
                case StatiAttoEnum.CHIUSO_DECADUTO:
                    return StatiAttoCSSConst.CHIUSO;
                case StatiAttoEnum.BOZZA_CARTACEA:
                    return StatiAttoCSSConst.CARTACEO;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stato), stato, null);
            }
        }

        public static string GetText_TipoRispostaCommissioneTooltipDASI(AttoDASIDto atto)
        {
            if (atto.IDTipo_Risposta != (int)TipoRispostaEnum.COMMISSIONE) return string.Empty;
            if (!atto.Commissioni.Any())
                return string.Empty;
            return atto.Commissioni.Select(c => c.nome_organo).Aggregate((i, j) => i + "<br>" + j);
        }

        #region GetFirmatari

        /// <summary>
        ///     Ritorna la lista dei firmatari per visualizzazione client
        /// </summary>
        /// <param name="firme"></param>
        /// <param name="currentUId"></param>
        /// <param name="tipo"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static async Task<string> GetFirmatari(IEnumerable<FirmeDto> firme, Guid currentUId,
            FirmeTipoEnum tipo,
            string token,
            bool tag = false)
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
                    result.Add(_chipTemplate.Replace("{{foto}}", proponente.foto)
                        .Replace("{{DisplayName}}",
                            $"<span style='text-decoration:line-through;color:grey'>{firmaProponente.FirmaCert}</span>")
                        .Replace("{{OPZIONALE}}", ""));
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
                : "Firmatari";
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

        /// <summary>
        ///     Ritorna la lista dei firmatari per visualizzazione client
        /// </summary>
        /// <param name="firme"></param>
        /// <param name="currentUId"></param>
        /// <param name="tipo"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static async Task<string> GetFirmatariDASI(IEnumerable<AttiFirmeDto> firme, Guid currentUId,
            FirmeTipoEnum tipo,
            string token,
            bool tag = false)
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
                    result.Add(_chipTemplate.Replace("{{foto}}", proponente.foto)
                        .Replace("{{DisplayName}}",
                            $"<span style='text-decoration:line-through;color:grey'>{firmaProponente.FirmaCert}</span>")
                        .Replace("{{OPZIONALE}}", ""));
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

            var header_firme = tipo == FirmeTipoEnum.DOPO_DEPOSITO
                ? "Firmatari dopo la presentazione"
                : "Firmatari";
            var titoloColonna = "Firmatari dell'atto";
            var table = "<ul class=\"collection\">{{BODY_FIRME}}</ul>";
            var body = $"<li class=\"collection-header\"><h4 style=\"margin-left:10px\">{header_firme}</h4></li>";
            var dto = await apiGateway.DASI.Get(firmeDtos.Select(f => f.UIDAtto).First());
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
                        if (dto.IDStato >= (int)StatiAttoEnum.PRESENTATO)
                            body +=
                                $"<a class='chip red center white-text secondary-content' style=\"min-width:unset;margin-top:-16px\" onclick=\"RitiraFirmaDASI('{firmeDto.UIDAtto}')\"><i class='icon material-icons'>delete</i> Ritira</a>";
                        else
                            body +=
                                $"<a class='chip red center white-text secondary-content' style=\"min-width:unset;margin-top:-16px\" onclick=\"EliminaFirmaDASI('{firmeDto.UIDAtto}')\"><i class='icon material-icons'>delete</i> Elimina</a>";
                    }
                }

                body += "</li>";
            }

            return table.Replace("{{titoloColonna}}", titoloColonna).Replace("{{BODY_FIRME}}", body);
        }

        #endregion
    }

    public class UtilityFilter
    {
        public void AddFilter_ByOggetto_Testo(ref BaseRequest<AttoDASIDto> model, string filtroOggetto)
        {
            if (!string.IsNullOrEmpty(filtroOggetto))
                model.filtro.Add(new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.Oggetto),
                    Operation = Operation.Contains,
                    Value = filtroOggetto,
                    Connector = FilterStatementConnector.And
                });
        }

        public void AddFilter_ByStato(ref BaseRequest<AttoDASIDto> model, string filtroStato, PersonaDto currentUser)
        {
            if (!string.IsNullOrEmpty(filtroStato))
                if (filtroStato != Convert.ToInt32(StatiAttoEnum.TUTTI).ToString())
                {
                    var operation = Operation.EqualTo;
                    if (filtroStato == Convert.ToInt32(StatiAttoEnum.PRESENTATO).ToString())
                    {
                        if (currentUser.IsSegreteriaAssemblea)
                            operation = Operation.EqualTo;
                        else
                            operation = Operation.GreaterThanOrEqualTo;
                    }

                    model.filtro.Add(new FilterStatement<AttoDASIDto>
                    {
                        PropertyId = nameof(AttoDASIDto.IDStato),
                        Operation = operation,
                        Value = filtroStato,
                        Connector = FilterStatementConnector.And
                    });

                    if (filtroStato == Convert.ToInt32(StatiAttoEnum.CHIUSO).ToString())
                    {
                        model.filtro.Add(new FilterStatement<AttoDASIDto>
                        {
                            PropertyId = nameof(AttoDASIDto.IDStato),
                            Operation = Operation.EqualTo,
                            Value = Convert.ToInt32(StatiAttoEnum.CHIUSO_RITIRATO).ToString(),
                            Connector = FilterStatementConnector.And
                        });
                        model.filtro.Add(new FilterStatement<AttoDASIDto>
                        {
                            PropertyId = nameof(AttoDASIDto.IDStato),
                            Operation = Operation.EqualTo,
                            Value = Convert.ToInt32(StatiAttoEnum.CHIUSO_DECADUTO).ToString(),
                            Connector = FilterStatementConnector.And
                        });
                    }
                }
                else
                {
                    if (currentUser.IsSegreteriaAssemblea)
                    {
                        model.filtro.Add(new FilterStatement<AttoDASIDto>
                        {
                            PropertyId = nameof(AttoDASIDto.IDStato),
                            Operation = Operation.EqualTo,
                            Value = ((int)StatiAttoEnum.PRESENTATO).ToString(),
                            Connector = FilterStatementConnector.And
                        });
                    }
                    else
                    {
                        model.filtro.Add(new FilterStatement<AttoDASIDto>
                        {
                            PropertyId = nameof(AttoDASIDto.IDStato),
                            Operation = Operation.EqualTo,
                            Value = ((int)StatiAttoEnum.BOZZA).ToString(),
                            Connector = FilterStatementConnector.And
                        });
                    }
                }
        }

        public void AddFilter_ByTipoRisposta(ref BaseRequest<AttoDASIDto> model, string filtroTipoRisposta)
        {
            if (!string.IsNullOrEmpty(filtroTipoRisposta))
                if (filtroTipoRisposta != Convert.ToInt32(StatiAttoEnum.TUTTI).ToString())
                {
                    var operation = Operation.EqualTo;
                    model.filtro.Add(new FilterStatement<AttoDASIDto>
                    {
                        PropertyId = nameof(AttoDASIDto.IDTipo_Risposta),
                        Operation = operation,
                        Value = filtroTipoRisposta,
                        Connector = FilterStatementConnector.And
                    });
                }
        }

        public void AddFilter_ByNumeroAtto(ref BaseRequest<AttoDASIDto> model, string filtroNAtto,
            string filtroNAtto2)
        {
            if (!string.IsNullOrEmpty(filtroNAtto))
            {
                var success = int.TryParse(filtroNAtto, out var number);
                if (success && number > 0)
                {
                    var operation = Operation.EqualTo;
                    var success2 = int.TryParse(filtroNAtto2, out var number2);
                    if (success2 && number2 > 0)
                    {
                        operation = Operation.GreaterThanOrEqualTo;
                        model.filtro.Add(new FilterStatement<AttoDASIDto>
                        {
                            PropertyId = nameof(AttoDASIDto.NAtto) + "_search",
                            Operation = Operation.LessThanOrEqualTo,
                            Value = number2,
                            Connector = FilterStatementConnector.And
                        });
                    }

                    model.filtro.Add(new FilterStatement<AttoDASIDto>
                    {
                        PropertyId = nameof(AttoDASIDto.NAtto) + "_search",
                        Operation = operation,
                        Value = number,
                        Connector = FilterStatementConnector.And
                    });
                }
            }
        }

        public void AddFilter_ByTipo(ref BaseRequest<AttoDASIDto> model, string filtroTipo,
            string filtroTipoTrattazione, ClientModeEnum mode)
        {
            if (filtroTipoTrattazione != "0" && mode == ClientModeEnum.TRATTAZIONE)
                model.filtro.Add(new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.Tipo),
                    Operation = Operation.EqualTo,
                    Value = filtroTipoTrattazione,
                    Connector = FilterStatementConnector.And
                });
            else if (filtroTipo != "0" && mode == ClientModeEnum.GRUPPI)
                if (filtroTipo != Convert.ToInt32(TipoAttoEnum.TUTTI).ToString())
                    model.filtro.Add(new FilterStatement<AttoDASIDto>
                    {
                        PropertyId = nameof(AttoDASIDto.Tipo),
                        Operation = Operation.EqualTo,
                        Value = filtroTipo,
                        Connector = FilterStatementConnector.And
                    });
        }

        public void AddFilter_BySoggetto(ref BaseRequest<AttoDASIDto> model, string filtroSoggettoDest)
        {
            if (string.IsNullOrEmpty(filtroSoggettoDest)) return;

            var filtro_split = filtroSoggettoDest.Split(',');
            if (filtro_split.Length <= 0) return;

            foreach (var id_Carica in filtro_split)
            {
                var filtro = new FilterStatement<AttoDASIDto>
                {
                    PropertyId = "SoggettiDestinatari",
                    Operation = Operation.EqualTo,
                    Value = id_Carica
                };
                model.filtro.Add(filtro);
            }
        }

        public void AddFilter_BySeduta(ref BaseRequest<AttoDASIDto> model, string filtroSeduta)
        {
            if (!string.IsNullOrEmpty(filtroSeduta))
            {
                var guid = new Guid(filtroSeduta);
                if (guid == Guid.Empty)
                    return;

                model.filtro.Add(new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.UIDSeduta),
                    Operation = Operation.EqualTo,
                    Value = guid.ToString(),
                    Connector = FilterStatementConnector.And
                });
            }
        }

        public void AddFilter_ByLegislatura(ref BaseRequest<AttoDASIDto> model, string filtroLegislatura)
        {
            if (!string.IsNullOrEmpty(filtroLegislatura))
            {
                var success = int.TryParse(filtroLegislatura, out var number);
                if (success && number > 0)
                    model.filtro.Add(new FilterStatement<AttoDASIDto>
                    {
                        PropertyId = nameof(AttoDASIDto.Legislatura),
                        Operation = Operation.EqualTo,
                        Value = number,
                        Connector = FilterStatementConnector.And
                    });
            }
        }

        /// <summary>
        ///     Aggiunge il filtro alla request di ricerca degli emendamenti
        /// </summary>
        /// <param name="model"></param>
        /// <param name="numero_em"></param>
        public void AddFilter_ByNUM(ref BaseRequest<EmendamentiDto> model, string numero_em)
        {
            if (!string.IsNullOrEmpty(numero_em))
                model.filtro.Add(new FilterStatement<EmendamentiDto>
                {
                    PropertyId = nameof(EmendamentiDto.N_EM),
                    Operation = Operation.EqualTo,
                    Value = numero_em,
                    Connector = FilterStatementConnector.And
                });
        }

        /// <summary>
        ///     Aggiunge il filtro alla request di ricerca degli emendamenti
        /// </summary>
        /// <param name="model"></param>
        /// <param name="stringa1"></param>
        /// <param name="stringa2"></param>
        /// <param name="connettore"></param>
        public void AddFilter_ByText(ref BaseRequest<EmendamentiDto> model, string stringa1,
            string stringa2 = "", int connettore = 0)
        {
            if (!string.IsNullOrEmpty(stringa1))
            {
                var filtro1 = new FilterStatement<EmendamentiDto>
                {
                    PropertyId = nameof(EmendamentiDto.TestoEM_originale),
                    Operation = Operation.Contains,
                    Value = stringa1
                };
                if (connettore > 0) filtro1.Connector = (FilterStatementConnector)connettore;

                model.filtro.Add(filtro1);
            }

            if (!string.IsNullOrEmpty(stringa2))
            {
                var filtro2 = new FilterStatement<EmendamentiDto>
                {
                    PropertyId = nameof(EmendamentiDto.TestoEM_originale),
                    Operation = Operation.Contains,
                    Value = stringa2
                };
                if (connettore > 0) filtro2.Connector = (FilterStatementConnector)connettore;

                model.filtro.Add(filtro2);
            }
        }

        /// <summary>
        ///     Aggiunge il filtro alla request di ricerca degli emendamenti
        /// </summary>
        /// <param name="model"></param>
        /// <param name="q_parte"></param>
        /// <param name="q_parte_titolo"></param>
        /// <param name="q_parte_capo"></param>
        /// <param name="q_parte_articolo"></param>
        /// <param name="q_parte_comma"></param>
        /// <param name="q_parte_lettera"></param>
        /// <param name="q_parte_letteraOLD"></param>
        /// <param name="q_parte_missione"></param>
        /// <param name="q_parte_programma"></param>
        public void AddFilter_ByPart(ref BaseRequest<EmendamentiDto> model, string q_parte,
            string q_parte_titolo,
            string q_parte_capo,
            string q_parte_articolo, string q_parte_comma, string q_parte_lettera, string q_parte_letteraOLD,
            string q_parte_missione, string q_parte_programma)
        {
            if (!string.IsNullOrEmpty(q_parte) && q_parte != "0")
            {
                model.filtro.Add(new FilterStatement<EmendamentiDto>
                {
                    PropertyId = nameof(EmendamentiDto.IDParte),
                    Operation = Operation.EqualTo,
                    Value = q_parte,
                    Connector = FilterStatementConnector.And
                });
                var filtro_parte_enum = Convert.ToInt16(q_parte);
                switch ((PartiEMEnum)filtro_parte_enum)
                {
                    case PartiEMEnum.Titolo_PDL:
                        break;
                    case PartiEMEnum.Titolo:
                        if (!string.IsNullOrEmpty(q_parte_titolo))
                            model.filtro.Add(new FilterStatement<EmendamentiDto>
                            {
                                PropertyId = nameof(EmendamentiDto.NTitolo),
                                Operation = Operation.EqualTo,
                                Value = q_parte_titolo,
                                Connector = FilterStatementConnector.And
                            });

                        break;
                    case PartiEMEnum.Capo:
                        if (!string.IsNullOrEmpty(q_parte_capo))
                            model.filtro.Add(new FilterStatement<EmendamentiDto>
                            {
                                PropertyId = nameof(EmendamentiDto.NCapo),
                                Operation = Operation.EqualTo,
                                Value = q_parte_capo,
                                Connector = FilterStatementConnector.And
                            });

                        break;
                    case PartiEMEnum.Articolo:
                        if (!string.IsNullOrEmpty(q_parte_articolo) && q_parte_articolo != "0")
                            model.filtro.Add(new FilterStatement<EmendamentiDto>
                            {
                                PropertyId = nameof(EmendamentiDto.UIDArticolo),
                                Operation = Operation.EqualTo,
                                Value = q_parte_articolo,
                                Connector = FilterStatementConnector.And
                            });

                        if (!string.IsNullOrEmpty(q_parte_comma) && q_parte_comma != "0")
                            model.filtro.Add(new FilterStatement<EmendamentiDto>
                            {
                                PropertyId = nameof(EmendamentiDto.UIDComma),
                                Operation = Operation.EqualTo,
                                Value = q_parte_comma,
                                Connector = FilterStatementConnector.And
                            });

                        if (!string.IsNullOrEmpty(q_parte_lettera) && q_parte_lettera != "0")
                            model.filtro.Add(new FilterStatement<EmendamentiDto>
                            {
                                PropertyId = nameof(EmendamentiDto.UIDLettera),
                                Operation = Operation.EqualTo,
                                Value = q_parte_lettera,
                                Connector = FilterStatementConnector.And
                            });

                        if (!string.IsNullOrEmpty(q_parte_letteraOLD))
                            model.filtro.Add(new FilterStatement<EmendamentiDto>
                            {
                                PropertyId = nameof(EmendamentiDto.NLettera),
                                Operation = Operation.EqualTo,
                                Value = q_parte_letteraOLD,
                                Connector = FilterStatementConnector.And
                            });

                        break;
                    case PartiEMEnum.Missione:
                        if (!string.IsNullOrEmpty(q_parte_missione) && q_parte_missione != "0")
                            model.filtro.Add(new FilterStatement<EmendamentiDto>
                            {
                                PropertyId = nameof(EmendamentiDto.NMissione),
                                Operation = Operation.EqualTo,
                                Value = q_parte_missione,
                                Connector = FilterStatementConnector.And
                            });

                        if (!string.IsNullOrEmpty(q_parte_programma))
                            model.filtro.Add(new FilterStatement<EmendamentiDto>
                            {
                                PropertyId = nameof(EmendamentiDto.NProgramma),
                                Operation = Operation.EqualTo,
                                Value = q_parte_programma,
                                Connector = FilterStatementConnector.And
                            });

                        break;
                    case PartiEMEnum.Allegato_Tabella:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        ///     Aggiunge il filtro alla request di ricerca degli emendamenti
        /// </summary>
        /// <param name="model"></param>
        /// <param name="statoId"></param>
        public void AddFilter_ByState(ref BaseRequest<EmendamentiDto> model, string statoId)
        {
            if (!string.IsNullOrEmpty(statoId))
            {
                if (statoId.Equals(((int)StatiEnum.Approvato).ToString()))
                {
                    model.filtro.Add(new FilterStatement<EmendamentiDto>
                    {
                        PropertyId = nameof(EmendamentiDto.IDStato),
                        Operation = Operation.EqualTo,
                        Value = Convert.ToInt32(statoId),
                        Connector = FilterStatementConnector.Or
                    });
                    model.filtro.Add(new FilterStatement<EmendamentiDto>
                    {
                        PropertyId = nameof(EmendamentiDto.IDStato),
                        Operation = Operation.EqualTo,
                        Value = (int)StatiEnum.Approvato_Con_Modifiche,
                        Connector = FilterStatementConnector.And
                    });
                }
                else
                {
                    model.filtro.Add(new FilterStatement<EmendamentiDto>
                    {
                        PropertyId = nameof(EmendamentiDto.IDStato),
                        Operation = Operation.EqualTo,
                        Value = statoId,
                        Connector = FilterStatementConnector.And
                    });
                }
            }
        }

        /// <summary>
        ///     Aggiunge il filtro alla request di ricerca degli emendamenti
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tipoId"></param>
        public void AddFilter_ByType(ref BaseRequest<EmendamentiDto> model, string tipoId)
        {
            if (!string.IsNullOrEmpty(tipoId) && tipoId != "0")
                model.filtro.Add(new FilterStatement<EmendamentiDto>
                {
                    PropertyId = nameof(EmendamentiDto.IDTipo_EM),
                    Operation = Operation.EqualTo,
                    Value = tipoId,
                    Connector = FilterStatementConnector.And
                });
        }

        /// <summary>
        ///     Aggiunge il filtro alla request di ricerca degli emendamenti
        /// </summary>
        /// <param name="model"></param>
        /// <param name="personaUID"></param>
        /// <param name="solo_miei"></param>
        public void AddFilter_My(ref BaseRequest<EmendamentiDto> model, Guid personaUID, string solo_miei)
        {
            if (solo_miei != "on") return;

            var filtro1 = new FilterStatement<EmendamentiDto>
            {
                PropertyId = nameof(EmendamentiDto.IDStato),
                Operation = Operation.NotEqualTo,
                Value = (int)StatiEnum.Bozza_Riservata,
                Connector = FilterStatementConnector.Or
            };
            model.filtro.Add(filtro1);
            var filtro2 = new FilterStatement<EmendamentiDto>
            {
                PropertyId = nameof(EmendamentiDto.IDStato),
                Operation = Operation.EqualTo,
                Value = (int)StatiEnum.Bozza_Riservata,
                Connector = FilterStatementConnector.And
            };
            model.filtro.Add(filtro2);
            var filtro3 = new FilterStatement<EmendamentiDto>
            {
                PropertyId = nameof(EmendamentiDto.UIDPersonaProponente),
                Operation = Operation.EqualTo,
                Value = personaUID,
                Connector = FilterStatementConnector.Or
            };
            model.filtro.Add(filtro3);
            var filtro4 = new FilterStatement<EmendamentiDto>
            {
                PropertyId = nameof(EmendamentiDto.UIDPersonaCreazione),
                Operation = Operation.EqualTo,
                Value = personaUID,
                Connector = FilterStatementConnector.Or
            };
            model.filtro.Add(filtro4);
        }

        /// <summary>
        ///     Aggiunge il filtro alla request di ricerca degli emendamenti
        /// </summary>
        /// <param name="model"></param>
        /// <param name="solo_effetti_finanziari"></param>
        public void AddFilter_Financials(ref BaseRequest<EmendamentiDto> model, string solo_effetti_finanziari)
        {
            if (solo_effetti_finanziari != "on") return;

            var filtro1 = new FilterStatement<EmendamentiDto>
            {
                PropertyId = nameof(EmendamentiDto.EffettiFinanziari),
                Operation = Operation.EqualTo,
                Value = 1,
                Connector = FilterStatementConnector.And
            };
            model.filtro.Add(filtro1);
        }

        /// <summary>
        ///     Aggiunge il filtro alla request di ricerca degli emendamenti
        /// </summary>
        /// <param name="model"></param>
        /// <param name="gruppi"></param>
        public void AddFilter_Groups(ref BaseRequest<EmendamentiDto> model, string gruppi)
        {
            if (string.IsNullOrEmpty(gruppi)) return;

            var gruppi_split = gruppi.Split(',');
            if (gruppi_split.Length <= 0) return;

            foreach (var groupId in gruppi_split)
            {
                var filtro = new FilterStatement<EmendamentiDto>
                {
                    PropertyId = nameof(EmendamentiDto.id_gruppo),
                    Operation = Operation.EqualTo,
                    Value = groupId,
                    Connector = FilterStatementConnector.Or
                };
                model.filtro.Add(filtro);
            }
        }

        /// <summary>
        ///     Aggiunge il filtro alla request di ricerca degli emendamenti
        /// </summary>
        /// <param name="model"></param>
        /// <param name="proponenti"></param>
        public void AddFilter_Proponents(ref BaseRequest<EmendamentiDto> model, string proponenti)
        {
            if (string.IsNullOrEmpty(proponenti)) return;

            var prop_split = proponenti.Split(',');
            if (prop_split.Length <= 0) return;

            foreach (var personaUId in prop_split)
            {
                var filtro = new FilterStatement<EmendamentiDto>
                {
                    PropertyId = nameof(EmendamentiDto.UIDPersonaProponente),
                    Operation = Operation.EqualTo,
                    Value = personaUId,
                    Connector = FilterStatementConnector.Or
                };
                model.filtro.Add(filtro);
            }
        }

        /// <summary>
        ///     Aggiunge il filtro alla request di ricerca degli emendamenti
        /// </summary>
        /// <param name="model"></param>
        /// <param name="firmatari"></param>
        public void AddFilter_Signers(ref BaseRequest<EmendamentiDto> model, string firmatari)
        {
            if (string.IsNullOrEmpty(firmatari)) return;

            var firma_split = firmatari.Split(',');
            if (firma_split.Length <= 0) return;

            foreach (var personaUId in firma_split)
            {
                var filtro = new FilterStatement<EmendamentiDto>
                {
                    PropertyId = "Firmatario",
                    Operation = Operation.EqualTo,
                    Value = personaUId
                };
                model.filtro.Add(filtro);
            }
        }

        public void AddFilter_ByDataPresentazione(ref BaseRequest<AttoDASIDto> model, string filtroDa,
            string filtroA)
        {
            if (!string.IsNullOrEmpty(filtroDa))
            {
                var operation = Operation.GreaterThanOrEqualTo;

                if (!string.IsNullOrEmpty(filtroA))
                {
                    operation = Operation.GreaterThanOrEqualTo;
                    model.filtro.Add(new FilterStatement<AttoDASIDto>
                    {
                        PropertyId = nameof(AttoDASIDto.Timestamp),
                        Operation = Operation.LessThanOrEqualTo,
                        Value = Convert.ToDateTime(filtroA).ToString("yyyy-MM-dd") + " 23:59:59",
                        Connector = FilterStatementConnector.And
                    });
                }
                else
                {
                    model.filtro.Add(new FilterStatement<AttoDASIDto>
                    {
                        PropertyId = nameof(AttoDASIDto.Timestamp),
                        Operation = Operation.LessThanOrEqualTo,
                        Value = Convert.ToDateTime(filtroDa).ToString("yyyy-MM-dd") + " 23:59:59",
                        Connector = FilterStatementConnector.And
                    });
                }

                model.filtro.Add(new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.Timestamp),
                    Operation = operation,
                    Value = Convert.ToDateTime(filtroDa).ToString("yyyy-MM-dd") + " 00:00:01",
                    Connector = FilterStatementConnector.And
                });
            }
        }

        public void AddFilter_ByDataSeduta(ref BaseRequest<AttoDASIDto> model, Guid sedutaUId)
        {
            if (sedutaUId != Guid.Empty)
                model.filtro.Add(new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.UIDSeduta),
                    Operation = Operation.EqualTo,
                    Value = sedutaUId,
                    Connector = FilterStatementConnector.And
                });
        }
    }
}