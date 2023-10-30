using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    public class MenuModelConfiguration : IModelConfiguration
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
                        //model.Property(p => p.Action).IsRequired();
                        //model.Property(p => p.Action).MaxLength = 50;

                        //model.Property(p => p.Application).IsRequired();
                        //model.Property(p => p.Application).MaxLength = 50;

                        //model.Property(p => p.Controller).IsRequired();
                        //model.Property(p => p.Controller).MaxLength = 50;

                        model.Property(p => p.Name).IsRequired();
                        model.Property(p => p.Name).MaxLength = 50;

                        break;
                    }
            }
        }
    }
}
