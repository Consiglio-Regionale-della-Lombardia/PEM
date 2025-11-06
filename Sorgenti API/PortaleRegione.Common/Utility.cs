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
                case AreaPoliticaIntEnum.Misto:
                    return AreaPoliticaEnum.Misto;
                case AreaPoliticaIntEnum.Misto_Maggioranza_Minoranza:
                    return AreaPoliticaEnum.Misto_Maggioranza_Minoranza;
                case AreaPoliticaIntEnum.Nessuno: // #1300
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

        public static string GetText_ChiusuraIterDASI(int? stato)
        {
            if (stato == null)
                return "--";

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
                    return "--";
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

        /// <summary>
        ///     Aggiunge il filtro alla request di ricerca degli emendamenti
        /// </summary>
        /// <param name="model"></param>
        /// <param name="numero_em"></param>
        public static void AddFilter_ByNUM(ref BaseRequest<EmendamentiDto> model, string numero_em, string subem)
        {
            if (subem != "on")
            {
                // cerca emendamenti
                if (!string.IsNullOrEmpty(numero_em))
                {
                    model.filtro.Add(new FilterStatement<EmendamentiDto>
                    {
                        PropertyId = nameof(EmendamentiDto.N_EM),
                        Operation = Operation.EqualTo,
                        Value = numero_em,
                        Connector = FilterStatementConnector.And
                    });
                }
            }
            else
            {
                // cerca subemendamenti
                if (!string.IsNullOrEmpty(numero_em))
                {
                    model.filtro.Add(new FilterStatement<EmendamentiDto>
                    {
                        PropertyId = nameof(EmendamentiDto.N_SUBEM),
                        Operation = Operation.EqualTo,
                        Value = numero_em,
                        Connector = FilterStatementConnector.And
                    });
                }
            }
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
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            return result;
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
    }
}