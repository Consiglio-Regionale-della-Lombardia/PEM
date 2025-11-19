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

using System.Linq;
using System.Web.Mvc;

namespace PortaleRegione.Client.Helpers
{
    /// <summary>
    /// Action Filter per aggiungere security headers alle response MVC
    /// ACT44: Content Security Policy
    /// </summary>
    public class SecurityHeadersAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var response = filterContext.HttpContext.Response;

            // Content Security Policy
            if (!response.Headers.AllKeys.Contains("Content-Security-Policy"))
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

            // Altri security headers
            if (!response.Headers.AllKeys.Contains("X-Content-Type-Options"))
                response.Headers.Add("X-Content-Type-Options", "nosniff");

            if (!response.Headers.AllKeys.Contains("X-Frame-Options"))
                response.Headers.Add("X-Frame-Options", "DENY");

            if (!response.Headers.AllKeys.Contains("X-XSS-Protection"))
                response.Headers.Add("X-XSS-Protection", "1; mode=block");

            if (!response.Headers.AllKeys.Contains("Referrer-Policy"))
                response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

            base.OnResultExecuted(filterContext);
        }
    }
}