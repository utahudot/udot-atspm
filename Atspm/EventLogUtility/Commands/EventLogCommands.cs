#region license
// Copyright 2025 Utah Departement of Transportation
// for EventLogUtility - Utah.Udot.Atspm.EventLogUtility.Commands/EventLogCommands.cs
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
using Utah.Udot.ATSPM.EventLogUtility.Commands;

namespace Utah.Udot.Atspm.EventLogUtility.Commands
{
    public class EventLogCommands : RootCommand
    {
        public EventLogCommands()
        {
            AddCommand(AggregationCommand);
            AddCommand(LogCommand);
            AddCommand(TransferCommand);
            AddCommand(ExtractCommand);
            AddCommand(DecodeEventsCommand);
        }

        public AggregationCommand AggregationCommand { get; set; } = new AggregationCommand();
        public LogConsoleCommand LogCommand { get; set; } = new LogConsoleCommand();
        public TransferLogsConsoleCommand TransferCommand { get; set; } = new TransferLogsConsoleCommand();
        public ExtractConsoleCommand ExtractCommand { get; set; } = new ExtractConsoleCommand();
        public DecodeEventsCommand DecodeEventsCommand { get; set; } = new DecodeEventsCommand();
    }
}
