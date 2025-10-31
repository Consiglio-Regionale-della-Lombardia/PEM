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
using ExpressionBuilder.Common;
using ExpressionBuilder.Configuration;
using ExpressionBuilder.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ExpressionBuilder.Helpers
{
    /// <summary>
    /// Useful methods regarding <seealso cref="Operation"></seealso>.
    /// </summary>
    public class OperationHelper : IOperationHelper
    {
        readonly Dictionary<TypeGroup, HashSet<Type>> TypeGroups;

        /// <summary>
        /// Instantiates a new OperationHelper.
        /// </summary>
        public OperationHelper()
        {
            TypeGroups = new Dictionary<TypeGroup, HashSet<Type>>
            {
                { TypeGroup.Text, new HashSet<Type> { typeof(string), typeof(char) } },
                { TypeGroup.Number, new HashSet<Type> { typeof(int), typeof(uint), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(long), typeof(ulong), typeof(Single), typeof(double), typeof(decimal) } },
                { TypeGroup.Boolean, new HashSet<Type> { typeof(bool) } },
                { TypeGroup.Date, new HashSet<Type> { typeof(DateTime) } },
                { TypeGroup.Nullable, new HashSet<Type> { typeof(Nullable<>) } }
            };
        }

        /// <summary>
        /// Retrieves a list of <see cref="Operation"></see> supported by a type.
        /// </summary>
        /// <param name="type">Type for which supported operations should be retrieved.</param>
        /// <returns></returns>
        public List<Operation> SupportedOperations(Type type)
        {
            var supportedOperations = ExtractSupportedOperationsFromAttribute(type);
            
            if (type.IsArray)
            {
                //The 'In' operation is supported by all types, as long as it's an array...
                supportedOperations.Add(Operation.In);
            }

            var underlyingNullableType = Nullable.GetUnderlyingType(type);
            if (underlyingNullableType != null)
            {
                var underlyingNullableTypeOperations = SupportedOperations(underlyingNullableType);
                supportedOperations.AddRange(underlyingNullableTypeOperations);
            }

            return supportedOperations;
        }

        private void GetCustomSupportedTypes()
        {
            var configSection = ConfigurationManager.GetSection(ExpressionBuilderConfig.SectionName) as ExpressionBuilderConfig;
            if (configSection == null)
            {
                return;
            }

            foreach (ExpressionBuilderConfig.SupportedTypeElement supportedType in configSection.SupportedTypes)
            {
                Type type = Type.GetType(supportedType.Type, false, true);
                if (type != null)
                {
                    TypeGroups[supportedType.TypeGroup].Add(type);
                }
            }
        }

        private List<Operation> ExtractSupportedOperationsFromAttribute(Type type)
        {
            var typeName = type.Name;
            if (type.IsArray)
            {
                typeName = type.GetElementType()?.Name;
            }

            GetCustomSupportedTypes();
            var typeGroup = TypeGroups.FirstOrDefault(i => i.Value.Any(v => v.Name == typeName)).Key;
            var fieldInfo = typeGroup.GetType().GetField(typeGroup.ToString());
            var attrs = fieldInfo.GetCustomAttributes(false);
            var attr = attrs.FirstOrDefault(a => a is SupportedOperationsAttribute);
            return (attr as SupportedOperationsAttribute)?.SupportedOperations;
        }

        /// <summary>
        /// Retrieves the exactly number of values acceptable by a specific operation.
        /// </summary>
        /// <param name="operation"><see cref="Operation"></see> for which the number of values acceptable should be verified.</param>
        /// <returns></returns>
        public int NumberOfValuesAcceptable(Operation operation)
        {
            var fieldInfo = operation.GetType().GetField(operation.ToString());
            var attrs = fieldInfo.GetCustomAttributes(false);
            var attr = attrs.FirstOrDefault(a => a is NumberOfValuesAttribute);
            return (attr as NumberOfValuesAttribute).NumberOfValues;
        }
    }
}