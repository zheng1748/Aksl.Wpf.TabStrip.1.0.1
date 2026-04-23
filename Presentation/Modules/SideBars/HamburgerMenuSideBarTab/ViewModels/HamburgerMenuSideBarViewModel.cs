using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Unity;

using Aksl.Dialogs.Services;

using Aksl.Infrastructure;

namespace Aksl.Modules.HamburgerMenuSideBarTab.ViewModels
{
    public class HamburgerMenuSideBarViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private readonly IMenuService _menuService;
         #endregion

        #region Constructors
        public HamburgerMenuSideBarViewModel(IEventAggregator eventAggregator, IMenuService menuService)
        {
            _eventAggregator = eventAggregator;
            _dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();
            _menuService = menuService;

            AllLeafHamburgerMenuSideBarItems = new();

            RegisterActiveTabItemEvent();
            RegisterOnSelectedTabItemEmptyEvent();
        }
        #endregion

        #region Properties
        public ObservableCollection<HamburgerMenuSideBarItemViewModel> AllLeafHamburgerMenuSideBarItems { get; private set; }
        public string WorkspaceViewEventName { get; set; }

        private HamburgerMenuSideBarItemViewModel _previewSelectedHamburgerMenuItem;
        internal HamburgerMenuSideBarItemViewModel PreviewSelectedHamburgerMenuItem => _previewSelectedHamburgerMenuItem;

        internal HamburgerMenuSideBarItemViewModel _selectedHamburgerMenuSideBarItem;
        public HamburgerMenuSideBarItemViewModel SelectedHamburgerMenuSideBarItem
        {
            get => _selectedHamburgerMenuSideBarItem;
            set
            {
                _previewSelectedHamburgerMenuItem = _selectedHamburgerMenuSideBarItem;

                var previewSelectedHamburgerMenuItem = _selectedHamburgerMenuSideBarItem;

                if (SetProperty(ref _selectedHamburgerMenuSideBarItem, value))
                {
                    if (previewSelectedHamburgerMenuItem is not null && previewSelectedHamburgerMenuItem.IsSelected)
                    {
                        previewSelectedHamburgerMenuItem.IsSelected = false;
                    }

                    if (_selectedHamburgerMenuSideBarItem is not null && !_selectedHamburgerMenuSideBarItem.IsSelected)
                    {
                        _selectedHamburgerMenuSideBarItem.IsSelected = true;
                    }
                }
            }
        }

