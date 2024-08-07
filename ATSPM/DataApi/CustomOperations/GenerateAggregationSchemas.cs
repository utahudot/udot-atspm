using ATSPM.Data.Models.AggregationModels;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ATSPM.DataApi.CustomOperations
{
    public class GenerateAggregationSchemas : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var i in typeof(AggregationModelBase).Assembly.GetTypes().Where(w => w.IsSubclassOf(typeof(AggregationModelBase))))
            {
                context.SchemaGenerator.GenerateSchema(i, context.SchemaRepository);
            }
        }
    }
}
