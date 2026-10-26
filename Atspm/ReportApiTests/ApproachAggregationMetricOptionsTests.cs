using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.ReportApi.DataAggregation;

namespace ReportApiTests
{
    public class ApproachAggregationMetricOptionsTests
    {
        [Fact]
        public void GetPhaseSeriesName_NamesProtectedAndPermissivePhasesApart()
        {
            var approach = new Approach { Description = "NBT", ProtectedPhaseNumber = 2, PermissivePhaseNumber = 6 };

            Assert.Equal("NBT - PH 2", ApproachAggregationMetricOptions.GetPhaseSeriesName(approach, true));
            Assert.Equal("NBT - PH 6", ApproachAggregationMetricOptions.GetPhaseSeriesName(approach, false));
        }
    }
}
