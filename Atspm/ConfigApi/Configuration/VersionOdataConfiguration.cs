using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.OData.ModelBuilder;
using Utah.Udot.ATSPM.ConfigApi.DTO;

namespace Utah.Udot.ATSPM.ConfigApi.Configuration
{
    /// <inheritdoc cref="IModelConfiguration"/>
    class VersionOdataConfiguration : IModelConfiguration
    {
        /// <inheritdoc cref="IModelConfiguration.Apply"/>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string? routePrefix)
        {
            builder.Function("GetCurrentVersion").Returns<GitHubReleaseDto>();
            builder.Function("GetVersionHistory").ReturnsCollection<GitHubReleaseDto>().Parameter<bool>("PreRelease");
            builder.Function("GetLatestVersion").Returns<GitHubReleaseDto>().Parameter<bool>("PreRelease");
        }
    }
}