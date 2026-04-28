# WatchDog

`WatchDog` is a command-line utility that runs ATSPM watchdog scans and sends email notifications for PM, AM, and ramp issues.

## What It Does

The app:

- loads ATSPM configuration, identity, event log, and aggregation databases
- gathers the latest active locations for the configured scan date
- reuses existing watchdog log events for a scan date when they already exist
- generates missing watchdog log events when they do not already exist
- segments errors into new, daily recurring, and recurring buckets
- sends watchdog emails to qualified recipients

## How To Run

From the repo root:

```powershell
dotnet run --project .\Atspm\WatchDog\WatchDog.csproj -- generate
```

Run a single scan type while debugging:

```powershell
dotnet run --project .\Atspm\WatchDog\WatchDog.csproj -- generate --emailPmErrors true --emailAmErrors false --emailRampErrors false
```

```powershell
dotnet run --project .\Atspm\WatchDog\WatchDog.csproj -- generate --emailPmErrors false --emailAmErrors true --emailRampErrors false
```

```powershell
dotnet run --project .\Atspm\WatchDog\WatchDog.csproj -- generate --emailPmErrors false --emailAmErrors false --emailRampErrors true
```

## Available Command

The current application root only exposes one runtime command:

- `generate`

The `generate` command accepts command-line overrides for watchdog settings such as:

- `--pmScanDate`
- `--amScanDate`
- `--rampMissedDetectorHitsStartScanDate`
- `--rampMissedDetectorHitsEndScanDate`
- `--emailPmErrors`
- `--emailAmErrors`
- `--emailRampErrors`
- `--defaultEmailAddress`
- `--sort`

These override values loaded from configuration.

## Configuration Sources

`Program.cs` uses `Host.CreateDefaultBuilder(args)` and then adds user secrets for this project.

Settings can come from:

- `appsettings.json`
- environment-specific `appsettings.*.json`
- environment variables
- user secrets for `Atspm/WatchDog/WatchDog.csproj`
- command-line overrides on `generate`

## Required Settings

The app requires configuration for:

- `DatabaseConfiguration`
- `WatchdogConfiguration`
- `EmailConfiguration`

### DatabaseConfiguration

The current code expects nested `DatabaseConfiguration` sections to bind to `DatabaseConfiguration`.

Each database entry must use this shape:

```json
{
  "DatabaseConfiguration": {
    "ConfigContext": {
      "DBType": "PostgreSQL",
      "Host": "your-postgres-host",
      "Port": 5432,
      "Database": "ATSPM-Config",
      "User": "admin",
      "Password": "your-password",
      "Options": {
        "Pooling": "true",
        "Timeout": "300",
        "CommandTimeout": "600",
        "Keepalive": "30"
      }
    },
    "AggregationContext": {
      "DBType": "PostgreSQL",
      "Host": "your-postgres-host",
      "Port": 5432,
      "Database": "ATSPM-Aggregation",
      "User": "admin",
      "Password": "your-password",
      "Options": {
        "Pooling": "true",
        "Timeout": "300",
        "CommandTimeout": "600",
        "Keepalive": "30"
      }
    },
    "EventLogContext": {
      "DBType": "PostgreSQL",
      "Host": "your-postgres-host",
      "Port": 5432,
      "Database": "ATSPM-EventLogs",
      "User": "admin",
      "Password": "your-password",
      "Options": {
        "Pooling": "true",
        "Timeout": "300",
        "CommandTimeout": "600",
        "Keepalive": "30"
      }
    },
    "IdentityContext": {
      "DBType": "PostgreSQL",
      "Host": "your-postgres-host",
      "Port": 5432,
      "Database": "ATSPM-Identity",
      "User": "admin",
      "Password": "your-password",
      "Options": {
        "Pooling": "true",
        "Timeout": "300",
        "CommandTimeout": "600",
        "Keepalive": "30"
      }
    }
  }
}
```

Important:

- keep the nested object shape shown above
- the current app expects `DBType`, `Host`, `Port`, `Database`, `User`, `Password`, and optional `Options`

### WatchdogConfiguration

Typical required watchdog settings:

```json
{
  "WatchdogConfiguration": {
    "PmScanDate": "2026-04-24",
    "AmScanDate": "2026-04-24",
    "RampMissedDetectorHitsStartScanDate": "2026-04-24",
    "RampMissedDetectorHitsEndScanDate": "2026-04-24",
    "AmStartHour": 1,
    "AmEndHour": 5,
    "PmPeakStartHour": 18,
    "PmPeakEndHour": 17,
    "RampDetectorStartHour": 15,
    "RampDetectorEndHour": 19,
    "RampMissedDetectorHitStartHour": 15,
    "RampMissedDetectorHitEndHour": 19,
    "RampMainlineStartHour": 15,
    "RampMainlineEndHour": 19,
    "RampStuckQueueStartHour": 1,
    "RampStuckQueueEndHour": 4,
    "WeekdayOnly": false,
    "ConsecutiveCount": 3,
    "MinPhaseTerminations": 50,
    "PercentThreshold": 0.9,
    "MinimumRecords": 500,
    "LowHitThreshold": 50,
    "LowHitRampThreshold": 4,
    "MaximumPedestrianEvents": 200,
    "RampMissedEventsThreshold": 4,
    "EmailAllErrors": true,
    "EmailPmErrors": true,
    "EmailAmErrors": true,
    "EmailRampErrors": true,
    "DefaultEmailAddress": "alerts@example.com",
    "Sort": "Error"
  }
}
```

Important:

- `Sort` should be a plain string such as `Error`, `Consecutive`, or `Location`
- do not put comments inside the JSON
- for the afternoon ramp test, use `15` to `19` for both ramp detector windows

### EmailConfiguration

The SMTP email service is registered when the config contains `EmailConfiguration:SmtpEmailService`.

Example:

```json
{
  "EmailConfiguration": {
    "SmtpEmailService": {
      "Host": "smtp.freesmtpservers.com",
      "Port": 25,
      "UserName": "",
      "Password": "",
      "EnableSsl": false
    }
  }
}
```

## Recipient Rules

Current watchdog recipient behavior:

- scoped watchdog emails use recipient assignments derived from region, jurisdiction, and area membership
- the "All Locations" admin-style email goes only to users who have both:
  - `Admin`
  - `WatchdogSubscriber`
- users must have the `WatchdogSubscriber` role
- users must still be watchdog-eligible through the `Watchdog:View` claim path attached to that role

## Troubleshooting

If watchdog runs but no emails are sent:

- verify `DatabaseConfiguration` uses the current nested object shape
- verify `ConfigContext` points to a populated ATSPM config database
- verify `IdentityContext` points to a populated identity database
- verify users have email addresses
- verify users have the expected roles and claims
- verify `EmailConfiguration:SmtpEmailService` exists

If logs show:

```text
Watchdog email recipients resolved. Total watchdog users: 0, admin subscriber users: 0, regions: 0, jurisdictions: 0, areas: 0
```

that usually means watchdog is connected to an empty or incorrect config/identity database, not that SMTP is failing.

## Notes

- Existing watchdog events for a given scan date are reused when present, so the app may skip regenerating scans and go straight to email segmentation/sending.
- With PM, AM, and ramp scans all enabled, the current implementation performs many event-log queries per location. For troubleshooting, it is often easier to run one scan type at a time.
