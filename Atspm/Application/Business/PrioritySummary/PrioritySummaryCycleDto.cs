#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.PrioritySummary/PrioritySummaryCycleDto.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

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
