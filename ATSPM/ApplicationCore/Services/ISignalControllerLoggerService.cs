using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Services
{
    public interface ISignalControllerLoggerService : IExecuteAsyncWithProgress<IList<Signal>, bool, int>, ISupportInitializeNotification, IDisposable
    {
        List<IDataflowBlock> Steps { get; set; }
    }
}
