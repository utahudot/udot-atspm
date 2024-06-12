#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Common.EqualityComparers/ControllerEventLogEqualityComparer.cs
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
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ATSPM.Application.Common.EqualityComparers
{
    public class ControllerEventLogEqualityComparer : EqualityComparer<ControllerEventLog>
    {
        public override bool Equals([AllowNull] ControllerEventLog x, [AllowNull] ControllerEventLog y)
        {
            DateTime.TryParse(x?.Timestamp.ToString("MM-dd-yyyy HH:mm:ss.f"), out DateTime timeX);
            DateTime.TryParse(y?.Timestamp.ToString("MM-dd-yyyy HH:mm:ss.f"), out DateTime timeY);

            return x.SignalIdentifier == y.SignalIdentifier && timeX.Ticks == timeY.Ticks && x.EventCode == y.EventCode && x.EventParam == y.EventParam;
        }

        public override int GetHashCode([DisallowNull] ControllerEventLog obj)
        {
            DateTime.TryParse(obj.Timestamp.ToString("MM-dd-yyyy HH:mm:ss.f"), out DateTime time);

            var h = obj.SignalIdentifier.GetHashCode() + time.Ticks + obj.EventCode + obj.EventParam;

            return h.GetHashCode();
        }
    }
}
