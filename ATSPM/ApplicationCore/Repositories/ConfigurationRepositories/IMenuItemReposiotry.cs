﻿using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories.ConfigurationRepositories
{
    /// <summary>
    /// Menu item repository
    /// </summary>
    public interface IMenuItemReposiotry : IAsyncRepository<MenuItem>
    {
    }
}