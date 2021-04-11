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
using ExpressionBuilder.Interfaces;

namespace ExpressionBuilder.Generics
{
    /// <summary>
    /// Connects to FilterStatement together.
    /// </summary>
	public class FilterStatementConnection<TClass> : IFilterStatementConnection where TClass : class
	{
		readonly IFilter _filter;
		readonly IFilterStatement _statement;
		
		internal FilterStatementConnection(IFilter filter, IFilterStatement statement)
		{
			_filter = filter;
			_statement = statement;
		}

        /// <summary>
		/// Defines that the last filter statement will connect to the next one using the 'AND' logical operator.
		/// </summary>
		public IFilter And
		{
			get
			{
				_statement.Connector = FilterStatementConnector.And;
				return _filter;
			}
		}

        /// <summary>
        /// Defines that the last filter statement will connect to the next one using the 'OR' logical operator.
        /// </summary>
		public IFilter Or
		{
			get
			{
				_statement.Connector = FilterStatementConnector.Or;
				return _filter;
			}
		}
	}
}
