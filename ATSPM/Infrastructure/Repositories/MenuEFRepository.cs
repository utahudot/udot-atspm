﻿using ATSPM.Data.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.ValueObjects;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ATSPM.Application.Specifications;
using ATSPM.Domain.Services;
using ATSPM.Data;

namespace ATSPM.Infrastructure.Repositories
{
    public class MenuEFRepository : ATSPMRepositoryEFBase<Menu>, IMenuRepository
    {
        public MenuEFRepository(ConfigContext db, ILogger<MenuEFRepository> log) : base(db, log) { }

        #region IMenuRepository

        #endregion
    }
}