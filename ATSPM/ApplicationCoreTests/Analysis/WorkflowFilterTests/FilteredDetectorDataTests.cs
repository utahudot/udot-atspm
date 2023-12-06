using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace ApplicationCoreTests.Analysis.WorkflowFilterTests
{
    public class FilteredDetectorDataTests : WorkflowFilterTestsBase
    {
        public FilteredDetectorDataTests(ITestOutputHelper output) : base(output)
        {
            filteredList.Add((int)DataLoggerEnum.DetectorOff);
            filteredList.Add((int)DataLoggerEnum.DetectorOn);

            sut = new FilteredDetectorData();
        }
    }
}
