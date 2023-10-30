using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.ConfigApi.Models;
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
            var model = builder.EntitySet<Signal>("Signal").EntityType;
            model.Page(default, default);
            model.Expand(1, SelectExpandType.Automatic, new string[] { "controllerType", "jurisdiction", "region" });

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

                        model.Property(p => p.Latitude).IsRequired();
                        model.Property(p => p.Longitude).IsRequired();
                        model.Property(p => p.Note).IsRequired();
                        model.Property(p => p.PrimaryName).IsRequired();
                        model.Property(p => p.SignalIdentifier).IsRequired();

                        model.Property(p => p.PrimaryName).MaxLength = 100;
                        model.Property(p => p.SecondaryName).MaxLength = 100;
                        model.Property(p => p.SignalIdentifier).MaxLength = 10;

                        model.Property(p => p.JurisdictionId).DefaultValueString = "0";
                        //model.EnumProperty(p => p.VersionAction).DefaultValueString = "10";
                        model.Property(p => p.Note).DefaultValueString = "Initial";


                        var a = model.Collection.Function("GetAllVersionsOfSignal");
                        a.Parameter<string>("identifier");
                        a.ReturnsCollectionFromEntitySet<Signal>("Signals");

                        var b = model.Collection.Function("GetLatestVersionOfAllSignals");
                        b.ReturnsCollectionFromEntitySet<Signal>("Signals");

                        var c = model.Collection.Function("GetLatestVersionOfSignal");
                        c.Parameter<string>("identifier");
                        c.ReturnsFromEntitySet<Signal>("Signal");

                        var d = model.Action("CopySignalToNewVersion");
                        d.ReturnsFromEntitySet<Signal>("Signal");

                        var e = model.Action("SetSignalToDeleted");

                        var f = model.Collection.Function("GetSignalsForSearch");
                        //f.Parameter<int>("areaId");
                        //f.Parameter<int>("regionId");
                        //f.Parameter<int>("jurisdictionId");
                        //f.Parameter<int>("metricTypeId");
                        f.ReturnsCollectionFromEntitySet<SearchSignal>("SearchSignals");

                        break;
                    }
            }
        }
    }
}
