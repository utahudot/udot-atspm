#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Extensions/PropertyExtensions.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Domain.Extensions
{
    /// <summary>
    /// Extensions for working with class properties using <see cref="System.Reflection"/>
    /// </summary>
    public static class PropertyExtensions
    {
        /// <summary>
        /// Checks to see if string value is valid property on object
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <param name="propertyName">String name of property to check</param>
        /// <returns>true if <paramref name="propertyName"/> exists in <paramref name="obj"/></returns>
        public static bool HasProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperties().Any(p => p.Name == propertyName);
        }

        /// <summary>
        /// Gets the property value
        /// </summary>
        /// <param name="obj">Object containing property</param>
        /// <param name="propertyName">String name of project to get value for</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            if (obj.HasProperty(propertyName))
            {
                var value = obj.GetType().GetProperty(propertyName).GetValue(obj, null);

                if (value is T result)
                    return result;
                else
                    throw new ArgumentException("Not a valid type");
            }

            throw new ArgumentException(propertyName, nameof(propertyName));
        }
    }
}
