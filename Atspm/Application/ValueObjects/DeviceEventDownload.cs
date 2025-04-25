using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.ValueObjects
{
    public class DeviceEventDownload
    {
        public int DeviceId { get; set; }
        public string Ipaddress { get; set; }
        public DeviceTypes DeviceType { get; set; }
        public int BeforeWorkflowEventCount { get; set; }
        public int AfterWorkflowEventCount { get; set; }
        public int ChangeInEventCount { get; set; }
    }
}
