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

namespace ExpressionBuilder.Interfaces
{
    /// <summary>
    ///     Defines how a property should be filtered.
    /// </summary>
    public interface IFilterStatement
    {
        /// <summary>
        ///     Establishes how this filter statement will connect to the next one.
        /// </summary>
        FilterStatementConnector Connector { get; set; }

        /// <summary>
        ///     Property identifier conventionalized by for the Expression Builder.
        /// </summary>
        string PropertyId { get; set; }

        /// <summary>
        ///     Express the interaction between the property and the constant value defined in this filter statement.
        /// </summary>
        Operation Operation { get; set; }

        /// <summary>
        ///     Constant value that will interact with the property defined in this filter statement.
        /// </summary>
        object Value { get; set; }

        /// <summary>
        ///     Constant value that will interact with the property defined in this filter statement when the operation demands a
        ///     second value to compare to.
        /// </summary>
        object Value2 { get; set; }

        /// <summary>
        ///     Validates the FilterStatement regarding the number of provided values and supported operations.
        /// </summary>
        void Validate();
    }
}