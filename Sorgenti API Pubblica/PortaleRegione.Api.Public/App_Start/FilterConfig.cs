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

using System.Web.Mvc;

namespace PortaleRegione.Api.Public
{
    /// <summary>
    ///     Configura i filtri globali dell'applicazione. I filtri globali sono applicati a tutte le richieste
    ///     gestite dall'applicazione, permettendo di implementare logica comune come la gestione delle eccezioni
    ///     o la sicurezza a livello applicativo.
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        ///     Registra i filtri globali da applicare a tutte le azioni in tutti i controller.
        /// </summary>
        /// <param name="filters">La collezione di filtri globali a cui aggiungere i filtri.</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Aggiunge un filtro per la gestione centralizzata delle eccezioni non gestite.
            // Questo permette di restituire risposte coerenti in caso di errore, mantenendo separata
            // la logica di gestione degli errori dalla logica di business dei controller.
            filters.Add(new HandleErrorAttribute());
        }
    }
}