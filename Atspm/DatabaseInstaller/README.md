# DatabaseInstaller

`DatabaseInstaller` is a command-line utility for applying database migrations, moving event log data, copying configuration data, and seeding test records.

## Running The Tool

Run commands from the `DatabaseInstaller` project root:

```bash
dotnet run --project DatabaseInstaller -- <command> [options]
```

Or run the built executable directly:

```bash
DatabaseInstaller.exe <command> [options]
```

Command-line arguments override values from `appsettings.json` and user secrets.

## Available Commands

| Command | Purpose |
| --- | --- |
| `update` | Apply migrations for all contexts and optionally seed the admin user |
| `transfer-config` | Copy configuration data from the ATSPM Config API into the target database |
| `transfer` | Transfer event logs from SQL Server into the current database format |
| `transfer-speed` | Transfer speed events from SQL Server into compressed event logs |
| `translate` | Translate logs from the initial compression format |
| `transfer-daily-hourly` | Convert daily compressed logs to hourly compressed logs |
| `setup-test` | Seed test data using `devices.json` |
| `copy-sql` | Copy event logs from SQL Server into PostgreSQL |

## `update`

Applies migrations for `ConfigContext`, `AggregationContext`, `EventLogContext`, and `IdentityContext`.
If `--seed-admin` is supplied, the command also seeds an admin user.

### Options

- `--config-connection`
- `--aggregation-connection`
- `--eventlog-connection`
- `--identity-connection`
- `--provider`
- `--admin-email`
- `--admin-password`
- `--admin-role`
- `--seed-admin`

### Example

```bash
dotnet run --project DatabaseInstaller -- update \
  --provider PostgreSQL \
  --config-connection "Host=localhost;Database=ATSPM-Config;Username=postgres;Password=postgres" \
  --aggregation-connection "Host=localhost;Database=ATSPM-Aggregation;Username=postgres;Password=postgres" \
  --eventlog-connection "Host=localhost;Database=ATSPM-EventLogs;Username=postgres;Password=postgres" \
  --identity-connection "Host=localhost;Database=ATSPM-Identity;Username=postgres;Password=postgres" \
  --seed-admin true \
  --admin-email admin@example.com \
  --admin-password "ChangeMe123!" \
  --admin-role Admin
```

## `transfer-config`

Imports configuration records from the ATSPM Config API into the target database.

### Options

- `--api-base-url`
- `--bearer-token`
- `--delete`
- `--update-locations`
- `--update-speed`

### Notes

- `--bearer-token` is required when importing configuration.
- `--delete` clears existing target records before inserting new data.
- `--update-locations` imports locations and related configuration data.
- `--update-speed` imports speed devices.

### Example

```bash
dotnet run --project DatabaseInstaller -- transfer-config \
  --api-base-url "https://atspm.udot.utah.gov/" \
  --bearer-token "<token>" \
  --update-locations true
```

## `transfer`

Transfers historical event logs from SQL Server into the event log store.

### Options

- `--source`
- `--start`
- `--end`
- `--device`
- `--batch`
- `--locations`

### Notes

- `--source` is the SQL Server connection string.
- `--start` and `--end` define the transfer range.
- `--device` limits the transfer to a device type id.
- `--batch` controls insert batch size.
- `--locations` accepts a comma-separated list of location identifiers.
- If `--batch` is omitted, the service uses a default batch size of `500`.

### Example

```bash
dotnet run --project DatabaseInstaller -- transfer \
  --source "Server=localhost;Database=MOE;Trusted_Connection=True;TrustServerCertificate=True" \
  --start "2024-01-01" \
  --end "2024-01-31" \
  --locations "LOC1,LOC2" \
  --batch 250
```

## `transfer-speed`

Transfers speed events from SQL Server into compressed event logs.

### Options

- `--source`
- `--start`
- `--end`

### Example

```bash
dotnet run --project DatabaseInstaller -- transfer-speed \
  --source "Server=localhost;Database=MOE;Trusted_Connection=True;TrustServerCertificate=True" \
  --start "2024-01-01" \
  --end "2024-01-31"
```

## `translate`

Translates logs from the initial compression format into the current hourly format.

### Options

