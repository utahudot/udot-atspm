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
