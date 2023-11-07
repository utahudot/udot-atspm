using ATSPM.ReportApi.Business.Common;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.ReportApi.Business.AppoachDelay
{
    public class ApproachDelayOptions : OptionsBase
    {
        [Range(0, 15, ErrorMessage = "Can only be between 0 .. 15")]
        public int BinSize { get; set; }
        
        
        public bool GetVolume { get; set; }
    }
}