#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Exceptions/ATSPMException.cs
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

using Utah.Udot.NetStandardToolkit.Exceptions;

#nullable enable
namespace Utah.Udot.Atspm.Exceptions
{
    /// <summary>
    /// Base for all Atspm exceptions.
    /// Use this base to help with tracking exceptions in external logging services
    /// </summary>
    public abstract class AtspmException : UdotExceptionBase
    {
        /// <inheritdoc/>
        public AtspmException(string? message) : base(message) { }

        /// <inheritdoc/>
        public AtspmException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
