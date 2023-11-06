using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Detector comments controller
    /// </summary>
    [ApiVersion(1.0)]
    public class DetectorCommentController : AtspmConfigControllerBase<DetectorComment, int>
    {
        private readonly IDetectorCommentRepository _repository;

        /// <inheritdoc/>
        public DetectorCommentController(IDetectorCommentRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        #endregion

        #region Actions

        #endregion

        #region Functions

        #endregion
    }
}
