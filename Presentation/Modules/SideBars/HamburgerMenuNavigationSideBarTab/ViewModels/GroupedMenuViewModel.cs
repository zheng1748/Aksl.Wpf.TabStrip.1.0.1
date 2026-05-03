using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;

using Prism.Events;

using Aksl.Infrastructure;

namespace Aksl.Modules.HamburgerMenuNavigationSideBarTab.ViewModels
{
    public class GroupedMenuViewModel : GroupedMenuViewModelBase
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
                SetProperty(ref _selectedMenuItem, value);
                //if (SetProperty(ref _selectedMenuItem, value))
                //{
                //    MenuContent.SelectedMenuItem = value;
                //}
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
                        //if (e.PropertyName == nameof(MenuContentViewModel.IsLoading) && !mcvm.IsLoading)
                        //{
                        //    IsLoading = false;
                        //}

                        if (e.PropertyName == nameof(MenuContentViewModel.SelectedMenuItem))
                        {
                            //_selectedMenuItem = mcvm.SelectedMenuItem;
                            SelectedMenuItem = mcvm.SelectedMenuItem;
                          //  RaisePropertyChanged(nameof(MenuContent));
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
}
