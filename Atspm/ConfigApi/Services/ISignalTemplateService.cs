using Utah.Udot.Atspm.ValueObjects;

namespace Utah.Udot.Atspm.ConfigApi.Services
{
    public interface ISignalTemplateService
    {
        TemplateLocationModifiedDto SyncNewLocationDetectorsAndApproaches(int locationId);
    }
}