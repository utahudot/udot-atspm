#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Extensions/EnumExtensions.cs
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
using System.Reflection;

namespace ATSPM.Domain.Extensions
{
    /// <summary>
    /// Extensions for Enums
    /// </summary>
    public static class EnumExtensions
    {
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
