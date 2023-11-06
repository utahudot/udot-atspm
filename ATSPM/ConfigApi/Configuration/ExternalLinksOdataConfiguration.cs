using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// External links oData configuration
    /// </summary>
    public class ExternalLinksOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<ExternalLink>("ExternalLink")
                .EntityType
                .Page(default, default);

            switch (apiVersion.MajorVersion)
            {
                default:
                    {
                        model.Property(p => p.Name).IsRequired();
                        model.Property(p => p.Url).IsRequired();

                        model.Property(p => p.Name).MaxLength = 64;
                        model.Property(p => p.Url).MaxLength = 512;

                        break;
                    }
            }
        }
    }
}
