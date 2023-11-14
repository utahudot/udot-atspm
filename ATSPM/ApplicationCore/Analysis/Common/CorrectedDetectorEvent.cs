using ATSPM.Data.Enums;
using ATSPM.Data.Interfaces;
using ATSPM.Data.Models;
using System;

namespace ATSPM.Application.Analysis.Common
{
    public class CorrectedDetectorEvent : ISignalDetector
    {
        #region ISignalDetector

        /// <inheritdoc/>
        public string SignalIdentifier { get; set; }

        /// <inheritdoc/>
        public int DetectorChannel { get; set; }

        #endregion

        public DateTime CorrectedTimeStamp { get; set; }

        public override string ToString()
        {
            return $"{SignalIdentifier} - {DetectorChannel} - {CorrectedTimeStamp:yyyy-MM-dd'T'HH:mm:ss.f}";
        }
    }
}
