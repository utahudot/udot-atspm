using System;
using System.Collections.Generic;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.EventLogUtility.Commands
{
    public interface ICommandOption<T>
    {
        ModelBinder<T> GetOptionsBinder();
    }
}
