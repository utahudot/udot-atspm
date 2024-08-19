﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models/UserJurisdiction.cs
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

#nullable disable
namespace Utah.Udot.Atspm.Data.Models
{
    /// <summary>
    /// Users for jurisdiction
    /// </summary>
    public class UserJurisdiction
    {
       /// <summary>
       /// User id
       /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Jurisdiction id
        /// </summary>
        public int JurisdictionId { get; set; }
        
        /// <summary>
        /// Jursidiction
        /// </summary>
        public virtual Jurisdiction Jurisdiction { get; set; }
    }
}
