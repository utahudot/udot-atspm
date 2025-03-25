using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;

namespace Utah.Udot.ATSPM.ConfigApi.Utility
{
    /// <summary>
    /// A document filter that generates schemas for all subclasses of <see cref="MeasureOptionsBase"/>.
    /// </summary>
    public class GenerateMeasureOptionSchemas : IDocumentFilter
    {
        /// <summary>
        /// Applies the document filter to generate schemas for all subclasses of <see cref="MeasureOptionsBase"/>.
        /// </summary>
        /// <param name="swaggerDoc">The OpenAPI document.</param>
        /// <param name="context">The document filter context.</param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var i in typeof(MeasureOptionsBase).Assembly.GetTypes().Where(w => w.IsSubclassOf(typeof(MeasureOptionsBase))))
            {
                context.SchemaGenerator.GenerateSchema(i, context.SchemaRepository);
            }
        }
    }
}
