#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Enums/SignalHeadType.cs
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

using System.ComponentModel.DataAnnotations;

namespace Utah.Udot.Atspm.Enums
{
    /// <summary>
    /// Signal head types
    /// </summary>
    public enum SignalHeadType
    {
        /// <summary>
        /// Protected only
        /// </summary>
        [Display(Name = "Protected Only")]
        ProtectedOnly,

        /// <summary>
        /// Permissive only
        /// </summary>
        [Display(Name = "Permissive Only")]
        PermissiveOnly,

        /// <summary>
        /// 5-head
        /// </summary>
        [Display(Name = "5-Head")]
        FiveHead,

        /// <summary>
        /// Flashing yellow arrow
        /// </summary>
        [Display(Name = "Flashing Yellow Arrow")]
        FYA
    }
}
