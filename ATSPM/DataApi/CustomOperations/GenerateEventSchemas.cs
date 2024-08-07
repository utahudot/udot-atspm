using ATSPM.Data.Models.EventLogModels;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ATSPM.DataApi.CustomOperations
{
    public class GenerateEventSchemas : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var i in typeof(EventLogModelBase).Assembly.GetTypes().Where(w => w.IsSubclassOf(typeof(EventLogModelBase))))
            {
                context.SchemaGenerator.GenerateSchema(i, context.SchemaRepository);
            }
        }
    }
}
