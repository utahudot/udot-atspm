using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Detection type oData configuration
    /// </summary>
    public class DetectionTypeOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<DetectionType>("DetectionType")
                .EntityType
                .Page(default, default);

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Description).IsRequired();

                        model.Property(p => p.Abbreviation).MaxLength = 5;
                        model.Property(p => p.Description).MaxLength = 128;

                        break;
                    }
            }
        }
    }
}
