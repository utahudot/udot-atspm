#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps/WorkflowStepTestBase.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Threading.Tasks;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.ApplicationTests.Fixtures;
using Utah.Udot.Atspm.Data.Models;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.ApplicationTests.Analysis.WorkflowSteps
{
    /// <summary>
    /// Base class for workflow step unit tests. Enforces consistent test execution,
    /// xUnit Theory file loading, and assertion patterns across all steps.
    /// </summary>
    /// <typeparam name="TStep">The workflow step class under test.</typeparam>
    /// <typeparam name="TTestData">The corresponding AnalysisTestDataBase class for file loading.</typeparam>
    /// <typeparam name="TConfig">The configuration type passed to the step (e.g., Location or Approach).</typeparam>
    /// <typeparam name="TInput">The input type passed to the step's Process method.</typeparam>
    /// <typeparam name="TOutput">The expected output type returned by the step's Process method.</typeparam>
    public abstract class WorkflowStepTestBase<TStep, TTestData, TConfig, TInput, TOutput> : IClassFixture<TestLocationFixture>, IDisposable
        where TStep : class
        where TTestData : AnalysisTestDataBase
    {
        /// <summary>
        /// Logger for xUnit output.
        /// </summary>
        protected readonly ITestOutputHelper Output;

        /// <summary>
        /// Pre-configured test location from the ClassFixture.
        /// </summary>
        protected readonly Location TestLocation;

        /// <summary>
        /// Initializes a new instance of the WorkflowStepTestBase class.
        /// </summary>
        /// <param name="output">The xUnit test output helper.</param>
        /// <param name="testLocationFixture">The test location class fixture.</param>
        protected WorkflowStepTestBase(ITestOutputHelper output, TestLocationFixture testLocationFixture)
        {
            Output = output;
            TestLocation = testLocationFixture.TestLocation;
        }

        /// <summary>
        /// Generic runner that executes the step and asserts equality against expected outcomes.
        /// Inheritors should override this method to add the [Theory] and [AnalysisTestData] attributes.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <param name="input">The input data packet.</param>
        /// <param name="expected">The expected output data packet.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task ExecuteStepFromFileTest(TConfig config, TInput input, TOutput expected)
        {
            var sut = CreateStep(config, input, expected);

            var actual = await ExecuteStepAsync(sut, config, input);

            AssertOutputs(actual, expected);
        }

        /// <summary>
        /// Factory method to instantiate the step under test.
        /// Override this to provide customized construction, options, or mock injections.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <param name="input">The input data packet.</param>
        /// <param name="expected">The expected output data packet.</param>
        /// <returns>An instance of the step under test.</returns>
        protected abstract TStep CreateStep(TConfig config, TInput input, TOutput expected);

        /// <summary>
        /// Abstract method to trigger step execution. Handles necessary packaging of inputs.
        /// </summary>
        /// <param name="step">The step under test.</param>
        /// <param name="config">The configuration object.</param>
        /// <param name="input">The input data packet.</param>
        /// <returns>A task returning the actual output.</returns>
        protected abstract Task<TOutput> ExecuteStepAsync(TStep step, TConfig config, TInput input);

        /// <summary>
        /// Asserts that the actual step output matches the expected outcome.
        /// Defaults to xUnit's Assert.Equivalent, but can be overridden for custom tolerances or collections.
        /// </summary>
        /// <param name="actual">The actual output.</param>
        /// <param name="expected">The expected output.</param>
        protected virtual void AssertOutputs(TOutput actual, TOutput expected)
        {
            Assert.Equivalent(expected, actual);
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
