using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Net;

namespace ATSPM.ConfigApi.Configuration
{
    /// <summary>
    /// Device oData configuration
    /// </summary>
    public class DeviceOdataConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<Device>("Device").EntityType;
            model.Page(default, default);
            model.Expand(2, SelectExpandType.Automatic, new string[] { "location", "deviceConfiguration", "product" });

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.Property(p => p.Ipaddress).IsRequired();
                        model.Property(p => p.Ipaddress).MaxLength = 15;

                        model.Property(p => p.Notes).MaxLength = 512;

                        var c = model.Collection.Function("GetActiveDevicesByLocation");
                        c.Parameter<int>("locationId");
                        c.ReturnsFromEntitySet<Device>("Devices");

                        break;
                    }
            }
        }
    }
}