#region license
// Copyright 2026 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.ATSPM.ConfigApi.DTO/GitHubAssetDto.cs
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
}
