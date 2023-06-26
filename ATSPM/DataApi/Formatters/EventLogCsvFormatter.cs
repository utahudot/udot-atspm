using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
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
            if (context.Object is IEnumerable<ControllerEventLog> result)
            {
                context.HttpContext.Request.Headers.TryGetValue("X-Timestamp-Format", out StringValues timestampFormat);
                timestampFormat = string.IsNullOrEmpty(timestampFormat) ? "yyyy-MM-dd'T'HH:mm:ss.f" : timestampFormat;

                var csv = result.Select(x => $"{x.SignalId},{x.Timestamp.ToString(timestampFormat)},{x.EventCode},{x.EventParam}").ToList();
                csv.Insert(0, "SignalID,Timestamp,EventCode,EventParam");

                return context.HttpContext.Response.WriteAsync(string.Join("\n", csv), selectedEncoding);
            }

            return context.HttpContext.Response.CompleteAsync();
        }
        protected override bool CanWriteType(Type type)
        {
            return typeof(IEnumerable<ControllerEventLog>).IsAssignableFrom(type);
        }
    }
}

public class CustomHeaderSwaggerAttribute : IOperationFilter
{

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Timestamp-Format",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }

}