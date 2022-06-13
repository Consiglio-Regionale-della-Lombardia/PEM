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
using System.Linq;
using System.Text.RegularExpressions;
using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Request;

namespace PortaleRegione.Common
{
    /// <summary>
    ///     Classe con metodi in comune
    /// </summary>
    public static class Utility
    {
        public const string WORD_OPEN_P = "<p[^>]*>";
        public const string WORD_OPEN_A = "<a name[^>]*>";

        /// <summary>
        ///     Metodo per avere i metadati dell'emendamento in formato visualizzabile
        /// </summary>
        /// <param name="em"></param>
        /// <returns></returns>
        public static string MetaDatiEM_Label(EmendamentiDto em)
        {
            var result = $"Emendamento {em.TIPI_EM.Tipo_EM} - {GetParteEM(em)}";
            return result;
        }

        /// <summary>
        ///     Metodo per visualizzare la parte dell'emendamento
        /// </summary>
        /// <param name="em"></param>
        /// <returns></returns>
        public static string GetParteEM(EmendamentiDto em)
        {
            switch (em.PARTI_TESTO.IDParte)
            {
                case PartiEMEnum.Titolo_PDL:
                    return em.PARTI_TESTO.Parte;
                case PartiEMEnum.Titolo:
                    return $"Titolo: {em.NTitolo}";
                case PartiEMEnum.Capo:
                    return $"Capo: {em.NCapo}";
                case PartiEMEnum.Articolo:
                {
                    var strArticolo = string.Empty;
                    if (em.UIDArticolo.HasValue) strArticolo += $"Articolo: {em.ARTICOLI.Articolo}";

                    if (em.UIDComma.HasValue && em.UIDComma.GetValueOrDefault() != Guid.Empty)
                        strArticolo += $", Comma: {em.COMMI.Comma}";

                    if (!string.IsNullOrEmpty(em.NLettera))
                    {
                        strArticolo += $", Lettera: {em.NLettera}";
                    }
                    else
                    {
                        if (em.UIDLettera.HasValue) strArticolo += $", Lettera: {em.LETTERE.Lettera}";
                    }

                    return strArticolo;
                }
                case PartiEMEnum.Missione:
                    return $"Missione: {em.NMissione} Programma: {em.NProgramma} titolo: {em.NTitoloB}";
                case PartiEMEnum.Allegato_Tabella:
                    return "Allegato/Tabella";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Metodo per visualizzare se l'emedamento ha effetti finanziari oppure no
        /// </summary>
        /// <param name="effetti_finanziari"></param>
        /// <returns></returns>
        public static string EffettiFinanziariEM(int? effetti_finanziari)
        {
            switch (effetti_finanziari)
            {
                case 0:
                {
                    return "NO";
                }
                case 1:
                {
                    return "SI";
                }
                default:
                {
                    return "NON SPECIFICATO";
                }
            }
        }

        public static string GetText_TipoDASI(int tipoAtto)
        {
            switch ((TipoAttoEnum) tipoAtto)
            {
                case TipoAttoEnum.ITR:
                    return TipoAttoEnum.ITR.ToString();
                case TipoAttoEnum.IQT:
                    return TipoAttoEnum.IQT.ToString();
                case TipoAttoEnum.ITL:
                    return TipoAttoEnum.ITL.ToString();
                case TipoAttoEnum.MOZ:
                    return TipoAttoEnum.MOZ.ToString();
                case TipoAttoEnum.ODG:
                    return TipoAttoEnum.ODG.ToString();
                case TipoAttoEnum.PDL:
                    return TipoAttoEnum.PDL.ToString();
                case TipoAttoEnum.PDA:
                    return TipoAttoEnum.PDA.ToString();
                case TipoAttoEnum.TUTTI:
                    return "Tutti";
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipoAtto), tipoAtto, null);
            }
        }

        public static string GetText_StatoDASI(int stato)
        {
            switch ((StatiAttoEnum) stato)
            {
                case StatiAttoEnum.BOZZA_RISERVATA:
                    return "Bozza Ris.";
                case StatiAttoEnum.BOZZA:
                    return "Bozza";
                case StatiAttoEnum.PRESENTATO:
                    return "Presentato";
                case StatiAttoEnum.IN_TRATTAZIONE:
                    return "In Trattazione";
                case StatiAttoEnum.COMUNICAZIONE_ASSEMBLEA:
                    return "Comunicazione all’Assemblea";
                case StatiAttoEnum.TRATTAZIONE_ASSEMBLEA:
                    return "Trattazione all’Assemblea";
                case StatiAttoEnum.APPROVATO:
                    return "Approvato";
                case StatiAttoEnum.RESPINTO:
                    return "Respinto";
                case StatiAttoEnum.INAMMISSIBILE:
                    return "Inammissibile";
                case StatiAttoEnum.RITIRATO:
                    return "Ritirato";
                case StatiAttoEnum.DECADUTO:
                    return "Decaduto";
                case StatiAttoEnum.DECADUTO_FINE_MANDATO:
                    return "Decadenza FMC";
                case StatiAttoEnum.DECADUTO_FINE_LEGISLATURA:
                    return "Decadenza FL";
                case StatiAttoEnum.ALTRO:
                    return "Chiusura Altro";
                case StatiAttoEnum.CHIUSO:
                    return "Chiuso";
                case StatiAttoEnum.TUTTI:
                    return "Tutti";
                default:
                    throw new ArgumentOutOfRangeException(nameof(stato), stato, null);
            }
        }

