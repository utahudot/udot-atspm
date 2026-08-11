# Data API configuration

Configuration options available to the **Data API** container.

Generated from [utahudot/udot-atspm at `ee3250431c6e5218a1d10871a46b4b9736743192`](https://github.com/utahudot/udot-atspm/tree/ee3250431c6e5218a1d10871a46b4b9736743192).

## Contents

- [DatabaseConfiguration:ConfigContext](#databaseconfigurationconfigcontext)
- [DatabaseConfiguration:AggregationContext](#databaseconfigurationaggregationcontext)
- [DatabaseConfiguration:EventLogContext](#databaseconfigurationeventlogcontext)
- [DatabaseConfiguration:IdentityContext](#databaseconfigurationidentitycontext)
- [Jwt](#jwt)
- [Oidc](#oidc)

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

## Jwt

> Configuration for API JWT bearer authentication

Configuration values used by the APIs to validate and issue JWT bearer tokens.

[View source](https://github.com/utahudot/udot-atspm/blob/ee3250431c6e5218a1d10871a46b4b9736743192/Atspm/Infrastructure/Configuration/Documentation/ApiConfigurationSections.cs#L26)

| Setting | Type | Default | Required | Environment variable | Description |
| --- | --- | --- | --- | --- | --- |
| `Issuer` | `string` | `string.Empty` | Yes | `Jwt__Issuer` | Issuer value required when validating incoming JWT bearer tokens and issuing identity tokens. |
| `Audience` | `string` | `Not set` | No | `Jwt__Audience` | Audience value configured for JWT bearer token validation. |
| `Key` | `string` | `string.Empty` | Yes | `Jwt__Key` | Symmetric signing key used to validate and issue JWT bearer tokens. |
| `ExpireDays` | `double?` | `Not set` | No | `Jwt__ExpireDays` | Number of days before identity API generated JWT bearer tokens expire. |

## Oidc

> Configuration for optional OpenID Connect authentication

Optional OpenID Connect configuration used when an external identity provider is enabled.

[View source](https://github.com/utahudot/udot-atspm/blob/ee3250431c6e5218a1d10871a46b4b9736743192/Atspm/Infrastructure/Configuration/Documentation/ApiConfigurationSections.cs#L55)

| Setting | Type | Default | Required | Environment variable | Description |
| --- | --- | --- | --- | --- | --- |
| `Authority` | `string` | `Not set` | No | `Oidc__Authority` | Authority URL for the OpenID Connect identity provider. |
| `ClientId` | `string` | `Not set` | No | `Oidc__ClientId` | Client identifier registered with the OpenID Connect identity provider. |
| `ClientSecret` | `string` | `Not set` | No | `Oidc__ClientSecret` | Client secret registered with the OpenID Connect identity provider. |
| `CallbackPath` | `string` | `Not set` | No | `Oidc__CallbackPath` | Callback path used by the OpenID Connect redirect flow. |

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
  "Jwt": {
    "Issuer": "https://identity.example.com",
    "Audience": "atspm",
    "Key": "replace-with-a-secret",
    "ExpireDays": null
  },
  "Oidc": {
    "Authority": "https://identity-provider.example.com",
    "ClientId": "atspm",
    "ClientSecret": "replace-with-a-secret",
    "CallbackPath": "/signin-oidc"
  }
}
```

