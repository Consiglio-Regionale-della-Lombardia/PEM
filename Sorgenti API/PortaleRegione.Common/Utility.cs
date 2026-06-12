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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
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

        public static string GetText_TipoEstesoDASI(int tipoAtto)
        {
            switch ((TipoAttoEnum)tipoAtto)
            {
                case TipoAttoEnum.ITR:
                    return "Interrogazione";
                case TipoAttoEnum.IQT:
                    return "Interrogazione question time"; // #1268
                case TipoAttoEnum.ITL:
                    return "Interpellanza";
                case TipoAttoEnum.MOZ:
                    return "Mozione";
                case TipoAttoEnum.ODG:
                    return "Ordine del giorno";
                case TipoAttoEnum.RIS:
                    return "Risoluzione";
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipoAtto), tipoAtto, null);
            }
        }

        public static string GetText_TipoOrganoDASI(int tipoOrgano)
        {
            switch ((TipoOrganoEnum)tipoOrgano)
            {
                case TipoOrganoEnum.COMMISSIONE:
                    return "Commissione";
                case TipoOrganoEnum.GIUNTA:
                    return "Giunta";
                default:
                    return string.Empty;
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

        public static string GetText_TipoMOZDettaglioDASI(int tipoMOZ)
        {
            switch ((TipoMOZEnum)tipoMOZ)
            {
                case TipoMOZEnum.ORDINARIA:
                    return "Ordinaria";
                case TipoMOZEnum.ABBINATA:
                    return "Abbinata";
                case TipoMOZEnum.URGENTE:
                    return "Urgente";
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
                case AreaPoliticaIntEnum.Misto_Maggioranza_Minoranza:
                    return AreaPoliticaEnum.Misto_Maggioranza_Minoranza;
                case AreaPoliticaIntEnum.Nessuno: // #1300
                case AreaPoliticaIntEnum.Misto:
                    return AreaPoliticaEnum.Nessuno;
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
                case StatiAttoEnum.COMPLETATO:
                    return "Concluso"; // #1281
                case StatiAttoEnum.TUTTI:
                    return "Tutti";
                case StatiAttoEnum.BOZZA_CARTACEA:
                    return "Bozza cartacea";
                default:
                    return "Stato non valido";
            }
        }

        public static string GetText_ChiusuraIterDASI(int? stato, bool public_api = false)
        {
            if (stato == null)
            {
                if (public_api)
                    return string.Empty;
                
                return "--";
            }

            switch ((TipoChiusuraIterEnum)stato)
            {
                case TipoChiusuraIterEnum.RESPINTO:
                    return "Respinto";
                case TipoChiusuraIterEnum.APPROVATO:
                    return "Approvato";
                case TipoChiusuraIterEnum.RITIRATO:
                    return "Ritirato";
                case TipoChiusuraIterEnum.DECADUTO:
                    return "Decaduto";
                case TipoChiusuraIterEnum.DECADENZA_PER_FINE_LEGISLATURA:
                    return "Decaduto per fine legislatura";
                case TipoChiusuraIterEnum.DECADENZA_PER_FINE_MANDATO_CONSIGLIERE:
                    return "Decaduto per fine mandato consigliere";
                case TipoChiusuraIterEnum.INAMMISSIBILE:
                    return "Inammissibile";
                case TipoChiusuraIterEnum.COMUNICAZIONE_ASSEMBLEA:
                    return "Comunicazione all'assemblea";
                case TipoChiusuraIterEnum.TRATTAZIONE_ASSEMBLEA:
                    return "Trattazione in assemblea";
                case TipoChiusuraIterEnum.CHIUSURA_PER_MOTIVI_DIVERSI:
                    return "Chiusura per motivi diversi";
                default:
                {
                    if (public_api)
                        return string.Empty;
                    
                    return "--";
                }
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
                    return string.Empty;
            }
        }

        public static string GetText_TipoVotazioneDASI(int? tipoVotazioneIter)
        {
            try
            {
                if (!tipoVotazioneIter.HasValue) return "";

                switch ((TipoVotazioneIterEnum)tipoVotazioneIter)
                {
                    case TipoVotazioneIterEnum.NESSUNO:
                        return string.Empty;
                    case TipoVotazioneIterEnum.APPELLO_NOMINALE:
                        return "Appello nominale";
                    case TipoVotazioneIterEnum.PALESE_ALZATA_DI_MANO:
                        return "Palese per alzata di mano"; // #1408
                    case TipoVotazioneIterEnum.SCRUTINIO_SEGRETO:
                        return "Scrutinio segreto";
                    default:
                        return string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string GetText_RisultatoVotazioneDASI(int? risultatoVotazioneIter)
        {
            try
            {
                if (!risultatoVotazioneIter.HasValue) return "";

                switch ((RisultatoVotazioneIterEnum)risultatoVotazioneIter)
                {
                    case RisultatoVotazioneIterEnum.NESSUNO:
                        return string.Empty;
                    case RisultatoVotazioneIterEnum.MAGGIORNAZA:
                        return "A maggioranza";
                    case RisultatoVotazioneIterEnum.UNANIMITÀ:
                        return "All'unanimità";
                    default:
                        return string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
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

        public static string GetText_TipoNotaDASI(int tipoNota)
        {
            try
            {
                switch ((TipoNotaEnum)tipoNota)
                {
                    case TipoNotaEnum.GENERALE_PRIVATA:
                        return "Privata";
                    case TipoNotaEnum.GENERALE_PUBBLICA:
                        return "Pubblica";
                    case TipoNotaEnum.CHIUSURA_ITER:
                        return "Chiusura iter";
                    case TipoNotaEnum.RISPOSTA:
                        return "Risposta";
                    case TipoNotaEnum.PRIVACY:
                        return "Privacy";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(tipoNota),
                            tipoNota, null);
                }
            }
            catch (Exception)
            {
                return " ";
            }
        }

        public static string GetNomeDocumentoStandard(int tipoDocumento)
        {
            switch ((TipoDocumentoEnum)tipoDocumento)
            {
                case TipoDocumentoEnum.TESTO_ALLEGATO:
                    return "Allegato parte integrante atto";
                case TipoDocumentoEnum.AGGIUNTIVO:
                    return "Documento aggiuntivo";
                case TipoDocumentoEnum.MONITORAGGIO:
                    return "Documento monitoraggio";
                case TipoDocumentoEnum.ABBINAMENTO:
                    return "Documento abbinamento";
                case TipoDocumentoEnum.CHIUSURA_ITER:
                    return "Testo approvato";
                case TipoDocumentoEnum.RISPOSTA:
                case TipoDocumentoEnum.TESTO_RISPOSTA:
                    return "Testo risposta";
                case TipoDocumentoEnum.TESTO_PRIVACY:
                    return "Documento privacy";
                case TipoDocumentoEnum.VERBALE_VOTAZIONE:
                    return "Verbale votazione";
                case TipoDocumentoEnum.VERBALE_VOTAZIONE_SEGRETA:
                    return "Verbale votazione segreta";
                default:
                    throw new ArgumentOutOfRangeException($"Tipo documento non riconosciuto: {tipoDocumento}");
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

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty).Replace("&nbsp;", " ");
        }

        public static string StripWordMarkup(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Rimuove i commenti condizionali di Word (<!--[if gte mso 9]><xml>...</xml><![endif]-->)
            var withoutComments = Regex.Replace(input, @"<!--\[if.*?\]\>.*?<!\[endif\]-->", string.Empty,
                RegexOptions.Singleline);

            // Rimuove tutti i tag XML/HTML (inclusi quelli di Word come <w:p>, <w:r>, <o:OfficeDocumentSettings>)
            var withoutTags = Regex.Replace(withoutComments, "<[^>]+>", string.Empty);

            // Rimuove anche eventuali tag incompleti o parole chiave XML che iniziano con "<w:" o "<o:"
            var cleaned = Regex.Replace(withoutTags, @"<\w+:[^\s>]+.*?", string.Empty);

            // Decodifica le entità HTML (&nbsp;, &amp;, etc.)
            var decodedText = HttpUtility.HtmlDecode(cleaned);

            // Rimuove eventuali caratteri di controllo Unicode invisibili
            var finalText = Regex.Replace(decodedText, @"[\u200B-\u200D\uFEFF]", string.Empty);

            // Normalizza gli spazi multipli e rimuove spazi iniziali/finali
            return Regex.Replace(finalText, @"\s+", " ").Trim();
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

        public static string CleanFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            // 1. Rimuove caratteri non validi per nome file (secondo Windows)
            //    Usa la lista dei caratteri invalidi per il filesystem
            var invalidChars = Path.GetInvalidFileNameChars();

            var sb = new StringBuilder();

            foreach (var ch in fileName)
            {
                if (Array.IndexOf(invalidChars, ch) < 0)
                {
                    // Permettiamo solo caratteri ASCII stampabili e alcuni altri, altrimenti sostituiamo con _
                    if (ch >= 32 && ch <= 126)
                    {
                        sb.Append(ch);
                    }
                    else
                    {
                        // sostituisci caratteri non ASCII o di controllo con underscore
                        sb.Append('_');
                    }
                }
                else
                {
                    sb.Append('_');
                }
            }

            var cleaned = sb.ToString();

            // 2. Ulteriore pulizia: rimuovi spazi all'inizio e alla fine e caratteri problematici nei link, come &, %, ?, #
            cleaned = cleaned.Trim();

            // 3. Rimuovo o sostituisco caratteri potenzialmente problematici per URL o sistemi vari
            //    (opzionale: se vuoi solo ASCII lettere, numeri, trattini, underscore e punto)
            cleaned = Regex.Replace(cleaned, @"[^a-zA-Z0-9\-\._]", "_");

            // 4. Eventualmente limita la lunghezza a 100 caratteri (per sicurezza)
            if (cleaned.Length > 100)
            {
                cleaned = cleaned.Substring(0, 100);
            }

            // 5. Evita nomi riservati come CON, PRN, AUX, NUL, ecc. (Windows)
            var reservedNames = new[]
            {
                "CON", "PRN", "AUX", "NUL",
                "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            };

            foreach (var reserved in reservedNames)
            {
                if (string.Equals(cleaned, reserved, StringComparison.OrdinalIgnoreCase))
                {
                    cleaned = "_" + cleaned + "_";
                    break;
                }
            }

            return cleaned;
        }

        public static string GetDisplayName(Type objectType, string propertyName)
        {
            var prop = objectType.GetProperty(propertyName);
            if (prop != null)
            {
                var displayNameAttribute = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                    .FirstOrDefault() as DisplayNameAttribute;

                return displayNameAttribute?.DisplayName ?? propertyName;
            }

            return string.Empty;
        }

        public static List<List<T>> Split<T>(IList<T> source, int slice = 100)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / slice)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public static string ConvertiCaratteriSpeciali(string input)
        {
            var sb = new StringBuilder(input);

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

        public static HttpResponseMessage ComposeFileResponse(string path)
        {
            var stream = new MemoryStream();
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                fileStream.CopyTo(stream);
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
            // Determina il Content-Type specifico
            var extension = Path.GetExtension(path)?.ToLowerInvariant();
            var contentType = GetContentTypeFromExtension(extension);
            
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            result.Content.Headers.Add("X-Content-Type-Options", "nosniff");

            return result;
        }
        
        /// <summary>
        /// Determina il Content-Type specifico basandosi sull'estensione del file
        /// ACT36: Fornire Content-Type specifico e non generico
        /// </summary>
        /// <param name="extension">Estensione file (con o senza punto)</param>
        /// <returns>Content-Type MIME specifico</returns>
        public static string GetContentTypeFromExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return "application/octet-stream"; // Fallback generico

            // Rimuovi il punto se presente
            extension = extension.TrimStart('.');

            // Mappa delle estensioni comuni con MIME type specifici
            var contentTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // PDF
                { "pdf", "application/pdf" },

                // Documenti Office
                { "doc", "application/msword" },
                { "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { "xls", "application/vnd.ms-excel" },
                { "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { "ppt", "application/vnd.ms-powerpoint" },
                { "pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },

                // Immagini
                { "jpg", "image/jpeg" },
                { "jpeg", "image/jpeg" },
                { "png", "image/png" },
                { "gif", "image/gif" },
                { "bmp", "image/bmp" },
                { "svg", "image/svg+xml" },

                // Testo
                { "txt", "text/plain" },
                { "csv", "text/csv" },
                { "xml", "application/xml" },
                { "json", "application/json" },

                // Archivi (per export, non per upload)
                { "zip", "application/zip" },
                { "rar", "application/x-rar-compressed" },
                { "7z", "application/x-7z-compressed" }
            };

            // Cerca il MIME type corrispondente
            if (contentTypeMap.TryGetValue(extension, out var contentType))
            {
                return contentType;
            }

            // Fallback: application/octet-stream per estensioni sconosciute
            return "application/octet-stream";
        }

        public static string FormatDateToISO(string dateStr)
        {
            var parts = dateStr.Split('/');
            return $"{parts[2]}-{parts[1].PadLeft(2, '0')}-{parts[0].PadLeft(2, '0')}";
        }

        public static bool IsDateProperty(string propertyName)
        {
            return propertyName == nameof(AttoDASIDto.Timestamp)
                   || propertyName == nameof(AttoDASIDto.DataAnnunzio)
                   || propertyName == nameof(AttoDASIDto.DataComunicazioneAssemblea)
                   || propertyName == nameof(AttoDASIDto.DataTrasmissione)
                   || propertyName == nameof(AttoDASIDto.DataTrattazione)
                   || propertyName == nameof(AttoDASIDto.DataRisposta)
                   || propertyName == nameof(AttoDASIDto.DataChiusuraIter)
                   || propertyName == nameof(AttoDASIDto.DataIscrizioneSeduta)
                   || propertyName == nameof(AttoDASIDto.UIDSeduta);
        }

        public static List<FilterStatement<AttoDASIDto>> ParseFilterDasi(List<FilterItem> clientFilters)
        {
            var result = new List<FilterStatement<AttoDASIDto>>();
            if (clientFilters == null)
                return result;

            if (clientFilters.Any(f => f.property.Equals(nameof(AttoDASIDto.id_gruppo_firmatari))))
            {
                var gruppi_firmatari =
                    clientFilters.First(f => f.property.Equals(nameof(AttoDASIDto.id_gruppo_firmatari)));
                if (string.IsNullOrEmpty(gruppi_firmatari.value))
                {
                    clientFilters.Remove(gruppi_firmatari);
                }
            }

            foreach (var filterItem in clientFilters)
            {
                if (filterItem.not_empty)
                {
                    if (filterItem.property.Equals(nameof(AttoDASIDto.DCR)))
                    {
                        result.Add(new FilterStatement<AttoDASIDto>
                        {
                            PropertyId = nameof(AttoDASIDto.DCRL),
                            Operation = Operation.IsNotEmpty,
                            Connector = FilterStatementConnector.And
                        });
                        continue;
                    }

                    result.Add(new FilterStatement<AttoDASIDto>
                    {
                        PropertyId = filterItem.property,
                        Operation = Operation.IsNotNullNorWhiteSpace,
                        Connector = FilterStatementConnector.And
                    });

                    continue;
                }

                if (string.IsNullOrEmpty(filterItem.value))
                {
                    if (IsDateProperty(filterItem.property)
                        || filterItem.property.Equals(nameof(AttoDASIDto.TipoVotazioneIter))
                        || filterItem.property.Equals(nameof(AttoDASIDto.TipoChiusuraIter))
                        || filterItem.property.Equals(nameof(AttoDASIDto.Risposte))
                        || filterItem.property.Equals(nameof(AttoDASIDto.Organi)))
                    {
                        result.Add(new FilterStatement<AttoDASIDto>
                        {
                            PropertyId = filterItem.property,
                            Operation = Operation.IsNull,
                            Connector = FilterStatementConnector.And
                        });
                    }
                    else
                    {
                        if (filterItem.property.Equals(nameof(AttoDASIDto.DCR)))
                        {
                            result.Add(new FilterStatement<AttoDASIDto>
                            {
                                PropertyId = filterItem.property,
                                Operation = Operation.EqualTo,
                                Value = 0,
                                Connector = FilterStatementConnector.And
                            });

                            result.Add(new FilterStatement<AttoDASIDto>
                            {
                                PropertyId = nameof(AttoDASIDto.DCCR),
                                Operation = Operation.EqualTo,
                                Value = 0,
                                Connector = FilterStatementConnector.And
                            });
                        }
                        else
                        {
                            // #1330
                            result.Add(new FilterStatement<AttoDASIDto>
                            {
                                PropertyId = filterItem.property,
                                Operation = Operation.IsNullOrWhiteSpace,
                                Connector = FilterStatementConnector.And
                            });
                        }
                    }

                    continue;
                }

                var values = filterItem.value.Split(',');
                if (values.Length > 1
                    && !filterItem.property.Equals(nameof(AttoDASIDto.NAtto))
                    && !filterItem.property.Equals(nameof(AttoDASIDto.DCR)))
                {
                    if (IsDateProperty(filterItem.property))
                    {
                        result.Add(new FilterStatement<AttoDASIDto>
                        {
                            PropertyId = filterItem.property,
                            Operation = Operation.GreaterThan,
                            Value = DateTime.Parse(values[0].Trim()).ToString("yyyy-MM-dd") + " 00:00:00",
                            Connector = FilterStatementConnector.And
                        });

                        result.Add(new FilterStatement<AttoDASIDto>
                        {
                            PropertyId = filterItem.property,
                            Operation = Operation.LessThan,
                            Value = DateTime.Parse(values[1].Trim()).ToString("yyyy-MM-dd") + " 23:59:59",
                            Connector = FilterStatementConnector.And
                        });
                    }
                    else if (filterItem.property.Equals(nameof(AttoDASIDto.DCR)))
                    {
                        if (int.Parse(values[0].Trim()) > 0)
                        {
                            result.Add(new FilterStatement<AttoDASIDto>
                            {
                                PropertyId = filterItem.property,
                                Operation = Operation.EqualTo,
                                Value = int.Parse(values[0].Trim()),
                                Connector = FilterStatementConnector.And
                            });
                        }

                        if (int.Parse(values[1].Trim()) > 0)
                        {
                            result.Add(new FilterStatement<AttoDASIDto>
                            {
                                PropertyId = nameof(AttoDASIDto.DCCR),
                                Operation = Operation.EqualTo,
                                Value = int.Parse(values[1].Trim()),
                                Connector = FilterStatementConnector.And
                            });
                        }
                    }
                    else
                    {
                        var orStatements = values.Select(value => new FilterStatement<AttoDASIDto>
                        {
                            PropertyId = filterItem.property,
                            Operation = Operation.EqualTo,
                            Value = value.Trim(),
                            Connector = FilterStatementConnector.Or
                        }).ToList();

                        // Add the first statement to the main list
                        result.Add(orStatements.First());

                        // Link the remaining statements with 'Or' connectors
                        for (var i = 1; i < orStatements.Count; i++)
                        {
                            orStatements[i].Connector = FilterStatementConnector.Or;
                            result.Add(orStatements[i]);
                        }
                    }
                }
                else
                {
                    if (IsDateProperty(filterItem.property)
                        && !Guid.TryParse(filterItem.value, out var resGuid))
                    {
                        result.Add(new FilterStatement<AttoDASIDto>
                        {
                            PropertyId = filterItem.property,
                            Operation = Operation.GreaterThan,
                            Value = DateTime.Parse(filterItem.value.Trim()).ToString("yyyy-MM-dd") + " 00:00:00",
                            Connector = FilterStatementConnector.And
                        });

                        result.Add(new FilterStatement<AttoDASIDto>
                        {
                            PropertyId = filterItem.property,
                            Operation = Operation.LessThan,
                            Value = DateTime.Parse(filterItem.value.Trim()).ToString("yyyy-MM-dd") + " 23:59:59",
                            Connector = FilterStatementConnector.And
                        });
                    }
                    else
                    {
                        if (filterItem.property.Equals(nameof(AttoDASIDto.Protocollo))
                            || filterItem.property.Equals(nameof(AttoDASIDto.CodiceMateria))
                            || filterItem.property.Equals(nameof(AttoDASIDto.BURL)))
                        {
                            result.Add(new FilterStatement<AttoDASIDto>
                            {
                                PropertyId = filterItem.property,
                                Operation = Operation.Contains,
                                Value = filterItem.value,
                                Connector = FilterStatementConnector.And
                            });
                        }
                        else
                        {
                            result.Add(new FilterStatement<AttoDASIDto>
                            {
                                PropertyId = filterItem.property,
                                Operation = Operation.EqualTo,
                                Value = filterItem.value,
                                Connector = FilterStatementConnector.And
                            });
                        }
                    }
                }
            }

            return result;
        }

        public static DateTime ParseDateTime(string dateTime)
        {
            try
            {
                // Definisci una cultura specifica per il parsing (italiano)
                var italianCulture = new CultureInfo("it-IT");
                string[] supportedFormats =
                {
                    "yyyy-MM-dd HH:mm:ss", // Formato ISO senza "T"
                    "yyyy-MM-ddTHH:mm:ss", // Formato ISO con "T"
                    "dd/MM/yyyy HH:mm:ss" // Formato italiano standard
                };

                if (dateTime.Contains("T"))
                {
                    // Parsing con il formato italiano
                    try
                    {
                        var dateT = DateTime.ParseExact(dateTime.Substring(0, 19).Replace("T", " "), supportedFormats,
                            italianCulture, DateTimeStyles.None);
                        return dateT;
                    }
                    catch (Exception e)
                    {
                        throw new Exception(
                            $"Data in errore: {dateTime}, modificata in {dateTime.Substring(0, 19).Replace("T", " ")}",
                            e);
                    }
                }

                // Parsing della stringa con la cultura italiana
                var date = DateTime.ParseExact(dateTime, supportedFormats, italianCulture, DateTimeStyles.None);
                return date;
            }
            catch (Exception e)
            {
                throw new Exception($"Data in errore: {dateTime}", e);
            }
        }

        /// <summary>
        ///     Traduce le chip del pannello filtri Emendamenti in <see cref="FilterStatement{EmendamentiDto}" />.
        ///     Per il testo libero la chip serializza fino a due frammenti separati da '|' col connettore
        ///     intermedio (es. "ciao|And|mondo"); la chip Parte e' composta, vedi <c>AggiungiStatementParteComposta</c>.
        /// </summary>
        public static List<FilterStatement<EmendamentiDto>> ParseFilterEM(List<FilterItem> clientFilters)
        {
            var result = new List<FilterStatement<EmendamentiDto>>();
            if (clientFilters == null) return result;

            foreach (var filterItem in clientFilters)
            {
                if (string.IsNullOrEmpty(filterItem.property)) continue;

                if (filterItem.property == nameof(EmendamentiDto.TestoEM_originale))
                {
                    AggiungiStatementTestoLibero(result, filterItem.value);
                    continue;
                }

                if (filterItem.property == nameof(EmendamentiDto.IDParte))
                {
                    // La chip Parte viene serializzata dal pannello filtri come valore composto
                    // "<idParte>|art:<uid>|com:<uid>|let:<uid>|tit:<n>|capo:<n>|mis:<n>|prog:<n>".
                    // Lo spacchettiamo in piu' FilterStatement separati (uno per IDParte e uno
                    // per ciascun campo di cascata) cosi' il backend puo' applicarli singolarmente.
                    AggiungiStatementParteComposta(result, filterItem.value);
                    continue;
                }

                if (filterItem.not_empty)
                {
                    result.Add(new FilterStatement<EmendamentiDto>
                    {
                        PropertyId = filterItem.property,
                        Operation = Operation.IsNotNullNorWhiteSpace,
                        Connector = FilterStatementConnector.And
                    });
                    continue;
                }

                if (string.IsNullOrEmpty(filterItem.value))
                {
                    result.Add(new FilterStatement<EmendamentiDto>
                    {
                        PropertyId = filterItem.property,
                        Operation = Operation.IsNullOrWhiteSpace,
                        Connector = FilterStatementConnector.And
                    });
                    continue;
                }

                result.Add(new FilterStatement<EmendamentiDto>
                {
                    PropertyId = filterItem.property,
                    Operation = Operation.EqualTo,
                    Value = filterItem.value,
                    Connector = FilterStatementConnector.And
                });
            }

            return result;
        }

        private static void AggiungiStatementParteComposta(List<FilterStatement<EmendamentiDto>> result, string raw)
        {
            if (string.IsNullOrEmpty(raw)) return;

            var segmenti = raw.Split('|');
            // Il primo segmento e' l'IDParte (intero), gli altri sono "<prefisso>:<valore>".
            var parteValue = segmenti[0]?.Trim();
            if (!string.IsNullOrEmpty(parteValue))
            {
                result.Add(new FilterStatement<EmendamentiDto>
                {
                    PropertyId = nameof(EmendamentiDto.IDParte),
                    Operation = Operation.EqualTo,
                    Value = parteValue,
                    Connector = FilterStatementConnector.And
                });
            }

            for (var i = 1; i < segmenti.Length; i++)
            {
                var seg = segmenti[i];
                if (string.IsNullOrEmpty(seg)) continue;
                var sep = seg.IndexOf(':');
                if (sep <= 0) continue;
                var prefisso = seg.Substring(0, sep).Trim().ToLowerInvariant();
                var valore = seg.Substring(sep + 1).Trim();
                if (string.IsNullOrEmpty(valore)) continue;

                string propertyId;
                switch (prefisso)
                {
                    case "art": propertyId = nameof(EmendamentiDto.UIDArticolo); break;
                    case "com": propertyId = nameof(EmendamentiDto.UIDComma); break;
                    case "let": propertyId = nameof(EmendamentiDto.UIDLettera); break;
                    case "tit": propertyId = nameof(EmendamentiDto.NTitolo); break;
                    case "capo": propertyId = nameof(EmendamentiDto.NCapo); break;
                    case "mis": propertyId = nameof(EmendamentiDto.NMissione); break;
                    case "prog": propertyId = nameof(EmendamentiDto.NProgramma); break;
                    default: continue;
                }

                result.Add(new FilterStatement<EmendamentiDto>
                {
                    PropertyId = propertyId,
                    Operation = Operation.EqualTo,
                    Value = valore,
                    Connector = FilterStatementConnector.And
                });
            }
        }

        private static void AggiungiStatementTestoLibero(List<FilterStatement<EmendamentiDto>> result, string raw)
        {
            if (string.IsNullOrEmpty(raw)) return;

            var parts = raw.Split('|');
            var primo = parts[0]?.Trim();
            if (!string.IsNullOrEmpty(primo))
            {
                result.Add(new FilterStatement<EmendamentiDto>
                {
                    PropertyId = nameof(EmendamentiDto.TestoEM_originale),
                    Operation = Operation.Contains,
                    Value = primo,
                    Connector = FilterStatementConnector.And
                });
            }

            if (parts.Length < 3) return;
            var secondo = parts[2]?.Trim();
            if (string.IsNullOrEmpty(secondo)) return;

            var connettoreRaw = parts[1]?.Trim();
            var connettore = FilterStatementConnector.And;
            if (string.Equals(connettoreRaw, "Or", StringComparison.OrdinalIgnoreCase)
                || connettoreRaw == ((int)FilterStatementConnector.Or).ToString())
                connettore = FilterStatementConnector.Or;

            result.Add(new FilterStatement<EmendamentiDto>
            {
                PropertyId = nameof(EmendamentiDto.TestoEM_originale),
                Operation = Operation.Contains,
                Value = secondo,
                Connector = connettore
            });
        }
    }
}