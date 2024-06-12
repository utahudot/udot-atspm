#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCoreTests - DomainCoreTests/QueryExtensionTests.cs
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
using ATSPM.Domain.Extensions;
using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;

namespace DomainCoreTests
{
    public class QueryExtensionTests
    {
        private readonly ITestOutputHelper _output;

        public QueryExtensionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region FromSpecification

        [Theory]
        [SpecificationTest]
        public void FromSpecificationCriteria(IEnumerable<TestModel> data)
        {
            var expected = data.Where(i => i.Name.StartsWith('A')).Count();
            var actual = data.AsQueryable().FromSpecification(new TestCriteriaSpecification()).Count();

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected, actual);
        }

        [Theory]
        [SpecificationTest]
        public void FromSpecificationOrderBy(IEnumerable<TestModel> data)
        {
            var expected = data.OrderBy(o => o.Name);
            var actual = data.AsQueryable().FromSpecification(new TestOrderBySpecifiation());

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.True(actual.SequenceEqual(expected));
        }

        [Theory]
        [SpecificationTest]
        public void FromSpecificationOrderByDecending(IEnumerable<TestModel> data)
        {
            var expected = data.OrderByDescending(o => o.Name);
            var actual = data.AsQueryable().FromSpecification(new TestOrderByDecendingSpecifiation());

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.True(actual.SequenceEqual(expected));
        }

        [Theory]
        [SpecificationTest]
        public void FromSpecificationGroupBy(IEnumerable<TestModel> data)
        {
            var expected = data.GroupBy(i => i.Name[0]).SelectMany(s => s);
            var actual = data.AsQueryable().FromSpecification(new TestGroupBySpecifiation());

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.True(actual.SequenceEqual(expected));
        }

        [Theory]
        [SpecificationTest]
        public void FromSpecificationPaging(IEnumerable<TestModel> data)
        {
            var expected = data.Skip(10).Take(10).Count();
            var actual = data.AsQueryable().FromSpecification(new TestPagingSpecifiation()).Count();

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected, actual);
        }

        [Theory]
        [SpecificationTest]
        public void FromSpecificationTestAll(IEnumerable<TestModel> data)
        {
            var expected = data.Where(i => i.Name.StartsWith('A'))
                .OrderBy(o => o.Name)
                .GroupBy(i => i.Name[0]).SelectMany(s => s)
                .Skip(10).Take(10);

            var actual = data.AsQueryable()
                .FromSpecification(new TestCriteriaSpecification())
                .FromSpecification(new TestOrderBySpecifiation())
                .FromSpecification(new TestGroupBySpecifiation())
                .FromSpecification(new TestPagingSpecifiation());

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.True(actual.SequenceEqual(expected));
        }

        //HACK: this needs to be in an ISpecification test
        [Fact]
        public void ISpecificationIsSatisfiedBy()
        {
            var valid = new TestModel() { Name = "Allen" };
            var notValid = new TestModel() { Name = "Bill" };
            var specification = new TestCriteriaSpecification();

            Assert.True(specification.IsSatisfiedBy(valid));
            Assert.False(specification.IsSatisfiedBy(notValid));
        }

        #endregion

        #region IfCondition

        [Fact]
        public void IfConditionIsTrue()
        {
            IQueryable<int> expected = Enumerable.Range(1, 10).AsQueryable();

            IQueryable<int> actual = expected.IfCondition(() => true, q => q.Take(10));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IfConditionIsFalse()
        {
            IQueryable<int> expected = Enumerable.Range(1, 10).AsQueryable();

            IQueryable<int> actual = expected.IfCondition(() => false, q => q.Take(5), q => q.Take(10));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IfConditionIsFalseNull()
        {
            IQueryable<int> expected = Enumerable.Range(1, 10).AsQueryable();

            IQueryable<int> actual = expected.IfCondition(() => false, q => q.Take(5));

            Assert.Equal(expected, actual);
        }

        #endregion
    }

    #region TestSpecifications

    public class TestModel
    {
        public string Name { get; set; }
    }

    internal class TestCriteriaSpecification : BaseSpecification<TestModel>
    {
        public TestCriteriaSpecification() : base()
        {
            base.Criteria = i => i.Name.StartsWith('A');
        }
    }
    internal class TestOrderBySpecifiation : BaseSpecification<TestModel>
    {
        public TestOrderBySpecifiation() : base()
        {
            ApplyOrderBy(i => i.Name);
        }
    }

    internal class TestOrderByDecendingSpecifiation : BaseSpecification<TestModel>
    {
        public TestOrderByDecendingSpecifiation() : base()
        {
            ApplyOrderByDescending(i => i.Name);
        }
    }

    internal class TestGroupBySpecifiation : BaseSpecification<TestModel>
    {
        public TestGroupBySpecifiation() : base()
        {
            ApplyGroupBy(i => i.Name[0]);
        }
    }

    internal class TestPagingSpecifiation : BaseSpecification<TestModel>
    {
        public TestPagingSpecifiation() : base()
        {
            ApplyPaging(10, 10);
        }
    }

    #endregion
}
