﻿using ATSPM.Application.Reports.ViewModels.ApproachVolume;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApproachVolumeController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public ApproachVolumeChart Test()
        {
            Fixture fixture = new();
            ApproachVolumeChart approachVolumeViewModel = fixture.Create<ApproachVolumeChart>();
            return approachVolumeViewModel;
        }

    }
}