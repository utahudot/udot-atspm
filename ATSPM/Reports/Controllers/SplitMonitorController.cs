﻿using ATSPM.Application.Reports.Business.SplitMonitor;
using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SplitMonitorController : ControllerBase
    {
        private readonly SplitMonitorService splitMonitorService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;

        public SplitMonitorController(
            SplitMonitorService splitMonitorService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository)
        {
            this.splitMonitorService = splitMonitorService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
        }

        // GET: api/<ApproachVolumeController>
        [HttpGet("test")]
        public SplitMonitorResult Test()
        {
            Fixture fixture = new();
            SplitMonitorResult viewModel = fixture.Create<SplitMonitorResult>();
            return viewModel;
        }

        [HttpPost("getChartData")]
        public List<SplitMonitorResult> GetChartData([FromBody] SplitMonitorOptions options)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(options.SignalId, options.Start);
            var signalEvents = controllerEventLogRepository.GetSignalEventsBetweenDates(options.SignalId, options.Start.AddHours(-12), options.End.AddHours(12));
            var planEvents = signalEvents.Where(e => e.EventCode == 131).ToList();
            var pedEvents = signalEvents.Where(e =>
                new List<int> { 21, 23 }.Contains(e.EventCode)
                && e.Timestamp >= options.Start
                && e.Timestamp <= options.End).ToList();
            var cycleEvents = signalEvents.Where(e =>
                new List<int> { 1, 4, 5, 6, 7, 8, 11 }.Contains(e.EventCode)
                && e.Timestamp >= options.Start
                && e.Timestamp <= options.End).ToList();
            var splitsEventCodes = new List<int>();
            for (var i = 130; i <= 151; i++)
                splitsEventCodes.Add(i);
            var splitsEvents = signalEvents.Where(e =>
                splitsEventCodes.Contains(e.EventCode)
                && e.Timestamp >= options.Start
                && e.Timestamp <= options.End).ToList();
            signalEvents = null;

            List<SplitMonitorResult> viewModel = splitMonitorService.GetChartData(
               options,
               planEvents,
               cycleEvents,
               pedEvents,
               splitsEvents,
               signal);
            return viewModel;
        }

    }
}