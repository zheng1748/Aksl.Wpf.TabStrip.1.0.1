

using Prism.Events;

namespace Aksl.Modules.HamburgerMenuTreeSideBarTab
{
    public class OnActiveTabItemEvent : PubSubEvent<OnActiveTabItemEvent>
    {
        #region Constructors
        public OnActiveTabItemEvent()
        {
        }
        #endregion

        #region Properties
        public TabInformation SelectedTabItem { get; set; }
        #endregion
    }
}