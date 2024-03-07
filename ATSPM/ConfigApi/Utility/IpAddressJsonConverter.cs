using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable disable

namespace ATSPM.ConfigApi.Utility
{
    /// <summary>
    /// Json converter for <see cref="IPAddress"/>
    /// </summary>
    public class IpAddressJsonConverter : JsonConverter<IPAddress>
    {
        /// <inheritdoc/>
        public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return IPAddress.Parse(reader.GetString());
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
