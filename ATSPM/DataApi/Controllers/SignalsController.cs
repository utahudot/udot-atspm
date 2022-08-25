using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace ATSPM.DataApi.Controllers
{
    public class SignalsController : ATSPMDataControllerBase<Signal>
    {
        private readonly ConfigContext _configContext;
        private readonly ISignalRepository _repository;

        public SignalsController(ConfigContext configContext, ISignalRepository repository) : base(configContext, repository)
        {
            _configContext = configContext;
            _repository = repository;
        }
    }
}
