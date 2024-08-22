using SpeedManagementImporter.Business.Pems;

public class RootObjectSpeed
{
    public Measurements measurements { get; set; }
}
public class Measurements
{
    public List<StationMeasurement> station { get; set; }
}