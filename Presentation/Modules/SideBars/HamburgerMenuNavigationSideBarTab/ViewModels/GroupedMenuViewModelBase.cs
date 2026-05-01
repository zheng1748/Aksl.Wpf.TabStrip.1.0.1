using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;

using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Prism.Unity;
using Unity;

using Aksl.Dialogs.Services;
using Aksl.Toolkit.Controls;
using Aksl.Toolkit.UI;

using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Modules.HamburgerMenuNavigationSideBarTab.Views;
using System.Windows.Media;

namespace Aksl.Modules.HamburgerMenuNavigationSideBarTab.ViewModels
{
    public abstract class GroupedMenuViewModelBase : BindableBase
    {
        #region Constructors
        public GroupedMenuViewModelBase()
        {
        }
        #endregion
    }
}
