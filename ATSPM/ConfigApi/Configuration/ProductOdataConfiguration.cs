using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Product oData configuration
    /// </summary>
    public class ProductOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<Product>("Product").EntityType;
            model.Page(default, default);

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Manufacturer).IsRequired();
                        model.Property(p => p.Model).IsRequired();

                        model.Property(p => p.Manufacturer).MaxLength = 48;
                        model.Property(p => p.Model).MaxLength = 48;
                        model.Property(p => p.Notes).MaxLength = 512;

                        break;
                    }
            }
        }
    }
}
