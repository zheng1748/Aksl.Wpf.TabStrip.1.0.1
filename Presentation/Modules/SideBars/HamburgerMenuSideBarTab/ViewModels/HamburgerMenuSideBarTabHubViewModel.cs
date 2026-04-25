using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Threading;

using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Unity;

using Aksl.Dialogs.Services;

using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Tabs.ViewModels;

namespace Aksl.Modules.HamburgerMenuSideBarTab.ViewModels
{
    public class HamburgerMenuSideBarTabHubViewModel : BindableBase, INavigationAware
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRegionManager _regionManager;
        private readonly IDialogViewService _dialogViewService;
        private readonly IMenuService _menuService;
        private string _workspaceViewEventName;
        #endregion

        #region Constructors
        public HamburgerMenuSideBarTabHubViewModel()
        {
            _container = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IUnityContainer>();
            _regionManager = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IRegionManager>();
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();
            _dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();

            _menuService = _container.Resolve<IMenuService>();

            TabViewModel = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<TabViewModel>();
            TabHubViewModel = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<TabHubViewModel>();

            SelectedDisplayMode = SplitViewDisplayMode.CompactInline;
            IsPaneOpen = true;
            SelectedPlacement = SplitViewPanePlacement.Left;

            _workspaceViewEventName = "OnBuildHamburgerMenuSideBarTabWorkspaceViewEvent";

            RegisterBuildWorkspaceViewEvents();
            RegisterHamburgerMenuBarPaneOpenEvent();
        }
        #endregion

