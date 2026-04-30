using System.Windows;
using System.Windows.Controls;

namespace Aksl.Modules.HamburgerMenuNavigationSideBarTab.ViewModels
{
    public class GroupedMenuDataTemplateSelector : DataTemplateSelector
    {
        public GroupedMenuDataTemplateSelector() { }

        public DataTemplate GroupedMenuTemplate { set; get; }

        public DataTemplate NavigationBarTemplate { set; get; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is NavigationBarBase  navigationBarBase)
            {
                if (navigationBarBase is GroupedMenuViewModel)
                {
                    return GroupedMenuTemplate;
                }
                else
                {
                    return NavigationBarTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
