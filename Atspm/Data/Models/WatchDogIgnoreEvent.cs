using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Interfaces;

namespace Utah.Udot.Atspm.Data.Models
{
    public class WatchDogIgnoreEvent : AtspmConfigModelBase<int>, ILocationLayer
    {

        /// <summary>
        /// Location id
        /// </summary>
        public int LocationId { get; set; }

        public virtual Location? Location { get; set; }

        /// <summary>
        /// Location identifier
        /// </summary>
        public string LocationIdentifier { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Component type
        /// </summary>
        public WatchDogComponentTypes? ComponentType { get; set; }

        /// <summary>
        /// Component id
        /// </summary>
        public int? ComponentId { get; set; }

        /// <summary>
        /// Issue type
        /// </summary>
        public WatchDogIssueTypes IssueType { get; set; }

        /// <summary>
        /// Phase
        /// </summary>
        public int? Phase { get; set; }

    }
}
