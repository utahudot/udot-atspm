#region license
// Copyright 2026 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.ATSPM.ConfigApi.DTO/GitHubAuthorDto.cs
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

namespace Utah.Udot.ATSPM.ConfigApi.DTO
{
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
}
