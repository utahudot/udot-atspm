﻿using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="DataLoggerEnum.DetectorOff"/></item>
    /// <item><see cref="DataLoggerEnum.DetectorOn"/></item>
    /// </list>
    /// </summary>
    public class FilteredDetectorData : FilterStepBase
    {
        /// <inheritdoc/>
        public FilteredDetectorData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.DetectorOff);
            filteredList.Add((int)DataLoggerEnum.DetectorOn);
        }
    }
}