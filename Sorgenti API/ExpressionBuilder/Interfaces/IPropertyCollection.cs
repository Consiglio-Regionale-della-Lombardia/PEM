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
using System.Collections;
using System.Resources;

namespace ExpressionBuilder.Interfaces
{
    /// <summary>
    /// Collection of <see cref="Property" />.
    /// </summary>
    public interface IPropertyCollection : ICollection
    {
        /// <summary>
        /// Type from which the properties are loaded.
        /// </summary>
        Type Type { get; }
        /// <summary>
        /// ResourceManager which the properties descriptions should be gotten from.
        /// </summary>
        ResourceManager ResourceManager { get; }
    }
}