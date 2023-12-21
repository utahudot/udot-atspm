using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.ConfigApi.Models;
using ATSPM.Data.Models;
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

            var ip = builder.ComplexType<IPAddress>();
            //ip.Property(i => i.Address);
            //ip.Ignore(i => i.AddressFamily);
            //ip.Ignore(i => i.IsIPv4MappedToIPv6);
            //ip.Ignore(i => i.IsIPv6LinkLocal);
            //ip.Ignore(i => i.IsIPv6Multicast);
            //ip.Ignore(i => i.IsIPv6Teredo);
            //ip.Ignore(i => i.IsIPv6UniqueLocal);
            ip.Ignore(i => i.ScopeId);

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.ComplexProperty(p => p.Ipaddress).IsRequired();

                        model.Property(p => p.Notes).MaxLength = 10;

                        var c = model.Collection.Function("GetActiveDevicesByLocation");
                        c.Parameter<int>("locationId");
                        c.ReturnsFromEntitySet<Device>("Devices");

                        break;
                    }
            }
        }
    }
}
