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

using ExpressionBuilder.Interfaces;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionBuilder.Helpers
{
	internal class BuilderHelper : IBuilderHelper
	{
        public readonly MethodInfo trimMethod = typeof(string).GetMethod("Trim", new Type[0]);
        public readonly MethodInfo toLowerMethod = typeof(string).GetMethod("ToLower", new Type[0]);

        public Expression GetMemberExpression(Expression param, string propertyName)
        {
        	if (propertyName.Contains("."))
        	{
        		int index = propertyName.IndexOf(".");
        		var subParam = Expression.Property(param, propertyName.Substring(0, index));
        		return GetMemberExpression(subParam, propertyName.Substring(index + 1));
        	}
            
            return Expression.Property(param, propertyName);
        }
	}
}