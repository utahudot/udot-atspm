using ATSPM.Data.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Net;

namespace ATSPM.ConfigApi.EntityDataModel
{
    public class ConfigEdm
    {
        public IEdmModel GetEntityDataModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.Namespace = "AtspmConfig";
            builder.ContainerName = "AtspmConfigContainer";

            //var ip = builder.ComplexType<IPAddress>();
            //ip.Property(i => i.Address);
            //ip.Ignore(i => i.AddressFamily);
            //ip.Ignore(i => i.IsIPv4MappedToIPv6);
            //ip.Ignore(i => i.IsIPv6LinkLocal);
            //ip.Ignore(i => i.IsIPv6Multicast);
            //ip.Ignore(i => i.IsIPv6Teredo);
            //ip.Ignore(i => i.IsIPv6UniqueLocal);
            //ip.Ignore(i => i.ScopeId);

            //var approach = builder.EntityType<Approach>();
            ////approach.HasKey(k => k.ApproachId);

            //var controllerType = builder.EntityType<ControllerType>();
            //controllerType.HasKey(k => k.Id);

            //var detector = builder.EntityType<Detector>();
            //detector.HasKey(k => k.Id);

            var signal = builder.EntityType<Signal>();
            //signal.HasKey(k => k.Id);
            //signal.HasMany(m => m.Areas);
            //signal.Property(p => p.Latitude).MaxLength = 30;
            //signal.Property(p => p.Latitude).IsRequired();
            //signal.Property(p => p.Longitude).MaxLength = 30;
            //signal.Property(p => p.Longitude).IsRequired();
            //signal.Property(p => p.Note).IsRequired();
            //signal.Property(p => p.PrimaryName).MaxLength = 100;
            //signal.Property(p => p.PrimaryName).IsRequired();
            //signal.Property(p => p.SecondaryName).MaxLength = 100;
            //signal.Property(p => p.SecondaryName).IsRequired();
            //signal.Property(p => p.SignalId).MaxLength = 10;
            //signal.Property(p => p.SignalId).IsRequired();
            //signal.ComplexProperty(p => p.Ipaddress);

            //var func = builder.Function("GetTime");
            //func.Returns<string>();
            //func.Namespace = "AtspmData.Test";

            //var area = builder.EntityType<Area>();
            //area.HasKey(p => p.Id);

            //builder.EntitySet<Data.Models.Action>("Actions");
            //builder.EntitySet<ActionLog>("ActionLogs");
            //builder.EntitySet<Agency>("Agencies");
            //builder.EntitySet<Data.Models.Application>("Applications");
            //builder.EntitySet<ApplicationSetting>("ApplicationSettings");
            //builder.EntitySet<Approach>("Approaches");
            //builder.EntitySet<Area>("Areas");
            //builder.EntitySet<ControllerType>("ControllerTypes");
            //builder.EntitySet<DetectionHardware>("DetectionHardwares");
            //builder.EntitySet<DetectionType>("DetectionTypes");
            //builder.EntitySet<Detector>("Detectors");
            //builder.EntitySet<DetectorComment>("DetectorComments");
            //builder.EntitySet<DirectionType>("DirectionTypes");
            //builder.EntitySet<ExternalLink>("ExternalLinks");
            //builder.EntitySet<Faq>("Faqs");
            //builder.EntitySet<Jurisdiction>("Jurisdictions");
            //builder.EntitySet<LaneType>("LaneTypes");
            //builder.EntitySet<MeasuresDefault>("MeasuresDefaults");
            //builder.EntitySet<Menu>("Menus");
            //builder.EntitySet<MetricComment>("MetricComments");
            //builder.EntitySet<MetricType>("MetricTypes");
            //builder.EntitySet<MovementType>("MovementTypes");
            //builder.EntitySet<Region>("Regions");
            //builder.EntitySet<Data.Models.Route>("Routes");
            //builder.EntitySet<RoutePhaseDirection>("RoutePhaseDirections");
            //builder.EntitySet<RouteSignal>("RouteSignals");
            //builder.EntitySet<Signal>("Signals");
            //builder.EntitySet<VersionAction>("VersionActions");

            return builder.GetEdmModel();
        }
    }

}
