using ATSPM.Data.Interfaces;
using System;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// Detector event
    /// </summary>
    public interface IDetectorEvent : ISignalDetector
    {
        /// <summary>
        /// Coreected timestamp of event
        /// </summary>
        DateTime CorrectedTimeStamp { get; set; }
    }
}
