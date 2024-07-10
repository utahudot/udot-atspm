#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Exceptions/ControllerLoggerExceptions.cs
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
using ATSPM.Application.Services;
using System;

#nullable enable
namespace ATSPM.Application.Exceptions
{
    public class EventLogDecoderException : ATSPMException
    {
        public EventLogDecoderException(string? message) : base(message) { }

        public EventLogDecoderException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
