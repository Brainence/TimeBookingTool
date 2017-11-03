﻿using TBT.App.Models.Base;

namespace TBT.App.Helpers
{
    public class MainWindowTabItem
    {
        public string Title { get; set; }
        public string Tag { get; set; }
        public TabsType Control { get; set; }
        public bool OnlyForAdmins { get; set; }
    }
}
