using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;
using System.Net;

namespace ATSPM.ConfigApi.Configuration
{
    public class SignalModelConfiguration : IModelConfiguration
    {
        ///<inheritdoc/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            var model = builder.EntitySet<Signal>("Signal")
                .EntityType
                .HasKey(p => p.Id);
            model.Page(default, default);


            var ip = builder.ComplexType<IPAddress>();
            ip.Property(i => i.Address);
            ip.Ignore(i => i.AddressFamily);
            ip.Ignore(i => i.IsIPv4MappedToIPv6);
            ip.Ignore(i => i.IsIPv6LinkLocal);
            ip.Ignore(i => i.IsIPv6Multicast);
            ip.Ignore(i => i.IsIPv6Teredo);
            ip.Ignore(i => i.IsIPv6UniqueLocal);
            ip.Ignore(i => i.ScopeId);

            switch (apiVersion.MajorVersion)
            {
                case 1:
                    {
                        model.ComplexProperty(p => p.Ipaddress).IsRequired();
                        model.Property(p => p.Latitude).IsRequired();
                        model.Property(p => p.Longitude).IsRequired();
                        model.Property(p => p.Note).IsRequired();
                        model.Property(p => p.PrimaryName).IsRequired();
                        model.Property(p => p.SignalId).IsRequired();

                        model.Property(p => p.PrimaryName).MaxLength = 100;
                        model.Property(p => p.SecondaryName).MaxLength = 100;
                        model.Property(p => p.SignalId).MaxLength = 10;

                        model.Property(p => p.JurisdictionId).DefaultValueString = "0";
                        model.EnumProperty(p => p.VersionActionId).DefaultValueString = "10";
                        model.Property(p => p.Note).DefaultValueString = "Initial";

                        model.Collection.Function("GetLatestVersionOfAllSignals").ReturnsFromEntitySet<Signal>("Signal");

                        model.Collection.Function("GetLatestVersionOfSignal").ReturnsFromEntitySet<Signal>("Signal");

                        break;
                    }
            }
        }
    }
}
