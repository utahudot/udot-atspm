using Utah.Udot.Atspm.ValueObjects;

namespace Utah.Udot.Atspm.ConfigApi.Services
{
    public interface ISignalTemplateService
    {
        Task<TemplateLocationModifiedDto> SyncNewLocationDetectorsAndApproaches(int locationId);
    }
}
