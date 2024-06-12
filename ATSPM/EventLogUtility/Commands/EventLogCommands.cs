#region license
// Copyright 2024 Utah Departement of Transportation
// for EventLogUtility - ATSPM.EventLogUtility.Commands/EventLogCommands.cs
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
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.EventLogUtility.Commands
{
    public class EventLogCommands : RootCommand
    {
        public EventLogCommands()
        {
            this.AddCommand(AggregationCommand);
            this.AddCommand(LogCommand);
            this.AddCommand(ExtractCommand);
            this.AddCommand(LocationInfoCommand);
        }

        public AggregationCommand AggregationCommand { get; set; } = new AggregationCommand();
        public LogConsoleCommand LogCommand { get; set; } = new LogConsoleCommand();
        public ExtractConsoleCommand ExtractCommand { get; set; } = new ExtractConsoleCommand();
        public LocationInfoCommand LocationInfoCommand { get; set; } = new LocationInfoCommand();
    }
}
