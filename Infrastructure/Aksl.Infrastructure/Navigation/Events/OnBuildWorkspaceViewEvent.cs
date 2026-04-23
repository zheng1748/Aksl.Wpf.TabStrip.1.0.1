using Prism.Events;
using System;
using System.Collections;

namespace Aksl.Infrastructure.Events
{
    #region Eventbase
    public class OnBuildWorkspaceViewEventbase : PubSubEvent<OnBuildWorkspaceViewEventbase>
    {
        #region Constructors
        public OnBuildWorkspaceViewEventbase()
        {
        }
        #endregion

        #region Properties
        public string Name { get; set; }

        public MenuItem CurrentMenuItem { get; set; }
        #endregion
    }
    #endregion

    #region SideBar Tab
    public class OnBuildHamburgerMenuSideBarTabWorkspaceViewEvent : OnBuildWorkspaceViewEventbase
    {
        #region Constructors
        public OnBuildHamburgerMenuSideBarTabWorkspaceViewEvent()
        {
            Name = typeof(OnBuildHamburgerMenuSideBarTabWorkspaceViewEvent).Name;
        }
        #endregion
    }

    public class OnBuildHamburgerMenuNavigationSideBarTabWorkspaceViewEvent : OnBuildWorkspaceViewEventbase
    {
        #region Constructors
        public OnBuildHamburgerMenuNavigationSideBarTabWorkspaceViewEvent()
        {
            Name = typeof(OnBuildHamburgerMenuNavigationSideBarTabWorkspaceViewEvent).Name;
        }
        #endregion
    }

    public class OnBuildHamburgerMenuTreeSideBarTabWorkspaceViewEvent : OnBuildWorkspaceViewEventbase
    {
        #region Constructors
        public OnBuildHamburgerMenuTreeSideBarTabWorkspaceViewEvent()
        {
            Name = typeof(OnBuildHamburgerMenuTreeSideBarTabWorkspaceViewEvent).Name;
        }
        #endregion
    }
    #endregion
}