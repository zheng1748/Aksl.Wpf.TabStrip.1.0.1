
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Prism;
using Unity;

using Aksl.Infrastructure;

namespace Aksl.Modules.Shell
{
    public class ShellModule : IModule
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;
        #endregion

        #region Constructors
        public ShellModule()
        {
            _regionManager = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IRegionManager>();
            _container = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IUnityContainer>();
        }
        #endregion

        #region IModule
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
          _regionManager.RequestNavigate(RegionNames.ShellContentRegion, nameof(Aksl.Modules.HamburgerMenuSideBarTab.Views.HamburgerMenuSideBarTabHubView));
           // _regionManager.RequestNavigate(RegionNames.ShellContentRegion, nameof(HamburgerMenuNavigationSideBarTab.Views.HamburgerMenuNavigationSideBarHubView));
          // _regionManager.RequestNavigate(RegionNames.ShellContentRegion, nameof(Aksl.Modules.HamburgerMenuTreeSideBarTab.Views.HamburgerMenuTreeSideBarTabHubView));

            _regionManager.RequestNavigate(RegionNames.ShellLoginRegion, nameof(Aksl.Modules.Account.Views.LoginStatusView));
        }
        #endregion
    }
}
