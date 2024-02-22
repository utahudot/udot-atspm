namespace ATSPM.ReportApi.Business.LeftTurnGapReport
{
    public interface IDataCheckResult
    {
        public bool LeftTurnVolumeOk { get; }
        public bool GapOutOk { get; }
        public bool PedCycleOk { get; }

    }
}
