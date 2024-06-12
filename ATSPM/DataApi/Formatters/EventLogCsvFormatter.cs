#region license
// Copyright 2024 Utah Departement of Transportation
// for DataApi - ATSPM.DataApi.Formatters/EventLogCsvFormatter.cs
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
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Text;

namespace ATSPM.DataApi.Formatters
{
    public class EventLogCsvFormatter : TextOutputFormatter
    {
        public EventLogCsvFormatter()
        {
            SupportedMediaTypes.Add(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("text/csv"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            //if (context.Object is IEnumerable<EventLogModelBase> stuff)
            //{
            //    context.HttpContext.Request.Headers.TryGetValue("X-Timestamp-Format", out StringValues timestampFormat);
            //    timestampFormat = string.IsNullOrEmpty(timestampFormat) ? "yyyy-MM-dd'T'HH:mm:ss.f" : timestampFormat;

            //    Console.WriteLine($"--------------------------------------------------------{stuff.Count()}");

            //    var csv = stuff.Select(x => $"{x.LocationIdentifier},{x.Timestamp.ToString(timestampFormat)}").ToList();

            //    csv.Insert(0, "locationId,Timestamp");

            //    return context.HttpContext.Response.WriteAsync(string.Join("\n", csv), selectedEncoding);
            //}

            if (context.Object is IEnumerable<ControllerEventLog> result)
            {
                context.HttpContext.Request.Headers.TryGetValue("X-Timestamp-Format", out StringValues timestampFormat);
                timestampFormat = string.IsNullOrEmpty(timestampFormat) ? "yyyy-MM-dd'T'HH:mm:ss.f" : timestampFormat;

                var csv = result.Select(x => $"{x.SignalIdentifier},{x.Timestamp.ToString(timestampFormat)},{x.EventCode},{x.EventParam}").ToList();
                csv.Insert(0, "locationId,Timestamp,EventCode,EventParam");

                return context.HttpContext.Response.WriteAsync(string.Join("\n", csv), selectedEncoding);
            }

            return context.HttpContext.Response.CompleteAsync();
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(IEnumerable<ControllerEventLog>).IsAssignableFrom(type);// || typeof(IEnumerable<CompressedAggregationBase>).IsAssignableFrom(type);
        }
    }
}
