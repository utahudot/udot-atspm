using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Device configuration oData configuration
    /// </summary>
    public class DeviceConfigurationOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<DeviceConfiguration>("DeviceConfiguration").EntityType;
            model.Page(default, default);
            model.Expand(1, SelectExpandType.Automatic, new string[] { "product" });

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Firmware).IsRequired();

                        model.Property(p => p.Firmware).MaxLength = 16;
                        model.Property(p => p.Notes).MaxLength = 512;
                        model.Property(p => p.Directory).MaxLength = 1024;
                        model.Property(p => p.UserName).MaxLength = 50;
                        model.Property(p => p.Password).MaxLength = 50;

                        break;
                    }
            }
        }
    }
}
