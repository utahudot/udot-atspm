# Event Log Utility configuration

Configuration options available to the **Event Log Utility** container.

<small>Generated on August 18, 2026 at 7:15 PM UTC.</small>

## Contents

- [SampleOptions](#sampleoptions)

## SampleOptions

> Sample configuration.

Options used by the sample utility.

[View source](https://github.com/utahudot/udot-atspm/blob/abc123/Atspm/Infrastructure/Configuration/SampleOptions.cs#L10)

| Setting | Type | Default | Required | Environment variable | Description |
| --- | --- | --- | --- | --- | --- |
| `Dates` | `IEnumerable<DateTime?>` | `[]` | No | `SampleOptions__Dates` | Dates to include. |
| `Path` | `string` | `System.IO.Path.GetTempPath()` | Yes | `SampleOptions__Path` | Output path \| directory. |

## Example JSON configuration

This example includes every documented setting. Replace placeholder secrets, URLs, paths, and connection details before use.

```json
{
  "SampleOptions": {
    "Dates": [],
    "Path": "./data"
  }
}
```
