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

using ExpressionBuilder.Attributes;

namespace ExpressionBuilder.Common
{
    /// <summary>
    /// Defines how the filter statements will be connected to each other.
    /// </summary>
    public enum FilterStatementConnector
    {
        /// <summary>
        /// Determines that both the current AND the next filter statement needs to be satisfied.
        /// </summary>
        And = 1,
        /// <summary>
        /// Determines that the current OR the next filter statement needs to be satisfied.
        /// </summary>
        Or = 2
    }

    /// <summary>
    /// Defines the operations supported by the <seealso cref="Builders.FilterBuilder" />.
    /// </summary>
    public enum Operation
    {
        /// <summary>
        /// Targets an object in which the property's value is equal to the provided value.
        /// </summary>
        /// <remarks>Accepts one value.</remarks>
        [NumberOfValues(1)]
        EqualTo = 1,

        /// <summary>
        /// Targets an object in which the property's value contains part of the provided value.
        /// </summary>
        /// <remarks>Accepts one value.</remarks>
        [NumberOfValues(1)]
        Contains,

        /// <summary>
        /// Targets an object in which the property's value starts with the provided value.
        /// </summary>
        /// <remarks>Accepts one value.</remarks>
        [NumberOfValues(1)]
        StartsWith,

        /// <summary>
        /// Targets an object in which the property's value ends with the provided value.
        /// </summary>
        /// <remarks>Accepts one value.</remarks>
        [NumberOfValues(1)]
        EndsWith,

        /// <summary>
        /// Targets an object in which the property's value is not equal to the provided value.
        /// </summary>
        /// <remarks>Accepts one value.</remarks>
        [NumberOfValues(1)]
        NotEqualTo,

        /// <summary>
        /// Targets an object in which the property's value is greater than the provided value.
        /// </summary>
        /// <remarks>Accepts one value.</remarks>
        [NumberOfValues(1)]
        GreaterThan,

        /// <summary>
        /// Targets an object in which the property's value is greater than or equal to the provided value.
        /// </summary>
        /// <remarks>Accepts one value.</remarks>
        [NumberOfValues(1)]
        GreaterThanOrEqualTo,

        /// <summary>
        /// Targets an object in which the property's value is less than the provided value.
        /// </summary>
        /// <remarks>Accepts one value.</remarks>
        [NumberOfValues(1)]
        LessThan,

        /// <summary>
        /// Targets an object in which the property's value is less than or equal to the provided value.
        /// </summary>
        /// <remarks>Accepts one value.</remarks>
        [NumberOfValues(1)]
        LessThanOrEqualTo,

        /// <summary>
        /// Targets an object in which the property's value is between the two provided values (greater than or equal to the first one and less then or equal to the second one).
        /// </summary>
        /// <remarks>Accepts two values.</remarks>
        [NumberOfValues(2)]
        Between,

        /// <summary>
        /// Targets an object in which the property's value is null.
        /// </summary>
        /// <remarks>Accepts no value at all.</remarks>
        [NumberOfValues(0)]
        IsNull,

        /// <summary>
        /// Targets an object in which the property's value is an empty string.
        /// </summary>
        /// <remarks>Accepts no value at all.</remarks>
        [NumberOfValues(0)]
        IsEmpty,

        /// <summary>
        /// Targets an object in which the property's value is null or an empty string.
        /// </summary>
        /// <remarks>Accepts no value at all.</remarks>
        [NumberOfValues(0)]
        IsNullOrWhiteSpace,

        /// <summary>
        /// Targets an object in which the property's value is not null.
        /// </summary>
        /// <remarks>Accepts no value at all.</remarks>
        [NumberOfValues(0)]
        IsNotNull,

        /// <summary>
        /// Targets an object in which the property's value is not an empty string.
        /// </summary>
        /// <remarks>Accepts no value at all.</remarks>
        [NumberOfValues(0)]
        IsNotEmpty,

        /// <summary>
        /// Targets an object in which the property's value is neither null nor an empty string.
        /// </summary>
        /// <remarks>Accepts no value at all.</remarks>
        [NumberOfValues(0)]
        IsNotNullNorWhiteSpace,

        /// <summary>
        /// Targets an object in which the provided value is presented in the property's value (as a list).
        /// </summary>
        /// <remarks>Accepts one value.</remarks>
        [NumberOfValues(1)]
        In
	}
    
    /// <summary>
    /// Groups types into simple groups and map the supported operations to each group.
    /// </summary>
    public enum TypeGroup
    {
        /// <summary>
        /// Default type group, only supports EqualTo and NotEqualTo.
        /// </summary>
        [SupportedOperations(Operation.EqualTo, Operation.NotEqualTo)]
        Default,

        /// <summary>
        /// Supports all text related operations.
        /// </summary>
        [SupportedOperations(Operation.Contains, Operation.EndsWith, Operation.EqualTo,
                             Operation.IsEmpty, Operation.IsNotEmpty, Operation.IsNotNull, Operation.IsNotNullNorWhiteSpace,
                             Operation.IsNull, Operation.IsNullOrWhiteSpace, Operation.NotEqualTo, Operation.StartsWith)]
        Text,

        /// <summary>
        /// Supports all numeric related operations.
        /// </summary>
        [SupportedOperations(Operation.Between, Operation.EqualTo, Operation.GreaterThan, Operation.GreaterThanOrEqualTo,
                             Operation.LessThan, Operation.LessThanOrEqualTo, Operation.NotEqualTo)]
        Number,

        /// <summary>
        /// Supports boolean related operations.
        /// </summary>
        [SupportedOperations(Operation.EqualTo, Operation.NotEqualTo)]
        Boolean,

        /// <summary>
        /// Supports all date related operations.
        /// </summary>
        [SupportedOperations(Operation.Between, Operation.EqualTo, Operation.GreaterThan, Operation.GreaterThanOrEqualTo,
                             Operation.LessThan, Operation.LessThanOrEqualTo, Operation.NotEqualTo)]
        Date,

        /// <summary>
        /// Supports nullable related operations.
        /// </summary>
        [SupportedOperations(Operation.IsNotNull, Operation.IsNull)]
        Nullable
    }
}
