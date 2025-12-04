#region license
// Copyright 2025 Utah Departement of Transportation
// for DataApi - Utah.Udot.Atspm.DataApi.Formatters/EventLogCsvFormatter.cs
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

using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Utah.Udot.Atspm.DataApi.Formatters
{
    /// <summary>
    /// A custom <see cref="TextOutputFormatter"/> that serializes collections of
    /// <see cref="EventLogModelBase"/> derived types into CSV format.
    /// </summary>
    /// <remarks>
    /// This formatter inspects the runtime type of the event log objects and uses reflection
    /// to generate a CSV file with headers based on the public properties of the derived type.
    /// It supports the <c>text/csv</c> media type and respects an optional
    /// <c>X-Timestamp-Format</c> request header to control the formatting of <see cref="DateTime"/> values.
    /// </remarks>
    public class EventLogCsvFormatter : TextOutputFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogCsvFormatter"/> class.
        /// Configures supported media types and encodings.
        /// </summary>
        public EventLogCsvFormatter()
        {
            SupportedMediaTypes.Add(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("text/csv"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        /// <summary>
        /// Writes the response body as CSV when the object is an <see cref="IEnumerable{T}"/>
        /// of <see cref="EventLogModelBase"/> instances.
        /// </summary>
        /// <param name="context">
        /// Provides access to the object being written and the HTTP context.
        /// </param>
        /// <param name="selectedEncoding">
        /// The character encoding to use when writing the response.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous write operation.
        /// </returns>
        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            if (context.Object is IEnumerable<EventLogModelBase> result)
            {
                // Retrieve timestamp format header (default if not provided)
                context.HttpContext.Request.Headers.TryGetValue("X-Timestamp-Format", out StringValues timestampFormat);
                var tsFormat = string.IsNullOrEmpty(timestampFormat) ? "yyyy-MM-dd'T'HH:mm:ss.f" : timestampFormat.ToString();

                var sb = new StringBuilder();

                // Determine the runtime type of the first item
                var first = result.FirstOrDefault();
                if (first != null)
                {
                    var type = first.GetType();
                    var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    // Header row
                    sb.AppendLine(string.Join(",", props.Select(p => p.Name)));

                    // Data rows
                    foreach (var item in result)
                    {
                        var values = props.Select(p =>
                        {
                            var val = p.GetValue(item);
                            if (val == null) return string.Empty;

                            // Special handling for DateTime
                            if (val is DateTime dt)
                                return dt.ToString(tsFormat, CultureInfo.InvariantCulture);

                            // Escape commas/quotes
                            var str = Convert.ToString(val, CultureInfo.InvariantCulture);
                            if (str.Contains(",") || str.Contains("\""))
                                str = $"\"{str.Replace("\"", "\"\"")}\"";

                            return str;
                        });

                        sb.AppendLine(string.Join(",", values));
                    }
                }

                await context.HttpContext.Response.WriteAsync(sb.ToString(), selectedEncoding);
                return;
            }

            await context.HttpContext.Response.CompleteAsync();
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(IEnumerable<CompressedEventLogBase>).IsAssignableFrom(type);
        }
    }
}
