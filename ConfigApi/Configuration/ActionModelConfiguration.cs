using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

public class ActionModelConfiguration : IModelConfiguration
{
    /// <inheritdoc />
    public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
    {
        var model = builder.EntitySet<ActionLog>("ActionLog")
            .EntityType
            .HasKey(p => p.Id)
            .Page(default, default);

        //var test = builder.EnumType<AgencyTypes>().Member(AgencyTypes.MPO);

        switch (apiVersion.MajorVersion)
        {
            case 1:
                {
                    model.Property(p => p.Comment).MaxLength = 255;
                    model.Property(p => p.Name).MaxLength = 100;
                    model.Property(p => p.SignalId).MaxLength = 10;
                    //model.EnumProperty(p => p.AgencyId);

                    break;
                }
        }
    }
}
