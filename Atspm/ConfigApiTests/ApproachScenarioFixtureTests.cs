using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Utah.Udot.Atspm.ConfigApi.Services;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
namespace ConfigApiTests.Services;
[TestClass]
public class ApproachScenarioFixtureTests
{
    private static string FixturePath([CallerFilePath] string source = "") => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(source)!, "../WebUI/e2e/fixtures/approachMockScenarios.json"));

    [TestMethod]
    public void BrowserScenarioResponsesMatchRealReconciliation()
    {
        var path = FixturePath();
        var scenarios = JsonNode.Parse(File.ReadAllText(path))!.AsArray();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
        var regenerate = Environment.GetEnvironmentVariable("REGENERATE_APPROACH_SCENARIOS") == "1";
        foreach (var scenario in scenarios)
        {
            var location = scenario!["location"]!.Deserialize<Location>(options)!;
            var events = scenario["events"]!.Deserialize<List<IndianaEvent>>(options)!;
            var actual = JsonSerializer.SerializeToNode(SignalTemplateService.ModifyLocationWithEvents(location, events), options);
            if (regenerate) scenario["expected"] = actual;
            else Assert.IsTrue(JsonNode.DeepEquals(scenario["expected"], actual), $"Scenario response drifted: {scenario["name"]}");
        }
        if (regenerate) File.WriteAllText(path, scenarios.ToJsonString(options) + Environment.NewLine);
    }
}
