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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
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

        public static List<int> statiNonVisibili_Segreteria = new List<int>
        {
            (int)StatiAttoEnum.BOZZA,
            (int)StatiAttoEnum.BOZZA_RISERVATA,
            (int)StatiAttoEnum.BOZZA_CARTACEA
        };

        public static List<int> statiNonVisibili_Standard = new List<int>
        {
            (int)StatiAttoEnum.BOZZA_CARTACEA
        };

        public static List<TipoAttoEnum> tipiNonVisibili = new List<TipoAttoEnum>
        {
            TipoAttoEnum.PDL,
            TipoAttoEnum.PDA,
            TipoAttoEnum.DOC,
            TipoAttoEnum.PLP,
            TipoAttoEnum.PRE,
            TipoAttoEnum.PDN,
            TipoAttoEnum.REF,
            TipoAttoEnum.REL,
            TipoAttoEnum.ORG,
            TipoAttoEnum.RIS,
            TipoAttoEnum.ALTRO
        };

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
        ///     Metodo per avere i metadati dell'emendamento in formato visualizzabile
        /// </summary>
        /// <param name="em"></param>
        /// <returns></returns>
        public static string MetaDatiEM_LabelHtml(EmendamentiDto em)
        {
            var result = $"Emendamento {em.TIPI_EM.Tipo_EM} <br> {GetParteEM(em)}";
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

        public static string GetText_Tipo(AttoDASIDto atto)
        {
            return GetText_Tipo(atto.Tipo);
        }

        public static string GetText_Tipo(int tipoAtto)
        {
            switch ((TipoAttoEnum)tipoAtto)
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
                case TipoAttoEnum.PLP:
                    return TipoAttoEnum.PLP.ToString();
                case TipoAttoEnum.PRE:
                    return TipoAttoEnum.PRE.ToString();
                case TipoAttoEnum.PDN:
                    return TipoAttoEnum.PDN.ToString();
                case TipoAttoEnum.DOC:
                    return TipoAttoEnum.DOC.ToString();
                case TipoAttoEnum.REF:
                    return TipoAttoEnum.REF.ToString();
                case TipoAttoEnum.REL:
                    return TipoAttoEnum.REL.ToString();
                case TipoAttoEnum.ORG:
                    return TipoAttoEnum.ORG.ToString();
                case TipoAttoEnum.RIS:
                    return TipoAttoEnum.RIS.ToString();
                case TipoAttoEnum.ALTRO:
                    return "Dibattito";
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipoAtto), tipoAtto, null);
            }
        }

        public static string GetText_TipoOrganoRispostaDASI(int tipoOrgano)
        {
            switch ((TipoOrganoEnum)tipoOrgano)
            {
                case TipoOrganoEnum.COMMISSIONE:
                    return "Commissione";
                case TipoOrganoEnum.GIUNTA:
                    return "Giunta";
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipoOrgano), tipoOrgano, null);
            }
        }

        public static string GetText_TipoMOZDASI(int tipoMOZ)
        {
            switch ((TipoMOZEnum)tipoMOZ)
            {
                case TipoMOZEnum.ORDINARIA:
                case TipoMOZEnum.URGENTE:
                case TipoMOZEnum.ABBINATA:
                    return "Mozione";
                case TipoMOZEnum.SFIDUCIA:
                    return "Sfiducia";
                case TipoMOZEnum.CENSURA:
                    return "Censura";
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipoMOZ), tipoMOZ, null);
            }
        }
        
        public static string GetText_AreaPolitica(int area)
        {
            switch ((AreaPoliticaIntEnum)area)
            {
                case AreaPoliticaIntEnum.Maggioranza:
                    return AreaPoliticaEnum.Maggioranza;
                case AreaPoliticaIntEnum.Minoranza:
                    return AreaPoliticaEnum.Minoranza;
                case AreaPoliticaIntEnum.Misto_Maggioranza:
                    return AreaPoliticaEnum.Misto_Maggioranza;
                case AreaPoliticaIntEnum.Misto_Minoranza:
                    return AreaPoliticaEnum.Misto_Minoranza;
                case AreaPoliticaIntEnum.Misto:
                    return AreaPoliticaEnum.Misto;
                default:
                    throw new ArgumentOutOfRangeException(nameof(area), area, null);
            }
        }

        public static string GetText_StatoDASI(int stato, bool excel = true)
        {
            switch ((StatiAttoEnum)stato)
            {
                case StatiAttoEnum.BOZZA_RISERVATA:
                    return "Bozza Ris.";
                case StatiAttoEnum.BOZZA:
                    return "Bozza";
                case StatiAttoEnum.PRESENTATO:
                {
                    return excel ? "Presentato" : "Depositato";
                }
                case StatiAttoEnum.IN_TRATTAZIONE:
                    return "In trattazione";
                case StatiAttoEnum.CHIUSO:
                    return "Chiuso";
                case StatiAttoEnum.CHIUSO_RITIRATO:
                    return "Chiuso Ritirato";
                case StatiAttoEnum.CHIUSO_DECADUTO:
                    return "Chiuso Decaduto";
                case StatiAttoEnum.TUTTI:
                    return "Tutti";
                case StatiAttoEnum.BOZZA_CARTACEA:
                    return "Bozza cartacea";
                case StatiAttoEnum.COMUNICAZIONE_ASSEMBLEA:
                    return "Comunicazione assemblea";

                case StatiAttoEnum.CHIUSO_DECADENZA_PER_FINE_LEGISLATURA:
                    return "Chiuso decaduto per fine legislatura";

                case StatiAttoEnum.CHIUSO_INAMMISSIBILE:
                    return "Chiuso inammissibile";

                case StatiAttoEnum.TRATTAZIONE_IN_ASSEMBLEA:
                    return "Trattazione in assemblea";

                default:
                    return "Stato non valido";
            }
        }

        public static string GetText_TipoRispostaDASI(int IdTipoRisposta, bool excel = false)
        {
            switch ((TipoRispostaEnum)IdTipoRisposta)
            {
                case TipoRispostaEnum.ORALE:
                    return "Orale";
                case TipoRispostaEnum.SCRITTA:
                    return "Scritta";
                case TipoRispostaEnum.COMMISSIONE:
                {
                    return excel ? "In commissione" : "In Commissione";
                }
                case TipoRispostaEnum.IMMEDIATA:
                {
                    return "Immediata";
                }
                case TipoRispostaEnum.ITER_IN_ASSEMBLEA:
                {
                    return "Iter in assemblea";
                }
                case TipoRispostaEnum.ITER_IN_ASSEMBLEA_COMMISSIONE:
                {
                    return "Iter in assemblea + commissione";
                }
                default:
                    return "";
            }
        }

        public static string GetText_TipoRichiestaDASI(int tipoRichiesta)
        {
            try
            {
                switch ((TipoRichiestaEnum)tipoRichiesta)
                {
                    case TipoRichiestaEnum.CHIEDE:
                        return "CHIEDE";
                    case TipoRichiestaEnum.INVITA:
                        return "INVITA";
                    case TipoRichiestaEnum.IMPEGNA:
                        return "IMPEGNA";
                    case TipoRichiestaEnum.INTERROGA:
                        return "INTERROGA";
                    default:
                        return " ";
                }
            }
            catch (Exception)
            {
                return " ";
            }
        }

        public static string GetText_TipoRichiestaDestDASI(int tipoRichiestaDestinatario)
        {
            try
            {
                switch ((TipoRichiestaDestEnum)tipoRichiestaDestinatario)
                {
                    case TipoRichiestaDestEnum.PRES_REG:
                        return "PRESIDENTE DELLA REGIONE";
                    case TipoRichiestaDestEnum.PRES_REG_ASS_AUTONOMIA_CULTURA:
                        return "PRESIDENTE DELLA REGIONE E L’ASSESSORE ALL’AUTONOMIA E CULTURA";
                    case TipoRichiestaDestEnum.PRES_G_REG_ASS_C:
                        return "PRESIDENTE DELLA GIUNTA REGIONALE E L’ASSESSORE COMPETENTE";
                    case TipoRichiestaDestEnum.PRES_G_REG_ASS_N_C:
                        return "PRESIDENTE DELLA GIUNTA REGIONALE E GLI ASSESSORI COMPETENTI";
                    case TipoRichiestaDestEnum.PRES_G_REG:
                        return "PRESIDENTE E LA GIUNTA REGIONALE";
                    case TipoRichiestaDestEnum.PRES_G_REG_E_ASS_C:
                        return "PRESIDENTE, LA GIUNTA REGIONALE E L’ASSESSORE COMPETENTE";
                    case TipoRichiestaDestEnum.PRES_G_REG_E_ASS_N_C:
                        return "PRESIDENTE, LA GIUNTA REGIONALE E GLI ASSESSORI COMPETENTI";
                    case TipoRichiestaDestEnum.G:
                        return "GIUNTA REGIONALE";
                    case TipoRichiestaDestEnum.G_REG_E_ASS_C:
                        return "GIUNTA REGIONALE E L’ASSESSORE COMPETENTE";
                    case TipoRichiestaDestEnum.ASS_C:
                        return "ASSESSORE COMPETENTE";
                    case TipoRichiestaDestEnum.ASS_C_N:
                        return "ASSESSORI COMPETENTI";
                    case TipoRichiestaDestEnum.ALTRO:
                        return "ALTRO (il soggetto viene messo nel testo delle richieste dell'atto)";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(tipoRichiestaDestinatario),
                            tipoRichiestaDestinatario, null);
                }
            }
            catch (Exception)
            {
                return " ";
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
                    id = (int)e,
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
        /// <param name="numero_em"></param>
        public static void AddFilter_ByAtto(ref BaseRequest<EmendamentiDto> model, string atto)
        {
            if (!string.IsNullOrEmpty(atto))
                model.filtro.Add(new FilterStatement<EmendamentiDto>
                {
                    PropertyId = nameof(EmendamentiDto.UIDAtto),
                    Operation = Operation.EqualTo,
                    Value = atto,
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
        public static void AddFilter_ByState(ref BaseRequest<EmendamentiDto> model, string statoId)
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

        /// <summary>
        ///     Aggiunge il filtro alla request di ricerca degli emendamenti
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tags"></param>
        public static void AddFilter_Tags(ref BaseRequest<EmendamentiDto> model, string tags)
        {
            if (string.IsNullOrEmpty(tags)) return;

            var filtro = new FilterStatement<EmendamentiDto>
            {
                PropertyId = "Tags",
                Operation = Operation.EqualTo,
                Value = tags
            };
            model.filtro.Add(filtro);
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

        public static string GetDisplayName(Type objectType, string propertyName)
        {
            var prop = objectType.GetProperty(propertyName);
            var displayNameAttribute = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                .FirstOrDefault() as DisplayNameAttribute;

            return displayNameAttribute?.DisplayName ?? propertyName;
        }

        public static List<List<T>> Split<T>(IList<T> source)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 100)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public static string ConvertiCaratteriSpeciali(string input)
        {
            StringBuilder sb = new StringBuilder(input);
        
            sb.Replace("À", "&Agrave;");
            sb.Replace("à", "&agrave;");
            sb.Replace("È", "&Egrave;");
            sb.Replace("è", "&egrave;");
            sb.Replace("Ì", "&Igrave;");
            sb.Replace("ì", "&igrave;");
            sb.Replace("Ò", "&Ograve;");
            sb.Replace("ò", "&ograve;");
            sb.Replace("Ù", "&Ugrave;");
            sb.Replace("ù", "&ugrave;");
        
            sb.Replace("Á", "&Aacute;");
            sb.Replace("á", "&aacute;");
            sb.Replace("É", "&Eacute;");
            sb.Replace("é", "&eacute;");
            sb.Replace("Í", "&Iacute;");
            sb.Replace("í", "&iacute;");
            sb.Replace("Ó", "&Oacute;");
            sb.Replace("ó", "&oacute;");
            sb.Replace("Ú", "&Uacute;");
            sb.Replace("ú", "&uacute;");
            
            sb.Replace("\"", "&quot;");
            sb.Replace("'", "&#39;");

            return sb.ToString();
        }
    }
}