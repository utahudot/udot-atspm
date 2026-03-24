using AutoMapper;
using Utah.Udot.ATSPM.ConfigApi.DTO;
using Utah.Udot.NetStandardToolkit.Services.GitHubReleaseService;

namespace Utah.Udot.ATSPM.ConfigApi.Mappings
{
    /// <summary>
    /// Defines the AutoMapper configurations for GitHub release entities.
    /// </summary>
    public class VersionMappingProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionMappingProfile"/> class.
        /// </summary>
        public VersionMappingProfile()
        {
            CreateMap<GitHubAuthor, GitHubAuthorDto>();

            CreateMap<GitHubAsset, GitHubAssetDto>();

            CreateMap<GitHubRelease, GitHubReleaseDto>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author))
                .ForMember(dest => dest.Assets, opt => opt.MapFrom(src => src.Assets));
        }
    }
}
