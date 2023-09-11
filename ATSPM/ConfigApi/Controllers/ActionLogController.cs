using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    public class ActionLogController : AtspmConfigControllerBase<ActionLog, int>
    {
        private readonly IActionLogRepository _repository;

        public ActionLogController(IActionLogRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
