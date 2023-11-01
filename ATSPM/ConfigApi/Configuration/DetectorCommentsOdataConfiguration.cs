using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Detector comments oData configuration
    /// </summary>
    public class DetectorCommentsOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<DetectorComment>("DetectorComment")
                .EntityType
                .Page(default, default)
                .Expand(1, SelectExpandType.Automatic, new string[] { "Detector" });

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        break;
                    }
            }
        }
    }
}
