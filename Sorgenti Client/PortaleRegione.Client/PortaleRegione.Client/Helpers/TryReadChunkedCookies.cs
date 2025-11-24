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
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;

namespace PortaleRegione.Client.Helpers
{
    public static class TryReadChunkedCookies
    {
        public static string CompressToBase64(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Compress))
                {
                    gzip.Write(bytes, 0, bytes.Length);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        private static string DecompressFromBase64(string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            using (var ms = new MemoryStream(bytes))
            using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
            using (var outStream = new MemoryStream())
            {
                gzip.CopyTo(outStream);
                return Encoding.UTF8.GetString(outStream.ToArray());
            }
        }
        
        public static T GetJson<T>(HttpRequest request, string cookieBaseName, int parts = 3)
        {
            var sb = new StringBuilder();

            for (int i = 1; i <= parts; i++)
            {
                var c = request.Cookies[cookieBaseName + i];
                if (c == null) return default;

                var t = FormsAuthentication.Decrypt(c.Value);
                if (t == null) return default;

                sb.Append(t.UserData);
            }
            
            try
            {
                return JsonConvert.DeserializeObject<T>(DecompressFromBase64(sb.ToString()));
            }
            catch
            {
                return default;
            }
        } 
        
        public static T GetJson<T>(HttpRequestBase request, string cookieBaseName, int parts = 3)
        {
            var sb = new StringBuilder();

            for (int i = 1; i <= parts; i++)
            {
                var c = request.Cookies[cookieBaseName + i];
                if (c == null) return default;

                var t = FormsAuthentication.Decrypt(c.Value);
                if (t == null) return default;

                sb.Append(t.UserData);
            }
            
            try
            {
                return JsonConvert.DeserializeObject<T>(DecompressFromBase64(sb.ToString()));
            }
            catch
            {
                return default;
            }
        }  
        
        public static string GetString(HttpRequest request, string cookieBaseName, int parts = 3)
        {
            var sb = new StringBuilder();

            for (int i = 1; i <= parts; i++)
            {
                var c = request.Cookies[cookieBaseName + i];
                if (c == null) return null;

                var t = FormsAuthentication.Decrypt(c.Value);
                if (t == null) return null;

                sb.Append(t.UserData);
            }

            try
            {
                return DecompressFromBase64(sb.ToString());
            }
            catch
            {
                return null;
            }
        } 
        
        public static string GetString(HttpRequestBase request, string cookieBaseName, int parts = 3)
        {
            var sb = new StringBuilder();

            for (int i = 1; i <= parts; i++)
            {
                var c = request.Cookies[cookieBaseName + i];
                if (c == null) return null;

                var t = FormsAuthentication.Decrypt(c.Value);
                if (t == null) return null;

                sb.Append(t.UserData);
            }

            try
            {
                return DecompressFromBase64(sb.ToString());
            }
            catch
            {
                return null;
            }
        } 
    }
}