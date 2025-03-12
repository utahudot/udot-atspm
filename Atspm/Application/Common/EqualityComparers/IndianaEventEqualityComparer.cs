#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Common.EqualityComparers/IndianaEventEqualityComparer.cs
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

using System.Diagnostics.CodeAnalysis;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Common.EqualityComparers
{
    /// <summary>
    /// Compares if two <see cref="IndianaEvent"/> are equal
    /// </summary>
    public class IndianaEventEqualityComparer : EqualityComparer<IndianaEvent>
    {
        /// <inheritdoc/>
        public override bool Equals([AllowNull] IndianaEvent x, [AllowNull] IndianaEvent y)
        {
            //DateTime.TryParse(x?.Timestamp.ToString("MM-dd-yyyy HH:mm:ss.f"), out DateTime timeX);
            //DateTime.TryParse(y?.Timestamp.ToString("MM-dd-yyyy HH:mm:ss.f"), out DateTime timeY);

            //return x.LocationIdentifier == y.LocationIdentifier && timeX.Ticks == timeY.Ticks && x.EventCode == y.EventCode && x.EventParam == y.EventParam;

            return x.LocationIdentifier == y.LocationIdentifier && x.Timestamp.Ticks == y.Timestamp.Ticks && x.EventCode == y.EventCode && x.EventParam == y.EventParam;
        }

        /// <inheritdoc/>
        public override int GetHashCode([DisallowNull] IndianaEvent obj)
        {
            //DateTime.TryParse(obj.Timestamp.ToString("MM-dd-yyyy HH:mm:ss.f"), out DateTime time);

            //var h = obj.LocationIdentifier.GetHashCode() + time.Ticks + obj.EventCode + obj.EventParam;

            //return h.GetHashCode();

            var h = obj.LocationIdentifier.GetHashCode() + obj.Timestamp.Ticks + obj.EventCode + obj.EventParam;

            return h.GetHashCode();
        }
    }
}
