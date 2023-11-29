using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using System;
using System.Text.Json;

namespace ATSPM.Application.Analysis.PreemptionDetails
{
    public abstract class PreempDetailValueBase : StartEndRange, ISignalLayer
    {
        #region ISignalLayer

        /// <inheritdoc/>
        public string SignalIdentifier { get; set; }

        #endregion

        public int PreemptNumber { get; set; }
        public TimeSpan Seconds { get; set; }

        public override bool InRange(DateTime time)
        {
            return time >= Start && time <= End;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
