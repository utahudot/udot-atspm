#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TimeSpaceDiagram/TimeSpaceDiagramPhaseResult.cs
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

namespace Utah.Udot.Atspm.Business.TimeSpaceDiagram
{
    /// <summary>
    /// Wrapper for time space diagram phase results that can represent either a successful result or an error
    /// </summary>
    public class TimeSpaceDiagramPhaseResult
    {
        /// <summary>
        /// Error message if the operation failed, null if successful
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// The phase result data if successful, null if there was an error
        /// </summary>
        public TimeSpaceDiagramResultForPhase Result { get; set; }

        /// <summary>
        /// Indicates whether the operation was successful (no error)
        /// </summary>
        public bool IsSuccess => Error == null;

        /// <summary>
        /// Creates a successful result wrapper
        /// </summary>
        public static TimeSpaceDiagramPhaseResult Success(TimeSpaceDiagramResultForPhase result)
            => new() { Result = result };

        /// <summary>
        /// Creates a failed result wrapper with an error message
        /// </summary>
        public static TimeSpaceDiagramPhaseResult Failure(string error)
            => new() { Error = error };
    }
}
