using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Utah.Udot.Atspm.Data;
using Utah.Udot.ATSPM.SqlLiteDatabaseProvider.Migrations;
using Xunit;

namespace DatabaseInstallerTests;

public class TimeOfDayDefaultsMigrationTests
{
    [Fact]
    public void RemoveDefaults_PreservesOtherDefaultsAndTimeOfDayPresets()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var context = new ConfigContext(new DbContextOptionsBuilder<ConfigContext>()
            .UseSqlite(connection).Options);
        context.Database.EnsureCreated();

        // New databases seed the measure type and presets without TOD defaults.
        Assert.False(context.MeasureOptions.Any(option => option.MeasureTypeId == 41));
        Assert.True(context.MeasureType.Any(measure => measure.Id == 41));
        var presets = context.MeasureOptionPresets.AsNoTracking()
            .Where(preset => preset.MeasureTypeId == 41)
            .OrderBy(preset => preset.Id)
            .ToList();
        Assert.Equal(4, presets.Count);
        var otherDefaults = context.MeasureOptions.AsNoTracking()
            .OrderBy(option => option.Id)
            .Select(option => new { option.Id, option.MeasureTypeId, option.Option, option.Value })
            .ToList();

        var migration = new RemoveTimeOfDayMeasureDefaults();
        // Recreate the old seeded rows to exercise an existing installation.
        Execute(context, migration.DownOperations);
        Assert.Equal(15, context.MeasureOptions.Count(option => option.MeasureTypeId == 41));
        context.MeasureOptions.Where(option => option.Id == 138)
            .ExecuteUpdate(update => update.SetProperty(option => option.Value, "1200"));

        Execute(context, migration.UpOperations);

        Assert.False(context.MeasureOptions.Any(option => option.MeasureTypeId == 41));
        Assert.True(context.MeasureType.Any(measure => measure.Id == 41));
        Assert.Equal(otherDefaults, context.MeasureOptions.AsNoTracking()
            .OrderBy(option => option.Id)
            .Select(option => new { option.Id, option.MeasureTypeId, option.Option, option.Value })
            .ToList());
        var remainingPresets = context.MeasureOptionPresets.AsNoTracking()
            .Where(preset => preset.MeasureTypeId == 41)
            .OrderBy(preset => preset.Id)
            .ToList();
        Assert.Equal(
            Newtonsoft.Json.JsonConvert.SerializeObject(presets),
            Newtonsoft.Json.JsonConvert.SerializeObject(remainingPresets));

        Execute(context, migration.DownOperations);
        Assert.Equal(15, context.MeasureOptions.Count(option => option.MeasureTypeId == 41));
        Assert.Equal("800", context.MeasureOptions.Single(option => option.Id == 138).Value);
    }

    private static void Execute(ConfigContext context, IReadOnlyList<MigrationOperation> operations)
    {
        var generator = context.GetService<IMigrationsSqlGenerator>();
        foreach (var command in generator.Generate(operations))
        {
            context.Database.ExecuteSqlRaw(command.CommandText);
        }
    }
}
