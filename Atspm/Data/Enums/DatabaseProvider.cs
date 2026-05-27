#region license
// Copyright 2026 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Enums/DatabaseProvider.cs
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

namespace Utah.Udot.Atspm.Data.Enums
{
    /// <summary>
    /// Specifies the type of database provider used by the application.
    /// </summary>
    public enum DatabaseProvider
    {
        /// <summary>
        /// An in-memory database, typically used for testing, prototyping, or caching.
        /// Data is volatile and does not persist across application restarts.
        /// </summary>
        InMemory,

        /// <summary>
        /// Microsoft SQL Server relational database management system.
        /// </summary>
        SqlServer,

        /// <summary>
        /// PostgreSQL open-source object-relational database system.
        /// </summary>
        PostgreSql,

        /// <summary>
        /// MySQL open-source relational database management system.
        /// </summary>
        MySql,

        /// <summary>
        /// Oracle Database enterprise relational database management system.
        /// </summary>
        Oracle,

        /// <summary>
        /// SQLite self-contained, serverless, zero-configuration relational database engine.
        /// Typically used for local storage, embedded systems, or mobile applications.
        /// </summary>
        Sqlite
    }
}
