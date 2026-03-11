#region license
// Copyright 2026 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Controllers/WatchdogPolicyControllerBase.cs
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
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Reflection;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{


    [ApiVersion(1.0)]
    public class UtilityController : ODataController
    {
        private readonly IGitHubReleaseService _github;
        private readonly IMapper _mapper;

        public UtilityController(IGitHubReleaseService github, IMapper mapper)
        {
            _github = github;
            _mapper = mapper;
        }

        [HttpGet("api/v{version:apiVersion}/GetCurrentVersion(PreRelease={preRelease})")]
        public async Task<IActionResult> GetCurrentVersion(bool preRelease = false)
        {
            try
            {
                var assembly = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Utah.Udot.Atspm");

                var info = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                var tag = info?.InformationalVersion?.Split('+')[0];

                if (!tag.StartsWith("v"))
                    tag = "v" + tag;

                var release = await _github.GetReleaseByTag(tag);
                var latest = await _github.GetLatestRelease(preRelease);

                release.IsLatest = string.Equals(release.TagName, latest.TagName, StringComparison.OrdinalIgnoreCase);

                return Ok(_mapper.Map<GitHubReleaseDto>(release));
            }
            catch (GitHubReleaseNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (GitHubApiException ex)
            {
                return StatusCode(502, ex.Message);
            }
        }

        [HttpGet("api/v{version:apiVersion}/GetLatestRelease(PreRelease={preRelease})")]
        public async Task<IActionResult> GetLatestRelease(bool preRelease = false)
        {
            try
            {
                var latest = await _github.GetLatestRelease(preRelease);
                return Ok(_mapper.Map<GitHubReleaseDto>(latest));
            }
            catch (GitHubApiException ex)
            {
                return StatusCode(502, ex.Message);
            }
        }

        [HttpGet("api/v{version:apiVersion}/GetVersionHistory(PreRelease={preRelease})")]
        public async Task<IActionResult> GetVersionHistory(bool preRelease = false)
        {
            try
            {
                var history = await _github.GetReleaseHistory(preRelease);
                return Ok(_mapper.Map<List<GitHubReleaseDto>>(history));
            }
            catch (GitHubApiException ex)
            {
                return StatusCode(502, ex.Message);
            }
        }
    }


    /// <summary>
    /// Represents a simplified, API‑friendly view of a GitHub release,
    /// suitable for returning to clients without exposing internal GitHub URLs.
    /// </summary>
    public class GitHubReleaseDto
    {
        /// <summary>
        /// The unique numeric identifier for the release.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The tag name associated with the release (e.g., <c>v5.2.0-rc2</c>).
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// The display name of the release.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The release notes or description text.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The public HTML URL for viewing the release on GitHub.
        /// </summary>
        public string HtmlUrl { get; set; }

        /// <summary>
        /// Indicates whether the release is a draft.
        /// </summary>
        public bool Draft { get; set; }

        /// <summary>
        /// Indicates whether the release is marked as a prerelease.
        /// </summary>
        public bool Prerelease { get; set; }

        /// <summary>
        /// Indicates whether this release is the newest release
        /// according to the server's versioning logic.
        /// </summary>
        public bool IsLatest { get; set; }

        /// <summary>
        /// The timestamp when the release was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The timestamp when the release was published.
        /// </summary>
        public DateTime PublishedAt { get; set; }

        /// <summary>
        /// Information about the GitHub user who authored the release.
        /// </summary>
        public GitHubAuthorDto Author { get; set; }

        /// <summary>
        /// A collection of assets attached to the release.
        /// </summary>
        public List<GitHubAssetDto> Assets { get; set; }
    }


    /// <summary>
    /// Represents a simplified view of a GitHub user who authored a release
    /// or uploaded an associated asset.
    /// </summary>
    public class GitHubAuthorDto
    {
        /// <summary>
        /// The GitHub username.
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// The numeric GitHub user identifier.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The URL of the user's avatar image.
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// The public HTML URL for the user's GitHub profile.
        /// </summary>
        public string HtmlUrl { get; set; }
    }


    /// <summary>
    /// Represents a downloadable asset attached to a GitHub release,
    /// simplified for API responses.
    /// </summary>
    public class GitHubAssetDto
    {
        /// <summary>
        /// The file name of the asset.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The public URL for downloading the asset.
        /// </summary>
        public string BrowserDownloadUrl { get; set; }

        /// <summary>
        /// The size of the asset in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// The timestamp when the asset was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The timestamp when the asset was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }


    //don't forget! builder.Services.AddAutoMapper(typeof(GitHubMappingProfile));


    public class GitHubMappingProfile : Profile
    {
        public GitHubMappingProfile()
        {
            CreateMap<GitHubRelease, GitHubReleaseDto>();
            CreateMap<GitHubAuthor, GitHubAuthorDto>();
            CreateMap<GitHubAsset, GitHubAssetDto>();
        }
    }



}

