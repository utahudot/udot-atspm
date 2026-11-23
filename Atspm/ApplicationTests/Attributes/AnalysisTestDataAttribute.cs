#region license
// Copyright 2026 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Attributes/AnalysisTestDataAttribute.cs
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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Utah.Udot.Atspm.ApplicationTests.Analysis.TestObjects;
using Utah.Udot.Atspm.Data.Interfaces;
using Xunit.Sdk;

namespace Utah.Udot.Atspm.ApplicationTests.Attributes
{
    /// <summary>
    /// Custom generic xUnit data attribute to dynamically load test inputs and expected outputs
    /// from JSON data files matching the specific test type name.
    /// </summary>
    /// <typeparam name="T">The test data type parameter.</typeparam>
    public class AnalysisTestDataAttribute<T> : DataAttribute where T : AnalysisTestDataBase
    {
        /// <inheritdoc/>
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            return AnalysisTestDataAttribute.LoadTestDataForType(typeof(T));
        }
    }

    /// <summary>
    /// Non-generic custom xUnit data attribute that dynamically infers the test data type
    /// from the generic base class parameter of the declaring class.
    /// </summary>
    public class AnalysisTestDataAttribute : DataAttribute
    {
        /// <summary>
        /// Resolves the shared <c>Analysis/TestData</c> directory across test projects in the solution.
        /// </summary>
        /// <returns>The resolved <see cref="DirectoryInfo"/> pointing to the test data folder.</returns>
        public static DirectoryInfo ResolveTestDataDirectory()
        {
            var projectRoot = Path.GetFullPath(@"..\..\..\");
            var localTestData = new DirectoryInfo(Path.Combine(projectRoot, "Analysis", "TestData"));
            if (localTestData.Exists)
            {
                return localTestData;
            }

            var siblingAppTestsData = new DirectoryInfo(Path.Combine(Path.GetFullPath(@"..\..\..\..\ApplicationTests"), "Analysis", "TestData"));
            if (siblingAppTestsData.Exists)
            {
                return siblingAppTestsData;
            }

            return localTestData;
        }

        /// <summary>
        /// Loads all JSON test data files matching <paramref name="testDataType"/> from the test data directory.
        /// </summary>
        /// <param name="testDataType">The concrete <see cref="AnalysisTestDataBase"/> type.</param>
        /// <returns>An enumerable of theory argument arrays <c>[Configuration, Input, Output]</c>.</returns>
        public static IEnumerable<object[]> LoadTestDataForType(Type testDataType)
        {
            var dir = ResolveTestDataDirectory();
            var hasFiles = false;
            var isStronglyTyped = IsStronglyTypedTestData(testDataType);

            if (dir.Exists)
            {
                foreach (var f in dir.GetFiles("*.json").Where(f => f.Name.Contains(testDataType.Name)).OrderBy(f => f.Name))
                {
                    hasFiles = true;
                    var json = File.ReadAllText(f.FullName);
                    var testFile = (AnalysisTestDataBase)JsonConvert.DeserializeObject(json, testDataType, new JsonSerializerSettings()
                    {
                        TypeNameHandling = isStronglyTyped ? TypeNameHandling.Auto : TypeNameHandling.All,
                    });

                    if (testFile.Configuration is ILocationLayer config)
                    {
                        if (testFile.Input is IEnumerable<ILocationLayer> input)
                        {
                            foreach (var i in input)
                            {
                                i.LocationIdentifier = config.LocationIdentifier;
                            }
                        }

                        if (testFile.Output is IEnumerable<ILocationLayer> output)
                        {
                            foreach (var o in output)
                            {
                                o.LocationIdentifier = config.LocationIdentifier;
                            }
                        }
                    }

                    yield return new object[] { testFile.Configuration, testFile.Input, testFile.Output };
                }
            }

            if (!hasFiles)
            {
                yield return new object[] { default, default, default };
            }
        }

        /// <summary>
        /// Determines whether the specified test data type inherits from the strongly-typed generic <see cref="AnalysisTestDataBase{TConfig, TInput, TOutput}"/>.
        /// </summary>
        /// <param name="type">The test data type to inspect.</param>
        /// <returns><c>true</c> if the type derives from <see cref="AnalysisTestDataBase{TConfig, TInput, TOutput}"/>; otherwise, <c>false</c>.</returns>
        public static bool IsStronglyTypedTestData(Type type)
        {
            var current = type;
            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(AnalysisTestDataBase<,,>))
                {
                    return true;
                }
                current = current.BaseType;
            }

            return false;
        }

        /// <inheritdoc/>
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var declaringType = testMethod.DeclaringType;
            Type testDataType = null;

            while (declaringType != null)
            {
                if (declaringType.IsGenericType)
                {
                    var genericTypeDefinition = declaringType.GetGenericTypeDefinition();
                    var genericArgs = declaringType.GetGenericArguments();

                    if (genericTypeDefinition.Name.StartsWith("WorkflowStepTestBase") && genericArgs.Length == 5)
                    {
                        testDataType = genericArgs[1];
                        break;
                    }
                }
                declaringType = declaringType.BaseType;
            }

            if (testDataType == null)
            {
                yield break;
            }

            foreach (var row in LoadTestDataForType(testDataType))
            {
                yield return row;
            }
        }
    }
}
