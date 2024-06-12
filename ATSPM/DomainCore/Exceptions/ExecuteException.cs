#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Exceptions/ExecuteException.cs
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
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Domain.Exceptions
{
    /// <summary>
    /// Used with <see cref="IExecute"/> when <c>CanExecute</c> is false
    /// </summary>
    public class ExecuteException : Exception
    {
        /// <summary>
        /// Used with <see cref="IExecute"/> when <c>CanExecute</c> is false
        /// </summary>
        public ExecuteException() : base("Did not pass CanExecute")
        {
        }

        /// <summary>
        /// Used with <see cref="IExecute"/> when <c>CanExecute</c> is false
        /// </summary>
        /// <param name="message">Enter custom <c>CanExecute</c> failed message</param>
        public ExecuteException(string message) : base(message)
        {
        }
    }
}
