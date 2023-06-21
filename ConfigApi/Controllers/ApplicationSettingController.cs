using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class ApplicationSettingController : AtspmConfigControllerBase<ApplicationSetting, int>
    {
        private readonly IApplicationSettingsRepository _repository;

        public ApplicationSettingController(IApplicationSettingsRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
