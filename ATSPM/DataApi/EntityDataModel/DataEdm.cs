using ATSPM.Data.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Net;

namespace ATSPM.DataApi.EntityDataModel
{
    public class DataEdm
    {
        public IEdmModel GetEntityDataModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.Namespace = "AtspmData";
            builder.ContainerName = "AtspmDataContainer";

            var ip = builder.ComplexType<IPAddress>();
            ip.Property(i => i.Address);
            ip.Ignore(i => i.AddressFamily);
            ip.Ignore(i => i.IsIPv4MappedToIPv6);
            ip.Ignore(i => i.IsIPv6LinkLocal);
            ip.Ignore(i => i.IsIPv6Multicast);
            ip.Ignore(i => i.IsIPv6Teredo);
            ip.Ignore(i => i.IsIPv6UniqueLocal);
            ip.Ignore(i => i.ScopeId);

            var controllerType = builder.EntityType<ControllerType>();
            controllerType.HasKey(k => k.Id);

            var signal = builder.EntityType<Signal>();
            //signal.HasKey(k => k.Id);
            signal.Property(p => p.Latitude).MaxLength = 30;
            signal.Property(p => p.Latitude).IsRequired();
            signal.Property(p => p.Longitude).MaxLength = 30;
            signal.Property(p => p.Longitude).IsRequired();
            signal.Property(p => p.Note).IsRequired();
            signal.Property(p => p.PrimaryName).MaxLength = 100;
            signal.Property(p => p.PrimaryName).IsRequired();
            signal.Property(p => p.SecondaryName).MaxLength = 100;
            signal.Property(p => p.SecondaryName).IsRequired();
            signal.Property(p => p.SignalId).MaxLength = 10;
            signal.Property(p => p.SignalId).IsRequired();
            signal.ComplexProperty(p => p.Ipaddress);
            signal.Namespace = "SignalNamespace.Stuff";

            var func = builder.Function("GetTime");
            func.Returns<string>();
            func.Namespace = "AtspmData.Test";

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
            builder.EntitySet<Signal>("Signals");
            //builder.EntitySet<VersionAction>("VersionActions");

            return builder.GetEdmModel();
        }

        public IEdmModel GetExplicitEdmModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            //EnumTypeConfiguration<Color> color = builder.EnumType<Color>();
            //color.Member(Color.Red);
            //color.Member(Color.Blue);
            //color.Member(Color.Green);
            //color.Member(Color.Yellow);

            ComplexTypeConfiguration<IPAddress> Ipaddress = builder.ComplexType<IPAddress>();
            //point.Property(c => c.X);
            //point.Property(c => c.Y);

            //ComplexTypeConfiguration<Shape> shape = builder.ComplexType<Shape>();
            //shape.EnumProperty(c => c.Color);
            //shape.Property(c => c.HasBorder);
            //shape.Abstract();

            //ComplexTypeConfiguration<Triangle> triangle = builder.ComplexType<Triangle>();
            //triangle.ComplexProperty(c => c.P1);
            //triangle.ComplexProperty(c => c.P2);
            //triangle.ComplexProperty(c => c.P2);
            //triangle.DerivesFrom<Shape>();

            //ComplexTypeConfiguration<Rectangle> rectangle = builder.ComplexType<Rectangle>();
            //rectangle.ComplexProperty(c => c.LeftTop);
            //rectangle.Property(c => c.Height);
            //rectangle.Property(c => c.Weight);
            //rectangle.DerivesFrom<Shape>();

            //ComplexTypeConfiguration<RoundRectangle> roundRectangle = builder.ComplexType<RoundRectangle>();
            //roundRectangle.Property(c => c.Round);
            //roundRectangle.DerivesFrom<Rectangle>();

            //ComplexTypeConfiguration<Circle> circle = builder.ComplexType<Circle>();
            //circle.ComplexProperty(c => c.Center);
            //circle.Property(c => c.Radius);
            //circle.DerivesFrom<Shape>();

            EntityTypeConfiguration<Signal> signal = builder.EntityType<Signal>();
            signal.HasKey(c => c.Id);
            signal.Property(c => c.SignalId);
            signal.Property(c => c.Latitude);
            signal.Property(c => c.Longitude);
            signal.Property(c => c.PrimaryName);
            signal.Property(c => c.SecondaryName);
            signal.ComplexProperty(c => c.Ipaddress);
            signal.Property(c => c.RegionId);
            signal.Property(c => c.ControllerTypeId);
            signal.Property(c => c.Enabled);
            //signal.Property(c => c.SignaVersionActions VersionActionId);
            signal.Property(c => c.Note);
            signal.Property(c => c.Start);
            signal.Property(c => c.JurisdictionId);
            signal.Property(c => c.Pedsare1to1);

            EntityTypeConfiguration<Faq> faq = builder.EntityType<Faq>();
            faq.HasKey(c => c.Id);

            builder.EntitySet<Signal>("Signals");

            builder.EntitySet<Faq>("Faqs");


            return builder.GetEdmModel();
        }
    }

}
