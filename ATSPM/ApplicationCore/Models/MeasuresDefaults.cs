using ATSPM.Application.Models;

namespace ATSPM.Application.Models
{
    public class MeasuresDefaults : ATSPMModelBase
    {
        public string Measure { get; set; }
        public string OptionName { get; set; }
        public string Value { get; set; }
    }
}