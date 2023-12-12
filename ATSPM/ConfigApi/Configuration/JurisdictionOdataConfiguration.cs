using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Jurisdiction oData configuration
    /// </summary>
    public class JurisdictionOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<Jurisdiction>("Jurisdiction")
                .EntityType
                .Page(default, default);

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.CountyParish).MaxLength = 50;
                        model.Property(p => p.Name).MaxLength = 50;
                        model.Property(p => p.Mpo).MaxLength = 50;
                        model.Property(p => p.OtherPartners).MaxLength = 50;

                        //model.HasMany(m => m.Locations);

                        break;
                    }
            }
        }
    }
}
