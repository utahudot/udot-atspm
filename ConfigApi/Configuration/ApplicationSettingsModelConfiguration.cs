using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    public class ApplicationSettingsModelConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<ApplicationSetting>("ApplicationSetting")
                .EntityType
                .HasKey(p => p.Id)
                .Page(default, default);

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Discriminator).IsRequired();
                        model.Property(p => p.Discriminator).MaxLength = 128;

                        break;
                    }
            }
        }
    }
}
