﻿using Caliburn.Light;

namespace Demo.SimpleMDI
{
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        int _count = 1;

        public void OpenTab()
        {
            var tab = IoC.GetInstance<TabViewModel>();
            tab.DisplayName = "Tab " + _count++;
            ActivateItem(tab);
        }
    }
}
