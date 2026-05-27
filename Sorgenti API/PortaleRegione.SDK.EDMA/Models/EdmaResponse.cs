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

namespace PortaleRegione.SDK.EDMA.Models
{
    /// <summary>
    ///     Risposta non tipizzata: utile per servizi che restituiscono solo un
    ///     boolean o uno stream e per i quali non serve un parsing strutturato.
    ///     I metodi piu' importanti del servizio tornano <see cref="EdmaResponse{T}"/>.
    /// </summary>
    public class EdmaResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string RawResponse { get; set; }
    }

    /// <summary>
    ///     Risposta tipizzata: <see cref="Data"/> contiene il payload parsato
    ///     dal body XML restituito da EDMA quando <see cref="Success"/> e' vero.
    ///     <see cref="RawResponse"/> e' sempre presente per facilitare il
    ///     debug e i log applicativi.
    /// </summary>
    public class EdmaResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string RawResponse { get; set; }
        public T Data { get; set; }

        public static EdmaResponse<T> Ok(T data, string raw)
        {
            return new EdmaResponse<T>
            {
                Success = true,
                Data = data,
                RawResponse = raw
            };
        }

        public static EdmaResponse<T> Fail(string message, string raw = null)
        {
            return new EdmaResponse<T>
            {
                Success = false,
                Message = message,
                RawResponse = raw
            };
        }
    }
}
