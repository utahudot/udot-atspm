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
