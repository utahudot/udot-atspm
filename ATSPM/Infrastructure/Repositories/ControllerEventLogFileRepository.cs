using ATSPM.Application.Models;
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
using ATSPM.Domain.Specifications;
using System.IO;
using Microsoft.Extensions.Options;
using ATSPM.Application.Configuration;

namespace ATSPM.Infrasturcture.Repositories
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
            return $"{item.GetType().Name}_{item.SignalId}_{item.ArchiveDate.ToString("dd-MM-yyyy")}{_fileTranscoder.FileExtension}";
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

        public IQueryable<ControllerEventLog> GetSignalEventsBetweenDates(string signalId, DateTime startTime, DateTime endTime)
        {
            var range = Enumerable.Range(0, 1 + endTime.Subtract(startTime).Days).Select(o => startTime.AddDays(o)).ToList();

            var result = GetFromDirectoriesByDateRange(range)
                .FromSpecification(new ControllerLogDateRangeSpecification(signalId, startTime, endTime))
                //.AsNoTracking() only needed for EF
                .AsEnumerable()
                .SelectMany(s => s.LogData)
                .AsQueryable();

            return result;
        }

        #endregion
    }
}
