using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ATSPM.Data.Utility
{
    /// <summary>
    /// <see cref="ValueConverter"/> used to convert a database string to an assembly type
    /// </summary>
    internal class CompressionTypeConverter : ValueConverter<Type, string>
    {
        /// <summary>
        /// <see cref="ValueConverter"/> used to convert a database string to an assembly type
        /// </summary>
        /// <param name="nameSpace">Namespace of class to convert to/from</param>
        /// <param name="assembly">Assembly of class to convert to/from</param>
        public CompressionTypeConverter(string nameSpace, string assembly) : base(v => v.Name, v => Type.GetType($"{nameSpace}.{v}, {assembly}")) { }
    }
}
