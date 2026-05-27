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

namespace PortaleRegione.SDK.EDMA.Models.Response
{
    /// <summary>
    ///     Esito della creazione di un FascicoloPratica. Servono l'id
    ///     interno EDMA (per le chiamate successive di associazione documenti
    ///     e per memorizzarlo lato GEDASI), il codice/numero pratica da
    ///     mostrare alla segreteria e l'identificatore univoco.
    /// </summary>
    public class FascicoloPraticaOutput
    {
        public string IdPratica { get; set; }
        public string NumeroPratica { get; set; }
        public string Identificatore { get; set; }
    }
}
