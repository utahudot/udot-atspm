using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utah.Udot.Atspm.Business.PrioritySummary
{
    public sealed record PrioritySummaryCycleDto(
      string LocationIdentifier,
      int TspNumber,

      DateTime CheckIn,     // 112
      DateTime CheckOut,    // 115

      DateTime? ServiceStart, // 118
      DateTime? ServiceEnd,   // 119

      IReadOnlyList<DateTime> EarlyGreen,       // 113
      IReadOnlyList<DateTime> ExtendGreen,      // 114
      IReadOnlyList<DateTime> PreemptForceOff,  // 116
      IReadOnlyList<DateTime> TspEarlyForceOff, // 117

      double? RequestEndOffsetSec,
      double? ServiceStartOffsetSec,
      double? ServiceEndOffsetSec
    );

}
