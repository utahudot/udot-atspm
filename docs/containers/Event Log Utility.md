# Event Log Utility configuration

Configuration options available to the **Event Log Utility** container.

Generated from [utahudot/udot-atspm at `ee3250431c6e5218a1d10871a46b4b9736743192`](https://github.com/utahudot/udot-atspm/tree/ee3250431c6e5218a1d10871a46b4b9736743192).

## Contents

- [DatabaseConfiguration:ConfigContext](#databaseconfigurationconfigcontext)
- [DatabaseConfiguration:AggregationContext](#databaseconfigurationaggregationcontext)
- [DatabaseConfiguration:EventLogContext](#databaseconfigurationeventlogcontext)
- [DatabaseConfiguration:IdentityContext](#databaseconfigurationidentitycontext)
- [DecodeEventsConfiguration](#decodeeventsconfiguration)
- [DeviceDownloaderConfiguration](#devicedownloaderconfiguration)
- [DeviceEventLoggingConfiguration](#deviceeventloggingconfiguration)
- [EventLogAggregateConfiguration](#eventlogaggregateconfiguration)
- [EventLogExtractConfiguration](#eventlogextractconfiguration)
- [EventLogImporterConfiguration](#eventlogimporterconfiguration)
- [EventLogTransferOptions](#eventlogtransferoptions)

## DatabaseConfiguration:ConfigContext

Contains configuration settings for database connectivity and provides logic to generate provider-specific connection strings.

[View source](https://github.com/utahudot/udot-atspm/blob/ee3250431c6e5218a1d10871a46b4b9736743192/Atspm/Infrastructure/Configuration/DatabaseConfiguration.cs#L32)

| Setting | Type | Default | Required | Environment variable | Description |
| --- | --- | --- | --- | --- | --- |
| `DBType` | `DatabaseProvider` | `DatabaseProvider.InMemory` | No | `DatabaseConfiguration__ConfigContext__DBType` | Gets or sets the type of database provider |
| `Host` | `string` | `string.Empty` | Yes | `DatabaseConfiguration__ConfigContext__Host` | Gets or sets the network address or hostname of the database server. |
| `Port` | `int?` | `Not set` | No | `DatabaseConfiguration__ConfigContext__Port` | Gets or sets the port number used to connect to the database server. If null, a provider-specific default port is used. |
| `Database` | `string` | `string.Empty` | Yes | `DatabaseConfiguration__ConfigContext__Database` | Gets or sets the name of the specific database or the file path for file-based databases like SQLite. |
| `User` | `string` | `string.Empty` | No | `DatabaseConfiguration__ConfigContext__User` | Gets or sets the username for database authentication. |
| `Password` | `string` | `string.Empty` | No | `DatabaseConfiguration__ConfigContext__Password` | Gets or sets the password for database authentication. |
| `RunMigrations` | `bool` | `Not set` | No | `DatabaseConfiguration__ConfigContext__RunMigrations` | Gets or sets a value indicating whether database migrations should be executed automatically on startup. |
| `Options` | `Dictionary<string, string>` | `new()` | No | `DatabaseConfiguration__ConfigContext__Options` | Gets or sets a dictionary of additional provider-specific connection options. |

## DatabaseConfiguration:AggregationContext

Contains configuration settings for database connectivity and provides logic to generate provider-specific connection strings.

[View source](https://github.com/utahudot/udot-atspm/blob/ee3250431c6e5218a1d10871a46b4b9736743192/Atspm/Infrastructure/Configuration/DatabaseConfiguration.cs#L32)

| Setting | Type | Default | Required | Environment variable | Description |
| --- | --- | --- | --- | --- | --- |
| `DBType` | `DatabaseProvider` | `DatabaseProvider.InMemory` | No | `DatabaseConfiguration__AggregationContext__DBType` | Gets or sets the type of database provider |
| `Host` | `string` | `string.Empty` | Yes | `DatabaseConfiguration__AggregationContext__Host` | Gets or sets the network address or hostname of the database server. |
| `Port` | `int?` | `Not set` | No | `DatabaseConfiguration__AggregationContext__Port` | Gets or sets the port number used to connect to the database server. If null, a provider-specific default port is used. |
| `Database` | `string` | `string.Empty` | Yes | `DatabaseConfiguration__AggregationContext__Database` | Gets or sets the name of the specific database or the file path for file-based databases like SQLite. |
| `User` | `string` | `string.Empty` | No | `DatabaseConfiguration__AggregationContext__User` | Gets or sets the username for database authentication. |
| `Password` | `string` | `string.Empty` | No | `DatabaseConfiguration__AggregationContext__Password` | Gets or sets the password for database authentication. |
| `RunMigrations` | `bool` | `Not set` | No | `DatabaseConfiguration__AggregationContext__RunMigrations` | Gets or sets a value indicating whether database migrations should be executed automatically on startup. |
| `Options` | `Dictionary<string, string>` | `new()` | No | `DatabaseConfiguration__AggregationContext__Options` | Gets or sets a dictionary of additional provider-specific connection options. |

## DatabaseConfiguration:EventLogContext

Contains configuration settings for database connectivity and provides logic to generate provider-specific connection strings.

[View source](https://github.com/utahudot/udot-atspm/blob/ee3250431c6e5218a1d10871a46b4b9736743192/Atspm/Infrastructure/Configuration/DatabaseConfiguration.cs#L32)

| Setting | Type | Default | Required | Environment variable | Description |
| --- | --- | --- | --- | --- | --- |
| `DBType` | `DatabaseProvider` | `DatabaseProvider.InMemory` | No | `DatabaseConfiguration__EventLogContext__DBType` | Gets or sets the type of database provider |
| `Host` | `string` | `string.Empty` | Yes | `DatabaseConfiguration__EventLogContext__Host` | Gets or sets the network address or hostname of the database server. |
| `Port` | `int?` | `Not set` | No | `DatabaseConfiguration__EventLogContext__Port` | Gets or sets the port number used to connect to the database server. If null, a provider-specific default port is used. |
| `Database` | `string` | `string.Empty` | Yes | `DatabaseConfiguration__EventLogContext__Database` | Gets or sets the name of the specific database or the file path for file-based databases like SQLite. |
| `User` | `string` | `string.Empty` | No | `DatabaseConfiguration__EventLogContext__User` | Gets or sets the username for database authentication. |
| `Password` | `string` | `string.Empty` | No | `DatabaseConfiguration__EventLogContext__Password` | Gets or sets the password for database authentication. |
| `RunMigrations` | `bool` | `Not set` | No | `DatabaseConfiguration__EventLogContext__RunMigrations` | Gets or sets a value indicating whether database migrations should be executed automatically on startup. |
| `Options` | `Dictionary<string, string>` | `new()` | No | `DatabaseConfiguration__EventLogContext__Options` | Gets or sets a dictionary of additional provider-specific connection options. |

## DatabaseConfiguration:IdentityContext

Contains configuration settings for database connectivity and provides logic to generate provider-specific connection strings.

[View source](https://github.com/utahudot/udot-atspm/blob/ee3250431c6e5218a1d10871a46b4b9736743192/Atspm/Infrastructure/Configuration/DatabaseConfiguration.cs#L32)

| Setting | Type | Default | Required | Environment variable | Description |
| --- | --- | --- | --- | --- | --- |
| `DBType` | `DatabaseProvider` | `DatabaseProvider.InMemory` | No | `DatabaseConfiguration__IdentityContext__DBType` | Gets or sets the type of database provider |
| `Host` | `string` | `string.Empty` | Yes | `DatabaseConfiguration__IdentityContext__Host` | Gets or sets the network address or hostname of the database server. |
| `Port` | `int?` | `Not set` | No | `DatabaseConfiguration__IdentityContext__Port` | Gets or sets the port number used to connect to the database server. If null, a provider-specific default port is used. |
| `Database` | `string` | `string.Empty` | Yes | `DatabaseConfiguration__IdentityContext__Database` | Gets or sets the name of the specific database or the file path for file-based databases like SQLite. |
| `User` | `string` | `string.Empty` | No | `DatabaseConfiguration__IdentityContext__User` | Gets or sets the username for database authentication. |
| `Password` | `string` | `string.Empty` | No | `DatabaseConfiguration__IdentityContext__Password` | Gets or sets the password for database authentication. |
| `RunMigrations` | `bool` | `Not set` | No | `DatabaseConfiguration__IdentityContext__RunMigrations` | Gets or sets a value indicating whether database migrations should be executed automatically on startup. |
| `Options` | `Dictionary<string, string>` | `new()` | No | `DatabaseConfiguration__IdentityContext__Options` | Gets or sets a dictionary of additional provider-specific connection options. |

## DecodeEventsConfiguration

> Configuration event log decoders

Configuration for event logs decoders

[View source](https://github.com/utahudot/udot-atspm/blob/ee3250431c6e5218a1d10871a46b4b9736743192/Atspm/Infrastructure/Configuration/DecodeEventsConfiguration.cs#L23)

| Setting | Type | Default | Required | Environment variable | Description |
| --- | --- | --- | --- | --- | --- |
| `Path` | `string` | `System.IO.Path.GetTempPath()` | No | `DecodeEventsConfiguration__Path` | Path to local directory where event logs are saved |

## DeviceDownloaderConfiguration

> Configuration for downloading event logs from devices

Options pattern model for services that implement IDeviceDownloader

[View source](https://github.com/utahudot/udot-atspm/blob/ee3250431c6e5218a1d10871a46b4b9736743192/Atspm/Infrastructure/Configuration/DeviceDownloaderConfiguration.cs#L23)

| Setting | Type | Default | Required | Environment variable | Description |
| --- | --- | --- | --- | --- | --- |
| `BasePath` | `string` | `Not set` | No | `DeviceDownloaderConfiguration__BasePath` | Base path to store downloaded event logs |
| `DeleteRemoteFile` | `bool` | `Not set` | No | `DeviceDownloaderConfiguration__DeleteRemoteFile` | Flag for deleting remote file after downloading |
| `Ping` | `bool` | `Not set` | No | `DeviceDownloaderConfiguration__Ping` | Flag to ping Device to verify Device.Ipaddress before downloading |

## DeviceEventLoggingConfiguration

> Configuration logging event log data from devices

Configuration options for device event logging

[View source](https://github.com/utahudot/udot-atspm/blob/ee3250431c6e5218a1d10871a46b4b9736743192/Atspm/Infrastructure/Configuration/DeviceEventLoggingConfiguration.cs#L25)

| Setting | Type | Default | Required | Environment variable | Description |
| --- | --- | --- | --- | --- | --- |
| `Path` | `string` | `System.IO.Path.GetTempPath()` | Yes | `DeviceEventLoggingConfiguration__Path` | The local directory path where event logs are temporarily stored or archived. |
| `ProcessingBatchSize` | `int` | `50000` | No | `DeviceEventLoggingConfiguration__ProcessingBatchSize` | The number of processed events to accumulate before performing a bulk database upsert. |
| `ParallelProcesses` | `int` | `5` | No | `DeviceEventLoggingConfiguration__ParallelProcesses` | The number of concurrent threads processing items within a single workflow instance. |
| `WorkflowBatchSize` | `int` | `20` | No | `DeviceEventLoggingConfiguration__WorkflowBatchSize` | The maximum number of workflow instances to run concurrently. |
| `DevicesBatchSize` | `int?` | `Not set` | No | `DeviceEventLoggingConfiguration__DevicesBatchSize` | The number of devices assigned to a single workflow instance. If null or 0, the system automatically balances the total device count across the available WorkflowBatchSize. |
| `SignalTimingPlanOffsetHours` | `int` | `12` | No | `DeviceEventLoggingConfiguration__SignalTimingPlanOffsetHours` | The time window (in hours) used to buffer event queries, ensuring overlapping plans are captured for comparison. |
| `DeviceEventLoggingQueryOptions` | `DeviceEventLoggingQueryOptions` | `new()` | No | `DeviceEventLoggingConfiguration__DeviceEventLoggingQueryOptions` | See DeviceEventLoggingQueryOptions. |

## EventLogAggregateConfiguration

> Configuration for aggregating the event logs

Provides configuration settings for aggregating event log data, including aggregation behavior, date filters, parallelization options, and query parameters used during the aggregation process.

[View source](https://github.com/utahudot/udot-atspm/blob/ee3250431c6e5218a1d10871a46b4b9736743192/Atspm/Infrastructure/Configuration/EventLogAggregateConfiguration.cs#L28)

| Setting | Type | Default | Required | Environment variable | Description |
| --- | --- | --- | --- | --- | --- |
| `AggregationType` | `string` | `"all"` | No | `EventLogAggregateConfiguration__AggregationType` | Gets or sets the type of aggregation to perform. This value determines how event log data is grouped, summarized, or transformed during the aggregation process. |
| `Dates` | `IEnumerable<DateTime>` | `Not set` | Yes | `EventLogAggregateConfiguration__Dates` | Gets or sets the collection of dates to include in the aggregation. Only events occurring on these dates will be processed. |
| `ParallelProcesses` | `int` | `1` | No | `EventLogAggregateConfiguration__ParallelProcesses` | Gets or sets the maximum number of aggregation processes that may run concurrently. Increasing this value can improve performance on systems with multiple CPU cores. |
| `EventAggregationQueryOptions` | `EventAggregationQueryOptions` | `new()` | No | `EventLogAggregateConfiguration__EventAggregationQueryOptions` | Gets or sets the query options used to filter and shape event data before aggregation is performed. |

## EventLogExtractConfiguration

> Configuration for extracting raw event log files

Provides configuration settings for extracting event log data, including formatting rules, date filters, inclusion and exclusion lists, and the destination directory for generated output.

[View source](https://github.com/utahudot/udot-atspm/blob/ee3250431c6e5218a1d10871a46b4b9736743192/Atspm/Infrastructure/Configuration/EventLogExtractConfiguration.cs#L25)

| Setting | Type | Default | Required | Environment variable | Description |
| --- | --- | --- | --- | --- | --- |
| `FileFormat` | `string` | `Not set` | No | `EventLogExtractConfiguration__FileFormat` | Gets or sets the file format used when exporting event log data. Common values might include CSV, JSON, or XML depending on the requirements of the consuming system. |
| `DateTimeFormat` | `string` | `Not set` | No | `EventLogExtractConfiguration__DateTimeFormat` | Gets or sets the date and time format applied to timestamps within the exported event log data. This should follow standard .NET date/time format patterns. |
| `Dates` | `IEnumerable<DateTime>` | `Not set` | No | `EventLogExtractConfiguration__Dates` | Gets or sets the collection of specific dates to extract event logs for. Only events occurring on these dates will be included in the output. |
| `Included` | `IEnumerable<string>` | `Not set` | No | `EventLogExtractConfiguration__Included` | Gets or sets a list of event identifiers or categories that should be explicitly included in the extraction. If populated, only matching events will be processed. |
| `Excluded` | `IEnumerable<string>` | `Not set` | No | `EventLogExtractConfiguration__Excluded` | Gets or sets a list of event identifiers or categories that should be excluded from the extraction. This is applied after any inclusion filters. |
| `Path` | `DirectoryInfo` | `Not set` | No | `EventLogExtractConfiguration__Path` | Gets or sets the directory where extracted event log files will be written. This must point to a valid, writable directory on the system. |

## EventLogImporterConfiguration

> Configuration for importing raw datalogs from devices

Options pattern model for services that implement IEventLogImporter

[View source](https://github.com/utahudot/udot-atspm/blob/ee3250431c6e5218a1d10871a46b4b9736743192/Atspm/Infrastructure/Configuration/EventLogImporterConfiguration.cs#L23)

| Setting | Type | Default | Required | Environment variable | Description |
| --- | --- | --- | --- | --- | --- |
| `EarliestAcceptableDate` | `DateTime` | `DateTime.Parse("01/01/1980")` | No | `EventLogImporterConfiguration__EarliestAcceptableDate` | Earliest acceptable date for importing from source |
| `DeleteSource` | `bool` | `Not set` | No | `EventLogImporterConfiguration__DeleteSource` | Flag for deleting source after importing |

## EventLogTransferOptions

> Configuration for transfering event logs between databases

Options for transferring event logs between repositories.

[View source](https://github.com/utahudot/udot-atspm/blob/ee3250431c6e5218a1d10871a46b4b9736743192/Atspm/Infrastructure/Configuration/EventLogTransferOptions.cs#L25)

| Setting | Type | Default | Required | Environment variable | Description |
| --- | --- | --- | --- | --- | --- |
| `SourceRepository` | `RepositoryConfiguration` | `new RepositoryConfiguration()` | No | `EventLogTransferOptions__SourceRepository` | Configuration for the source repository from which logs will be transferred |
| `DestinationRepository` | `RepositoryConfiguration` | `new RepositoryConfiguration()` | No | `EventLogTransferOptions__DestinationRepository` | Configuration for the destination repository to which logs will be transferred |
| `IncludedLocations` | `IEnumerable<string>` | `[]` | No | `EventLogTransferOptions__IncludedLocations` | List of Location.LocationIdentifier to include |
| `ExcludedLocations` | `IEnumerable<string>` | `[]` | No | `EventLogTransferOptions__ExcludedLocations` | List of Location.LocationIdentifier to exclude |
| `StartDate` | `DateTime?` | `Not set` | No | `EventLogTransferOptions__StartDate` | Start date for the transfer |
| `EndDate` | `DateTime?` | `Not set` | No | `EventLogTransferOptions__EndDate` | End date for the transfer |
| `IncludedDeviceIds` | `IEnumerable<int>` | `[]` | No | `EventLogTransferOptions__IncludedDeviceIds` | List of Device Id's to include |
| `DataType` | `string` | `"all"` | No | `EventLogTransferOptions__DataType` | Data type of the event logs to transfer. Defaults to "all" for all types. |

