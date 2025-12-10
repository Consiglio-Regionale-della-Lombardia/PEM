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

using PortaleRegione.Logger;
using System;
using System.Security.Cryptography;
using System.Text;
using PortaleRegione.Crypto;

namespace PortaleRegione.BAL
{
    public class BALHelper
    {
        internal static string Decrypt(string strData, string key = "")
        {
            try
            {
                key = !string.IsNullOrEmpty(key)
                    ? CryptoHelper.DecryptString(key, AppSettingsConfiguration.masterKey)
                    : AppSettingsConfiguration.masterKey;

                return CryptoHelper.DecryptString(strData, key);
            }
            catch (Exception e)
            {
                Log.Error("DecryptString", e);
                throw e;
            }
        }
    }
}