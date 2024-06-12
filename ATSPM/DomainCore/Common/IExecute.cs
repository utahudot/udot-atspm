#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Common/IExecute.cs
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ATSPM.Domain.Common
{
    /// <summary>
    /// Defines an async command or process which can conditionally be executed.
    /// </summary>
    public interface IExecuteAsync : ICommand
    {
        /// <summary>
        /// Defines the async method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns></returns>
        Task ExecuteAsync(object parameter);
    }

    /// <summary>
    /// Defines a command which can conditionally be executed.
    /// </summary>
    public interface IExecute : ICommand
    {
    }

    /// <summary>
    /// Defines a command with input and ouput parameters which can conditionally be executed.
    /// </summary>
    /// <typeparam name="Tin">Input paramter type.</typeparam>
    /// <typeparam name="Tout">Output parameter type.</typeparam>
    public interface IExecute<Tin, Tout> : IExecute
    {
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Input parameter</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        bool CanExecute(Tin parameter);

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <param name="cancelToken">Token to cancel command</param>
        /// <returns>Ouput result.</returns>
        Tout Execute(Tin parameter, CancellationToken cancelToken = default);
    }

    /// <summary>
    /// Defines a command with input and ouput parameters which can conditionally be executed and use IProgress.
    /// </summary>
    /// <typeparam name="Tin">Input paramter type.</typeparam>
    /// <typeparam name="Tout">Output parameter type.</typeparam>
    /// <typeparam name="Tp">IProgress type.</typeparam>
    public interface IExecuteWithProgress<Tin, Tout, Tp> : IExecute<Tin, Tout>
    {
        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <param name="cancelToken">Token to cancel command</param>
        /// <param name="progress">IProgress reporting implmentation</param>
        /// <returns>Ouput result.</returns>
        Tout Execute(Tin parameter, IProgress<Tp> progress = default, CancellationToken cancelToken = default);
    }

    /// <summary>
    /// Defines an async command or process with input and ouput parameters which can conditionally be executed.
    /// </summary>
    /// <typeparam name="Tin">Input paramter type.</typeparam>
    /// <typeparam name="Tout">Output parameter type.</typeparam>
    public interface IExecuteAsync<Tin, Tout> : IExecuteAsync
    {
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Input parameter</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        bool CanExecute(Tin parameter);

        /// <summary>
        /// Defines the async method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <param name="cancelToken">Token to cancel command</param>
        /// <returns>Ouput result.</returns>
        Task<Tout> ExecuteAsync(Tin parameter, CancellationToken cancelToken = default);
    }

    /// <summary>
    /// Defines an async command or operation with input and ouput parameters which can conditionally be executed and use IProgress./>
    /// </summary>
    /// <typeparam name="Tin">Input paramter type.</typeparam>
    /// <typeparam name="Tout">Output parameter type.</typeparam>
    /// <typeparam name="Tp">IProgress type.</typeparam>
    public interface IExecuteAsyncWithProgress<Tin, Tout, Tp> : IExecuteAsync<Tin, Tout>
    {
        /// <summary>
        /// Defines the async method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <param name="cancelToken">Token to cancel command</param>
        /// <param name="progress">IProgress reporting implmentation</param>
        /// <returns>Ouput result.</returns>
        Task<Tout> ExecuteAsync(Tin parameter, IProgress<Tp> progress = default, CancellationToken cancelToken = default);
    }
}
