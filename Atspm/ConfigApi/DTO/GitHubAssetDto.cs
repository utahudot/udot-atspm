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
