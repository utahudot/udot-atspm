using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Route distance oData configuration
    /// </summary>
    public class RouteDistanceOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<RouteDistance>("RouteDistance").EntityType;
            model.Page(default, default);

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Distance).IsRequired();

                        var c = model.Collection.Function("GetRouteDistanceByLocationIdentifiers");
                        c.Parameter<string>("locationA");
                        c.Parameter<string>("locationB");
                        c.ReturnsFromEntitySet<RouteDistance>("RouteDistance");

                        break;
                    }
            }
        }
    }
}
