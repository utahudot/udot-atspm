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

            //builder.EntityType<Signal>().ComplexProperty(p => p.Ipaddress);


            builder.EntitySet<Signal>("Signals");
            builder.EntitySet<Faq>("Faqs");

            //builder.EntityType<Signal>().Ignore(i => i.Ipaddress);
            //builder.EntityType<Signal>().ComplexProperty(p => p.Ipaddress);
            builder.EntityType<Signal>().Ignore(i => i.Ipaddress);

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
