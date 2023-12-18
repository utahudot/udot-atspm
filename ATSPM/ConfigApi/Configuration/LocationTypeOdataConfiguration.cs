using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Location type oData configuration
    /// </summary>
    public class LocationTypeOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<LocationType>("LocationType").EntityType;
            model.Page(default, default);

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Name).IsRequired();

                        model.Property(p => p.Name).MaxLength = 50;
                        model.Property(p => p.Icon).MaxLength = 1024;

                        break;
                    }
            }
        }
    }
}
