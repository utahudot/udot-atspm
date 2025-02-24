#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Exceptions/ControllerLoggerException.cs
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

using Utah.Udot.Atspm.Services;

#nullable enable
namespace Utah.Udot.Atspm.Exceptions
{
    public abstract class ControllerLoggerException : AtspmException
    {
        public ControllerLoggerException(string? message) : base(message) { }

        public ControllerLoggerException(string? message, Exception? innerException) : base(message, innerException) { }
    }

    public class ControllerLoggerExecutionException : ControllerLoggerException
    {
        public ControllerLoggerExecutionException(ILocationControllerLoggerService LocationControllerLoggerService, string? message, Exception? innerException)
            : base(message ?? $"Exception running Location Controller Logger Service",
                  innerException)
        {
            LocationControllerLoggerService = LocationControllerLoggerService;
        }

        public ILocationControllerLoggerService LocationControllerLoggerService { get; private set; }
    }

    public class ControllerLoggerStepExecutionException<T> : ControllerLoggerExecutionException
    {
        public ControllerLoggerStepExecutionException(ILocationControllerLoggerService LocationControllerLoggerService,
            string step,
            T item,
            string? message, Exception? innerException) : base(LocationControllerLoggerService,
                message ?? $"Exception running Location Controller Logger Service Step {step} on item {item}",
                innerException)
        {
            Step = step;
            Item = item;
        }

        public string Step { get; private set; }
        public T Item { get; private set; }
    }
}
