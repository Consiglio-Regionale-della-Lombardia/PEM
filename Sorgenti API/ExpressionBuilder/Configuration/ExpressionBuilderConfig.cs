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
using System.Configuration;

namespace ExpressionBuilder.Configuration
{
    internal class ExpressionBuilderConfig : ConfigurationSection
    {
        public const string SectionName = "ExpressionBuilder";

        private const string SupportedTypeCollectionName = "SupportedTypes";

        [ConfigurationProperty(SupportedTypeCollectionName)]
        
        public SupportedTypesElementConfiguration SupportedTypes { get { return (SupportedTypesElementConfiguration)base[SupportedTypeCollectionName]; } }

        public class SupportedTypeElement : ConfigurationElement
        {
            [ConfigurationProperty("type", IsRequired = true, IsKey = true)]
            public string Type
            {
                get
                {
                    return (string)base["type"];
                }
            }

            [ConfigurationProperty("typeGroup", IsRequired = true, IsKey = false)]
            public TypeGroup TypeGroup
            {
                get
                {
                    return (TypeGroup)base["typeGroup"];
                }
            }
        }

        [ConfigurationCollection(typeof(SupportedTypesElementConfiguration), AddItemName = "add")]
        public class SupportedTypesElementConfiguration : ConfigurationElementCollection
        {
            protected override ConfigurationElement CreateNewElement()
            {
                return new SupportedTypeElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                if (element == null)
                    throw new ArgumentNullException("element");

                return ((SupportedTypeElement)element).Type;
            }
        }
    }
}