using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    public class DetectorModelConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<Detector>("Detector")
                .EntityType
                .Page(default, default)
                .Expand(1, SelectExpandType.Automatic, new string[] { "approach", "detectionHardware", "laneType", "movementType" });

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.DectectorIdentifier).MaxLength = 50;
                        model.HasOptional(p => p.Approach);
                        model.HasOptional(p => p.DetectionHardware);
                        model.HasOptional(p => p.LaneType);
                        model.HasOptional(p => p.MovementType);

                        break;
                    }
            }
        }
    }
}
