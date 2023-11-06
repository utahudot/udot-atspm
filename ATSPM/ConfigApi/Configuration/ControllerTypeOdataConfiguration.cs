using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Controller type oData configuration
    /// </summary>
    public class ControllerTypeOdataConfiguration : IModelConfiguration
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
                        model.Property(p => p.Product).IsRequired();

                        model.Property(p => p.Product).MaxLength = 50;
                        model.Property(p => p.Product).MaxLength = 32;
                        model.Property(p => p.Directory).MaxLength = 1024;
                        model.Property(p => p.SearchTerm).MaxLength = 128;
                        model.Property(p => p.UserName).MaxLength = 50;
                        model.Property(p => p.Password).MaxLength = 50;

                        break;
                    }
            }
        }
    }
}
