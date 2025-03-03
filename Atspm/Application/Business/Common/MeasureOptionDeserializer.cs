using System;
using System.Text.Json;
using Utah.Udot.Atspm.Business.TransitSignalPriority;
using Utah.Udot.Atspm.Data.Interfaces;

public static class MeasureOptionDeserializer
{
    public static object Deserialize(JsonDocument json, int measureTypeId)
    {
        if (json == null)
            return null;

        return measureTypeId switch
        {
            38 => JsonSerializer.Deserialize<TransitSignalPriorityOptions>(json),
            _ => throw new NotSupportedException($"Unsupported MeasureTypeId: {measureTypeId}")
        };
    }

    public static string Serialize(object measureOption)
    {
        return measureOption == null ? null : JsonSerializer.Serialize(measureOption);
    }
}
