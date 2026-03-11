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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Reflection;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{


    [ApiVersion(1.0)]
    public class UtilityController : ODataController
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

            [HttpGet("api/v{version:apiVersion}/GetLatestRelease(PreRelease={preRelease})")]
            public async Task<IActionResult> GetLatestRelease(bool preRelease = false)
            {
                var latest = await _github.GetLatestRelease(preRelease);
                return Ok(_mapper.Map<GitHubReleaseDto>(latest));
            }

            [HttpGet("api/v{version:apiVersion}/GetVersionHistory(PreRelease={preRelease})")]
            public async Task<IActionResult> GetVersionHistory(bool preRelease = false)
            {
                var history = await _github.GetReleaseHistory(preRelease);
                return Ok(_mapper.Map<List<GitHubReleaseDto>>(history));
            }
        }

    }

    public class GitHubReleaseDto
    {
        public long Id { get; set; }
        public string TagName { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
        public string HtmlUrl { get; set; }
        public bool Draft { get; set; }
        public bool Prerelease { get; set; }
        public bool IsLatest { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PublishedAt { get; set; }
        public GitHubAuthorDto Author { get; set; }
        public List<GitHubAssetDto> Assets { get; set; }
    }

    public class GitHubAuthorDto
    {
        public string Login { get; set; }
        public long Id { get; set; }
        public string AvatarUrl { get; set; }
        public string HtmlUrl { get; set; }
    }

    public class GitHubAssetDto
    {
        public string Name { get; set; }
        public string BrowserDownloadUrl { get; set; }
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

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

