using System.Windows;
using System.Windows.Controls;

using Aksl.Modules.HamburgerMenuNavigationSideBarTab.ViewModels;

namespace Aksl.Modules.HamburgerMenuNavigationSideBarTab.Selectors
{
    public class GroupedMenuDataTemplateSelector : DataTemplateSelector
    {
        public GroupedMenuDataTemplateSelector() { }

        public DataTemplate GroupedMenuTemplate { set; get; }

        public DataTemplate NoGroupedMenuTemplate { set; get; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is GroupedMenuViewModelBase  groupedMenuViewModelBase)
            {
                if (groupedMenuViewModelBase is GroupedMenuViewModel)
                {
                    return GroupedMenuTemplate;
                }
                else
                {
                    return NoGroupedMenuTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
