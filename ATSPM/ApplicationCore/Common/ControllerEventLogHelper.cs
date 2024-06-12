#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Common/ControllerEventLogHelper.cs
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
