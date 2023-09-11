using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    [ApiVersion(1.0)]
    public class ApplicationSettingController : AtspmConfigControllerBase<ApplicationSetting, int>
    {
        private readonly IApplicationSettingsRepository _repository;

        public ApplicationSettingController(IApplicationSettingsRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
