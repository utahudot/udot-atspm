using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class DetectorComment
    {
        public int CommentId { get; set; }
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string CommentText { get; set; } = null!;

        public virtual Detector IdNavigation { get; set; } = null!;
    }
}
