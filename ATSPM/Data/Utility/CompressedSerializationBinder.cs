using Newtonsoft.Json.Serialization;
#nullable disable

namespace ATSPM.Data.Utility
{
    /// <summary>
    /// Custom <see cref="ISerializationBinder"/> for marking compressed lists with type
    /// for serializing/deserializing compressed data.
    /// <seealso cref="https://www.newtonsoft.com/json/help/html/SerializeSerializationBinder.htm"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompressedSerializationBinder<T> : DefaultSerializationBinder
    {
        /// <inheritdoc/>
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            if (serializedType.IsAssignableTo(typeof(IEnumerable<T>)) &&
                serializedType.IsGenericType &&
                serializedType.GetGenericArguments()[0].IsSubclassOf(typeof(T)))
            {
                assemblyName = null;
                typeName = serializedType.GetGenericArguments()[0].Name;
            }
            else
            {
                base.BindToName(serializedType, out assemblyName, out typeName);
            }
        }

        /// <inheritdoc/>
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (string.IsNullOrEmpty(assemblyName) && typeof(T).Assembly.GetTypes().Where(w => w.Name == typeName).Count() > 0)
            {
                return typeof(List<>).MakeGenericType(Type.GetType($"{typeof(T).Namespace}.{typeName}"));
            }

            return base.BindToType(assemblyName, typeName);
        }
    }
}
