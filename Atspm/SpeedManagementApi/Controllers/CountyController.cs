﻿using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.Common;
using Microsoft.AspNetCore.Mvc;

namespace SpeedManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CountyController : SpeedConfigBaseController<NameAndIdDto, Guid>
    {
        public CountyController(ICountyRepository repository) : base(repository)
        {
        }
    }
}