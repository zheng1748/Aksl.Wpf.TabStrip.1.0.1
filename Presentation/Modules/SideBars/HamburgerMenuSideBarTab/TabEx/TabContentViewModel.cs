using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

using Aksl.Toolkit.Controls;

namespace Aksl.Tabs.ViewModels
{
    public class TabContentViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        #endregion

        #region Constructors
        public TabContentViewModel()
        {
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();


            TabContentItems=new();
        }
        #endregion

        #region Properties
        public ObservableCollection<TabContentItemViewModel> TabContentItems { get; set; }

        private TabContentItemViewModel _selectedTabContentItem;
        public TabContentItemViewModel SelectedTabContentItem
        {
            get => _selectedTabContentItem;
            set => SetProperty<TabContentItemViewModel>(ref _selectedTabContentItem, value);
        }
        #endregion

        #region Methods
        public void Add(TabInformation tabInformation)
        {
            TabContentItemViewModel newTabContentItemViewModel = new(tabInformation);

            if (tabInformation.ViewElement is not null)
            {
                newTabContentItemViewModel.ViewElement = tabInformation.ViewElement;
            }

           AddCore(newTabContentItemViewModel);
        }

        private void AddCore(TabContentItemViewModel newTabContentItemViewModel)
        {
            if (!IsExistsTabItems(newTabContentItemViewModel.Name, newTabContentItemViewModel.Title))
            {
                TabContentItems.Add(newTabContentItemViewModel);
            }

            AddPropertyChanged();
            void AddPropertyChanged()
            {
                newTabContentItemViewModel.PropertyChanged += (sender, e) =>
                {
                    if (sender is TabContentItemViewModel tcivm)
                    {
                        if (e.PropertyName == nameof(TabContentItemViewModel.IsSelected))
                        {
                            if (SelectedTabContentItem is null)
                            {
                                SelectedTabContentItem = tcivm;
                            }

                            if (SelectedTabContentItem is not null && tcivm != SelectedTabContentItem)
                            {
                                SelectedTabContentItem.IsSelected = false;

                                SelectedTabContentItem = tcivm;
                            }
                        }
                    }
                };
            }

            SetActiveTabItem(newTabContentItemViewModel);

            RaisePropertyChanged(nameof(TabContentItems));
        }

        public void SetActiveTabItem(TabContentItemViewModel tabContentItemViewModel)
        {
            if (tabContentItemViewModel is not null && !IsEqualsTabItemViewModel(tabContentItemViewModel, SelectedTabContentItem))
            {
                if (SelectedTabContentItem is null)
                {
                    SelectedTabContentItem = tabContentItemViewModel; 
                    SelectedTabContentItem.IsSelected = true;
                }

                if (SelectedTabContentItem is not null && tabContentItemViewModel != SelectedTabContentItem)
                {
                    SelectedTabContentItem.IsSelected = false;
                   // SelectedTabContentItem.ViewElementVisibility = Visibility.Collapsed;

                    SelectedTabContentItem = tabContentItemViewModel;
                    SelectedTabContentItem.IsSelected = true;
                  //  SelectedTabContentItem.ViewElementVisibility = Visibility.Visible;
                }
            }
        }

        public void SetTabItem(TabInformation tabInformation)
        {
            var tabContentItemViewModel = GetTabContentItemByInfo(tabInformation);
            if (tabContentItemViewModel is not null)
            {
                SetActiveTabItem(tabContentItemViewModel);
            }
        }

        public void SetTabItemOnClose(TabInformation tabInformation)
        {
            var tabContentItemViewModel = GetTabContentItemByInfo(tabInformation);
            if (tabContentItemViewModel is not null)
            {
                Remove(tabContentItemViewModel);
            }
        }

        public void Remove(TabContentItemViewModel tabContentItemViewModel)
        {
            if (tabContentItemViewModel is not null)
            {
                if (IsExistsTabItems(tabContentItemViewModel.Name, tabContentItemViewModel.Title))
                {
                    TabContentItems.Remove(tabContentItemViewModel);
                }

                if (!TabContentItems.Any())
                {
                    SelectedTabContentItem = null;
                }

                RaisePropertyChanged(nameof(TabContentItems));
            }
        }

        public void SetTabItemOnSelected(TabInformation tabInformation)
        {
            //var tabContentItemViewModel = TabContentItems.FirstOrDefault(ti => ti.ViewElementType == viewType);
            //if (tabContentItemViewModel is not null)
            //{
            //    SetActiveTabItem(tabContentItemViewModel);
            //}
            var tabContentItemViewModel = GetTabContentItemByInfo(tabInformation);
            if (tabContentItemViewModel is not null)
            {
                SetActiveTabItem(tabContentItemViewModel);
            }
        }

        private TabContentItemViewModel GetTabContentItemByInfo(TabInformation tabInformation)
        {
            var tabContentItemViewModel = TabContentItems.FirstOrDefault(ti => IsEqualsNameOrTitle(ti.Name, tabInformation.Name) || IsEqualsNameOrTitle(ti.Title, tabInformation.Title));

            return tabContentItemViewModel;
        }

        public System.Windows.DependencyObject GetViewElementByType(Type viewType)
        {
            var tabContentItemViewModel = TabContentItems.FirstOrDefault(ti => ti.ViewElementType == viewType);

            return tabContentItemViewModel?.ViewElement;
        }
        #endregion

        #region Contain Methods
        private bool IsExistsTabItems(string name, string title)
        {
            var isAny = TabContentItems.Any(ti => IsEqualsNameOrTitle(ti.Name, name) || IsEqualsNameOrTitle(ti.Title, title));

            return isAny;
        }

        private bool IsEqualsTabItemViewModel(TabContentItemViewModel tabContentItemViewModel, TabContentItemViewModel otherTabContentItemViewModel)
        {
            var isEquals = (IsEqualsNameOrTitle(tabContentItemViewModel?.Name, otherTabContentItemViewModel?.Name) ||
                            IsEqualsNameOrTitle(tabContentItemViewModel?.Title, otherTabContentItemViewModel?.Title));

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
