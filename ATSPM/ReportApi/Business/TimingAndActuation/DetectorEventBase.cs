namespace ATSPM.ReportApi.Business.TimingAndActuation
{
    public class DetectorEventBase
    {
        public DetectorEventBase(DateTime? start, DateTime? stop)
        {
            DetectorOn = start;
            DetectorOff = stop;
        }


        public DateTime? DetectorOn { get; set; }
        public DateTime? DetectorOff { get; set; }
    }
}
