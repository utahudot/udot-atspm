namespace Identity.Business.Agency
{
    public interface IAgencyService
    {
        Task<bool> AgencyExistsAsync(string agencyName);
    }
}