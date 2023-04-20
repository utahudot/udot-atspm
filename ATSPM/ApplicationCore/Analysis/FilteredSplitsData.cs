using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis
{
    public class FilteredSplitsData : FilterStepBase
    {
        public FilteredSplitsData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            for (int i = (int)DataLoggerEnum.Split1Change; i <= (int)DataLoggerEnum.Split16Change; i++)
            {
                filteredList.Add(i);
            }
        }
    }
}
