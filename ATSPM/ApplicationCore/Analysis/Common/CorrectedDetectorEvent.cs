using ATSPM.Data.Enums;
using ATSPM.Data.Interfaces;
using ATSPM.Data.Models;
using System;
using System.Text.Json;

namespace ATSPM.Application.Analysis.Common
{
    public class CorrectedDetectorEvent : IDetectorEvent
    {
        #region IDetectorEvent

        /// <inheritdoc/>
        public string SignalIdentifier { get; set; }

        /// <inheritdoc/>
        public int DetectorChannel { get; set; }

        /// <inheritdoc/>
        public DateTime CorrectedTimeStamp { get; set; }

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