        private bool _isPaneOpen = false;
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set
            {
                if (SetProperty<bool>(ref _isPaneOpen, value))
                {
                    foreach (var hmbi in AllLeafHamburgerMenuSideBarItems)
                    {
                        hmbi.IsPaneOpen = value;
                    }
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
        }
        #endregion

        #region Register SelectedTabItem Empty Event
        private void RegisterOnSelectedTabItemEmptyEvent()
        {
            _eventAggregator.GetEvent<Aksl.Tabs.OnSelectedTabHeaderItemEmptyEvent>().Subscribe(async (osthiee) =>
            {
                try
                {
                    if (SelectedHamburgerMenuSideBarItem is not null)
                    {
                        SelectedHamburgerMenuSideBarItem.IsSelected = false;
                    }

                    SelectedHamburgerMenuSideBarItem = null;
                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertAsync(message: $"Exception : \"{ex.Message}\"", title: "Error: Selected TabItem Is Empty");
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Register Active TabItem Event
        private void RegisterActiveTabItemEvent()
        {
            _eventAggregator.GetEvent<Aksl.Tabs.OnActiveTabItemEvent>().Subscribe(async (oatie) =>
            {
                var currentTabItem = oatie.SelectedTabItem;

                try
                {
                    SetSelectedHamburgerMenuItem();

                    #region Set Selected HamburgerMenuItem Method
                    void SetSelectedHamburgerMenuItem()
                    {
                        var hamburgerMenuSideBarItemViewModel = AllLeafHamburgerMenuSideBarItems.FirstOrDefault(hmi => hmi.Name.Equals(currentTabItem.Name, StringComparison.InvariantCultureIgnoreCase) ||
                                                                                                                       hmi.Title.Equals(currentTabItem.Title, StringComparison.InvariantCultureIgnoreCase));
                        if (hamburgerMenuSideBarItemViewModel is not null)
                        {
                            if (hamburgerMenuSideBarItemViewModel != SelectedHamburgerMenuSideBarItem)
                            {
                                SelectedHamburgerMenuSideBarItem = hamburgerMenuSideBarItemViewModel;
                            }
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertAsync(message: $"Exception : \"{ex.Message}\"", title: "Error: Active TabItem");
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Create HamburgerMenuItemBar ViewModel Method
        internal async Task CreateHamburgerMenuBarItemViewModelsAsync()
        {
            IsLoading = true;

            var rootMenuItem = await _menuService.GetMenuAsync("All");

            var subMenuItems = rootMenuItem.SubMenus;
            foreach (var smi in subMenuItems)
            {
                List<MenuItem> travelMenuItems = new();
                var allLeafHierarchicalMenuItemViewModels = await GetAllLeafHamburgerMenuSideBarItemViewModels(smi, travelMenuItems);
                AllLeafHamburgerMenuSideBarItems.AddRange(allLeafHierarchicalMenuItemViewModels);
            }

            var allDistinctLeafHamburgerMenuSideBarItems = AllLeafHamburgerMenuSideBarItems.DistinctBy(item => (item.Name, item.Title));
            AllLeafHamburgerMenuSideBarItems = new ObservableCollection<HamburgerMenuSideBarItemViewModel>(allDistinctLeafHamburgerMenuSideBarItems);

            SetWorkspaceViewEventName();

            void SetWorkspaceViewEventName()
            {
                foreach (var hsmi in AllLeafHamburgerMenuSideBarItems)
                {
                    hsmi.WorkspaceViewEventName = this.WorkspaceViewEventName;
                }
            }

            IsLoading = false;
        }
        #endregion

        #region Get All Leaf HamburgerMenuSideBarItemViewModels Method
        internal async Task<IEnumerable<HamburgerMenuSideBarItemViewModel>> GetAllLeafHamburgerMenuSideBarItemViewModels(MenuItem menuItem, IList<MenuItem> travelMenuItems)
        {
            List<HamburgerMenuSideBarItemViewModel> leafHamburgerMenuSideBarItemViewModels = new();

            await RecursiveSubMenuItem(menuItem);

            async Task RecursiveSubMenuItem(MenuItem currentMenuItem)
            {
                var isAddOnLeaf = IsLeaf(currentMenuItem) && (!HasNavigationName(currentMenuItem) || (HasNavigationName(currentMenuItem) && !IsNextNavigation(currentMenuItem)));
                var isAddOnNotLeaf = !IsLeaf(currentMenuItem) && !IsNexOnNotLeaf(currentMenuItem);
                if (!AnyEqualsMenuItems(travelMenuItems, currentMenuItem) && HasTitle(currentMenuItem) && (isAddOnLeaf || isAddOnNotLeaf))
                {
                    leafHamburgerMenuSideBarItemViewModels.Add(new(_eventAggregator, currentMenuItem));
                    travelMenuItems.Add(currentMenuItem);
                }

                if (HasNavigationName(currentMenuItem) && IsNextNavigation(currentMenuItem))
                {
                    currentMenuItem = await _menuService.GetMenuAsync(currentMenuItem.NavigationName);
                }

                if (HasSubMenu(currentMenuItem) && IsNexOnNotLeaf(currentMenuItem))
                {
                    foreach (var smi in currentMenuItem.SubMenus)
                    {
                        await RecursiveSubMenuItem(smi);
                    }
                }
            }

            bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

            bool IsLeaf(MenuItem mi) => (mi is not null) && mi.SubMenus.Count <= 0;

            bool HasTitle(MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.Title);

            bool IsNextNavigation(MenuItem mi) => (mi is not null) && mi.IsNextNavigation;

            bool HasNavigationName(MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.NavigationName);

            bool IsNexOnNotLeaf(MenuItem mi) => (mi is not null) && mi.IsNexOnNotLeaf;

            return leafHamburgerMenuSideBarItemViewModels;
        }
        #endregion

        #region Contain Methods
        private bool AnyEqualsMenuItems(IEnumerable<MenuItem> menuItems, MenuItem menuItem)
        {
            var isEquals = menuItems.Any(mi => IsEqualsNameOrTitle(mi.Name, menuItem.Name) || IsEqualsNameOrTitle(mi.Title, menuItem.Title));

            return isEquals;
        }

        private bool IsEqualsNameOrTitle(string nameOrTitle, string otherNameOrTitle)
        {
            if (string.IsNullOrEmpty(nameOrTitle) || string.IsNullOrEmpty(otherNameOrTitle))
            {
                return false;
            }

            var isEquals = (!string.IsNullOrEmpty(nameOrTitle) && nameOrTitle.Equals(otherNameOrTitle, StringComparison.InvariantCultureIgnoreCase)) ||
                           (!string.IsNullOrEmpty(otherNameOrTitle) && otherNameOrTitle.Equals(nameOrTitle, StringComparison.InvariantCultureIgnoreCase));

            return isEquals;
        }
        #endregion
    }
}
