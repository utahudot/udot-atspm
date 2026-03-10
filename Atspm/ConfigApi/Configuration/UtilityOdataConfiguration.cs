using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.OData.ModelBuilder;
using Utah.Udot.Atspm.ConfigApi.Controllers;

namespace Utah.Udot.ATSPM.ConfigApi.Configuration
{
    public class UtilityOdataConfiguration : IModelConfiguration
    {
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string? routePrefix)
        {
            builder.Function("GetCurrentVersion").Returns<GitHubReleaseDto>().Parameter<bool>("PreRelease");
            builder.Function("GetVersionHistory").ReturnsCollection<GitHubReleaseHistoryDto>().Parameter<bool>("PreRelease");
            builder.Function("GetLatestRelease").Returns<GitHubReleaseDto>().Parameter<bool>("PreRelease");
        }
    }
}
