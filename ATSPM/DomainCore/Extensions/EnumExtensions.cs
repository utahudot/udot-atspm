#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - Utah.Udot.NetStandardToolkit.Extensions/EnumExtensions.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Utah.Udot.NetStandardToolkit.Extensions
{
    /// <summary>
    /// Enumeration type extension methods.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets an attribute on an enum field value.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to retrieve.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>
        /// The attribute of the specified type or null.
        /// </returns>
        public static T GetAttributeOfType<T>(this Enum enumValue) where T : Attribute
        {
            var type = enumValue.GetType();
            var memInfo = type.GetMember(enumValue.ToString()).First();
            var attributes = memInfo.GetCustomAttributes<T>(false);
            return attributes.FirstOrDefault();
        }

        /// <summary>
        /// Gets the enum display name.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>
        /// Use <see cref="DisplayAttribute"/> if exists.
        /// Otherwise, use the standard string representation.
        /// </returns>
        public static string GetDisplayName(this Enum enumValue)
        {
            var attribute = enumValue.GetAttributeOfType<DisplayAttribute>();
            return attribute == null ? enumValue.ToString() : attribute.Name;
        }

        /// <summary>
        /// Checks to see if Enum is decorated with <see cref="DisplayAttribute"/> and returns it
        /// Returns null if <see cref="DisplayAttribute"/> is not present
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static DisplayAttribute GetDisplayAttribute(this Enum enumValue)
        {
            Type type = enumValue.GetType();
            MemberInfo[] memberInfo = type.GetMember(enumValue.ToString());

            if (memberInfo != null && memberInfo.Length > 0)
            {
                var atts = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);

                if (atts.Length > 0)
                {
                    if (atts[0] is DisplayAttribute att)
                        return att;
                }
            }

            return null;
        }
    }
}
