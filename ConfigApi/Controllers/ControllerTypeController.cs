﻿using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    [ApiVersion(1.0)]
    public class ControllerTypeController : AtspmConfigControllerBase<ControllerType, int>
    {
        private readonly IControllerTypeRepository _repository;

        public ControllerTypeController(IControllerTypeRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}