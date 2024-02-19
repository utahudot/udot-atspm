using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

#nullable disable

namespace ATSPM.Data.Utility
{
    /// <summary>
    /// <see cref="ValueConverter"/> used to convert compressed list of <typeparamref name="T"/>
    /// to/from gzip compressed json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CompressedListComverter<T> : ValueConverter<IEnumerable<T>, byte[]>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public CompressedListComverter() : base(
            v => Newtonsoft.Json.JsonConvert.SerializeObject(v, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Arrays,
                // Add Converters with StringEnumConverter to handle enum serialization as integers
                Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter { AllowIntegerValues = true } }

            }).GZipCompressToByte(),
            v => JsonConvert.DeserializeObject<IEnumerable<T>>(v.GZipDecompressToString(), new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Arrays
            }))
        { }
    }
}
