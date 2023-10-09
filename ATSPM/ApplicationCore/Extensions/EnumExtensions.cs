using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        Type type = enumValue.GetType();
        MemberInfo[] memberInfo = type.GetMember(enumValue.ToString());

        if (memberInfo != null && memberInfo.Length > 0)
        {
            object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);

            if (attrs != null && attrs.Length > 0)
            {
                return ((DisplayAttribute)attrs[0]).Name;
            }
        }

        // If no Display attribute is found, return the string representation of the enum value.
        return enumValue.ToString();
    }
}
