using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    public class ControllerTypeModelConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<ControllerType>("ControllerType")
                .EntityType
                .Page(default, default);

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Description).MaxLength = 50;
                        model.Property(p => p.UserName).MaxLength = 50;
                        model.Property(p => p.Password).MaxLength = 50;

                        break;
                    }
            }
        }
    }
}
