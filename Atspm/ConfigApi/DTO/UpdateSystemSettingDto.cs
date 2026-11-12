#region license
// Copyright 2026 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.ATSPM.ConfigApi.DTO/UpdateSystemSettingDto.cs
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

namespace Utah.Udot.ATSPM.ConfigApi.DTO
{
    /// <summary>
    /// Request payload for creating or updating a system-configurable setting value.
    /// </summary>
    public class UpdateSystemSettingDto
    {
        /// <summary>
        /// New configuration value to persist as an override.
        /// </summary>
        public string? Value { get; set; }
    }
}
