namespace ATSPM.ConfigApi.Models
{
    public class SearchLocation
    {
        public int Id { get; set; }
        public string locationIdentifier { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public bool ChartEnabled { get; set; }
        public int? RegionId { get; set; }
        public int? JurisdictionId { get; set; }
        public IEnumerable<int> Areas { get; set; }
        public IEnumerable<int> Charts { get; set; }
        public DateTime Start { get; set; }
        public int LocationTypeId { get; set; }
    }
}
