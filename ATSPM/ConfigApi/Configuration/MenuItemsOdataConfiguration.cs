using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Menu items oData configuration
    /// </summary>
    public class MenuItemsOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<MenuItem>("Menu")
                .EntityType
                .Page(default, default);

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Name).IsRequired();

                        model.Property(p => p.Name).MaxLength = 24;
                        model.Property(p => p.Icon).MaxLength = 1024;
                        //model.Property(p => p.Link).MaxLength = 512;

                        break;
                    }
            }
        }
    }
}
