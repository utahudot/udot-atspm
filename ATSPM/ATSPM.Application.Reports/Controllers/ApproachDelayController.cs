using ATSPM.Application.Reports.ViewModels.ApproachDelay;
using ATSPM.Application.Reports.ViewModels.ApproachVolume;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApproachDelayController : ControllerBase
    {
        [HttpGet("test")]
        public ApproachDelayChart Test()
        {
            Fixture fixture = new();
            ApproachDelayChart viewModel = fixture.Create<ApproachDelayChart>();
            return viewModel;
        }

    }
}
