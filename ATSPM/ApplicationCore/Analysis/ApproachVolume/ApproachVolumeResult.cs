using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ATSPM.Application.Analysis.ApproachVolume
{
    public class ApproachVolumeResult
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public int PrimaryTotalVolume { get; set; }
        public int PrimaryPeakVolume { get; set; }
        public double PrimaryPHF { get; set; }
        public double PrimaryDFactor { get; set; }
        public double PrimaryKFactor { get; set; }

        public int OpposingTotalVolume { get; set; }
        public int OpposingPeakVolume { get; set; }
        public double OpposingPHF { get; set; }
        public double OpposingDFactor { get; set; }
        public double OpposingKFactor { get; set; }

        public int TotalVolume { get; set; }
        public int TotalPeakVolume { get; set; }
        public double TotalPHF { get; set; }
        public double TotalKFactor { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
