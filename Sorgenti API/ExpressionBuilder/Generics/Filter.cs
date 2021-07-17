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

using ExpressionBuilder.Builders;
using ExpressionBuilder.Common;
using ExpressionBuilder.Helpers;
using ExpressionBuilder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ExpressionBuilder.Generics
{
    /// <summary>
    ///     Aggregates <see cref="FilterStatement{TPropertyType}" /> and build them into a LINQ expression.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class Filter<T> : IFilter, IXmlSerializable where T : class
    {
        private List<IFilterStatement> _statements;

        /// <summary>
        ///     Instantiates a new <see cref="Filter{TClass}" />
        /// </summary>
        public Filter()
        {
            _statements = new List<IFilterStatement>();
        }

        /// <summary>
        ///     List of <see cref="IFilterStatement" /> that will be combined and built into a LINQ expression.
        /// </summary>
        public IEnumerable<IFilterStatement> Statements => _statements.ToArray();

        /// <summary>
        ///     Adds a new <see cref="FilterStatement{TPropertyType}" /> to the <see cref="Filter{TClass}" />.
        ///     (To be used by <see cref="Operation" /> that need no values)
        /// </summary>
        /// <param name="propertyId">Property identifier conventionalized by for the Expression Builder.</param>
        /// <param name="operation"></param>
        /// <param name="connector"></param>
        /// <returns></returns>
        public IFilterStatementConnection By(string propertyId, Operation operation,
            FilterStatementConnector connector = FilterStatementConnector.And)
        {
            return By<string>(propertyId, operation, null, null, connector);
        }

        /// <summary>
        ///     Adds a new <see cref="FilterStatement{TPropertyType}" /> to the <see cref="Filter{TClass}" />.
        /// </summary>
        /// <typeparam name="TPropertyType"></typeparam>
        /// <param name="propertyId">Property identifier conventionalized by for the Expression Builder.</param>
        /// <param name="operation"></param>
        /// <param name="value"></param>
        /// <param name="value2"></param>
        /// <param name="connector"></param>
        /// <returns></returns>
        public IFilterStatementConnection By<TPropertyType>(string propertyId, Operation operation, TPropertyType value,
            TPropertyType value2 = default, FilterStatementConnector connector = FilterStatementConnector.And)
        {
            IFilterStatement statement =
                new FilterStatement<TPropertyType>(propertyId, operation, value, value2, connector);
            _statements.Add(statement);
            return new FilterStatementConnection<T>(this, statement);
        }

        public IFilterStatementConnection By<TPropertyType>(FilterStatement<TPropertyType> filter)
        {
            IFilterStatement statement = filter;
            _statements.Add(statement);
            return new FilterStatementConnection<T>(this, statement);
        }

        public void ImportStatements<TPropertyType>(ICollection<FilterStatement<TPropertyType>> filters)
        {
            if (filters == null)
            {
                return;
            }

            foreach (var filterStatement in filters)
            {
                By(filterStatement);
            }
        }

        /// <summary>
        ///     Removes all <see cref="FilterStatement{TPropertyType}" />, leaving the <see cref="Filter{TClass}" /> empty.
        /// </summary>
        public void Clear()
        {
            _statements.Clear();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        ///     Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The System.Xml.XmlReader stream from which the object is deserialized.</param>
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name.StartsWith("FilterStatementOf"))
                {
                    var type = reader.GetAttribute("Type");
                    var filterType = typeof(FilterStatement<>).MakeGenericType(Type.GetType(type));
                    var serializer = new XmlSerializer(filterType);
                    var statement = (IFilterStatement) serializer.Deserialize(reader);
                    _statements.Add(statement);
                }
            }
        }

        /// <summary>
        ///     Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The System.Xml.XmlWriter stream to which the object is serialized.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Type", typeof(T).AssemblyQualifiedName);
            writer.WriteStartElement("Statements");
            foreach (var statement in _statements)
            {
                var serializer = new XmlSerializer(statement.GetType());
                serializer.Serialize(writer, statement);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        ///     Implicitly converts a <see cref="Filter{TClass}" /> into a <see cref="Func{T,TResult}" />.
        /// </summary>
        /// <param name="filter"></param>
        public static implicit operator Func<T, bool>(Filter<T> filter)
        {
            var builder = new FilterBuilder(new BuilderHelper());
            return builder.GetExpression<T>(filter).Compile();
        }

        /// <summary>
        ///     String representation of <see cref="Filter{TClass}" />.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = "";
            var lastConector = FilterStatementConnector.And;
            foreach (var statement in _statements)
            {
                if (!string.IsNullOrWhiteSpace(result))
                {
                    result += " " + lastConector + " ";
                }

                result += statement.ToString();
                lastConector = statement.Connector;
            }

            return result.Trim();
        }


        public void BuildExpression(ref IQueryable<T> query)
        {
            query = Statements.Aggregate(query,
                (current, filterStatement) => current.Where(GetExpression(filterStatement)));
        }

        internal Expression<Func<T, bool>> GetExpression(IFilterStatement statement)
        {
            var builder = new FilterBuilder(new BuilderHelper());
            var exp = builder.GetExpression<T>(this);
            return exp;
        }
    }
}