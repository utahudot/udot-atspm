using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

namespace ATSPM.ConfigApi.Configuration
{
    public class FaqsModelConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        var model = builder.EntitySet<Faq>("Faq").EntityType.HasKey(p => p.Id);

                        model.Property(p => p.Header).IsRequired();
                        model.Property(p => p.Body).IsRequired();

                        break;
                    }
            }
        }
    }
}
