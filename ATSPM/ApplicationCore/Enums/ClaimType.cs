#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Enums/ClaimType.cs
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
using System.ComponentModel;

namespace ATSPM.Application.Enums
{
    public enum ClaimTypes
    {
        [Description("Admin")]
        Admin,

        [Description("User:View")]
        UserView,

        [Description("User:Edit")]
        UserEdit,

        [Description("User:Delete")]
        UserDelete,

        [Description("Role:View")]
        RoleView,

        [Description("Role:Edit")]
        RoleEdit,

        [Description("Role:Delete")]
        RoleDelete,

        [Description("LocationConfiguration:View")]
        LocationConfigurationView,

        [Description("LocationConfiguration:Edit")]
        LocationConfigurationEdit,

        [Description("LocationConfiguration:Delete")]
        LocationConfigurationDelete,

        [Description("GeneralConfiguration:View")]
        GeneralConfigurationView,

        [Description("GeneralConfiguration:Edit")]
        GeneralConfigurationEdit,

        [Description("GeneralConfiguration:Delete")]
        GeneralConfigurationDelete,

        [Description("Data:View")]
        DataView,

        [Description("Data:Edit")]
        DataEdit,

        [Description("Watchdog:View")]
        WatchdogView,

        [Description("Report:View")]
        ReportView
    }
}

