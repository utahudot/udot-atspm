using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Models
{
    public class ApplicationSettings
    {
        public int ID { get; set; }

        public int ApplicationID { get; set; }
        public virtual Applicationz Application { get; set; }
    }
}
