# Watchdog configuration

Configuration options available to the **Watchdog** container.

Generated from [utahudot/udot-atspm at `ee3250431c6e5218a1d10871a46b4b9736743192`](https://github.com/utahudot/udot-atspm/tree/ee3250431c6e5218a1d10871a46b4b9736743192).

## Contents

- [DatabaseConfiguration:ConfigContext](#databaseconfigurationconfigcontext)
- [DatabaseConfiguration:AggregationContext](#databaseconfigurationaggregationcontext)
- [DatabaseConfiguration:EventLogContext](#databaseconfigurationeventlogcontext)
- [DatabaseConfiguration:IdentityContext](#databaseconfigurationidentitycontext)
- [WatchdogConfiguration](#watchdogconfiguration)

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

## WatchdogConfiguration

> Configuration for Watchdog scan behavior

Configuration values used by Watchdog scans, report windows, thresholds, and email behavior.

[View source](https://github.com/utahudot/udot-atspm/blob/ee3250431c6e5218a1d10871a46b4b9736743192/Atspm/Infrastructure/Configuration/WatchdogConfiguration.cs#L23)

| Setting | Type | Default | Required | Environment variable | Description |
| --- | --- | --- | --- | --- | --- |
| `PmScanDate` | `DateTime` | `Not set` | No | `WatchdogConfiguration__PmScanDate` |  |
| `AmScanDate` | `DateTime` | `Not set` | No | `WatchdogConfiguration__AmScanDate` |  |
| `RampMissedDetectorHitsStartScanDate` | `DateTime` | `Not set` | No | `WatchdogConfiguration__RampMissedDetectorHitsStartScanDate` |  |
| `RampMissedDetectorHitsEndScanDate` | `DateTime` | `Not set` | No | `WatchdogConfiguration__RampMissedDetectorHitsEndScanDate` |  |
| `TimeZoneId` | `string` | `DefaultTimeZoneId` | No | `WatchdogConfiguration__TimeZoneId` |  |
| `AmStartHour` | `int` | `1` | No | `WatchdogConfiguration__AmStartHour` |  |
| `AmEndHour` | `int` | `5` | No | `WatchdogConfiguration__AmEndHour` |  |
| `PmPeakStartHour` | `int` | `18` | No | `WatchdogConfiguration__PmPeakStartHour` |  |
| `PmPeakEndHour` | `int` | `17` | No | `WatchdogConfiguration__PmPeakEndHour` |  |
| `RampDetectorStartHour` | `int` | `7` | No | `WatchdogConfiguration__RampDetectorStartHour` |  |
| `RampDetectorEndHour` | `int` | `8` | No | `WatchdogConfiguration__RampDetectorEndHour` |  |
| `RampMissedDetectorHitStartHour` | `int` | `15` | No | `WatchdogConfiguration__RampMissedDetectorHitStartHour` |  |
| `RampMissedDetectorHitEndHour` | `int` | `7` | No | `WatchdogConfiguration__RampMissedDetectorHitEndHour` |  |
| `RampMainlineStartHour` | `int` | `15` | No | `WatchdogConfiguration__RampMainlineStartHour` |  |
| `RampMainlineEndHour` | `int` | `19` | No | `WatchdogConfiguration__RampMainlineEndHour` |  |
| `RampStuckQueueStartHour` | `int` | `1` | No | `WatchdogConfiguration__RampStuckQueueStartHour` |  |
| `RampStuckQueueEndHour` | `int` | `4` | No | `WatchdogConfiguration__RampStuckQueueEndHour` |  |
| `WeekdayOnly` | `bool` | `true` | No | `WatchdogConfiguration__WeekdayOnly` |  |
| `ConsecutiveCount` | `int` | `3` | No | `WatchdogConfiguration__ConsecutiveCount` |  |
| `MinPhaseTerminations` | `int` | `50` | No | `WatchdogConfiguration__MinPhaseTerminations` |  |
| `PercentThreshold` | `double` | `.9` | No | `WatchdogConfiguration__PercentThreshold` |  |
| `MinimumRecords` | `int` | `500` | No | `WatchdogConfiguration__MinimumRecords` |  |
| `LowHitThreshold` | `int` | `50` | No | `WatchdogConfiguration__LowHitThreshold` |  |
| `LowHitRampThreshold` | `int` | `10` | No | `WatchdogConfiguration__LowHitRampThreshold` |  |
| `MaximumPedestrianEvents` | `int` | `200` | No | `WatchdogConfiguration__MaximumPedestrianEvents` |  |
| `RampMissedEventsThreshold` | `int` | `3` | No | `WatchdogConfiguration__RampMissedEventsThreshold` |  |
| `EmailAllErrors` | `bool` | `Not set` | No | `WatchdogConfiguration__EmailAllErrors` |  |
| `EmailPmErrors` | `bool` | `true` | No | `WatchdogConfiguration__EmailPmErrors` |  |
| `EmailAmErrors` | `bool` | `true` | No | `WatchdogConfiguration__EmailAmErrors` |  |
| `EmailRampErrors` | `bool` | `true` | No | `WatchdogConfiguration__EmailRampErrors` |  |
| `DefaultEmailAddress` | `string` | `Not set` | No | `WatchdogConfiguration__DefaultEmailAddress` |  |
| `Sort` | `string` | `Not set` | No | `WatchdogConfiguration__Sort` |  |

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

