using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ATSPM.Application.Common
{
    /// <summary>
    /// Helper methods to help interact with external controller event logs
    /// </summary>
    public static class ControllerEventLogHelper
    {
        /// <summary>
        /// Imports controller event logs from .csv file
        /// </summary>
        /// <param name="file">Path to file</param>
        /// <param name="hasHeaders">Skips first line of csv file</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static IReadOnlyList<ControllerEventLog> ImportLogsFromCsvFile(string file, bool hasHeaders = false)
        {
            return ImportLogsFromCsvFile(new FileInfo(file), hasHeaders);
        }

        /// <summary>
        /// Imports controller event logs from .csv file
        /// </summary>
        /// <param name="file">Path to file</param>
        /// <param name="hasHeaders">Skips first line of csv file</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static IReadOnlyList<ControllerEventLog> ImportLogsFromCsvFile(FileInfo file, bool hasHeaders = false)
        {
            if (!file.Exists)
                throw new FileNotFoundException($"{file.Name} does not exist");

            return File.ReadAllLines(file.FullName)
                .IfCondition(() => hasHeaders == true, q => q.Skip(1), q => q)
                .Select(x => x.Split(','))
                   .Select(x => new ControllerEventLog
                   {
                       SignalIdentifier = x[0],
                       Timestamp = DateTime.Parse(x[1]),
                       EventCode = int.Parse(x[2]),
                       EventParam = int.Parse(x[3])
                   }).ToList();
        }
    }
}
