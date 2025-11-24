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
using System.IO;
using System.Linq;

namespace PortaleRegione.Client.Helpers
{
    public static class FileValidator
    {
        private const int MAX_FILE_SIZE = 50 * 1024 * 1024; // 50 MB

        private static readonly HashSet<string> BlacklistedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".dll", ".bat", ".cmd", ".com", ".pif", ".scr", ".vbs", ".js", ".jse",
            ".ws", ".wsf", ".wsc", ".wsh", ".ps1", ".ps1xml", ".ps2", ".ps2xml", ".psc1",
            ".psc2", ".msh", ".msh1", ".msh2", ".mshxml", ".msh1xml", ".msh2xml",
            ".scf", ".lnk", ".inf", ".reg", ".app", ".application", ".gadget", ".msi",
            ".msp", ".mst", ".cpl", ".msc", ".jar", ".zip", ".rar", ".7z", ".tar", ".gz",
            ".bz2", ".xz", ".iso", ".img", ".dmg", ".pkg", ".deb", ".rpm"
        };

        private static readonly Dictionary<string, string> AllowedMimeTypes = new Dictionary<string, string>
        {
            { ".pdf", "application/pdf" }
        };

        public static ValidationResult ValidateFile(string fileName, string contentType, byte[] fileContent)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(fileName))
            {
                result.IsValid = false;
                result.ErrorMessage = "Nome file non valido.";
                return result;
            }

            if (fileContent.Length > MAX_FILE_SIZE)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Il file supera la dimensione massima consentita di {MAX_FILE_SIZE / (1024 * 1024)} MB.";
                return result;
            }

            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = "Il file non ha un'estensione valida.";
                return result;
            }

            if (BlacklistedExtensions.Contains(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = $"Il tipo di file '{extension}' non è consentito per motivi di sicurezza.";
                return result;
            }

            if (!AllowedMimeTypes.ContainsKey(extension.ToLower()))
            {
                result.IsValid = false;
                result.ErrorMessage = $"Il tipo di file '{extension}' non è consentito. Solo file PDF sono accettati.";
                return result;
            }

            var expectedMimeType = AllowedMimeTypes[extension.ToLower()];
            if (!contentType.Equals(expectedMimeType, StringComparison.OrdinalIgnoreCase))
            {
                result.IsValid = false;
                result.ErrorMessage = $"Il tipo MIME del file ('{contentType}') non corrisponde al tipo atteso ('{expectedMimeType}').";
            }
            
            return result;
        }

        public static bool IsExtensionBlacklisted(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return !string.IsNullOrWhiteSpace(extension) && BlacklistedExtensions.Contains(extension);
        }

        public static bool IsZipFile(string fileName, byte[] fileContent)
        {
            var extension = Path.GetExtension(fileName);
            
            if (extension != null && (
                extension.Equals(".zip", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".rar", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".7z", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".tar", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".gz", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (fileContent == null || fileContent.Length < 4)
                return false;

            var zipSignatures = new List<byte[]>
            {
                new byte[] { 0x50, 0x4B, 0x03, 0x04 }, // ZIP
                new byte[] { 0x50, 0x4B, 0x05, 0x06 }, // ZIP empty
                new byte[] { 0x50, 0x4B, 0x07, 0x08 }, // ZIP spanned
                new byte[] { 0x52, 0x61, 0x72, 0x21 }, // RAR
                new byte[] { 0x37, 0x7A, 0xBC, 0xAF }, // 7Z
                new byte[] { 0x1F, 0x8B }, // GZIP
            };

            return zipSignatures.Any(signature =>
                fileContent.Take(signature.Length).SequenceEqual(signature));
        }

        public static string GetSafeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return $"file_{Guid.NewGuid()}.pdf";

            var invalidChars = Path.GetInvalidFileNameChars();
            var safeName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            safeName = safeName.Replace("..", "_");
            safeName = safeName.Replace("/", "_");
            safeName = safeName.Replace("\\", "_");

            if (safeName.Length > 200)
                safeName = safeName.Substring(0, 200);

            return safeName;
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }
}