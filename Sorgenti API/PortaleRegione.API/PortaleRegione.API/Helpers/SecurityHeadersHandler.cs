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

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PortaleRegione.API.Helpers
{
   /// <summary>
    /// Handler per aggiungere header di sicurezza a tutte le response
    /// ACT44: Content Security Policy e altri security headers
    /// </summary>
    public class SecurityHeadersHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            // Esegui la richiesta
            var response = await base.SendAsync(request, cancellationToken);

            // Aggiungi header di sicurezza
            AddSecurityHeaders(response);

            return response;
        }

        private void AddSecurityHeaders(HttpResponseMessage response)
        {
            // Content Security Policy
            if (!response.Headers.Contains("Content-Security-Policy"))
            {
                response.Headers.Add("Content-Security-Policy",
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdnjs.cloudflare.com https://code.jquery.com https://cdn.jsdelivr.net; " +
                    "style-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com https://fonts.googleapis.com; " +
                    "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
                    "img-src 'self' data: https:; " +
                    "connect-src 'self'; " +
                    "frame-ancestors 'none'; " +
                    "base-uri 'self'; " +
                    "form-action 'self';"
                );
            }

            // X-Content-Type-Options (già implementato in ACT36 ma aggiungiamo comunque)
            if (!response.Headers.Contains("X-Content-Type-Options"))
            {
                response.Headers.Add("X-Content-Type-Options", "nosniff");
            }

            // X-Frame-Options (protezione clickjacking)
            if (!response.Headers.Contains("X-Frame-Options"))
            {
                response.Headers.Add("X-Frame-Options", "DENY");
            }

            // X-XSS-Protection (legacy ma utile per browser vecchi)
            if (!response.Headers.Contains("X-XSS-Protection"))
            {
                response.Headers.Add("X-XSS-Protection", "1; mode=block");
            }

            // Referrer-Policy
            if (!response.Headers.Contains("Referrer-Policy"))
            {
                response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
            }

            // Permissions-Policy (Feature-Policy deprecato)
            if (!response.Headers.Contains("Permissions-Policy"))
            {
                response.Headers.Add("Permissions-Policy",
                    "geolocation=(), microphone=(), camera=()");
            }
        }
    }
}