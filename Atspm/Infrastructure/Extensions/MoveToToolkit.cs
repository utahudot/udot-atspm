#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Extensions/MoveToToolkit.cs
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

using System.ComponentModel;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.ATSPM.Infrastructure.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
    //HACK: move to toolkit
    public static class WorkflowExtensions
    {
        public static async Task BatchRunAsync<TInput, TOutput>(this Func<WorkflowBase<TInput, TOutput>> factory, IAsyncEnumerable<TInput> source, int batchSize, int parallelInstances, CancellationToken cancellationToken)
        {
            var batcher = new BatchBlock<TInput>(batchSize, new GroupingDataflowBlockOptions
            {
                BoundedCapacity = batchSize * (parallelInstances + 2),
                CancellationToken = cancellationToken
            });

            var manager = new ActionBlock<TInput[]>(async chunk =>
            {
                var workflow = factory();

                await workflow.Initialize();

                workflow.Output.LinkTo(DataflowBlock.NullTarget<TOutput>(), new DataflowLinkOptions { PropagateCompletion = true });

                foreach (var item in chunk)
                {
                    await workflow.Input.SendAsync(item, cancellationToken);
                }

                workflow.Input.Complete();

                await workflow.Output.Completion;
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = parallelInstances,
                BoundedCapacity = parallelInstances + 1,
                CancellationToken = cancellationToken
            });

            batcher.LinkTo(manager, new DataflowLinkOptions { PropagateCompletion = true });

            await foreach (var item in source.WithCancellation(cancellationToken))
            {
                await batcher.SendAsync(item, cancellationToken);
            }
            batcher.Complete();

            await manager.Completion;
        }

        public static Task WhenInitialized(this ISupportInitializeNotification service)
        {
            if (service.IsInitialized)
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            void Handler(object s, EventArgs e)
            {
                service.Initialized -= Handler;
                tcs.TrySetResult(true);
            }

            service.Initialized += Handler;

            if (service.IsInitialized)
            {
                service.Initialized -= Handler;
                tcs.TrySetResult(true);
            }

            return tcs.Task;
        }
    }
}
