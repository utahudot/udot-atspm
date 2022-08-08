using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class Menu
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; } = null!;
        public int ParentId { get; set; }
        public string Application { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public string Controller { get; set; } = null!;
        public string Action { get; set; } = null!;
    }
}
