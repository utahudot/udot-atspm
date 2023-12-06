using System.Collections.Generic;
using System.Reflection;

namespace ATSPM.Infrastructure.Migrations.Identity
{
    public static class CustomClaims
    {
        public const string AdminViewUsers = "Admin:ViewUsers";
        public const string AdminEditUsers = "Admin:EditUsers";
        public const string AdminDeleteUsers = "Admin:DeleteUsers";

        public const string AdminViewRoles = "Admin:ViewRoles";
        public const string AdminEditRoles = "Admin:EditRoles";
        public const string AdminDeleteRoles = "Admin:DeleteRoles";
        public const string AdminCreateRoles = "Admin:CreateRoles";

        public const string AdminViewSignalConfig = "Admin:ViewSignalConfig";
        public const string AdminEditSignalConfig = "Admin:EditSignalConfig";

        public const string AdminViewAreaConfig = "Admin:ViewAreaConfig";
        public const string AdminEditAreaConfig = "Admin:EditAreaConfig";

        public const string AdminViewJurisdictionConfig = "Admin:ViewJurisdictionConfig";
        public const string AdminEditJurisdictionConfig = "Admin:EditJurisdictionConfig";

        public const string AdminViewSettingsConfig = "Admin:ViewSettingsConfig";
        public const string AdminEditSettingsConfig = "Admin:EditSettingsConfig";

        public const string AdminViewFAQConfig = "Admin:ViewFAQConfig";
        public const string AdminEditFAQConfig = "Admin:EditFAQConfig";

        public const string AdminViewAboutConfig = "Admin:ViewAboutConfig";
        public const string AdminEditAboutConfig = "Admin:EditAboutConfig";

        public const string AdminViewMenuConfig = "Admin:ViewMenuConfig";
        public const string AdminEditMenuConfig = "Admin:EditMenuConfig";
        public const string AdminWatchDogConfig = "Admin:WatchDog";

        public const string DownloadControllerEventLogs = "Download:ControllerEventLogs";
        public const string DownloadAggregateData = "Download:AggregateData";

        public const string ReportsApproachDelay = "Reports:ApproachDelay";
        public const string ReportsApproachSpeed = "Reports:ApproachSpeed";
        public const string ReportsApproachVolume = "Reports:ApproachVolume";
        public const string ReportsArrivalOnRed = "Reports:ArrivalOnRed";
        public const string ReportsGreenTypeUtilization = "Reports:GreenTypeUtilization";
        public const string ReportsLeftTurnGapAnalysis = "Reports:LeftTurnGapAnalysis";
        public const string ReportsLeftTurnReport = "Reports:LeftTurnReport";
        public const string ReportsPedDelay = "Reports:PedDelay";
        public const string ReportsPhaseTermination = "Reports:PhaseTermination";
        public const string ReportsPreempt = "Reports:Preempt";
        public const string ReportsPurdueCoordinationDiagram = "Reports:PurdueCoordinationDiagram";
        public const string ReportsSplitFail = "Reports:SplitFail";
        public const string ReportsSplitMonitor = "Reports:SplitMonitor";
        public const string ReportsTimingAndActuation = "Reports:TimingAndActuation";
        public const string ReportsTurningMovementCounts = "Reports:TurningMovementCounts";
        public const string ReportsWaitTime = "Reports:WaitTime";
        public const string ReportsYellowRedActivations = "Reports:YellowRedActivations";
        public const string ReportsAggregate = "Reports:Aggregate";
        public const string ReportsLinkPivot = "Reports:LinkPivot";

        public static List<string> GetAllClaimsFromCustomClaims()
        {
            FieldInfo[] fields = typeof(CustomClaims).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            List<string> claimsList = new List<string>(fields.Length);
            foreach (FieldInfo field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly)
                {
                    claimsList.Add((string)field.GetValue(null));
                }
            }

            return claimsList;
        }

    }
}