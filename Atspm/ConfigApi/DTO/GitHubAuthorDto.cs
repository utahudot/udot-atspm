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
