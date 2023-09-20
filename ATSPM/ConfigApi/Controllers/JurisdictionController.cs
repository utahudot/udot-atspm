using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData;

namespace ATSPM.ConfigApi.Controllers
{
    [ApiVersion(1.0)]
    public class JurisdictionController : AtspmConfigControllerBase<Jurisdiction, int>
    {
        private readonly IJurisdictionRepository _repository;

        public JurisdictionController(IJurisdictionRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
