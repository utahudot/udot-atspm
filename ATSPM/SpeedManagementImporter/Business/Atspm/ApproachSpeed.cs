namespace SpeedManagementImporter.Business.Atspm
{
    public class ApproachSpeed
    {
        public string SignalId { get; set; }
        public int ApproachId { get; set; }
        public int SummedSpeed { get; set; }
        public int SpeedVolume { get; set; }
        public int Speed85th { get; set; }
        public int Speed15th { get; set; }
    }
}
