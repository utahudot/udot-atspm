using System;
using System.Collections.Generic;

#nullable disable

namespace ControllerLogger.Models
{
    public partial class Applicationz
    {
        public Applicationz()
        {
            ApplicationSettings = new HashSet<ApplicationSetting>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ApplicationSetting> ApplicationSettings { get; set; }
    }
}
