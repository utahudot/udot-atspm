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
using Asp.Versioning.OData;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.ConfigApi.Models;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.ConfigurationModels;
using Utah.Udot.NetStandardToolkit.Services;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace Utah.Udot.Atspm.ConfigApi.Controllers
{


    [ApiVersion(1.0)]
    public class UtilityController : ODataController
    {
        [ProducesResponseType(typeof(GitHubReleaseDto), Status200OK)]
        [HttpGet("api/v{version:apiVersion}/GetCurrentVersion(PreRelease={preRelease})")]
        public async Task<IActionResult> GetCurrentVersion(bool preRelease = false)
        {
            var assembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(a =>
                    string.Equals(a.GetName().Name, "Utah.Udot.Atspm", StringComparison.OrdinalIgnoreCase));

            var info = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var fullVersion = info?.InformationalVersion;

            var tag = fullVersion?.Split('+')[0];
            if (!tag.StartsWith("v"))
                tag = "v" + tag;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ATSPM-Version-Checker");

            // Get the release for this version
            var releaseJson = await client.GetStringAsync(
                $"https://api.github.com/repos/utahudot/udot-atspm/releases/tags/{tag}");

            var release = JsonConvert.DeserializeObject<GitHubReleaseDto>(releaseJson);

            // Get the latest release (pre-release or stable)
            var latestUrl = preRelease
                ? "https://api.github.com/repos/utahudot/udot-atspm/releases"
                : "https://api.github.com/repos/utahudot/udot-atspm/releases/latest";

            var latestJson = await client.GetStringAsync(latestUrl);
            var latest = JsonConvert.DeserializeObject<GitHubReleaseDto>(latestJson);

            release.IsLatest = string.Equals(
                latest.TagName,
                release.TagName,
                StringComparison.OrdinalIgnoreCase);

            return Ok(release);
        }

        [ProducesResponseType(typeof(GitHubReleaseDto), Status200OK)]
        [HttpGet("api/v{version:apiVersion}/GetLatestRelease(PreRelease={preRelease})")]
        public async Task<IActionResult> GetLatestRelease(bool preRelease = false)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ATSPM-Version-Checker");

            //
            // 1. If prerelease = false → return GitHub’s official latest stable release
            //
            if (!preRelease)
            {
                var latestJson = await client.GetStringAsync(
                    "https://api.github.com/repos/utahudot/udot-atspm/releases/latest");

                var latest = JsonConvert.DeserializeObject<GitHubReleaseDto>(latestJson);
                return Ok(latest);
            }

            //
            // 2. prerelease = true → include prereleases when determining the newest release
            //    GitHub returns releases sorted newest → oldest
            //
            var allJson = await client.GetStringAsync(
                "https://api.github.com/repos/utahudot/udot-atspm/releases");

            dynamic releases = JsonConvert.DeserializeObject(allJson);

            // The first item is ALWAYS the most recent release (stable OR prerelease)
            var newest = releases[0];

            var dto = JsonConvert.DeserializeObject<GitHubReleaseDto>(
                JsonConvert.SerializeObject(newest));

            return Ok(dto);
        }

        [ProducesResponseType(typeof(IEnumerable<GitHubReleaseHistoryDto>), Status200OK)]
        [HttpGet("api/v{version:apiVersion}/GetVersionHistory(PreRelease={preRelease})")]
        public async Task<IActionResult> GetVersionHistory(bool preRelease = false)
        {
            const string url = "https://api.github.com/repos/utahudot/udot-atspm/releases";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ATSPM-Version-Checker");

                var json = await client.GetStringAsync(url);
                dynamic releases = JsonConvert.DeserializeObject(json);

                var list = new List<GitHubReleaseHistoryDto>();

                foreach (var r in releases)
                {
                    // Skip prereleases unless explicitly requested
                    if (!preRelease && r.prerelease == true)
                        continue;

                    var dto = new GitHubReleaseHistoryDto
                    {
                        Id = r.id,
                        Url = r.url,
                        HtmlUrl = r.html_url,
                        TagName = r.tag_name,
                        Name = r.name,
                        Body = r.body,
                        TargetCommitish = r.target_commitish,
                        Draft = r.draft,
                        Prerelease = r.prerelease,
                        CreatedAt = r.created_at,
                        PublishedAt = r.published_at,
                        TarballUrl = r.tarball_url,
                        ZipballUrl = r.zipball_url,
                        Author = new GitHubAuthorDto
                        {
                            Login = r.author.login,
                            Id = r.author.id,
                            AvatarUrl = r.author.avatar_url,
                            HtmlUrl = r.author.html_url
                        },
                        Assets = new List<GitHubAssetDto>()
                    };

                    foreach (var a in r.assets)
                    {
                        dto.Assets.Add(new GitHubAssetDto
                        {
                            Name = a.name,
                            BrowserDownloadUrl = a.browser_download_url,
                            Size = a.size,
                            CreatedAt = a.created_at,
                            UpdatedAt = a.updated_at
                        });
                    }

                    list.Add(dto);
                }

                return Ok(list);
            }
        }

    }



    public class GitHubReleaseDto
    {
        public string Url { get; set; }
        public string AssetsUrl { get; set; }
        public string UploadUrl { get; set; }
        public string HtmlUrl { get; set; }
        public long Id { get; set; }
        public GitHubUserDto Author { get; set; }
        public string NodeId { get; set; }
        public string TagName { get; set; }
        public string TargetCommitish { get; set; }
        public string Name { get; set; }
        public bool Draft { get; set; }
        public bool Immutable { get; set; }
        public bool Prerelease { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
        public DateTime Published_At { get; set; }
        public List<GitHubAssetDto> Assets { get; set; }
        public string TarballUrl { get; set; }
        public string ZipballUrl { get; set; }
        public string Body { get; set; }
        public bool IsLatest { get; set; }
    }

    public class GitHubUserDto
    {
        public string Login { get; set; }
        public long Id { get; set; }
        public string Node_Id { get; set; }
        public string Avatar_Url { get; set; }
        public string Html_Url { get; set; }
        public string Url { get; set; }
    }

    public class GitHubAssetDto
    {
        public string Name { get; set; }
        public string BrowserDownloadUrl { get; set; }
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }








    public class GitHubReleaseHistoryDto
    {
        public long Id { get; set; }
        public string Url { get; set; }
        public string HtmlUrl { get; set; }
        public string TagName { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
        public string TargetCommitish { get; set; }
        public bool Draft { get; set; }
        public bool Prerelease { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PublishedAt { get; set; }
        public string TarballUrl { get; set; }
        public string ZipballUrl { get; set; }
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





    
}

