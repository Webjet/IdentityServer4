using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminPortal.BusinessServices
{
    public class UiLinkMapper
    {
        public List<Tab> Tabs { get; set; }
    }

    public class Tab
    {
        public string Key { get; set; }

        public string Text { get; set; }

        public List<Section> Sections { get; set; }
    }

    public class Section
    {
        public string  Key { get; set; }

        public string Text { get; set; }

        public List<MenuItem> MenuItems { get; set; }
    }

    public class MenuItem
    {
        //Key is same as ResourceKey defined in RoleBasedMenuItemMap.xml
        public string Key { get; set; }

        public string Text { get; set; }

        public string  Link { get; set; }

    }
}
