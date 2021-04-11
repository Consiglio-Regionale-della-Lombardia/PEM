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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ExpressionBuilder.Attributes
{
    internal class NumberOfValuesAttribute : Attribute
    {
        [Range(0, 2, ErrorMessage = "Operations may only have from none to two values.")]
        [DefaultValue(1)]
        public int NumberOfValues { get; private set; }

        /// <summary>
        /// Defines the number of values supported by the operation.
        /// </summary>
        /// <param name="numberOfValues">Number of values the operation demands.</param>
        public NumberOfValuesAttribute(int numberOfValues = 1)
        {
            NumberOfValues = numberOfValues;
        }
    }
}
