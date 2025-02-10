using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
namespace Utah.Udot.Atspm.Data.Utility
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class DictionaryToJsonValueConverter<TKey, TValue> : ValueConverter<Dictionary<TKey, TValue>, string>
    {
        public DictionaryToJsonValueConverter() : base(
             v => JsonConvert.SerializeObject(v),
             v => JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(v))
        { }
    }
}
