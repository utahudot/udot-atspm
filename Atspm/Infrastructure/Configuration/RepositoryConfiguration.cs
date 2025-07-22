#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/RepositoryConfiguration.cs
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

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration for the repository connections.
    /// </summary>
    public class RepositoryConfiguration
    {
        /// <summary>
        /// The provider for the repository, e.g., MySql, Oracle, PostgreSQL, SqlServer|SqlLite
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Connection string for the repository.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"{Provider}|{ConnectionString}";
    }
}
