using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Route Location oData configuration
    /// </summary>
    public class RouteLocationOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<RouteLocation>("RouteLocation")
                .EntityType
                .Page(default, default)
                .Expand(1, SelectExpandType.Automatic, new string[] { "primaryDirection", "opposingDirection", "route" });

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.LocationIdentifier).IsRequired();

                        model.Property(p => p.LocationIdentifier).MaxLength = 10;

                        break;
                    }
            }
        }
    }
}