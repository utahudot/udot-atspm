#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Workflows/ProcessStepBase.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Domain.Workflows
{
    /// <summary>
    /// Base class for workflow process steps
    /// </summary>
    /// <typeparam name="T1">Input data type</typeparam>
    /// <typeparam name="T2">Output data type</typeparam>
    public abstract class ProcessStepBase<T1, T2> : IPropagatorBlock<T1, T2>, IDataflowBlock, ISourceBlock<T2>, ITargetBlock<T1>, IReceivableSourceBlock<T2>
    {
        protected IPropagatorBlock<T1, T2> workflowProcess;
        protected DataflowBlockOptions options;

        /// <inheritdoc/>
        public ProcessStepBase(DataflowBlockOptions dataflowBlockOptions = default)
        {
            options = dataflowBlockOptions ?? new();
            options.NameFormat = GetType().Name;
        }

        #region IPropagatorBlock

        #region IDataflowBlock

        /// <inheritdoc/>
        public Task Completion => workflowProcess.Completion;

        /// <inheritdoc/>
        public void Complete()
        {
            workflowProcess.Complete();
        }

        /// <inheritdoc/>
        public void Fault(Exception exception)
        {
            workflowProcess.Fault(exception);
        }

        #endregion

        #region ISourceBlock

        /// <inheritdoc/>
        public T2 ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T2> target, out bool messageConsumed)
        {
            return workflowProcess.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        /// <inheritdoc/>
        public IDisposable LinkTo(ITargetBlock<T2> target, DataflowLinkOptions linkOptions)
        {
            return workflowProcess.LinkTo(target, linkOptions);
        }

        /// <inheritdoc/>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T2> target)
        {
            workflowProcess?.ReleaseReservation(messageHeader, target);
        }

        /// <inheritdoc/>
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T2> target)
        {
            return workflowProcess.ReserveMessage(messageHeader, target);
        }

        #endregion

        #region ITargetBlock

        /// <inheritdoc/>
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T1 messageValue, ISourceBlock<T1> source, bool consumeToAccept)
        {
            return workflowProcess.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        #endregion

        #endregion

        #region IReceivableSourceBlock

        /// <inheritdoc/>
        public bool TryReceive(Predicate<T2> filter, [MaybeNullWhen(false)] out T2 item)
        {
            var process = (IReceivableSourceBlock<T2>)workflowProcess;

            return process.TryReceive(filter, out item);
        }

        /// <inheritdoc/>
        public bool TryReceiveAll([NotNullWhen(true)] out IList<T2> items)
        {
            var process = (IReceivableSourceBlock<T2>)workflowProcess;

            return process.TryReceiveAll(out items);
        }

        #endregion
    }
}
