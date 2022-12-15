using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Data.Models
{
    public partial class Approach
    {
        [NotMapped]
        public string Index { get; set; }

        public override string? ToString()
        {
            return $"{Signal} {DirectionTypeId} Phase: {ProtectedPhaseNumber}";
        }
    }
}
