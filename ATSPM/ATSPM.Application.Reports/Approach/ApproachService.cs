using ATSPM.Application.Reports.Detector;
using ATSPM.Application.Reports.ViewModels.ApproachDelay;
using System;

namespace ATSPM.Application.Reports.Approach
{
    public class ApproachService
    {
        private readonly DetectorService detectorService;

        public ApproachService(
            DetectorService detectorService
            )
        {
            this.detectorService = detectorService;
        }




        //internal static ApproachDelayChart GetApproachDelayChart()
        //{
        //    var signalPhase =GetSignalPhase()
        //    throw new NotImplementedException();
        //}
    }


}