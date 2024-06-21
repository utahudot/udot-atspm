
using Microsoft.OpenApi.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Application.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            return value.GetAttributeOfType<DisplayAttribute>().Name;
        }
    }
}
