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

using Ganss.Xss;
using System;
using System.Collections.Generic;

namespace PortaleRegione.BAL.Helpers
{
    public static class InputSanitizer
    {
        private static readonly HtmlSanitizer _sanitizer;

        static InputSanitizer()
        {
            _sanitizer = new HtmlSanitizer();
            ConfigureSanitizer();
        }

        private static void ConfigureSanitizer()
        {
            _sanitizer.AllowedTags.Clear();
            
            _sanitizer.AllowedTags.Add("p");
            _sanitizer.AllowedTags.Add("br");
            _sanitizer.AllowedTags.Add("strong");
            _sanitizer.AllowedTags.Add("b");
            _sanitizer.AllowedTags.Add("em");
            _sanitizer.AllowedTags.Add("i");
            _sanitizer.AllowedTags.Add("u");
            _sanitizer.AllowedTags.Add("del");
            _sanitizer.AllowedTags.Add("s");
            _sanitizer.AllowedTags.Add("ul");
            _sanitizer.AllowedTags.Add("ol");
            _sanitizer.AllowedTags.Add("li");
            _sanitizer.AllowedTags.Add("h1");
            _sanitizer.AllowedTags.Add("h2");
            _sanitizer.AllowedTags.Add("h3");
            _sanitizer.AllowedTags.Add("h4");
            _sanitizer.AllowedTags.Add("h5");
            _sanitizer.AllowedTags.Add("h6");
            _sanitizer.AllowedTags.Add("table");
            _sanitizer.AllowedTags.Add("thead");
            _sanitizer.AllowedTags.Add("tbody");
            _sanitizer.AllowedTags.Add("tr");
            _sanitizer.AllowedTags.Add("th");
            _sanitizer.AllowedTags.Add("td");
            _sanitizer.AllowedTags.Add("a");
            _sanitizer.AllowedTags.Add("blockquote");
            _sanitizer.AllowedTags.Add("pre");
            _sanitizer.AllowedTags.Add("code");
            _sanitizer.AllowedTags.Add("div");
            _sanitizer.AllowedTags.Add("span");
            _sanitizer.AllowedTags.Add("hr");
            _sanitizer.AllowedTags.Add("sup");
            _sanitizer.AllowedTags.Add("sub");

            _sanitizer.AllowedAttributes.Clear();
            
            _sanitizer.AllowedAttributes.Add("href");
            _sanitizer.AllowedAttributes.Add("title");
            _sanitizer.AllowedAttributes.Add("class");
            _sanitizer.AllowedAttributes.Add("style");
            _sanitizer.AllowedAttributes.Add("colspan");
            _sanitizer.AllowedAttributes.Add("rowspan");
            _sanitizer.AllowedAttributes.Add("align");
            _sanitizer.AllowedAttributes.Add("target");

            _sanitizer.AllowedCssProperties.Clear();
            _sanitizer.AllowedCssProperties.Add("text-align");
            _sanitizer.AllowedCssProperties.Add("font-weight");
            _sanitizer.AllowedCssProperties.Add("font-style");
            _sanitizer.AllowedCssProperties.Add("text-decoration");
            _sanitizer.AllowedCssProperties.Add("color");
            _sanitizer.AllowedCssProperties.Add("background-color");
            _sanitizer.AllowedCssProperties.Add("font-family");
            _sanitizer.AllowedCssProperties.Add("font-size");
            _sanitizer.AllowedCssProperties.Add("padding");
            _sanitizer.AllowedCssProperties.Add("margin");
            _sanitizer.AllowedCssProperties.Add("line-height");

            _sanitizer.AllowedSchemes.Clear();
            _sanitizer.AllowedSchemes.Add("http");
            _sanitizer.AllowedSchemes.Add("https");
            _sanitizer.AllowedSchemes.Add("mailto");

            _sanitizer.AllowDataAttributes = false;
        }

        public static string SanitizeHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            try
            {
                return _sanitizer.Sanitize(html);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Errore durante la sanitizzazione dell'HTML", ex);
            }
        }

        public static string SanitizeText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&#x27;")
                       .Replace("/", "&#x2F;");

            return text;
        }

        public static bool ContainsDangerousContent(string html)
        {
            if (string.IsNullOrEmpty(html))
                return false;

            var dangerousPatterns = new List<string>
            {
                "<script",
                "javascript:",
                "onerror=",
                "onload=",
                "onclick=",
                "onmouseover=",
                "onfocus=",
                "onblur=",
                "onchange=",
                "onsubmit=",
                "<iframe",
                "<object",
                "<embed",
                "vbscript:",
                "data:text/html"
            };

            var lowerHtml = html.ToLower();
            foreach (var pattern in dangerousPatterns)
            {
                if (lowerHtml.Contains(pattern.ToLower()))
                    return true;
            }

            return false;
        }

        public static void ValidateAndThrowIfDangerous(string html, string fieldName = "campo")
        {
            if (ContainsDangerousContent(html))
            {
                throw new InvalidOperationException(
                    $"Il {fieldName} contiene contenuto potenzialmente pericoloso che non è permesso.");
            }
        }
    }
}