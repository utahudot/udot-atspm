using ATSPM.ReportApi.Business.Common;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.ReportApi.Business.AppoachDelay
{
    public class ApproachDelayOptions : OptionsBase
    {
        public int BinSize { get; set; }
        
        
        public bool GetVolume { get; set; }
    }
}