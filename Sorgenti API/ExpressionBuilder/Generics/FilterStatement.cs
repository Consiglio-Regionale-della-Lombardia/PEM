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
using ExpressionBuilder.Exceptions;
using ExpressionBuilder.Helpers;
using ExpressionBuilder.Interfaces;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace ExpressionBuilder.Generics
{
    /// <summary>
	/// Defines how a property should be filtered.
	/// </summary>
    [Serializable]
    public class FilterStatement<TPropertyType> : IFilterStatement
    {
        /// <summary>
		/// Establishes how this filter statement will connect to the next one. 
		/// </summary>
        public FilterStatementConnector Connector { get; set; }
        /// <summary>
		/// Property identifier conventionalized by for the Expression Builder.
		/// </summary>
        public string PropertyId { get; set; }
        /// <summary>
		/// Express the interaction between the property and the constant value defined in this filter statement.
		/// </summary>
        public Operation Operation { get; set; }
        /// <summary>
		/// Constant value that will interact with the property defined in this filter statement.
		/// </summary>
        public object Value { get; set; }
        /// <summary>
        /// Constant value that will interact with the property defined in this filter statement when the operation demands a second value to compare to.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Value2 { get; set; }
        
        /// <summary>
        /// Instantiates a new <see cref="FilterStatement{TPropertyType}" />.
        /// </summary>
        /// <param name="propertyId"></param>
        /// <param name="operation"></param>
        /// <param name="value"></param>
        /// <param name="value2"></param>
        /// <param name="connector"></param>
		public FilterStatement(string propertyId, Operation operation, TPropertyType value, TPropertyType value2 = default(TPropertyType), FilterStatementConnector connector = FilterStatementConnector.And)
		{
			PropertyId = propertyId;
			Connector = connector;
			Operation = operation;
			if (typeof(TPropertyType).IsArray)
			{
				if (operation != Operation.Contains && operation != Operation.In)
                {
                    throw new ArgumentException("Only 'Operacao.Contains' and 'Operacao.In' support arrays as parameters.");
                }

                var listType = typeof(List<>);
                var constructedListType = listType.MakeGenericType(typeof(TPropertyType).GetElementType());
                Value = value != null ? Activator.CreateInstance(constructedListType, value) : null;
                Value2 = value2 != null ? Activator.CreateInstance(constructedListType, value2) : null;
            }
			else
			{
				Value = value;
                Value2 = value2;
			}

            Validate();
		}

        /// <summary>
        /// Instantiates a new <see cref="FilterStatement{TPropertyType}" />.
        /// </summary>
        public FilterStatement() { }

        /// <summary>
        /// Validates the FilterStatement regarding the number of provided values and supported operations.
        /// </summary>
        public void Validate()
        {
            var helper = new OperationHelper();            
            ValidateNumberOfValues(helper);
            ValidateSupportedOperations(helper);
        }

        private void ValidateNumberOfValues(OperationHelper helper)
        {
            var numberOfValues = helper.NumberOfValuesAcceptable(Operation);
            var failsForSingleValue = numberOfValues == 1 && Value2 != null && !Value2.Equals(default(TPropertyType));
            var failsForNoValueAtAll = numberOfValues == 0 && Value != null && Value2 != null && (!Value.Equals(default(TPropertyType)) || !Value2.Equals(default(TPropertyType)));

            if (failsForSingleValue || failsForNoValueAtAll)
            {
                throw new WrongNumberOfValuesException(Operation);
            }
        }

        private void ValidateSupportedOperations(OperationHelper helper)
        {
            List<Operation> supportedOperations = null;
            if (typeof(TPropertyType) == typeof(object))
            {
                //TODO: Issue regarding the TPropertyType that comes from the UI always as 'Object'
                //supportedOperations = helper.GetSupportedOperations(Value.GetType());
                System.Diagnostics.Debug.WriteLine("WARN: Not able to check if the operation is supported or not.");
                return;
            }
            
            supportedOperations = helper.SupportedOperations(typeof(TPropertyType));

            if (!supportedOperations.Contains(Operation))
            {
                throw new UnsupportedOperationException(Operation, typeof(TPropertyType).Name);
            }
        }

        /// <summary>
        /// String representation of <see cref="FilterStatement{TPropertyType}" />.
        /// </summary>
        /// <returns></returns>
		public override string ToString()
        {
            var operationHelper = new OperationHelper();

            switch (operationHelper.NumberOfValuesAcceptable(Operation))
            {
                case 0:
                    return string.Format("{0} {1}", PropertyId, Operation);
                case 2:
                    return string.Format("{0} {1} {2} And {3}", PropertyId, Operation, Value, Value2);
                default:
                    return string.Format("{0} {1} {2}", PropertyId, Operation, Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        ///  Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The System.Xml.XmlReader stream from which the object is deserialized.</param>
        public void ReadXml(XmlReader reader)
        {
            reader.Read();
            PropertyId = reader.ReadElementContentAsString();
            Operation = (Operation)Enum.Parse(typeof(Operation), reader.ReadElementContentAsString());
            if (typeof(TPropertyType).IsEnum)
            {
                Value = Enum.Parse(typeof(TPropertyType), reader.ReadElementContentAsString());
            }
            else
            {
                Value = Convert.ChangeType(reader.ReadElementContentAsString(), typeof(TPropertyType));
            }

            Connector = (FilterStatementConnector)Enum.Parse(typeof(FilterStatementConnector), reader.ReadElementContentAsString());
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The System.Xml.XmlWriter stream to which the object is serialized.</param>
        public void WriteXml(XmlWriter writer)
        {
            var type = Value.GetType();
            var serializer = new XmlSerializer(type);
            writer.WriteAttributeString("Type", type.AssemblyQualifiedName);
            writer.WriteElementString("PropertyId", PropertyId);
            writer.WriteElementString("Operation", Operation.ToString("d"));
            writer.WriteElementString("Value", Value.ToString());
            writer.WriteElementString("Connector", Connector.ToString("d"));
        }
    }
}