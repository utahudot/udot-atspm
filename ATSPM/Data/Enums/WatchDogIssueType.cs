namespace ATSPM.Data.Enums
{
    public enum WatchDogIssueType
    {
        RecordCount = 1,
        LowDetectorHits = 2,
        StuckPed = 3,
        ForceOffThreshold = 4,
        MaxOutThreshold = 5,
        UnconfiguredApproach = 6,
        UnconfiguredDetector = 7,

    }

    [System.Flags]
    public enum WatchDogComponentType
    {
        Signal,
        Approach,
        Detector
    }
}