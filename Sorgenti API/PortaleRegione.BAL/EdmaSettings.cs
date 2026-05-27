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
using System.Collections.Specialized;
using System.Configuration;

namespace PortaleRegione.BAL
{
    /// <summary>
    ///     Accesso alle chiavi EDMA_* della sezione custom &lt;edmaSettings&gt;
    ///     dichiarata nel Web.config e popolata via configSource="Edma.config".
    ///     La sezione e' gestita da NameValueSectionHandler, quindi restituisce
    ///     una NameValueCollection (stessa semantica di AppSettings).
    /// </summary>
    /// <remarks>
    ///     Se la sezione non esiste (es. Edma.config mancante o non ancora
    ///     copiato dal template) la collection e' vuota e i Get tornano null:
    ///     i chiamanti devono gestire i valori mancanti senza crashare il
    ///     processo (non vogliamo bloccare l'avvio di GEDASI se l'integrazione
    ///     EDMA non e' configurata).
    /// </remarks>
    internal static class EdmaSettings
    {
        private static readonly Lazy<NameValueCollection> _section = new Lazy<NameValueCollection>(LoadSection);

        private static NameValueCollection LoadSection()
        {
            try
            {
                return ConfigurationManager.GetSection("edmaSettings") as NameValueCollection
                       ?? new NameValueCollection();
            }
            catch (ConfigurationErrorsException)
            {
                // Se Edma.config e' assente o malformato non vogliamo che l'errore
                // si propaghi in fase di lettura di altre chiavi: l'integrazione
                // EDMA risultera' semplicemente disattivata.
                return new NameValueCollection();
            }
        }

        public static string Get(string key)
        {
            return _section.Value[key];
        }

        public static string GetOrDefault(string key, string defaultValue)
        {
            var v = _section.Value[key];
            return string.IsNullOrEmpty(v) ? defaultValue : v;
        }

        public static int GetInt(string key, int defaultValue)
        {
            var v = _section.Value[key];
            return int.TryParse(v, out var parsed) ? parsed : defaultValue;
        }

        public static bool GetBool(string key, bool defaultValue)
        {
            var v = _section.Value[key];
            return bool.TryParse(v, out var parsed) ? parsed : defaultValue;
        }
    }
}
