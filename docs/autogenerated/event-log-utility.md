# Event Log Utility configuration

Configuration options available to the **Event Log Utility** container.

<small>Generated on August 18, 2026 at 8:09 PM UTC.</small>

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

[View source](https://github.com/utahudot/udot-atspm/blob/6b7f1ab23f0ae3d22fc0c437531aa05695e2e70b/Atspm/Infrastructure/Configuration/DatabaseConfiguration.cs#L32)

| Setting | Type | Default | Required | Options | Environment variable | Description |
| --- | --- | --- | --- | --- | --- | --- |
| `DBType` | `DatabaseProvider` | `DatabaseProvider.InMemory` | No | `InMemory`<br>`SqlServer`<br>`PostgreSql`<br>`MySql`<br>`Oracle`<br>`Sqlite` | `DatabaseConfiguration__ConfigContext__DBType` | Gets or sets the type of database provider |
| `Host` | `string` | `string.Empty` | Yes |  | `DatabaseConfiguration__ConfigContext__Host` | Gets or sets the network address or hostname of the database server. |
| `Port` | `int?` | `Not set` | No |  | `DatabaseConfiguration__ConfigContext__Port` | Gets or sets the port number used to connect to the database server. If null, a provider-specific default port is used. |
| `Database` | `string` | `string.Empty` | Yes |  | `DatabaseConfiguration__ConfigContext__Database` | Gets or sets the name of the specific database or the file path for file-based databases like SQLite. |
| `User` | `string` | `string.Empty` | No |  | `DatabaseConfiguration__ConfigContext__User` | Gets or sets the username for database authentication. |
| `Password` | `string` | `string.Empty` | No |  | `DatabaseConfiguration__ConfigContext__Password` | Gets or sets the password for database authentication. |
| `RunMigrations` | `bool` | `Not set` | No |  | `DatabaseConfiguration__ConfigContext__RunMigrations` | Gets or sets a value indicating whether database migrations should be executed automatically on startup. |
| `Options` | `Dictionary<string, string>` | `new()` | No |  | `DatabaseConfiguration__ConfigContext__Options__KEY` | Gets or sets a dictionary of additional provider-specific connection options. |

## DatabaseConfiguration:AggregationContext

Contains configuration settings for database connectivity and provides logic to generate provider-specific connection strings.

[View source](https://github.com/utahudot/udot-atspm/blob/6b7f1ab23f0ae3d22fc0c437531aa05695e2e70b/Atspm/Infrastructure/Configuration/DatabaseConfiguration.cs#L32)

| Setting | Type | Default | Required | Options | Environment variable | Description |
| --- | --- | --- | --- | --- | --- | --- |
| `DBType` | `DatabaseProvider` | `DatabaseProvider.InMemory` | No | `InMemory`<br>`SqlServer`<br>`PostgreSql`<br>`MySql`<br>`Oracle`<br>`Sqlite` | `DatabaseConfiguration__AggregationContext__DBType` | Gets or sets the type of database provider |
| `Host` | `string` | `string.Empty` | Yes |  | `DatabaseConfiguration__AggregationContext__Host` | Gets or sets the network address or hostname of the database server. |
| `Port` | `int?` | `Not set` | No |  | `DatabaseConfiguration__AggregationContext__Port` | Gets or sets the port number used to connect to the database server. If null, a provider-specific default port is used. |
| `Database` | `string` | `string.Empty` | Yes |  | `DatabaseConfiguration__AggregationContext__Database` | Gets or sets the name of the specific database or the file path for file-based databases like SQLite. |
| `User` | `string` | `string.Empty` | No |  | `DatabaseConfiguration__AggregationContext__User` | Gets or sets the username for database authentication. |
| `Password` | `string` | `string.Empty` | No |  | `DatabaseConfiguration__AggregationContext__Password` | Gets or sets the password for database authentication. |
| `RunMigrations` | `bool` | `Not set` | No |  | `DatabaseConfiguration__AggregationContext__RunMigrations` | Gets or sets a value indicating whether database migrations should be executed automatically on startup. |
| `Options` | `Dictionary<string, string>` | `new()` | No |  | `DatabaseConfiguration__AggregationContext__Options__KEY` | Gets or sets a dictionary of additional provider-specific connection options. |

## DatabaseConfiguration:EventLogContext

Contains configuration settings for database connectivity and provides logic to generate provider-specific connection strings.

[View source](https://github.com/utahudot/udot-atspm/blob/6b7f1ab23f0ae3d22fc0c437531aa05695e2e70b/Atspm/Infrastructure/Configuration/DatabaseConfiguration.cs#L32)

| Setting | Type | Default | Required | Options | Environment variable | Description |
| --- | --- | --- | --- | --- | --- | --- |
| `DBType` | `DatabaseProvider` | `DatabaseProvider.InMemory` | No | `InMemory`<br>`SqlServer`<br>`PostgreSql`<br>`MySql`<br>`Oracle`<br>`Sqlite` | `DatabaseConfiguration__EventLogContext__DBType` | Gets or sets the type of database provider |
| `Host` | `string` | `string.Empty` | Yes |  | `DatabaseConfiguration__EventLogContext__Host` | Gets or sets the network address or hostname of the database server. |
| `Port` | `int?` | `Not set` | No |  | `DatabaseConfiguration__EventLogContext__Port` | Gets or sets the port number used to connect to the database server. If null, a provider-specific default port is used. |
| `Database` | `string` | `string.Empty` | Yes |  | `DatabaseConfiguration__EventLogContext__Database` | Gets or sets the name of the specific database or the file path for file-based databases like SQLite. |
| `User` | `string` | `string.Empty` | No |  | `DatabaseConfiguration__EventLogContext__User` | Gets or sets the username for database authentication. |
| `Password` | `string` | `string.Empty` | No |  | `DatabaseConfiguration__EventLogContext__Password` | Gets or sets the password for database authentication. |
| `RunMigrations` | `bool` | `Not set` | No |  | `DatabaseConfiguration__EventLogContext__RunMigrations` | Gets or sets a value indicating whether database migrations should be executed automatically on startup. |
| `Options` | `Dictionary<string, string>` | `new()` | No |  | `DatabaseConfiguration__EventLogContext__Options__KEY` | Gets or sets a dictionary of additional provider-specific connection options. |

## DatabaseConfiguration:IdentityContext

Contains configuration settings for database connectivity and provides logic to generate provider-specific connection strings.

[View source](https://github.com/utahudot/udot-atspm/blob/6b7f1ab23f0ae3d22fc0c437531aa05695e2e70b/Atspm/Infrastructure/Configuration/DatabaseConfiguration.cs#L32)

| Setting | Type | Default | Required | Options | Environment variable | Description |
| --- | --- | --- | --- | --- | --- | --- |
| `DBType` | `DatabaseProvider` | `DatabaseProvider.InMemory` | No | `InMemory`<br>`SqlServer`<br>`PostgreSql`<br>`MySql`<br>`Oracle`<br>`Sqlite` | `DatabaseConfiguration__IdentityContext__DBType` | Gets or sets the type of database provider |
| `Host` | `string` | `string.Empty` | Yes |  | `DatabaseConfiguration__IdentityContext__Host` | Gets or sets the network address or hostname of the database server. |
| `Port` | `int?` | `Not set` | No |  | `DatabaseConfiguration__IdentityContext__Port` | Gets or sets the port number used to connect to the database server. If null, a provider-specific default port is used. |
| `Database` | `string` | `string.Empty` | Yes |  | `DatabaseConfiguration__IdentityContext__Database` | Gets or sets the name of the specific database or the file path for file-based databases like SQLite. |
| `User` | `string` | `string.Empty` | No |  | `DatabaseConfiguration__IdentityContext__User` | Gets or sets the username for database authentication. |
| `Password` | `string` | `string.Empty` | No |  | `DatabaseConfiguration__IdentityContext__Password` | Gets or sets the password for database authentication. |
| `RunMigrations` | `bool` | `Not set` | No |  | `DatabaseConfiguration__IdentityContext__RunMigrations` | Gets or sets a value indicating whether database migrations should be executed automatically on startup. |
| `Options` | `Dictionary<string, string>` | `new()` | No |  | `DatabaseConfiguration__IdentityContext__Options__KEY` | Gets or sets a dictionary of additional provider-specific connection options. |

## DecodeEventsConfiguration

> Configuration for event log decoders

[View source](https://github.com/utahudot/udot-atspm/blob/6b7f1ab23f0ae3d22fc0c437531aa05695e2e70b/Atspm/Infrastructure/Configuration/DecodeEventsConfiguration.cs#L20)

| Setting | Type | Default | Required | Options | Environment variable | Description |
| --- | --- | --- | --- | --- | --- | --- |
| `Path` | `string` | `System.IO.Path.GetTempPath()` | No |  | `DecodeEventsConfiguration__Path` | Path to local directory where event logs are saved |

## DeviceDownloaderConfiguration

> Configuration for downloading event logs from devices

Options pattern model for services that implement IDeviceDownloader

[View source](https://github.com/utahudot/udot-atspm/blob/6b7f1ab23f0ae3d22fc0c437531aa05695e2e70b/Atspm/Infrastructure/Configuration/DeviceDownloaderConfiguration.cs#L23)

| Setting | Type | Default | Required | Options | Environment variable | Description |
| --- | --- | --- | --- | --- | --- | --- |
| `BasePath` | `string` | `Not set` | No |  | `DeviceDownloaderConfiguration__BasePath` | Base path to store downloaded event logs |
| `DeleteRemoteFile` | `bool` | `Not set` | No |  | `DeviceDownloaderConfiguration__DeleteRemoteFile` | Flag for deleting remote file after downloading |
| `Ping` | `bool` | `Not set` | No |  | `DeviceDownloaderConfiguration__Ping` | Flag to ping Device to verify Device.Ipaddress before downloading |

## DeviceEventLoggingConfiguration

> Configuration for device event logging

Configuration options for the Device Event Logging background service.

[View source](https://github.com/utahudot/udot-atspm/blob/6b7f1ab23f0ae3d22fc0c437531aa05695e2e70b/Atspm/Infrastructure/Configuration/DeviceEventLoggingConfiguration.cs#L25)

| Setting | Type | Default | Required | Options | Environment variable | Description |
| --- | --- | --- | --- | --- | --- | --- |
| `Path` | `string` | `System.IO.Path.GetTempPath()` | Yes |  | `DeviceEventLoggingConfiguration__Path` | The local directory path where event logs are temporarily stored or archived. |
| `ProcessingBatchSize` | `int` | `50000` | No |  | `DeviceEventLoggingConfiguration__ProcessingBatchSize` | The number of processed events to accumulate before performing a bulk database upsert. |
| `ParallelProcesses` | `int` | `5` | No |  | `DeviceEventLoggingConfiguration__ParallelProcesses` | The number of concurrent threads processing items within a single workflow instance. |
| `WorkflowBatchSize` | `int` | `20` | No |  | `DeviceEventLoggingConfiguration__WorkflowBatchSize` | The maximum number of workflow instances to run concurrently. |
| `DevicesBatchSize` | `int?` | `Not set` | No |  | `DeviceEventLoggingConfiguration__DevicesBatchSize` | The number of devices assigned to a single workflow instance. If null or 0, the system automatically balances the total device count across the available WorkflowBatchSize. |
| `SignalTimingPlanOffsetHours` | `int` | `12` | No |  | `DeviceEventLoggingConfiguration__SignalTimingPlanOffsetHours` | The time window (in hours) used to buffer event queries, ensuring overlapping plans are captured for comparison. |
| `DeviceEventLoggingQueryOptions` | `DeviceEventLoggingQueryOptions` | `new()` | No |  | `DeviceEventLoggingConfiguration__DeviceEventLoggingQueryOptions__IncludedDevices__0`<br>`DeviceEventLoggingConfiguration__DeviceEventLoggingQueryOptions__IncludeConfigurations__0`<br>`DeviceEventLoggingConfiguration__DeviceEventLoggingQueryOptions__DeviceType`<br>`DeviceEventLoggingConfiguration__DeviceEventLoggingQueryOptions__TransportProtocol`<br>`DeviceEventLoggingConfiguration__DeviceEventLoggingQueryOptions__DeviceStatus`<br>`DeviceEventLoggingConfiguration__DeviceEventLoggingQueryOptions__IncludedLocations__0`<br>`DeviceEventLoggingConfiguration__DeviceEventLoggingQueryOptions__ExcludedLocations__0`<br>`DeviceEventLoggingConfiguration__DeviceEventLoggingQueryOptions__IncludedLocationTypes__0`<br>`DeviceEventLoggingConfiguration__DeviceEventLoggingQueryOptions__IncludedAreas__0`<br>`DeviceEventLoggingConfiguration__DeviceEventLoggingQueryOptions__IncludedJurisdictions__0`<br>`DeviceEventLoggingConfiguration__DeviceEventLoggingQueryOptions__IncludedRegions__0` | See DeviceEventLoggingQueryOptions. |

## EventLogAggregateConfiguration

> Configuration for aggregating the event logs

[View source](https://github.com/utahudot/udot-atspm/blob/6b7f1ab23f0ae3d22fc0c437531aa05695e2e70b/Atspm/Infrastructure/Configuration/EventLogAggregateConfiguration.cs#L23)

| Setting | Type | Default | Required | Options | Environment variable | Description |
| --- | --- | --- | --- | --- | --- | --- |
| `AggregationType` | `string` | `"all"` | No |  | `EventLogAggregateConfiguration__AggregationType` |  |
| `Dates` | `IEnumerable<DateTime>` | `Not set` | Yes |  | `EventLogAggregateConfiguration__Dates__0` |  |
| `ParallelProcesses` | `int` | `1` | No |  | `EventLogAggregateConfiguration__ParallelProcesses` | Amount of processes that can be run in parallel |
| `EventAggregationQueryOptions` | `EventAggregationQueryOptions` | `new()` | No |  | `EventLogAggregateConfiguration__EventAggregationQueryOptions__IncludedLocations__0`<br>`EventLogAggregateConfiguration__EventAggregationQueryOptions__ExcludedLocations__0`<br>`EventLogAggregateConfiguration__EventAggregationQueryOptions__IncludedLocationTypes__0`<br>`EventLogAggregateConfiguration__EventAggregationQueryOptions__IncludedAreas__0`<br>`EventLogAggregateConfiguration__EventAggregationQueryOptions__IncludedJurisdictions__0`<br>`EventLogAggregateConfiguration__EventAggregationQueryOptions__IncludedRegions__0` | See EventAggregationQueryOptions. |

## EventLogExtractConfiguration

> Configuration for extracting raw event log files

[View source](https://github.com/utahudot/udot-atspm/blob/6b7f1ab23f0ae3d22fc0c437531aa05695e2e70b/Atspm/Infrastructure/Configuration/EventLogExtractConfiguration.cs#L20)

| Setting | Type | Default | Required | Options | Environment variable | Description |
| --- | --- | --- | --- | --- | --- | --- |
| `FileFormat` | `string` | `Not set` | No |  | `EventLogExtractConfiguration__FileFormat` |  |
| `DateTimeFormat` | `string` | `Not set` | No |  | `EventLogExtractConfiguration__DateTimeFormat` |  |
| `Dates` | `IEnumerable<DateTime>` | `Not set` | No |  | `EventLogExtractConfiguration__Dates__0` |  |
| `Included` | `IEnumerable<string>` | `Not set` | No |  | `EventLogExtractConfiguration__Included__0` |  |
| `Excluded` | `IEnumerable<string>` | `Not set` | No |  | `EventLogExtractConfiguration__Excluded__0` |  |
| `Path` | `DirectoryInfo` | `Not set` | No |  | `EventLogExtractConfiguration__Path` |  |

## EventLogImporterConfiguration

> Configuration for importing raw data logs from devices

Options pattern model for services that implement IEventLogImporter

[View source](https://github.com/utahudot/udot-atspm/blob/6b7f1ab23f0ae3d22fc0c437531aa05695e2e70b/Atspm/Infrastructure/Configuration/EventLogImporterConfiguration.cs#L23)

| Setting | Type | Default | Required | Options | Environment variable | Description |
| --- | --- | --- | --- | --- | --- | --- |
| `EarliestAcceptableDate` | `DateTime` | `DateTime.Parse("01/01/1980")` | No |  | `EventLogImporterConfiguration__EarliestAcceptableDate` | Earliest acceptable date for importing from source |
| `DeleteSource` | `bool` | `Not set` | No |  | `EventLogImporterConfiguration__DeleteSource` | Flag for deleting source after importing |

## EventLogTransferOptions

> Configuration for transferring event logs between databases

Options for transferring event logs between repositories.

[View source](https://github.com/utahudot/udot-atspm/blob/6b7f1ab23f0ae3d22fc0c437531aa05695e2e70b/Atspm/Infrastructure/Configuration/EventLogTransferOptions.cs#L25)

| Setting | Type | Default | Required | Options | Environment variable | Description |
| --- | --- | --- | --- | --- | --- | --- |
| `SourceRepository` | `RepositoryConfiguration` | `new RepositoryConfiguration()` | No |  | `EventLogTransferOptions__SourceRepository__Provider`<br>`EventLogTransferOptions__SourceRepository__ConnectionString` | Configuration for the source repository from which logs will be transferred |
| `DestinationRepository` | `RepositoryConfiguration` | `new RepositoryConfiguration()` | No |  | `EventLogTransferOptions__DestinationRepository__Provider`<br>`EventLogTransferOptions__DestinationRepository__ConnectionString` | Configuration for the destination repository to which logs will be transferred |
| `IncludedLocations` | `IEnumerable<string>` | `[]` | No |  | `EventLogTransferOptions__IncludedLocations__0` | List of Location.LocationIdentifier to include |
| `ExcludedLocations` | `IEnumerable<string>` | `[]` | No |  | `EventLogTransferOptions__ExcludedLocations__0` | List of Location.LocationIdentifier to exclude |
| `StartDate` | `DateTime?` | `Not set` | No |  | `EventLogTransferOptions__StartDate` | Start date for the transfer |
| `EndDate` | `DateTime?` | `Not set` | No |  | `EventLogTransferOptions__EndDate` | End date for the transfer |
| `IncludedDeviceIds` | `IEnumerable<int>` | `[]` | No |  | `EventLogTransferOptions__IncludedDeviceIds__0` | List of Device Id's to include |
| `DataType` | `string` | `"all"` | No |  | `EventLogTransferOptions__DataType` | Data type of the event logs to transfer. Defaults to "all" for all types. |

## Example JSON configuration

This example includes every documented setting. Replace placeholder secrets, URLs, paths, and connection details before use.

```json
{
  "DatabaseConfiguration": {
    "ConfigContext": {
      "DBType": "InMemory",
      "Host": "localhost",
      "Port": null,
      "Database": "atspm",
      "User": "atspm",
      "Password": "replace-with-a-secret",
      "RunMigrations": false,
      "Options": {}
    },
    "AggregationContext": {
      "DBType": "InMemory",
      "Host": "localhost",
      "Port": null,
      "Database": "atspm",
      "User": "atspm",
      "Password": "replace-with-a-secret",
      "RunMigrations": false,
      "Options": {}
    },
    "EventLogContext": {
      "DBType": "InMemory",
      "Host": "localhost",
      "Port": null,
      "Database": "atspm",
      "User": "atspm",
      "Password": "replace-with-a-secret",
      "RunMigrations": false,
      "Options": {}
    },
    "IdentityContext": {
      "DBType": "InMemory",
      "Host": "localhost",
      "Port": null,
      "Database": "atspm",
      "User": "atspm",
      "Password": "replace-with-a-secret",
      "RunMigrations": false,
      "Options": {}
    }
  },
  "DecodeEventsConfiguration": {
    "Path": "./data"
  },
  "DeviceDownloaderConfiguration": {
    "BasePath": "./data",
    "DeleteRemoteFile": false,
    "Ping": false
  },
  "DeviceEventLoggingConfiguration": {
    "Path": "./data",
    "ProcessingBatchSize": 50000,
    "ParallelProcesses": 5,
    "WorkflowBatchSize": 20,
    "DevicesBatchSize": null,
    "SignalTimingPlanOffsetHours": 12,
    "DeviceEventLoggingQueryOptions": {}
  },
  "EventLogAggregateConfiguration": {
    "AggregationType": "all",
    "Dates": [],
    "ParallelProcesses": 1,
    "EventAggregationQueryOptions": {}
  },
  "EventLogExtractConfiguration": {
    "FileFormat": "csv",
    "DateTimeFormat": "yyyy-MM-dd HH:mm:ss",
    "Dates": [],
    "Included": [],
    "Excluded": [],
    "Path": "./data"
  },
  "EventLogImporterConfiguration": {
    "EarliestAcceptableDate": "2026-01-01T00:00:00Z",
    "DeleteSource": false
  },
  "EventLogTransferOptions": {
    "SourceRepository": {
      "Provider": "PostgreSql",
      "ConnectionString": "Host=localhost;Port=5432;Database=atspm;Username=atspm;Password=replace-with-a-secret"
    },
    "DestinationRepository": {
      "Provider": "PostgreSql",
      "ConnectionString": "Host=localhost;Port=5432;Database=atspm;Username=atspm;Password=replace-with-a-secret"
    },
    "IncludedLocations": [],
    "ExcludedLocations": [],
    "StartDate": null,
    "EndDate": null,
    "IncludedDeviceIds": [],
    "DataType": "all"
  }
}
```
