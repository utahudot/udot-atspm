using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ATSPM.ConfigApi
{
    public class IgnoreTypeSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(Route)) // Replace Route with the type you want to ignore
            {
                schema.Properties.Clear();
            }
        }
    }
}