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

namespace PortaleRegione.Api.Public
{
    /// <summary>
    ///  Definisce le rotte per le chiamate API pubbliche al portale della regione.
    /// </summary>
    public static class ApiRoutes
    {
        private const string Root = "api";

        /// <summary>
        /// Rotta per testare la connettività o la configurazione base dell'API.
        /// </summary>
        public const string Test = Root + "/test";

        /// <summary>
        /// Rotta per ottenere l'elenco delle legislature correnti e passate.
        /// </summary>
        public const string GetLegislature = Root + "/legislature";

        /// <summary>
        /// Rotta per ottenere i tipi di documenti o entità gestiti dall'API.
        /// </summary>
        public const string GetTipi = Root + "/tipi";

        /// <summary>
        /// Rotta per ottenere i tipi di risposte possibili per certe richieste o moduli.
        /// </summary>
        public const string GetTipiRisposte = Root + "/tipi-risposte";

        /// <summary>
        /// Rotta per ottenere gli stati possibili di un documento o di una procedura.
        /// </summary>
        public const string GetStati = Root + "/stati";

        /// <summary>
        /// Rotta per ottenere l'elenco dei gruppi parlamentari.
        /// </summary>
        public const string GetGruppi = Root + "/gruppi";

        /// <summary>
        /// Rotta per ottenere l'elenco dei firmatari di un atto o di un documento.
        /// </summary>
        public const string GetFirmatari = Root + "/firmatari";

        /// <summary>
        /// Rotta per ottenere l'elenco delle cariche presenti all'interno della Giunta.
        /// </summary>
        public const string GetCaricheGiunta = Root + "/cariche";

        /// <summary>
        /// Rotta per ottenere l'elenco delle commissioni parlamentari.
        /// </summary>
        public const string GetCommissioni = Root + "/commissioni";

        /// <summary>
        /// Rotta per ottenere i dettagli di un atto specifico mediante il suo identificativo.
        /// </summary>
        public const string GetAtto = Root + "/atto";

        /// <summary>
        /// Rotta per effettuare ricerche complesse all'interno del portale mediante parametri specifici.
        /// </summary>
        public const string GetSearch = Root + "/cerca";
    }
}
