#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Repositories/ControllerEventLogFileRepository.cs
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
using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Application.Specifications;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories
{
    public class ControllerEventLogFileRepository : ATSPMFileRepositoryBase<ControllerLogArchive>, IControllerEventLogRepository
    {
        public ControllerEventLogFileRepository(IFileTranscoder fileTranscoder, IOptions<FileRepositoryConfiguration> options, ILogger<ATSPMFileRepositoryBase<ControllerLogArchive>> log) : base(fileTranscoder, options, log) { }

        protected string GenerateFolderStructure(DateTime date)
        {
            var folder = new DirectoryInfo(Path.Combine(_options.Value.RootPath, typeof(ControllerLogArchive).Name, $"{date.Year}", $"{date.Month}", $"{date.Day}"));

            return Path.Combine(folder.FullName);
        }

        protected override string GenerateFileName(ControllerLogArchive item)
        {
            return $"{item.GetType().Name}_{item.SignalIdentifier}_{item.ArchiveDate.ToString("dd-MM-yyyy")}{_fileTranscoder.FileExtension}";
        }

        protected override string GenerateFilePath(ControllerLogArchive item)
        {
            if (item == null)
            {
                return base.GenerateFilePath(item);
            }

            var folder = new DirectoryInfo(Path.Combine(GenerateFolderStructure(item.ArchiveDate)));

            if (!folder.Exists)
                folder.Create();

            return Path.Combine(folder.FullName);
        }

        private IQueryable<ControllerLogArchive> GetFromDirectoriesByDateRange(IEnumerable<DateTime> range)
        {
            var items = new List<ControllerLogArchive>();

            foreach (DateTime d in range)
            {
                var folder = new DirectoryInfo(GenerateFolderStructure(d));

                var fileQuery = folder.GetFiles($"{typeof(ControllerLogArchive).Name}*{d:dd-MM-yyyy}*{_fileTranscoder.FileExtension}", SearchOption.AllDirectories).AsQueryable();

                items.AddRange(fileQuery.Select(s => _fileTranscoder.DecodeItem<ControllerLogArchive>(File.ReadAllBytes(s.FullName))).ToList());
            }

            return items.AsQueryable();
        }

        #region IControllerEventLogRepository

        public IReadOnlyList<ControllerEventLog> GetLocationEventsBetweenDates(string locationId, DateTime startTime, DateTime endTime)
        {
            var range = Enumerable.Range(0, 1 + endTime.Subtract(startTime).Days).Select(o => startTime.AddDays(o)).ToList();

            var result = GetFromDirectoriesByDateRange(range)
                .FromSpecification(new ControllerLogDateRangeSpecification(locationId, startTime, endTime))
                //.AsNoTracking() only needed for EF
                .AsEnumerable()
                .SelectMany(s => s.LogData)
                .FromSpecification(new ControllerLogDateTimeRangeSpecification(locationId, startTime, endTime))
                .AsQueryable();

            return result.ToList();
        }

        #endregion
    }
}
