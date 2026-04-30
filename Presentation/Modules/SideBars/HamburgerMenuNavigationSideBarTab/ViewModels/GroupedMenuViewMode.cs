using System;
using System.Collections.Generic;

using Prism.Events;
using Prism.Mvvm;

using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Toolkit.Controls;

namespace Aksl.Modules.HamburgerMenuNavigationSideBarTab.ViewModels
{
    public abstract class NavigationBarBase : BindableBase
    {
        #region Constructors
        public NavigationBarBase()
        {
        }
        #endregion
    }

    public class GroupedMenuViewModel : NavigationBarBase
    {
        #region Members
        private readonly IEventAggregator _eventAggregator;
        private readonly MenuItem _headerMenuItem;
        private IEnumerable<MenuItem> _leafMenuItems;
        #endregion

        #region Constructors
        public GroupedMenuViewModel(IEventAggregator eventAggregator, int groupIndex, MenuItem headerMenuItem, IEnumerable<MenuItem> leafMenuItems) : base()
        {
            _eventAggregator = eventAggregator;
            GroupIndex = groupIndex;
            _leafMenuItems = leafMenuItems;
            _headerMenuItem = headerMenuItem;

            CreateMenuContentViewModels();
        }
        #endregion

        #region Properties
        public int GroupIndex { get; }

        public string HeaderTitle => _headerMenuItem.Title;

        public MenuContentViewModel MenuContent { get; private set; }

        // public MenuItemViewModel SelectedMenuItem { get; private set; }
        private MenuItemViewModel _selectedMenuItem;
        public MenuItemViewModel SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                if (SetProperty(ref _selectedMenuItem, value))
                {
                    MenuContent.SelectedMenuItem = value;
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
                    MenuContent.IsPaneOpen = value;
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        #endregion

        #region Create MenuContent ViewModel Method
        internal void CreateMenuContentViewModels()
        {
            IsLoading = true;

            MenuContentViewModel menuContentViewModel = new(_eventAggregator, GroupIndex, _leafMenuItems);
            AddPropertyChanged();

            void AddPropertyChanged()
            {
                menuContentViewModel.PropertyChanged += (sender, e) =>
                {
                    if (sender is MenuContentViewModel mcvm)
                    {
                        if (e.PropertyName == nameof(MenuContentViewModel.IsLoading) && !mcvm.IsLoading)
                        {
                            IsLoading = false;
                        }

                        if (e.PropertyName == nameof(MenuContentViewModel.SelectedMenuItem))
                        {
                            _selectedMenuItem = mcvm.SelectedMenuItem;
                            RaisePropertyChanged(nameof(MenuContent));
                        }
                    }
                };
            }

            menuContentViewModel.CreateMenuItemViewModels();
            MenuContent = menuContentViewModel;

            IsLoading = false;
        }
        #endregion
    }

    public class NavigationBarItemViewModel : NavigationBarBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        private readonly MenuItem _menuItem;
        #endregion

        #region Constructors
        public NavigationBarItemViewModel(IEventAggregator eventAggregator,int index, MenuItem menuItem)
        {
            _eventAggregator = eventAggregator;
            Index = index;
            _menuItem = menuItem;
        }
        #endregion

        #region Properties
        public MenuItem MenuItem => _menuItem;
        public int Index { get; }
        public string WorkspaceViewEventName { get; set; }
        public string Name => _menuItem.Name;
        public string Title => _menuItem.Title;
        public bool IsLeaf => _menuItem.SubMenus.Count <= 0;
        private bool IsNextNavigation => _menuItem.IsNextNavigation;
        private bool HasNavigationName => !string.IsNullOrEmpty(_menuItem.NavigationName);
        private bool IsNexOnNotLeaf => _menuItem.IsNexOnNotLeaf;

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty<bool>(ref _isSelected, value))
                {
                    var isSelectedOnLeaf = IsLeaf && (!HasNavigationName || (HasNavigationName && !IsNextNavigation));
                    var isSelectedOnNotLeaf = !IsLeaf && !IsNexOnNotLeaf;

                    if (isSelectedOnLeaf && _isSelected)
                    {
                        var buildHWorkspaceViewEvent = _eventAggregator.GetEvent(WorkspaceViewEventName) as OnBuildWorkspaceViewEventbase;
                        buildHWorkspaceViewEvent.Publish(new() { CurrentMenuItem = _menuItem });
                    }

                    if (isSelectedOnNotLeaf && _isSelected)
                    {
                        var buildHWorkspaceViewEvent = _eventAggregator.GetEvent(WorkspaceViewEventName) as OnBuildWorkspaceViewEventbase;
                        buildHWorkspaceViewEvent.Publish(new() { CurrentMenuItem = _menuItem });
                    }
                }
            }
        }

        public PackIconKind IconKind
        {
            get
            {
                PackIconKind kind = PackIconKind.None;

                _ = Enum.TryParse(_menuItem.IconKind, out kind);

                return kind;
            }
        }

        private bool _isPaneOpen = false;
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set => SetProperty<bool>(ref _isPaneOpen, value);
        }

        protected bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty<bool>(ref _isEnabled, value);
        }
        #endregion
    }
}
