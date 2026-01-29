# Event Log Utility — Configuration Reference

This page documents all configuration options available to the **Event Log Utility** container.

## Table of Contents

- [DecodeEventsConfiguration](#decodeeventsconfiguration)
- [DeviceDownloaderConfiguration](#devicedownloaderconfiguration)
- [EventLogAggregateConfiguration](#eventlogaggregateconfiguration)
- [EventLogExtractConfiguration](#eventlogextractconfiguration)
- [EventLogImporterConfiguration](#eventlogimporterconfiguration)
- [EventLogTransferOptions](#eventlogtransferoptions)

---

## DecodeEventsConfiguration

> Configuration event log decoders
> Configuration for event logs decoders

[View Source](src/MyApp.Configuration/DecodeEventsConfiguration.cs)

<details>
<summary><strong>View Settings</strong></summary>

| Key | Type | Default | Required | Env Var | Description |
|-----|------|---------|----------|---------|-------------|
| `Path` | `String` | `C:\Users\christianbaker\AppData\Local\Temp\` | No | `DecodeEventsConfiguration__Path` | Path to local directory where event logs are saved |

</details>

---

## DeviceDownloaderConfiguration

> Configuration for downloading event logs from devices
> Options pattern model for services that implement

[View Source](src/MyApp.Configuration/DeviceDownloaderConfiguration.cs)

<details>
<summary><strong>View Settings</strong></summary>

| Key | Type | Default | Required | Env Var | Description |
|-----|------|---------|----------|---------|-------------|
| `BasePath` | `String` | `` | No | `DeviceDownloaderConfiguration__BasePath` | Base path to store downloaded event logs |
| `DeleteRemoteFile` | `Boolean` | `False` | No | `DeviceDownloaderConfiguration__DeleteRemoteFile` | Flag for deleting remote file after downloading |
| `Ping` | `Boolean` | `False` | No | `DeviceDownloaderConfiguration__Ping` | Flag to ping to verify before downloading |

</details>

---

## EventLogAggregateConfiguration

> Configuration for aggregating the event logs
> Provides configuration settings for aggregating event log data, including aggregation behavior, date filters, parallelization options, and query parameters used during the aggregation process.

[View Source](src/MyApp.Configuration/EventLogAggregateConfiguration.cs)

<details>
<summary><strong>View Settings</strong></summary>

| Key | Type | Default | Required | Env Var | Description |
|-----|------|---------|----------|---------|-------------|
| `AggregationType` | `String` | `` | No | `EventLogAggregateConfiguration__AggregationType` | Gets or sets the type of aggregation to perform. This value determines how event log data is grouped, summarized, or transformed during the aggregation process. |
| `Dates` | `IEnumerable`1` | `` | No | `EventLogAggregateConfiguration__Dates` | Gets or sets the collection of dates to include in the aggregation. Only events occurring on these dates will be processed. |
| `ParallelProcesses` | `Int32` | `1` | No | `EventLogAggregateConfiguration__ParallelProcesses` | Gets or sets the maximum number of aggregation processes that may run concurrently. Increasing this value can improve performance on systems with multiple CPU cores. |
| `EventAggregationQueryOptions` | `EventAggregationQueryOptions` | `EventAggregationQueryOptions***************************************************
EventAggregationQueryOptions***************************************************
` | No | `EventLogAggregateConfiguration__EventAggregationQueryOptions` | Gets or sets the query options used to filter and shape event data before aggregation is performed. |

</details>

---

## EventLogExtractConfiguration

> Configuration for extracting raw event log files
> Provides configuration settings for extracting event log data, including formatting rules, date filters, inclusion and exclusion lists, and the destination directory for generated output.

[View Source](src/MyApp.Configuration/EventLogExtractConfiguration.cs)

<details>
<summary><strong>View Settings</strong></summary>

| Key | Type | Default | Required | Env Var | Description |
|-----|------|---------|----------|---------|-------------|
| `FileFormat` | `String` | `` | No | `EventLogExtractConfiguration__FileFormat` | Gets or sets the file format used when exporting event log data. Common values might include CSV, JSON, or XML depending on the requirements of the consuming system. |
| `DateTimeFormat` | `String` | `` | No | `EventLogExtractConfiguration__DateTimeFormat` | Gets or sets the date and time format applied to timestamps within the exported event log data. This should follow standard .NET date/time format patterns. |
| `Dates` | `IEnumerable`1` | `` | No | `EventLogExtractConfiguration__Dates` | Gets or sets the collection of specific dates to extract event logs for. Only events occurring on these dates will be included in the output. |
| `Included` | `IEnumerable`1` | `` | No | `EventLogExtractConfiguration__Included` | Gets or sets a list of event identifiers or categories that should be explicitly included in the extraction. If populated, only matching events will be processed. |
| `Excluded` | `IEnumerable`1` | `` | No | `EventLogExtractConfiguration__Excluded` | Gets or sets a list of event identifiers or categories that should be excluded from the extraction. This is applied after any inclusion filters. |
| `Path` | `DirectoryInfo` | `` | No | `EventLogExtractConfiguration__Path` | Gets or sets the directory where extracted event log files will be written. This must point to a valid, writable directory on the system. |

</details>

---

## EventLogImporterConfiguration

> Configuration for importing raw datalogs from devices
> Options pattern model for services that implement

[View Source](src/MyApp.Configuration/EventLogImporterConfiguration.cs)

<details>
<summary><strong>View Settings</strong></summary>

| Key | Type | Default | Required | Env Var | Description |
|-----|------|---------|----------|---------|-------------|
| `EarliestAcceptableDate` | `DateTime` | `1/1/1980 12:00:00 AM` | No | `EventLogImporterConfiguration__EarliestAcceptableDate` | Earliest acceptable date for importing from source |
| `DeleteSource` | `Boolean` | `False` | No | `EventLogImporterConfiguration__DeleteSource` | Flag for deleting source after importing |

</details>

---

## EventLogTransferOptions

> Configuration for transfering event logs between databases
> Options for transferring event logs between repositories.

[View Source](src/MyApp.Configuration/EventLogTransferOptions.cs)

<details>
<summary><strong>View Settings</strong></summary>

| Key | Type | Default | Required | Env Var | Description |
|-----|------|---------|----------|---------|-------------|
| `SourceRepository` | `RepositoryConfiguration` | `|` | No | `EventLogTransferOptions__SourceRepository` | Configuration for the source repository from which logs will be transferred |
| `DestinationRepository` | `RepositoryConfiguration` | `|` | No | `EventLogTransferOptions__DestinationRepository` | Configuration for the destination repository to which logs will be transferred |
| `IncludedLocations` | `IEnumerable`1` | `System.String[]` | No | `EventLogTransferOptions__IncludedLocations` | List of to include |
| `ExcludedLocations` | `IEnumerable`1` | `System.String[]` | No | `EventLogTransferOptions__ExcludedLocations` | List of to exclude |
| `StartDate` | `Nullable`1` | `` | No | `EventLogTransferOptions__StartDate` | Start date for the transfer |
| `EndDate` | `Nullable`1` | `` | No | `EventLogTransferOptions__EndDate` | End date for the transfer |
| `IncludedDeviceIds` | `IEnumerable`1` | `System.Int32[]` | No | `EventLogTransferOptions__IncludedDeviceIds` | List of Id's to include |
| `DataType` | `String` | `all` | No | `EventLogTransferOptions__DataType` | Data type of the event logs to transfer. Defaults to "all" for all types. |

</details>

---

