#region license
// Copyright 2025 Utah Departement of Transportation
// for DataApi - Utah.Udot.Atspm.DataApi.Configuration/ConfigureSwaggerOptions.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

namespace Utah.Udot.Atspm.DataApi.Configuration
{
    /// <summary>
    /// Configures Swagger generation options for each discovered API version.
    /// </summary>
    /// <remarks>
    /// This class uses <see cref="IApiVersionDescriptionProvider"/> to dynamically create
    /// Swagger documents for all API versions exposed by the application. It also enriches
    /// each document with metadata from <see cref="SwaggerSettings"/>, including title,
    /// description, contact, license, and optional sunset policy information.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
    /// </remarks>
    /// <param name="provider">
    /// The <see cref="IApiVersionDescriptionProvider"/> used to enumerate all API versions.
    /// </param>
    /// <param name="settings">
    /// The bound <see cref="SwaggerSettings"/> configuration section containing metadata
    /// such as title, description, contact, and license information.
    /// </param>
    public class ConfigureSwaggerOptions(
        IApiVersionDescriptionProvider provider,
        IOptions<SwaggerSettings> settings) : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider = provider;
        private readonly SwaggerSettings _settings = settings.Value;

        /// <summary>
        /// Configures Swagger generation by registering a Swagger document for each API version.
        /// </summary>
        /// <param name="options">
        /// The <see cref="SwaggerGenOptions"/> to configure.
        /// </param>
        /// <remarks>
        /// For each API version description provided by <see cref="IApiVersionDescriptionProvider"/>,
        /// this method registers a Swagger document with metadata created by
        /// <see cref="CreateInfoForApiVersion(ApiVersionDescription)"/>.
        /// </remarks>
        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                Console.WriteLine($"GroupName: '{description.GroupName}', Version: {description.ApiVersion}");

                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }
        }

        /// <summary>
        /// Creates an <see cref="OpenApiInfo"/> instance for a given API version description.
        /// </summary>
        /// <param name="description">
        /// The <see cref="ApiVersionDescription"/> representing the API version.
        /// </param>
        /// <returns>
        /// An <see cref="OpenApiInfo"/> populated with metadata such as title, version,
        /// contact, license, and optional sunset policy information.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The description is enriched with:
        /// </para>
        /// <list type="bullet">
        ///   <item><description>Deprecation notice if the version is marked as deprecated.</description></item>
        ///   <item><description>Sunset policy details including effective date and links.</description></item>
        ///   <item><description>Clickable Markdown links for sunset policy references.</description></item>
        /// </list>
        /// </remarks>
        private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var text = new StringBuilder(_settings.Description);

            var info = new OpenApiInfo
            {
                Title = _settings.Title,
                Version = description.ApiVersion.ToString(),
                Contact = new OpenApiContact
                {
                    Name = _settings.Contact.Name,
                    Email = _settings.Contact.Email,
                    Url = new Uri(_settings.Contact.Url)
                },
                License = new OpenApiLicense
                {
                    Name = _settings.License.Name,
                    Url = new Uri(_settings.License.Url)
                }
            };

            if (description.IsDeprecated)
            {
                text.Append(" This API version has been deprecated.");
            }

            if (description.SunsetPolicy is SunsetPolicy policy)
            {
                if (policy.Date is DateTimeOffset when)
                {
                    text.Append(" The API will be sunset on ")
                        .Append(when.Date.ToShortDateString())
                        .Append('.');
                }

                if (policy.HasLinks)
                {
                    text.AppendLine();
                    foreach (var link in policy.Links)
                    {
                        if (link.Type == "text/html")
                        {
                            text.AppendLine();
                            var title = link.Title.HasValue ? link.Title.Value : "More info";
                            text.Append($"[{title}]({link.LinkTarget.OriginalString})");
                        }
                    }
                }
            }

            info.Description = text.ToString();
            return info;
        }
    }
}