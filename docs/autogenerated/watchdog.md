# Watchdog configuration

Configuration options available to the **Watchdog** container.

<small>Generated on September 1, 2026 at 8:35 PM UTC.</small>

## Contents

- [DatabaseConfiguration:ConfigContext](#databaseconfigurationconfigcontext)
- [DatabaseConfiguration:AggregationContext](#databaseconfigurationaggregationcontext)
- [DatabaseConfiguration:EventLogContext](#databaseconfigurationeventlogcontext)
- [DatabaseConfiguration:IdentityContext](#databaseconfigurationidentitycontext)
- [WatchdogConfiguration](#watchdogconfiguration)

## DatabaseConfiguration:ConfigContext

Contains configuration settings for database connectivity and provides logic to generate provider-specific connection strings.

[View source](https://github.com/avenueconsultants/udot-atspm-development/blob/959a545293005ce48ea370b1258fd3768f2e9032/Atspm/Infrastructure/Configuration/DatabaseConfiguration.cs#L32)

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

[View source](https://github.com/avenueconsultants/udot-atspm-development/blob/959a545293005ce48ea370b1258fd3768f2e9032/Atspm/Infrastructure/Configuration/DatabaseConfiguration.cs#L32)

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

[View source](https://github.com/avenueconsultants/udot-atspm-development/blob/959a545293005ce48ea370b1258fd3768f2e9032/Atspm/Infrastructure/Configuration/DatabaseConfiguration.cs#L32)

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

[View source](https://github.com/avenueconsultants/udot-atspm-development/blob/959a545293005ce48ea370b1258fd3768f2e9032/Atspm/Infrastructure/Configuration/DatabaseConfiguration.cs#L32)

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

## WatchdogConfiguration

> Configuration for Watchdog scan behavior

Configuration values used by Watchdog scans, report windows, thresholds, and email behavior.

[View source](https://github.com/avenueconsultants/udot-atspm-development/blob/959a545293005ce48ea370b1258fd3768f2e9032/Atspm/Infrastructure/Configuration/WatchdogConfiguration.cs#L23)

| Setting | Type | Default | Required | Options | Environment variable | Description |
| --- | --- | --- | --- | --- | --- | --- |
| `PmScanDate` | `DateTime` | `Not set` | No |  | `WatchdogConfiguration__PmScanDate` | Date whose evening period is checked for detector and record-count issues. |
| `AmScanDate` | `DateTime` | `Not set` | No |  | `WatchdogConfiguration__AmScanDate` | Date whose early-morning period is checked for phase-termination and pedestrian issues. |
| `RampMissedDetectorHitsStartScanDate` | `DateTime` | `Not set` | No |  | `WatchdogConfiguration__RampMissedDetectorHitsStartScanDate` | First date in the ramp-detector missed-event scan range. |
| `RampMissedDetectorHitsEndScanDate` | `DateTime` | `Not set` | No |  | `WatchdogConfiguration__RampMissedDetectorHitsEndScanDate` | Last date in the ramp-detector missed-event scan range. |
| `TimeZoneId` | `string` | `DefaultTimeZoneId` | No |  | `WatchdogConfiguration__TimeZoneId` | Time-zone identifier used to derive scan dates. |
| `AmStartHour` | `int` | `1` | No |  | `WatchdogConfiguration__AmStartHour` | Inclusive start hour for the morning phase-termination scan. |
| `AmEndHour` | `int` | `5` | No |  | `WatchdogConfiguration__AmEndHour` | Exclusive end hour for the morning phase-termination scan. |
| `PmPeakStartHour` | `int` | `18` | No |  | `WatchdogConfiguration__PmPeakStartHour` | Inclusive start hour for the previous-day PM detector scan. |
| `PmPeakEndHour` | `int` | `17` | No |  | `WatchdogConfiguration__PmPeakEndHour` | Exclusive end hour for the PM detector scan. |
| `RampDetectorStartHour` | `int` | `7` | No |  | `WatchdogConfiguration__RampDetectorStartHour` | Inclusive start hour for ramp-detector volume checks. |
| `RampDetectorEndHour` | `int` | `8` | No |  | `WatchdogConfiguration__RampDetectorEndHour` | Exclusive end hour for ramp-detector volume checks. |
| `RampMissedDetectorHitStartHour` | `int` | `15` | No |  | `WatchdogConfiguration__RampMissedDetectorHitStartHour` | Inclusive start hour for the ramp missed-event analysis window. |
| `RampMissedDetectorHitEndHour` | `int` | `7` | No |  | `WatchdogConfiguration__RampMissedDetectorHitEndHour` | Exclusive end hour for the ramp missed-event analysis window. |
| `RampMainlineStartHour` | `int` | `15` | No |  | `WatchdogConfiguration__RampMainlineStartHour` | Inclusive start hour for ramp-mainline detector checks. |
| `RampMainlineEndHour` | `int` | `19` | No |  | `WatchdogConfiguration__RampMainlineEndHour` | Exclusive end hour for ramp-mainline detector checks. |
| `RampStuckQueueStartHour` | `int` | `1` | No |  | `WatchdogConfiguration__RampStuckQueueStartHour` | Inclusive start hour for ramp stuck-queue checks. |
| `RampStuckQueueEndHour` | `int` | `4` | No |  | `WatchdogConfiguration__RampStuckQueueEndHour` | Exclusive end hour for ramp stuck-queue checks. |
| `WeekdayOnly` | `bool` | `true` | No |  | `WatchdogConfiguration__WeekdayOnly` | Whether scheduled scans are skipped on Saturdays and Sundays. |
| `ConsecutiveCount` | `int` | `3` | No |  | `WatchdogConfiguration__ConsecutiveCount` | Number of consecutive occurrences required to report a stuck-pedestrian issue. |
| `MinPhaseTerminations` | `int` | `50` | No |  | `WatchdogConfiguration__MinPhaseTerminations` | Minimum non-gap or total phase terminations required before percentage thresholds are evaluated. |
| `PercentThreshold` | `double` | `.9` | No |  | `WatchdogConfiguration__PercentThreshold` | Fraction of phase terminations that must be force-offs or max-outs to report an issue. |
| `MinimumRecords` | `int` | `500` | No |  | `WatchdogConfiguration__MinimumRecords` | Minimum event records a location must have to pass the PM record-count check. |
| `LowHitThreshold` | `int` | `50` | No |  | `WatchdogConfiguration__LowHitThreshold` | Detector-volume count below which a standard detector is reported as low-hit. |
| `LowHitRampThreshold` | `int` | `10` | No |  | `WatchdogConfiguration__LowHitRampThreshold` | Detector-volume count below which a ramp detector is reported as low-hit. |
| `MaximumPedestrianEvents` | `int` | `200` | No |  | `WatchdogConfiguration__MaximumPedestrianEvents` | Pedestrian-event count above which a phase is reported for excessive activations. |
| `RampMissedEventsThreshold` | `int` | `3` | No |  | `WatchdogConfiguration__RampMissedEventsThreshold` | Number of missed ramp event buckets above which an issue is reported. |
| `EmailAllErrors` | `bool` | `Not set` | No |  | `WatchdogConfiguration__EmailAllErrors` | Whether emails include recurring errors in addition to newly detected errors. |
| `EmailPmErrors` | `bool` | `true` | No |  | `WatchdogConfiguration__EmailPmErrors` | Whether PM detector and record-count issues are analyzed and emailed. |
| `EmailAmErrors` | `bool` | `true` | No |  | `WatchdogConfiguration__EmailAmErrors` | Whether AM phase-termination and pedestrian issues are analyzed and emailed. |
| `EmailRampErrors` | `bool` | `true` | No |  | `WatchdogConfiguration__EmailRampErrors` | Whether ramp detector issues are analyzed and emailed. |
| `DefaultEmailAddress` | `string` | `Not set` | No |  | `WatchdogConfiguration__DefaultEmailAddress` | Fallback recipient address used for watchdog email messages. |
| `Sort` | `string` | `Not set` | No |  | `WatchdogConfiguration__Sort` | Sort expression applied when ordering watchdog errors in reports. |

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
  "WatchdogConfiguration": {
    "PmScanDate": "2026-01-01T00:00:00Z",
    "AmScanDate": "2026-01-01T00:00:00Z",
    "RampMissedDetectorHitsStartScanDate": "2026-01-01T00:00:00Z",
    "RampMissedDetectorHitsEndScanDate": "2026-01-01T00:00:00Z",
    "TimeZoneId": "America/Denver",
    "AmStartHour": 1,
    "AmEndHour": 5,
    "PmPeakStartHour": 18,
    "PmPeakEndHour": 17,
    "RampDetectorStartHour": 7,
    "RampDetectorEndHour": 8,
    "RampMissedDetectorHitStartHour": 15,
    "RampMissedDetectorHitEndHour": 7,
    "RampMainlineStartHour": 15,
    "RampMainlineEndHour": 19,
    "RampStuckQueueStartHour": 1,
    "RampStuckQueueEndHour": 4,
    "WeekdayOnly": true,
    "ConsecutiveCount": 3,
    "MinPhaseTerminations": 50,
    "PercentThreshold": 0.9,
    "MinimumRecords": 500,
    "LowHitThreshold": 50,
    "LowHitRampThreshold": 10,
    "MaximumPedestrianEvents": 200,
    "RampMissedEventsThreshold": 3,
    "EmailAllErrors": false,
    "EmailPmErrors": true,
    "EmailAmErrors": true,
    "EmailRampErrors": true,
    "DefaultEmailAddress": "atspm@example.com",
    "Sort": "replace-me"
  }
}
```