        #region Properties
        public HamburgerMenuSideBarViewModel HamburgerMenuSideBar { get; private set; }
        public TabViewModel TabViewModel { get; set; }
        public TabHubViewModel TabHubViewModel { get; set; }
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
        }
        #endregion

        #region HamburgerMenu Properties
        private Brush _paneBackground = new SolidColorBrush(Colors.White);
        public Brush PaneBackground
        {
            get => _paneBackground;
            set => SetProperty<Brush>(ref _paneBackground, value);
        }

        public GridLength OpenPaneGridLength
        {
            get { return new GridLength(OpenPaneLength); }
        }

        private double _openPaneLength = 320d;
        public double OpenPaneLength
        {
            get => _openPaneLength;
            set => SetProperty<double>(ref _openPaneLength, value);
        }

        public GridLength CompactPaneGridLength
        {
            get { return new GridLength(CompactPaneLength); }
        }

        private double _compactPaneLength = 48d;
        public double CompactPaneLength
        {
            get => _compactPaneLength;
            set => SetProperty<double>(ref _compactPaneLength, value);
        }

        private bool _isPaneOpen = false;
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set
            {
                if (SetProperty<bool>(ref _isPaneOpen, value))
                {
                    if (HamburgerMenuSideBar is not null)
                    {
                        HamburgerMenuSideBar.IsPaneOpen = value;
                    }

                    VisualState = GetVisualState();
                }
            }
        }

        public List<SplitViewDisplayMode> DisplayModeList
        {
            get => Enum.GetValues(typeof(SplitViewDisplayMode)).Cast<SplitViewDisplayMode>().ToList();
        }

        private SplitViewDisplayMode _selectedDisplayMode = SplitViewDisplayMode.Overlay;
        public SplitViewDisplayMode SelectedDisplayMode
        {
            get => _selectedDisplayMode;
            set
            {
                if (SetProperty<SplitViewDisplayMode>(ref _selectedDisplayMode, value))
                {
                    VisualState = GetVisualState();
                }
            }
        }

        public List<SplitViewPanePlacement> PanePlacementList
        {
            get => Enum.GetValues(typeof(SplitViewPanePlacement)).Cast<SplitViewPanePlacement>().ToList();
        }

        private SplitViewPanePlacement _selectedPanePlacement = SplitViewPanePlacement.Left;
        public SplitViewPanePlacement SelectedPlacement
        {
            get => _selectedPanePlacement;
            set
            {
                if (SetProperty<SplitViewPanePlacement>(ref _selectedPanePlacement, value))
                {
                    VisualState = GetVisualState();
                }
            }
        }

        private string _visualState;
        public string VisualState
        {
            get => _visualState;
            set => SetProperty<string>(ref _visualState, value);
        }
        #endregion

        #region Get HamburgerMenu State Method
        private bool IsCompact
        {
            get
            {
                return SelectedDisplayMode switch
                {
                    SplitViewDisplayMode.CompactInline or SplitViewDisplayMode.CompactOverlay => true,
                    _ => false,
                };
            }
        }

        private bool IsInline
        {
            get
            {
                return SelectedDisplayMode switch
                {
                    SplitViewDisplayMode.CompactInline or SplitViewDisplayMode.Inline => true,
                    _ => false
                };
            }
        }

        protected virtual string GetVisualState()
        {
            string state;

            if (IsPaneOpen)
            {
                state = "Open";
                state += IsInline ? "Inline" : SelectedDisplayMode.ToString();
            }
            else
            {
                state = "Closed";
                if (IsCompact)
                {
                    state += "Compact";
                }
                //else
                //{
                //    return state;
                //}
            }

            state += SelectedPlacement.ToString();

            return state;
        }
        #endregion

        #region Register BuildWorkspaceView Event
        private void RegisterBuildWorkspaceViewEvents()
        {
            var buildHWorkspaceViewEvent = _eventAggregator.GetEvent(_workspaceViewEventName) as OnBuildWorkspaceViewEventbase;
            Debug.Assert(buildHWorkspaceViewEvent is not null);

            buildHWorkspaceViewEvent.Subscribe(async (bmve) =>
            {
                var currentMenuItem = bmve.CurrentMenuItem;
              
                try
                {
                    IEnumerable<MenuItem> subMenus = null;
                    Tabs.Views.TabHubView subTabView = default;

                    if (!string.IsNullOrEmpty(currentMenuItem.NavigationName))
                    {
                        var parentMenuItem = await _menuService.GetMenuAsync(currentMenuItem.NavigationName);
                        subMenus = parentMenuItem.SubMenus;
                    }

                    if (string.IsNullOrEmpty(currentMenuItem.NavigationName) && HasSubMenu(currentMenuItem) && IsExistsViewInSubMenu(currentMenuItem))
                    {
                        subMenus = currentMenuItem.SubMenus.Where(sm => !string.IsNullOrEmpty(sm.ViewName)).ToList();
                    }

                    if (subMenus is not null)
                    {
                        string viewTypeAssemblyQualifiedName = currentMenuItem.ViewName;
                        Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);
                        if (viewType is not null)
                        {
                            //var currentView = TabViewModel.GetStoreViewElement(viewType);
                            var currentView = TabHubViewModel.GetViewElementByType(viewType);

                            if (currentView is not null)
                            {
                                if (currentMenuItem.IsCacheable)
                                {
                                }
                                else
                                {
                                    AddSubTabView();
                                }
                            }
                            else
                            {
                                AddSubTabView();
                            }
                        }
                    }

                    bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

                    bool IsExistsViewInSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any(sm => !string.IsNullOrEmpty(sm.ViewName));

                    void AddSubTabView()
                    {
                        TabHubViewModel subTtabViewModel = new();

                        subTabView = new Tabs.Views.TabHubView();
                        subTabView.DataContext = subTtabViewModel;

                        foreach (var smi in subMenus)
                        {
                            Aksl.Tabs.TabInformation subTabInformation = new()
                            {
                                Name = smi.Name,
                                Title = smi.Title,
                                IconKind = smi.IconKind,
                                ViewName = smi.ViewName,
                                CloseTabButtonVisibility = Visibility.Collapsed
                            };

                            subTtabViewModel.Add(subTabInformation);
                        }

                        //subTtabViewModel.SetFirstActiveTabItem();
                    }

                    Aksl.Tabs.TabInformation tabInformation = new()
                    {
                        Name = currentMenuItem.Name,
                        Title = currentMenuItem.Title,
                        IconKind = currentMenuItem.IconKind,
                        ViewName = currentMenuItem.ViewName
                    };

                    if (subTabView is not null)
                    {
                        tabInformation.ViewElement = subTabView;
                    }

                    if (TabHubViewModel.IsActiveTabItem(tabInformation))
                    {
                        return;
                    }
                    await LoadViewAsync();

                    #region LoadView Method
                    async Task LoadViewAsync()
                    {
                        string viewTypeAssemblyQualifiedName = currentMenuItem.ViewName;
                        Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);
                        if (viewType is not null)
                        {
                            var currentView = TabHubViewModel.GetViewElementByType(viewType);

                            if (currentView is not null)
                            {
                                if (currentMenuItem.IsCacheable)
                                {
                                    TabHubViewModel.SetTabItem(tabInformation);
                                }
                                else
                                {
                                    TabHubViewModel.RetsetTabItem(tabInformation);
                                }
                            }
                            else
                            {
                                AddView();
                            }

                            void AddView()
                            {
                                if (CanAddView())
                                {
                                    TabHubViewModel.Add(tabInformation);
                                }
                            }

                            bool CanAddView() => !string.IsNullOrEmpty(currentMenuItem.ModuleName);
                        }
                        else
                        {
                            await _dialogViewService.AlertAsync(message: $"Unable to find \"{viewTypeAssemblyQualifiedName}\".", title: $"Error:Missing Type");
                        }
                    }
                    #endregion

                    #region LoadView Method
                    async Task LoadView()
                    {
                        async Task RecursiveSubMenuItem(MenuItem menuItem)
                        {
                            IEnumerable<MenuItem> subMenus = null;

                            if (HasNavigationName(menuItem))
                            {
                                var parentMenuItem = await _menuService.GetMenuAsync(currentMenuItem.NavigationName);
                                subMenus = parentMenuItem.SubMenus;
                            }

                            if (!HasNavigationName(menuItem) && HasSubMenu(currentMenuItem) && IsExistsViewInSubMenu(currentMenuItem))
                            {
                                subMenus = currentMenuItem.SubMenus.Where(sm => !string.IsNullOrEmpty(sm.ViewName)).ToList();
                            }

                            if (subMenus is not null)
                            {
                                foreach (var smi in subMenus)
                                {
                                    await RecursiveSubMenuItem(smi);
                                }
                            }
                        }

                        bool HasNavigationName(MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.NavigationName);

                        bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();
                    }
                    #endregion

                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertAsync(message: $"Unable to loading \"{currentMenuItem.ModuleName}\" module.: \"{ex.Message}\"", title: "Error: Load Module");
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Register HamburgerMenuBarPaneOpen Event
        private void RegisterHamburgerMenuBarPaneOpenEvent()
        {
            _eventAggregator.GetEvent<OnHamburgerMenuBarPaneOpenEvent>().Subscribe(async (hmbpoe) =>
            {
                try
                {
                    IsPaneOpen = hmbpoe.IsPaneOpen;
                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertAsync(message: $"Subscribe PaneOpen Event Error.: \"{ex.Message}\"", title: "Error");
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Create HamburgerBarMenu ViewModel Method
        private async Task CreateHamburgerMenuBarViewModelAsync()
        {
            IsLoading = true;

            try
            {
                HamburgerMenuSideBar = new(_eventAggregator, _menuService);
                AddPropertyChanged();

                void AddPropertyChanged()
                {
                    HamburgerMenuSideBar.PropertyChanged += (sender, e) =>
                    {
                        if (sender is HamburgerMenuSideBarViewModel hmbvm)
                        {
                            if (e.PropertyName == nameof(HamburgerMenuSideBarViewModel.IsLoading) && !hmbvm.IsLoading)
                            {
                                IsLoading = false;
                            }
                        }
                    };
                }

                HamburgerMenuSideBar.WorkspaceViewEventName = _workspaceViewEventName;
                await HamburgerMenuSideBar.CreateHamburgerMenuBarItemViewModelsAsync();
                HamburgerMenuSideBar.IsPaneOpen = IsPaneOpen;
                RaisePropertyChanged(nameof(HamburgerMenuSideBar));
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync(message: $"Unable to create hamburger menu : \"{ex.Message}\"", title: "Error: Create HamburgerMenu");
            }
            finally
            {
                if (IsLoading)
                {
                    IsLoading = false;
                }
            }
        }
        #endregion

        #region INavigationAware
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;
            if (parameters is not null)
            {
                if (parameters.Count == 0)
                {
                    CreateHamburgerMenuBarViewModelAsync().GetAwaiter().GetResult();
                }
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
        #endregion
    }
}
