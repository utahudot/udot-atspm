using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Frequently asked questions oData configuration
    /// </summary>
    public class FaqsOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<Faq>("Faq")
                .EntityType
                .Page(default, default);

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Header).IsRequired();
                        model.Property(p => p.Body).IsRequired();

                        model.Property(p => p.Header).MaxLength = 256;
                        model.Property(p => p.Body).MaxLength = 8000;

                        break;
                    }
            }
        }
    }
}
