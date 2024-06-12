#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Extensions/JsonExtensions.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
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
