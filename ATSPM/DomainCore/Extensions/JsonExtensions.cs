using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ATSPM.Domain.Extensions
{
    public static class JsonExtensions
    {
        public static T FromJson<T>(this string json) where T : new()
        {
            return JsonSerializer.Deserialize<T>(json);
        }

        public static string ToJson<T>(this T item) where T : new()
        {
            return JsonSerializer.Serialize(item);
        }

        public static T FromEncodedJson<T>(this byte[] bytes) where T : new()
        {
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(bytes));
        }

        public static byte[] ToEncodedJson<T>(this T item) where T : new()
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(item));
        }
    }
}
