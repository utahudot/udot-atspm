using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

public class SignalModelConfiguration : IModelConfiguration
{
    /// <inheritdoc />
    public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
    {
        //var actionLog = builder.EntitySet<ActionLog>("ActionLog").EntityType.HasKey(p => p.Id);

        ////var agency = builder.EntitySet<Agency>("Agency").EntityType.HasKey(p => p.Id);

        //var area = builder.EntitySet<Area>("Area").EntityType.HasKey(p => p.Id);

        //var approach = builder.EntitySet<Approach>("Approach").EntityType.HasKey(p => p.Id);

        //var region = builder.EntitySet<Region>("Region").EntityType;
        //region.HasKey(p => p.Id);
        //region.Property(p => p.Description);
        //region.Ignore(p => p.Signals);


        //Console.WriteLine($"-------------------signal: {apiVersion}");

        //var signal = builder.EntitySet<Signal>("Signal").EntityType;
        //signal.HasKey(p => p.Id);

        //signal.Action("Promote").Parameter<string>("title");

        ////var func = signal.Function("Test");
        ////func.Parameter<string>("hello");
        ////func.ReturnsCollectionFromEntitySet<Signal>("Signal");

        //signal.Function("Test").ReturnsFromEntitySet<Signal>("Signal");

        ////var function = signal.Collection.Function("NewHires");

        ////function.Parameter<DateTime>("Since");
        ////function.ReturnsFromEntitySet<Signal>("Signal");

        ////signal.HasOptional(p => p.ControllerType);
        ////signal.HasOptional(p => p.Jurisdiction);
        ////signal.HasOptional(p => p.Region);
        ////signal.HasOptional(p => p.VersionAction);

        ////signal.Property(p => p.Latitude).IsRequired();
        ////signal.Property(p => p.Longitude).IsRequired();
        ////signal.Property(p => p.Note).IsRequired();
        ////signal.Property(p => p.PrimaryName).MaxLength = 100;
        ////signal.Property(p => p.PrimaryName).IsRequired();
        ////signal.Property(p => p.SecondaryName).MaxLength = 100;
        ////signal.Property(p => p.SecondaryName).IsRequired();
        ////signal.ComplexProperty(p => p.Ipaddress);
        ////signal.Property(p => p.SignalId).MaxLength = 10;
        ////signal.Property(p => p.SignalId).IsRequired();
        ////signal.Property(p => p.RegionId);
        ////signal.Property(p => p.ControllerTypeId);
        ////signal.Property(p => p.ChartEnabled);
        ////signal.Property(p => p.LoggingEnabled);
        ////signal.Property(p => p.VersionActionId);
        ////signal.Property(p => p.Note);
        ////signal.Property(p => p.Start);
        ////signal.Property(p => p.JurisdictionId);
        ////signal.Property(p => p.Pedsare1to1);

        ////signal.Ignore(p => p.Latitude);
        ////signal.Ignore(p => p.Longitude);
        ////signal.Ignore(p => p.Note);
        ////signal.Ignore(p => p.PrimaryName);
        ////signal.Ignore(p => p.SecondaryName);
        ////signal.Ignore(p => p.Ipaddress);
        ////signal.Ignore(p => p.SignalId);
        ////signal.Ignore(p => p.RegionId);
        ////signal.Ignore(p => p.ControllerTypeId);
        ////signal.Ignore(p => p.ChartEnabled);
        ////signal.Ignore(p => p.LoggingEnabled);
        ////signal.Ignore(p => p.VersionActionId);
        ////signal.Ignore(p => p.Note);
        ////signal.Ignore(p => p.Start);
        ////signal.Ignore(p => p.JurisdictionId);
        ////signal.Ignore(p => p.Pedsare1to1);



        ////signal.HasMany(p => p.Areas);


        ////signal.Ignore(p => p.ControllerType);
        ////signal.Ignore(p => p.Jurisdiction);
        ////signal.Ignore(p => p.Region);
        ////signal.Ignore(p => p.VersionAction);

        ////signal.Ignore(p => p.Approaches);
        ////signal.Ignore(p => p.Areas);

        ////var movementType = builder.EntitySet<MovementType>("MovementType").EntityType;
        ////movementType.HasKey(p => p.Id);
    }
}