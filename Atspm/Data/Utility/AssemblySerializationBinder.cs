using Newtonsoft.Json.Serialization;

namespace Utah.Udot.Atspm.Data.Utility
{
    /// <summary>
    /// A custom serialization binder that handles type binding for types within the same assembly as the specified generic type parameter <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type used to determine the assembly for type binding.</typeparam>
    public class AssemblySerializationBinder<T> : DefaultSerializationBinder
    {
        /// <summary>
        /// Binds a serialized type to its assembly name and type name.
        /// </summary>
        /// <param name="serializedType">The type to be serialized.</param>
        /// <param name="assemblyName">The name of the assembly where the type is located.</param>
        /// <param name="typeName">The name of the type.</param>
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            if (serializedType.Assembly == typeof(T).Assembly)
            {
                assemblyName = string.Empty;
                typeName = serializedType.Name;
            }
            else
            {
                base.BindToName(serializedType, out assemblyName, out typeName);
            }
        }

        /// <summary>
        /// Binds an assembly name and type name to a type.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly where the type is located.</param>
        /// <param name="typeName">The name of the type.</param>
        /// <returns>The type associated with the specified assembly name and type name.</returns>
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (string.IsNullOrEmpty(assemblyName) && typeof(T).Assembly.GetTypes().Count(w => w.Name == typeName) > 0)
            {
                return Type.GetType($"{typeof(T).Namespace}.{typeName}");
            }

            return base.BindToType(assemblyName, typeName);
        }
    }
}
