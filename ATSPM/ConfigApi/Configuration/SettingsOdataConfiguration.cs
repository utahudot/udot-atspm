using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.ConfigApi.Models;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;
using System.Net;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Settings oData configuration
    /// </summary>
    public class SettingsOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<Settings>("Settings").EntityType;
            model.Page(default, default);

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Setting).IsRequired();
                        model.Property(p => p.Value).IsRequired();

                        model.Property(p => p.Setting).MaxLength = 32;
                        model.Property(p => p.Value).MaxLength = 128;

                        var a = model.Collection.Function("LookupSetting");
                        a.Parameter<string>("setting");
                        a.Returns<string>();

                        break;
                    }
            }
        }
    }
}
