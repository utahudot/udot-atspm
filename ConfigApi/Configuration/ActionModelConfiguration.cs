using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Data.Models;
using Microsoft.OData.ModelBuilder;

public class ActionModelConfiguration : IModelConfiguration
{
    /// <inheritdoc />
    public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
    {
        //var actionLog = builder.EntitySet<ActionLog>("ActionLogs").EntityType;
        //actionLog.HasKey(p => p.Id);

        //var action = builder.EntitySet<ATSPM.Data.Models.Action>("Actions").EntityType;
        //action.HasKey(p => p.Id);

        //Console.WriteLine($"-------------------stuf: {apiVersion}");

        //var signal = builder.EntitySet<Signal>("Signals").EntityType;
        //signal.HasKey(p => p.Id);

        //var applicationSetting = builder.EntitySet<ApplicationSetting>("ApplicationSettings").EntityType;
        //action.HasKey(p => p.Id);

        //if (apiVersion < ApiVersions.V3)
        //{
        //    person.Ignore(p => p.Phone);
        //}

        //if (apiVersion <= ApiVersions.V1)
        //{
        //    person.Ignore(p => p.HomeAddress);
        //    person.Ignore(p => p.WorkAddress);
        //    person.Ignore(p => p.Email);
        //}

        //if (apiVersion == ApiVersions.V1)
        //{
        //    person.Function("MostExpensive").ReturnsFromEntitySet<Person>("People");
        //    person.Collection.Function("MostExpensive").ReturnsFromEntitySet<Person>("People");
        //}

        //if (apiVersion > ApiVersions.V1)
        //{
        //    person.ContainsOptional(p => p.HomeAddress);
        //    person.Ignore(p => p.WorkAddress);

        //    var function = person.Collection.Function("NewHires");

        //    function.Parameter<DateTime>("Since");
        //    function.ReturnsFromEntitySet<Person>("People");
        //}

        //if (apiVersion > ApiVersions.V2)
        //{
        //    person.ContainsOptional(p => p.WorkAddress);
        //    person.Action("Promote").Parameter<string>("title");
        //}
    }
}
