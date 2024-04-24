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
using System.Security.Cryptography;
using System.Text;

namespace PortaleRegione.Crypto
{
    /// <summary>
    ///     Fornisce metodi di utilità per la crittografia e decrittografia delle stringhe.
    ///     Utilizza l'algoritmo TripleDES per la crittografia e MD5 per la generazione della chiave.
    /// </summary>
    public static class CryptoHelper
    {
        /// <summary>
        ///     Decripta una stringa cifrata utilizzando una chiave specifica.
        /// </summary>
        /// <param name="EncryptedString">La stringa cifrata in formato Base64 da decriptare.</param>
        /// <param name="Key">La chiave utilizzata per cifrare la stringa.</param>
        /// <returns>La stringa decriptata se la decriptazione ha successo; altrimenti, un messaggio di errore.</returns>
        public static string DecryptString(string EncryptedString, string Key)
        {
            try
            {
                byte[] Results;
                var UTF8 = new UTF8Encoding();

                // Utilizza MD5 per generare una chiave da 128 bit a partire dalla chiave fornita.
                var HashProvider = new MD5CryptoServiceProvider();
                var TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Key));

                // Configura l'algoritmo TripleDES.
                var TDESAlgorithm = new TripleDESCryptoServiceProvider
                {
                    Key = TDESKey,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };

                // Converti la stringa cifrata da Base64 a byte[].
                var DataToDecrypt = Convert.FromBase64String(EncryptedString);

                try
                {
                    var Decryptor = TDESAlgorithm.CreateDecryptor();
                    Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
                }
                finally
                {
                    // Pulizia delle risorse crittografiche.
                    TDESAlgorithm.Clear();
                    HashProvider.Clear();
                }

                return UTF8.GetString(Results);
            }
            catch (Exception)
            {
                // In caso di errore, restituisce un messaggio predefinito.
                return "<font style='color:red'>Valore Corrotto</font>";
            }
        }

        /// <summary>
        ///     Cripta una stringa in chiaro utilizzando una chiave specifica.
        /// </summary>
        /// <param name="InString">La stringa in chiaro da cifrare.</param>
        /// <param name="Key">La chiave utilizzata per la cifratura.</param>
        /// <returns>La stringa cifrata in formato Base64.</returns>
        public static string EncryptString(string InString, string Key)
        {
            byte[] Results;
            var UTF8 = new UTF8Encoding();

            // Generazione della chiave di crittografia usando MD5.
            var HashProvider = new MD5CryptoServiceProvider();
            var TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Key));

            // Configurazione dell'algoritmo TripleDES.
            var TDESAlgorithm = new TripleDESCryptoServiceProvider
            {
                Key = TDESKey,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            // Converti la stringa in chiaro in un array di byte.
            var DataToEncrypt = UTF8.GetBytes(InString);

            try
            {
                var Encryptor = TDESAlgorithm.CreateEncryptor();
                Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
            }
            finally
            {
                // Pulizia delle risorse crittografiche.
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            // Ritorna la stringa cifrata convertita in Base64.
            return Convert.ToBase64String(Results);
        }
    }
}