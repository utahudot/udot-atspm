#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeOfDay/TimeOfDayDirectionHelper.cs
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

using System.ComponentModel.DataAnnotations;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Business.TimeOfDay
{
    internal static class TimeOfDayDirectionHelper
    {
        public static string NormalizeDirection(string direction)
        {
            if (string.IsNullOrWhiteSpace(direction))
            {
                return string.Empty;
            }

            foreach (DirectionTypes directionType in Enum.GetValues(typeof(DirectionTypes)))
            {
                var displayName = GetDisplayName(directionType);
                if (string.Equals(direction, directionType.ToString(), StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(direction, displayName, StringComparison.OrdinalIgnoreCase))
                {
                    return displayName;
                }
            }

            return direction.Trim();
        }

        public static string GetDisplayName(Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttributes(typeof(DisplayAttribute), false)
                .OfType<DisplayAttribute>()
                .FirstOrDefault()
                ?.Name ?? value.ToString();
        }
    }
}
