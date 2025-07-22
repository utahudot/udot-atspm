#region license
// Copyright 2025 Utah Departement of Transportation
// for EventLogUtility - Utah.Udot.ATSPM.EventLogUtility.Commands/StartOption.cs
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

using System.CommandLine;

namespace Utah.Udot.ATSPM.EventLogUtility.Commands
{
    public class StartOption : Option<DateTime>
    {
        public StartOption() : base("--start", "Start date/time range")
        {
            IsRequired = true;
            AddAlias("-s");
            AddValidator(r =>
            {
                if (r.GetValueForOption(this) > DateTime.Now)
                    r.ErrorMessage = "Start date must not be greater than current date/time";
            });
        }
    }
}
