﻿using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Controllers
{
    [ApiVersion(1.0)]
    public class MeasuresDefaultController : AtspmConfigControllerBase<MeasuresDefault, int>
    {
        private readonly IMeasuresDefaultsRepository _repository;

        public MeasuresDefaultController(IMeasuresDefaultsRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}