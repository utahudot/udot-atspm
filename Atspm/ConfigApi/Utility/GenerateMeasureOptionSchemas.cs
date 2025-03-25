using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;

namespace Utah.Udot.ATSPM.ConfigApi.Utility
{
    public class GenerateMeasureOptionSchemas : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var i in typeof(MeasureOptionsBase).Assembly.GetTypes().Where(w => w.IsSubclassOf(typeof(MeasureOptionsBase))))
            {
                context.SchemaGenerator.GenerateSchema(i, context.SchemaRepository);
            }
        }
    }
}
