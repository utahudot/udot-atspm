using ATSPM.Application.Reports.Approach;
using ATSPM.Application.Reports.Business;
using ATSPM.Application.Reports.ViewModels.ApproachDelay;
using ATSPM.Application.Reports.ViewModels.ApproachVolume;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApproachDelayController : ControllerBase
    {
        [HttpGet("test")]
        public ApproachDelayResult Test()
        {
            Fixture fixture = new();
            ApproachDelayResult viewModel = fixture.Create<ApproachDelayResult>();
            return viewModel;
        }

        //[HttpPost("chart")]
        //public ApproachDelayChart Chart( )
        //{
        //  return ApproachService.GetApproachDelayChart()
        //}

    }
}
