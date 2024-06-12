#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Workflows/WorkflowBase.cs
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
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Domain.Workflows
{
    /// <summary>
    /// Used as a base to create complex, inter-linkable, parallel workflows
    /// </summary>
    /// <typeparam name="T1">Input data type</typeparam>
    /// <typeparam name="T2">Output data type</typeparam>
    public abstract class WorkflowBase<T1, T2> : ExecutableServiceWithProgressAsyncBase<T1, T2, int>
    {
        /// <inheritdoc/>
        public event EventHandler CanExecuteChanged;
        protected DataflowBlockOptions blockOptions;

        /// <inheritdoc/>
        public WorkflowBase(DataflowBlockOptions dataflowBlockOptions = default) :base(true)
        {
            blockOptions = dataflowBlockOptions ?? new();
            blockOptions.NameFormat = GetType().Name;
        }

        /// <summary>
        /// Used for tracking workflow step task completion results
        /// </summary>
        public List<IDataflowBlock> Steps { get; set; }

        /// <summary>
        /// Can be used to post or send data or link from other workflows
        /// </summary>
        public BroadcastBlock<T1> Input { get; set; }

        /// <summary>
        /// Can be used to recieve data or link to other workflows
        /// </summary>
        public BufferBlock<T2> Output { get; set; }

        /// <inheritdoc/>
        public override Task Initialize()
        {
            Steps = new();

            Input = new(null, blockOptions);
            Output = new(blockOptions);

            InstantiateSteps();

            Steps.Add(Input);

            AddStepsToTracker();

            LinkSteps();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Instantiate workflow steps objects
        /// </summary>
        protected abstract void InstantiateSteps();

        /// <summary>
        /// Add steps to <see cref="Steps"/> for step task tracking
        /// </summary>
        protected abstract void AddStepsToTracker();

        /// <summary>
        /// Link workflow steps
        /// </summary>
        protected abstract void LinkSteps();

        #region IExecuteWithProgress

        /// <inheritdoc/>
        public override async IAsyncEnumerable<T2> Execute(T1 parameter, IProgress<int> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            //if (!IsInitialized)
            //    BeginInit();

            if (CanExecute(parameter))
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                try
                {
                    await Input.SendAsync(parameter);

                    Input.Complete();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Output.Completion.ContinueWith(t =>
                    {
                        Console.WriteLine($"Output is complete! {t.Status}");
                        this.IsInitialized = false;
                    }, cancelToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                }
                catch (Exception e)
                {
                    //logMessages.LoggerExecutionException(new ControllerLoggerExecutionException(this, "Exception running Location Controller Logger Service", e));
                }
                finally
                {
                    //logMessages.LoggerCompletedMessage(DateTime.Now, sw.Elapsed);
                    sw.Stop();
                }

                await foreach (var item in Output.ReceiveAllAsync(cancelToken))
                    yield return item;

                await Task.WhenAll(Steps.Select(s => s.Completion)).ContinueWith(t =>
                {
                    IsInitialized = false;
                    //Console.WriteLine($"All steps are complete! {t.Status} in {sw.Elapsed}");
                }, cancelToken);
            }
            else
            {
                throw new ExecuteException();
            }
        }

        #endregion
    }
}
