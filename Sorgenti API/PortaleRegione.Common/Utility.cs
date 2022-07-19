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
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;

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
            (int) StatiAttoEnum.BOZZA,
            (int) StatiAttoEnum.BOZZA_RISERVATA,
            (int) StatiAttoEnum.RITIRATO
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
                    return "";
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipoAtto), tipoAtto, null);
            }
        }

        public static string GetText_TipoMOZDASI(int tipoMOZ)
        {
            switch ((TipoMOZEnum) tipoMOZ)
            {
                case TipoMOZEnum.ORDINARIA:
                    return "Mozione";
                case TipoMOZEnum.URGENTE:
                    return "Urgente";
                case TipoMOZEnum.ABBINATA:
                    return "Abbinata";
                case TipoMOZEnum.SFIDUCIA:
                    return "Sfiducia";
                case TipoMOZEnum.CENSURA:
                    return "Censura";
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipoMOZ), tipoMOZ, null);
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
                case StatiAttoEnum.RITIRATO:
                    return "Ritirato";
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
            switch ((TipoRispostaEnum) IdTipoRisposta)
            {
                case TipoRispostaEnum.ORALE:
                    return "Orale";
                case TipoRispostaEnum.SCRITTO:
                    return "Scritto";
                case TipoRispostaEnum.COMMISSIONE:
                {
                    return "In Commissione";
                }
                case TipoRispostaEnum.IMMEDIATA:
                {
                    return "Immediata";
                }
                default:
                    return "";
            }
        }

        public static string GetText_TipoRichiestaDASI(int tipoRichiesta)
        {
            try
            {
                switch ((TipoRichiestaEnum) tipoRichiesta)
                {
                    case TipoRichiestaEnum.CHIEDE:
                        return "Chiede al";
                    case TipoRichiestaEnum.INVITA:
                        return "Invita il";
                    case TipoRichiestaEnum.IMPEGNA:
                        return "Impegna il";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(tipoRichiesta), tipoRichiesta, null);
                }
            }
            catch (Exception e)
            {
                return " ";
            }
        }

        public static string GetText_TipoRichiestaDestDASI(int tipoRichiestaDestinatario)
        {
            try
            {
                switch ((TipoRichiestaDestEnum) tipoRichiestaDestinatario)
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
                    case TipoRichiestaDestEnum.ALTRO:
                        return "ALTRO (il soggetto viene messo nel testo delle richieste dell'atto)";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(tipoRichiestaDestinatario),
                            tipoRichiestaDestinatario, null);
                }
            }
            catch (Exception e)
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
                    id = (int) e,
                    descr = e.ToString().Replace("_", " ")
                }).ToList();
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
    }
}