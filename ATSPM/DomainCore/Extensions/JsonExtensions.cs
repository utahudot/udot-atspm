using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ATSPM.Domain.Extensions
{
    public static class JsonExtensions
    {
        public static T ToJson<T>(this byte[] bytes) where T : class
        {
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(bytes), null);
        }
    }
}