        public static string GetText_TipoRispostaDASI(int IdTipoRisposta)
        {
            switch ((TipoRispostaEnum)IdTipoRisposta)
            {
                case TipoRispostaEnum.ORALE:
                    return "Orale";
                case TipoRispostaEnum.SCRITTO:
                    return "Scritto";
                case TipoRispostaEnum.COMMISSIONE:
                {
                    return "In Commissione";
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(IdTipoRisposta), IdTipoRisposta, null);
            }
        }

        /// <summary>
        ///     Metodo per convertire un enum in KeyValueDto
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<KeyValueDto> GetEnumList<T>()
        {
            return (from object e in Enum.GetValues(typeof(T))
                select new KeyValueDto
                {
                    id = (int) e,
                    descr = e.ToString().Replace("_", " ")
                }).ToList();
        }

        /// <summary>
        ///     Aggiunge il filtro alla request di ricerca degli emendamenti
        /// </summary>
        /// <param name="model"></param>
        /// <param name="numero_em"></param>
        public static void AddFilter_ByNUM(ref BaseRequest<EmendamentiDto> model, string numero_em)
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
        public static void AddFilter_ByText(ref BaseRequest<EmendamentiDto> model, string stringa1,
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
                if (connettore > 0) filtro1.Connector = (FilterStatementConnector) connettore;

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
                if (connettore > 0) filtro2.Connector = (FilterStatementConnector) connettore;

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
        public static void AddFilter_ByPart(ref BaseRequest<EmendamentiDto> model, string q_parte,
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
                switch ((PartiEMEnum) filtro_parte_enum)
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
        public static void AddFilter_ByState(ref BaseRequest<EmendamentiDto> model, string statoId)
        {
            if (!string.IsNullOrEmpty(statoId))
            {
                if (statoId.Equals(((int) StatiEnum.Approvato).ToString()))
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
                        Value = (int) StatiEnum.Approvato_Con_Modifiche,
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
        public static void AddFilter_ByType(ref BaseRequest<EmendamentiDto> model, string tipoId)
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
        public static void AddFilter_My(ref BaseRequest<EmendamentiDto> model, Guid personaUID, string solo_miei)
        {
            if (solo_miei != "on") return;

            var filtro1 = new FilterStatement<EmendamentiDto>
            {
                PropertyId = nameof(EmendamentiDto.IDStato),
                Operation = Operation.NotEqualTo,
                Value = (int) StatiEnum.Bozza_Riservata,
                Connector = FilterStatementConnector.Or
            };
            model.filtro.Add(filtro1);
            var filtro2 = new FilterStatement<EmendamentiDto>
            {
                PropertyId = nameof(EmendamentiDto.IDStato),
                Operation = Operation.EqualTo,
                Value = (int) StatiEnum.Bozza_Riservata,
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
        public static void AddFilter_Financials(ref BaseRequest<EmendamentiDto> model, string solo_effetti_finanziari)
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
        public static void AddFilter_Groups(ref BaseRequest<EmendamentiDto> model, string gruppi)
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
        public static void AddFilter_Proponents(ref BaseRequest<EmendamentiDto> model, string proponenti)
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
        public static void AddFilter_Signers(ref BaseRequest<EmendamentiDto> model, string firmatari)
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

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty).Replace("&nbsp;", " ");
        }

        private static string RegexPatterSubstitute(string text, string substitute, string regex_pattern)
        {
            var regex = new Regex(regex_pattern);
            return regex.Replace(text, substitute);
        }

        public static string CleanWordText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            text = RegexPatterSubstitute(text, "<br/>", WORD_OPEN_P);
            text = text.Replace("</p>", string.Empty);
            text = RegexPatterSubstitute(text, "<a>", WORD_OPEN_A);

            return text;
        }

        public static void AddFilter_ByOggetto(ref BaseRequest<AttoDASIDto> model, string filtroOggetto)
        {
            if (!string.IsNullOrEmpty(filtroOggetto))
            {
                model.filtro.Add(new FilterStatement<AttoDASIDto>
                {
                    PropertyId = nameof(AttoDASIDto.Oggetto),
                    Operation = Operation.Contains,
                    Value = filtroOggetto,
                    Connector = FilterStatementConnector.And
                });
            }
        }

        public static void AddFilter_ByStato(ref BaseRequest<AttoDASIDto> model, string filtroStato)
        {
            if (!string.IsNullOrEmpty(filtroStato))
            {
                if (filtroStato != Convert.ToInt32(StatiAttoEnum.TUTTI).ToString())
                {
                    model.filtro.Add(new FilterStatement<AttoDASIDto>
                    {
                        PropertyId = nameof(AttoDASIDto.IDStato),
                        Operation = Operation.EqualTo,
                        Value = filtroStato,
                        Connector = FilterStatementConnector.And
                    });
                }
            }
        }

        public static void AddFilter_ByTipo(ref BaseRequest<AttoDASIDto> model, string filtroTipo)
        {
            if (!string.IsNullOrEmpty(filtroTipo))
            {
                if (filtroTipo != Convert.ToInt32(TipoAttoEnum.TUTTI).ToString())
                {
                    model.filtro.Add(new FilterStatement<AttoDASIDto>
                    {
                        PropertyId = nameof(AttoDASIDto.Tipo),
                        Operation = Operation.EqualTo,
                        Value = filtroTipo,
                        Connector = FilterStatementConnector.And
                    });
                }
            }
        }

        public static void AddFilter_BySoggetto(ref BaseRequest<AttoDASIDto> model, string filtroSoggettoDest)
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
    }
}