#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Enums/ClaimType.cs
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
    public enum ClaimTypes
    {
        [Display(Name = "Admin")]
        Admin,

        [Display(Name = "User:View")]
        UserView,

        [Display(Name = "User:Edit")]
        UserEdit,

        [Display(Name = "User:Delete")]
        UserDelete,

        [Display(Name = "Role:View")]
        RoleView,

        [Display(Name = "Role:Edit")]
        RoleEdit,

        [Display(Name = "Role:Delete")]
        RoleDelete,

        [Display(Name = "LocationConfiguration:View")]
        LocationConfigurationView,

        [Display(Name = "LocationConfiguration:Edit")]
        LocationConfigurationEdit,

        [Display(Name = "LocationConfiguration:Delete")]
        LocationConfigurationDelete,

        [Display(Name = "GeneralConfiguration:View")]
        GeneralConfigurationView,

        [Display(Name = "GeneralConfiguration:Edit")]
        GeneralConfigurationEdit,

        [Display(Name = "GeneralConfiguration:Delete")]
        GeneralConfigurationDelete,

        [Display(Name = "Data:View")]
        DataView,

        [Display(Name = "Data:Edit")]
        DataEdit,

        [Display(Name = "Watchdog:View")]
        WatchdogView,

        [Display(Name = "Report:View")]
        ReportView
    }
}

