namespace ATSPM.Data.Models.CrashModels;

public partial class Crash
{
    public int Id { get; set; }

    public int Severity { get; set; }

    public int PersonId { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public DateTime Timestamp { get; set; }

    public int Speedinvolved { get; set; }

    public int NonmotorActionId { get; set; }

    public int NonmotorContribCirId { get; set; }

    public int? ClearguideRouteId { get; set; }

    public int? Avgspd15 { get; set; }

    public int? Percentilespd8515 { get; set; }

    public int? Avgspd30 { get; set; }

    public int? Percentilespd8530 { get; set; }

    public int? AvgspdHour { get; set; }

    public int? Percentilespd85Hour { get; set; }
}
