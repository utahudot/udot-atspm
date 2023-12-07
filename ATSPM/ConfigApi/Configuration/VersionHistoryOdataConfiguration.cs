using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Version history oData configuration
    /// </summary>
    public class VersionHistoryOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<VersionHistory>("VersionHistory")
                .EntityType
                .Page(default, default)
                .Expand(2, SelectExpandType.Automatic, new string[] { "children" });

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Name).IsRequired();
                        model.Property(p => p.Date).IsRequired();
                        model.Property(p => p.Version).IsRequired();

                        model.Property(p => p.Name).MaxLength = 64;
                        model.Property(p => p.Notes).MaxLength = 512;

                        break;
                    }
            }
        }
    }
}
