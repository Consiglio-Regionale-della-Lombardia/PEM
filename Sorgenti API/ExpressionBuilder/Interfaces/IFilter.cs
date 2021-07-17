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
using ExpressionBuilder.Generics;
using System.Collections.Generic;

namespace ExpressionBuilder.Interfaces
{
    /// <summary>
    /// Defines a filter from which a expression will be built.
    /// </summary>
    public interface IFilter
	{
		/// <summary>
		/// Group of statements that compose this filter.
		/// </summary>
		IEnumerable<IFilterStatement> Statements { get; }
        /// <summary>
        /// Add a statement, that doesn't need value, to this filter.
        /// </summary>
        /// <param name="propertyId">Property identifier conventionalized by for the Expression Builder.</param>
        /// <param name="operation">Express the interaction between the property and the constant value.</param>
        /// <param name="connector">Establishes how this filter statement will connect to the next one.</param>
        /// <returns>A FilterStatementConnection object that defines how this statement will be connected to the next one.</returns>
        IFilterStatementConnection By(string propertyId, Operation operation, FilterStatementConnector connector = FilterStatementConnector.And);
        /// <summary>
        /// Adds another statement to this filter.
        /// </summary>
        /// <param name="propertyId">Name of the property that will be filtered.</param>
        /// <param name="operation">Express the interaction between the property and the constant value.</param>
        /// <param name="value">Constant value that will interact with the property, required by operations that demands one value or more.</param>
        /// <param name="value2">Constant value that will interact with the property, required by operations that demands two values.</param>
        /// <param name="connector">Establishes how this filter statement will connect to the next one.</param>
        /// <returns>A FilterStatementConnection object that defines how this statement will be connected to the next one.</returns>
        IFilterStatementConnection By<TPropertyType>(string propertyId, Operation operation, TPropertyType value, TPropertyType value2 = default(TPropertyType), FilterStatementConnector connector = FilterStatementConnector.And);

        /// <summary>
        /// Adds another statement to this filter.
        /// </summary>
        /// <param name="filter">Name of the property that will be filtered.</param>
        /// <returns>A FilterStatementConnection object that defines how this statement will be connected to the next one.</returns>
        IFilterStatementConnection By<TPropertyType>(FilterStatement<TPropertyType> filter);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TPropertyType"></typeparam>
        /// <param name="filters"></param>
        void ImportStatements<TPropertyType>(ICollection<FilterStatement<TPropertyType>> filters);
        /// <summary>
		/// Removes all statements from this filter.
		/// </summary>
		void Clear();
    }
}