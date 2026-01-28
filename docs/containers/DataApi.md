# DataApi — Configuration Reference

This page documents all configuration options available to the **DataApi** container.

## Table of Contents

- [Redis](#redis)

---

## Redis

> Redis cache configuration

[View Source](src/MyApp.Configuration/RedisOptions.cs)

<details>
<summary><strong>View Settings</strong></summary>

| Key | Type | Default | Required | Env Var | Description |
|-----|------|---------|----------|---------|-------------|
| `Host` | `String` | `localhost` | No | `Redis__Host` | The hostname of the Redis server. |
| `Port` | `Int32` | `6379` | No | `Redis__Port` | The port Redis listens on. |
| `Password` | `String` | `` | No | `Redis__Password` | Optional password for Redis authentication. |

</details>

---