- `--source`
- `--start`
- `--end`
- `--batch`
- `--device`
- `--locations`

### Example

```bash
dotnet run --project DatabaseInstaller -- translate \
  --source "Server=localhost;Database=MOE;Trusted_Connection=True;TrustServerCertificate=True" \
  --start "2024-01-01" \
  --end "2024-01-31" \
  --batch 500
```

## `transfer-daily-hourly`

Converts daily compressed logs into hourly compressed logs.

### Options

- `--source`
- `--source-table`
- `--start`
- `--end`
- `--data-type`
- `--device`
- `--batch`

### Notes

- `--source-table` defaults to `public."CompressedEvents"`.
- `--data-type` defaults to `IndianaEvent`.

### Example

```bash
dotnet run --project DatabaseInstaller -- transfer-daily-hourly \
  --source "Host=localhost;Database=ATSPM;Username=postgres;Password=postgres" \
  --source-table "public.\"CompressedEvents\"" \
  --start "2024-01-01" \
  --end "2024-01-31" \
  --data-type IndianaEvent
```

## `setup-test`

Seeds test configuration and device data using `devices.json`.

### Options

- `--location-count`
- `--device-configuration-id`
- `--protocol`

### Notes

- `--protocol` filters seeded devices by protocol.
- Supported protocol values are `ftp`, `ftps`, and `http`.

### Example

```bash
dotnet run --project DatabaseInstaller -- setup-test \
  --location-count 10 \
  --device-configuration-id 2 \
  --protocol ftp
```

## `copy-sql`

Copies rows from the source SQL Server `CompressedEvents` table into the PostgreSQL `CompressedEvents` table.

### Options

- `--source`
- `--start`
- `--end`
- `--locations`
- `--device`
- `--batch-size`
- `MaxConcurrency` from `TransferCommandConfiguration`

### Notes

- This command copies the stored compressed `Data` payload directly instead of decompressing and recompressing it.
- That makes it much faster than rebuilding each hourly record from expanded events.
- `MaxConcurrency` controls how many locations are processed in parallel. The default is `2`. Start there and increase carefully.
- `--batch-size` controls how many rows are sent in each insert batch. The default is `250`.
- Each location worker keeps one SQL Server connection and one PostgreSQL connection open for all of its batches.
- The command reads `TransferCommandConfiguration:MaxConcurrency` and `TransferCommandConfiguration:CopyBatchSize` from `appsettings.json` or user secrets, so you can tune both without changing the command line.
- This assumes the source SQL Server database is already using the same `CompressedEvents` schema and payload format as the target PostgreSQL database.
- If you need to migrate older raw event tables instead, use `transfer` instead of `copy-sql`.

### Example

```bash
dotnet run --project DatabaseInstaller -- copy-sql \
  --source "Server=localhost;Database=ATSPM-EventLogs;Trusted_Connection=True;TrustServerCertificate=True" \
  --start "2024-01-01" \
  --end "2024-01-31" \
  --batch-size 500 \
  --locations "LOC1,LOC2"
```

## Configuration

`DatabaseInstaller` reads from `appsettings.json`, environment variables, and user secrets.

For `copy-sql`, the most relevant settings are:

- `TransferCommandConfiguration:Source` for the source SQL Server connection string
- `TransferCommandConfiguration:MaxConcurrency` for the number of location workers
- `TransferCommandConfiguration:CopyBatchSize` for the insert batch size
- `DatabaseConfiguration:EventLogContext` for the PostgreSQL destination connection

Useful sections:

- `ConnectionStrings` for the database contexts
- `CommandLineOptions` for `update`
- `TransferDailyToHourlyConfiguration` for `transfer-daily-hourly`
- `TransferCommandConfiguration` for `transfer`, `transfer-speed`, and `translate`
- `TransferCommandConfiguration:CopyBatchSize` for `copy-sql`
- `TransferCommandConfiguration:MaxConcurrency` for `copy-sql`
- `TransferConfigCommandConfiguration` for `transfer-config`

## Important Note

`copy-sql` is intended for moving already-compressed event log rows between compatible ATSPM event log databases. It is not the right tool for upgrading legacy raw event log tables into the compressed schema.
