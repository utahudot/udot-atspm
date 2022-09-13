using System.Collections.Generic;
using System.Linq;
using MOE.Common;

namespace MOE.Common.Models.ViewModel._MainMenu
{
    public class MenuItem
    {
        public MenuItem(Menu menuItem)
        {
            IsAdmin = HttpContextHelper.Current.User.IsInRole("Admin");
            IsTechnician = HttpContextHelper.Current.User.IsInRole("Technician");
            IsData = HttpContextHelper.Current.User.IsInRole("Data");
            IsConfiguration = HttpContextHelper.Current.User.IsInRole("Configuration");
            IsRestrictedConfiguration = HttpContextHelper.Current.User.IsInRole("Restricted Configuration");
            SubMenuItems = new List<MenuItem>();
            MenuObject = menuItem;

            var db = new SPM();

            var _children = (from m in db.Menus
                where m.ParentId == menuItem.MenuId
                orderby m.DisplayOrder
                select m).ToList();

            foreach (var m in _children)
                SubMenuItems.Add(new MenuItem(m));

            ExternalLinks = new List<ExternalLink>();
            if (MenuObject.MenuName == "Links")
                ExternalLinks = db.ExternalLinks.ToList();
        }

        public Menu MenuObject { get; set; }
        public List<MenuItem> SubMenuItems { get; set; }
        public List<ExternalLink> ExternalLinks { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsTechnician { get; set; }
        public bool IsData { get; set; }
        public bool IsConfiguration { get; set; }
        public bool IsRestrictedConfiguration { get; set; }
    }
}