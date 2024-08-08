namespace SpeedManagementImporter.Business.Pems
{
    public class Quantity
    {
        public string label { get; set; }
        public double? value { get; set; }
    }

    public class Quantities
    {
        public List<Quantity> quantity { get; set; }
    }

    public class StationMeasurement
    {
        public int station_id { get; set; }
        public string abs_postmile { get; set; }
        public string state_postmile { get; set; }
        public Quantities quantities { get; set; }
    }
}
