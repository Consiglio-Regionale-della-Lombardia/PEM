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

using ExpressionBuilder.Common;
using System;

namespace ExpressionBuilder.Exceptions
{
    /// <summary>
    /// Represents an attempt to use an operation not currently supported by a type.
    /// </summary>
    public class UnsupportedOperationException : Exception
    {
        /// <summary>
        /// Gets the <see cref="Operation" /> attempted to be used.
        /// </summary>
        public Operation Operation { get; private set; }

        /// <summary>
        /// Gets name of the type.
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return string.Format("The type '{0}' does not have support for the operation '{1}'.", TypeName, Operation);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedOperationException" /> class.
        /// </summary>
        /// <param name="operation">Operation used.</param>
        /// <param name="typeName">Name of the type</param>
        public UnsupportedOperationException(Operation operation, String typeName) : base()
        {
            Operation = operation;
            TypeName = typeName;
        }
    }
}