#region license
// Copyright 2025 Utah Departement of Transportation
// for ReportApi - Utah.Udot.ATSPM.ReportApi.Controllers/WatchDogDashboardController.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNetCore.Mvc;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.ATSPM.ReportApi.ReportServices;

namespace Utah.Udot.ATSPM.ReportApi.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class WatchDogDashboardController : ControllerBase
    {
        private readonly WatchDogDashboardReportService watchDogDashboardReportService;

        public WatchDogDashboardController(WatchDogDashboardReportService watchDogDashboardReportService)
        {
            this.watchDogDashboardReportService = watchDogDashboardReportService;
        }

        [HttpPost("getDashboardGroup")]
        //[Produces("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<WatchDogIssueTypeGroup> GetDashboardGroup([FromBody] WatchDogDashboardOptions options)
        {
            try
            {
                var result = watchDogDashboardReportService.GetDashboardGroup(options);

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
