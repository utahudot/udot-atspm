using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Data.Interfaces
{
    /// <summary>
    /// For objects in the signal layer
    /// </summary>
    public interface ISignalLayer
    {
        /// <summary>
        /// Identifier of signal controller
        /// </summary>
        string SignalIdentifier { get; set; }
    }
}
