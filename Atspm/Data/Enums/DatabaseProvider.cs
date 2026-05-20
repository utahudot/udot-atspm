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
