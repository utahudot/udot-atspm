using System;
using System.Collections.Generic;

#nullable disable

namespace ControllerLogger.Models
{
    public partial class Menu
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public int ParentId { get; set; }
        public string Application { get; set; }
        public int DisplayOrder { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
    }
}
