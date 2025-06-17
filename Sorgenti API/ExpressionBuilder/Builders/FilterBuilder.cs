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
using ExpressionBuilder.Helpers;
using ExpressionBuilder.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionBuilder.Builders
{
    internal class FilterBuilder
    {
        private readonly MethodInfo endsWithMethod = typeof(string).GetMethod("EndsWith", new[] {typeof(string)});

        public readonly Dictionary<Operation, Func<Expression, Expression, Expression, Expression>> Expressions;
        private readonly BuilderHelper helper;
        private readonly MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new[] {typeof(string)});

        private readonly MethodInfo stringContainsMethod = typeof(string).GetMethod("Contains");

        internal FilterBuilder(BuilderHelper helper)
        {
            this.helper = helper;

            Expressions = new Dictionary<Operation, Func<Expression, Expression, Expression, Expression>>
            {
                {Operation.EqualTo, (member, constant, constant2) => Expression.Equal(member, constant)},
                {Operation.NotEqualTo, (member, constant, constant2) => Expression.NotEqual(member, constant)},
                {Operation.GreaterThan, (member, constant, constant2) => Expression.GreaterThan(member, constant)},
                {
                    Operation.GreaterThanOrEqualTo,
                    (member, constant, constant2) => Expression.GreaterThanOrEqual(member, constant)
                },
                {Operation.LessThan, (member, constant, constant2) => Expression.LessThan(member, constant)},
                {
                    Operation.LessThanOrEqualTo,
                    (member, constant, constant2) => Expression.LessThanOrEqual(member, constant)
                },
                {Operation.Contains, (member, constant, constant2) => Contains(member, constant)},
                {
                    Operation.StartsWith,
                    (member, constant, constant2) => Expression.Call(member, startsWithMethod, constant)
                },
                {
                    Operation.EndsWith,
                    (member, constant, constant2) => Expression.Call(member, endsWithMethod, constant)
                },
                {Operation.Between, (member, constant, constant2) => Between(member, constant, constant2)},
                {Operation.In, (member, constant, constant2) => Contains(member, constant)},
                {
                    Operation.IsNull,
                    (member, constant, constant2) => Expression.Equal(member, Expression.Constant(null))
                },
                {
                    Operation.IsNotNull,
                    (member, constant, constant2) => Expression.NotEqual(member, Expression.Constant(null))
                },
                {
                    Operation.IsEmpty,
                    (member, constant, constant2) => Expression.Equal(member, Expression.Constant(string.Empty))
                },
                {
                    Operation.IsNotEmpty,
                    (member, constant, constant2) => Expression.NotEqual(member, Expression.Constant(string.Empty))
                },
                {Operation.IsNullOrWhiteSpace, (member, constant, constant2) => IsNullOrWhiteSpace(member)},
                {Operation.IsNotNullNorWhiteSpace, (member, constant, constant2) => IsNotNullNorWhiteSpace(member)}
            };
        }

        public Expression<Func<T, bool>> GetExpression<T>(IFilter filter) where T : class
        {
            var param = Expression.Parameter(typeof(T), "x");
            Expression expression = null;
            var connector = FilterStatementConnector.And;
            foreach (var statement in filter.Statements)
            {
                try
                {
                    Expression expr = null;
                    if (IsList(statement))
                    {
                        expr = ProcessListStatement(param, statement);
                    }
                    else
                    {
                        expr = GetExpression(param, statement);
                    }

                    expression = expression == null ? expr : CombineExpressions(expression, expr, connector);
                    connector = statement.Connector;
                }
                catch (Exception e)
                {
                    continue;
                }
            }

            expression = expression ?? Expression.Constant(true);

            return Expression.Lambda<Func<T, bool>>(expression, param);
        }

        private bool IsList(IFilterStatement statement)
        {
            return statement.PropertyId.Contains("[") && statement.PropertyId.Contains("]");
        }

        private Expression CombineExpressions(Expression expr1, Expression expr2, FilterStatementConnector connector)
        {
            return connector == FilterStatementConnector.And
                ? Expression.AndAlso(expr1, expr2)
                : Expression.OrElse(expr1, expr2);
        }

        private Expression ProcessListStatement(ParameterExpression param, IFilterStatement statement)
        {
            var propertyName = statement.PropertyId.Replace("[", "").Replace("]", "");

            var listItemParam = Expression.Parameter(param.Type, "i");
            var lambda = Expression.Lambda(GetExpression(listItemParam, statement, propertyName), listItemParam);
            var member = helper.GetMemberExpression(param, propertyName);
            var enumerableType = typeof(Enumerable);
            var anyInfo = enumerableType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "Any" && m.GetParameters().Count() == 2);
            anyInfo = anyInfo.MakeGenericMethod(param.Type);
            return Expression.Call(anyInfo, member, lambda);
        }

        private Expression GetExpression(ParameterExpression param, IFilterStatement statement,
            string propertyName = null)
        {
            Expression resultExpr = null;
            var memberName = propertyName ?? statement.PropertyId;
            var member = helper.GetMemberExpression(param, memberName);
            var constant = GetConstantExpression(member, statement.Value);
            var constant2 = GetConstantExpression(member, statement.Value2);

            //if (Nullable.GetUnderlyingType(member.Type) != null && statement.Value != null)
            //{
            //    return Expression.Equal(member, constant);
            //}

            var safeStringExpression = GetSafeStringExpression(member, statement.Operation, constant, constant2);
            resultExpr = resultExpr != null
                ? Expression.AndAlso(resultExpr, safeStringExpression)
                : safeStringExpression;
            resultExpr = GetSafePropertyMember(param, memberName, resultExpr);

            if ((statement.Operation == Operation.IsNull || statement.Operation == Operation.IsNullOrWhiteSpace) &&
                memberName.Contains("."))
            {
                resultExpr = Expression.OrElse(CheckIfParentIsNull(param, member, memberName), resultExpr);
            }

            return resultExpr;
        }

        private Expression GetSafeStringExpression(Expression member, Operation operation, Expression constant,
            Expression constant2)
        {
            if (member.Type != typeof(string))
            {
                return Expressions[operation].Invoke(member, constant, constant2);
            }

            var newMember = member;

            if (operation != Operation.IsNullOrWhiteSpace && operation != Operation.IsNotNullNorWhiteSpace)
            {
                var trimMemberCall = Expression.Call(member, helper.trimMethod);
                newMember = Expression.Call(trimMemberCall, helper.toLowerMethod);
            }

            var resultExpr = operation != Operation.IsNull
                ? Expressions[operation].Invoke(newMember, constant, constant2)
                : Expressions[operation].Invoke(member, constant, constant2);

            if (member.Type == typeof(string) && operation != Operation.IsNull)
            {
                if (operation != Operation.IsNullOrWhiteSpace && operation != Operation.IsNotNullNorWhiteSpace)
                {
                    Expression memberIsNotNull = Expression.NotEqual(member, Expression.Constant(null));
                    resultExpr = Expression.AndAlso(memberIsNotNull, resultExpr);
                }
            }

            return resultExpr;
        }

        public Expression GetSafePropertyMember(ParameterExpression param, string memberName, Expression expr)
        {
            if (!memberName.Contains("."))
            {
                return expr;
            }

            var parentName = memberName.Substring(0, memberName.IndexOf("."));
            var parentMember = helper.GetMemberExpression(param, parentName);
            return Expression.AndAlso(Expression.NotEqual(parentMember, Expression.Constant(null)), expr);
        }

        private Expression CheckIfParentIsNull(Expression param, Expression member, string memberName)
        {
            var parentName = memberName.Substring(0, memberName.IndexOf("."));
            var parentMember = helper.GetMemberExpression(param, parentName);
            return Expression.Equal(parentMember, Expression.Constant(null));
        }

        private Expression GetConstantExpression(Expression member, object value)
        {
            if (value == null)
            {
                return null;
            }

            var converter = TypeDescriptor.GetConverter(member.Type);

            if (!converter.CanConvertFrom(typeof(string)))
            {
                throw new NotSupportedException();
            }

            var propertyValue = converter.ConvertFromInvariantString(value.ToString());
            var constantVal = Expression.Constant(propertyValue);
            return Expression.Convert(constantVal, member.Type);
        }

        #region Operations

        private Expression Contains(Expression member, Expression expression)
        {
            MethodCallExpression contains = null;
            if (expression is ConstantExpression constant && constant.Value is IList &&
                constant.Value.GetType().IsGenericType)
            {
                var type = constant.Value.GetType();
                var containsInfo = type.GetMethod("Contains", new[] {type.GetGenericArguments()[0]});
                contains = Expression.Call(constant, containsInfo, member);
            }

            return contains ?? Expression.Call(member, stringContainsMethod, expression);
        }

        private Expression Between(Expression member, Expression constant, Expression constant2)
        {
            var left = Expressions[Operation.GreaterThanOrEqualTo].Invoke(member, constant, null);
            var right = Expressions[Operation.LessThanOrEqualTo].Invoke(member, constant2, null);

            return CombineExpressions(left, right, FilterStatementConnector.And);
        }

        private Expression IsNullOrWhiteSpace(Expression member)
        {
            Expression exprNull = Expression.Constant(null);
            var trimMemberCall = Expression.Call(member, helper.trimMethod);
            Expression exprEmpty = Expression.Constant(string.Empty);
            return Expression.OrElse(
                Expression.Equal(member, exprNull),
                Expression.Equal(trimMemberCall, exprEmpty));
        }

        private Expression IsNotNullNorWhiteSpace(Expression member)
        {
            Expression exprNull = Expression.Constant(null);
            var trimMemberCall = Expression.Call(member, helper.trimMethod);
            Expression exprEmpty = Expression.Constant(string.Empty);
            return Expression.AndAlso(
                Expression.NotEqual(member, exprNull),
                Expression.NotEqual(trimMemberCall, exprEmpty));
        }

        #endregion
    }
}