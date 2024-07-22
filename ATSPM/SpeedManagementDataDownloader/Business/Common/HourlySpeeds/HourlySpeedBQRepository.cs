using Google.Cloud.BigQuery.V2;
using SpeedManagementDataDownloader.Common.HourlySpeeds;
using System.Collections.Generic;
using System.Threading.Tasks;

public class HourlySpeedBQRepository : IHourlySpeedRepository
{

    private readonly BigQueryClient _client;
    private readonly string _datasetId;
    private readonly string _tableId;

    public HourlySpeedBQRepository(BigQueryClient client, string datasetId, string tableId)
    {
        _client = client;
        _datasetId = datasetId;
        _tableId = tableId;
    }


    public async Task AddHourlySpeedAsync(HourlySpeed hourlySpeed)
    {
        var table = _client.GetTable(_datasetId, _tableId);
        var insertRow = CreateRow(hourlySpeed);
        await table.InsertRowAsync(insertRow);
    }

    public async Task AddHourlySpeedsAsync(List<HourlySpeed> hourlySpeeds)
    {
        var table = _client.GetTable(_datasetId, _tableId);
        List<BigQueryInsertRow> insertRows = new List<BigQueryInsertRow>();
        int batchSize = 10000;

        foreach (var hourlySpeed in hourlySpeeds)
        {
            insertRows.Add(CreateRow(hourlySpeed));

            if (insertRows.Count == batchSize)
            {
                await table.InsertRowsAsync(insertRows);
                insertRows.Clear(); // Clear the list for the next batch
            }
        }

        // Insert any remaining rows that didn't make a full batch
        if (insertRows.Count > 0)
        {
            await table.InsertRowsAsync(insertRows);
        }
    }

    protected BigQueryInsertRow CreateRow(HourlySpeed item)
    {
        return new BigQueryInsertRow
            {
                {"Date", item.Date.AsBigQueryDate() },
                {"BinStartTime", item.BinStartTime.TimeOfDay },
                {"RouteId", item.RouteId },
                {"SourceId", item.SourceId },
                {"ConfidenceId", item.ConfidenceId },
                {"Average", item.Average },
                {"FifteenthSpeed", item.FifteenthSpeed },
                {"EightyFifthSpeed", item.EightyFifthSpeed },
                {"NinetyFifthSpeed", item.NinetyFifthSpeed },
                {"NinetyNinthSpeed", item.NinetyNinthSpeed },
                {"Violation", item.Violation },
                {"Flow", item.Flow }
            };
    }
}
