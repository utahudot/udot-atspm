using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Business.TimeSpaceDiagram
{
    public class MergingApproaches
    {
        public DirectionTypes RightTurnFrom { get; set; }
        public DirectionTypes LeftTurnFrom { get; set; }
    }

    public class TmcEventDto : DataPointDateDouble
    {
        public TmcEventDto(DateTime start, double value) : base(start, value) { }

        public bool IsRightTurnEvent { get; set; }
        public bool IsLeftTurnEvent { get; set; }
        public LaneTypes LaneType { get; set; }
        public DirectionTypes DirectionType { get; set; }
    }

    public class TmcForPhaseDto
    {
        public List<TmcEventDto> RightTurnEvents { get; set; } = new List<TmcEventDto>();
        public List<TmcEventDto> LeftTurnEvents { get; set; } = new List<TmcEventDto>();
    }
}
