using SpeedManagementImporter.Business.Pems;

public class RootObjectFlow : RootObjectBase
{
    public StationsHourlySummary? stationsHourlySummary { get; set; }
}

public class StationsHourlySummary
{
    public List<Station>? stations { get; set; }
}


