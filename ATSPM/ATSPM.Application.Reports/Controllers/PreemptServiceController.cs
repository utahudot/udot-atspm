﻿using ATSPM.Application.Reports.ViewModels.PreemptService;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreemptServiceController : ControllerBase
    {
        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public PreemptServiceChart Test()
        {
            Fixture fixture = new();
            PreemptServiceChart viewModel = fixture.Create<PreemptServiceChart>();
            return viewModel;
        }

    }
}