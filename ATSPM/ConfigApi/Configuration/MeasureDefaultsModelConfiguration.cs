using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    public class MeasureDefaultsModelConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<MeasureOption>("MeasuresDefault")
                .EntityType
                .Page(default, default);

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Measure).MaxLength = 128;
                        model.Property(p => p.Option).MaxLength = 128;
                        model.Property(p => p.Value).MaxLength = 512;

                        break;
                    }
            }
        }
    }
}
