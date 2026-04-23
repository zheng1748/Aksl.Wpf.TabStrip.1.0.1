using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

namespace Aksl.Tabs.ViewModels
{
    public class TabHeaderViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        #endregion

        #region Constructors
        public TabHeaderViewModel()
        {
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();

            TabHeaderItems = new();
        }
        #endregion

        #region Properties
        public ObservableCollection<TabHeaderItemViewModel> TabHeaderItems { get; set; }

        private TabHeaderItemViewModel _selectedTabHeaderItem;
        public TabHeaderItemViewModel SelectedTabHeaderItem
        {
            get => _selectedTabHeaderItem;
            set => SetProperty<TabHeaderItemViewModel>(ref _selectedTabHeaderItem, value);
        }

        private bool _isRequestClose;
        public bool IsRequestClose
        {
            get => _isRequestClose;
            set => SetProperty<bool>(ref _isRequestClose, value);
        }
        #endregion

        public event EventHandler RequestClose;

        #region Methods
        public void Add(TabInformation tabInformation)
        {
            TabHeaderItemViewModel newTabHeaderItemViewModel = new(tabInformation);

            AddCore(newTabHeaderItemViewModel);
        }

        private void AddCore(TabHeaderItemViewModel newTabHeaderItemViewModel)
        {
            if (!IsExistsTabItems(newTabHeaderItemViewModel.Name, newTabHeaderItemViewModel.Title))
            {
                TabHeaderItems.Add(newTabHeaderItemViewModel);

                newTabHeaderItemViewModel.RequestClose += this.OnTabHeaderItemRequestClose;
            }

            AddPropertyChanged();
            void AddPropertyChanged()
            {
                newTabHeaderItemViewModel.PropertyChanged += (sender, e) =>
                {
                    if (sender is TabHeaderItemViewModel tbhivm)
                    {
                        if (e.PropertyName == nameof(TabHeaderItemViewModel.IsSelected))
                        {
                            if (SelectedTabHeaderItem is null)
                            {
                                SelectedTabHeaderItem = tbhivm;
                            }

                            if (SelectedTabHeaderItem is not null && tbhivm.IsSelected && tbhivm != SelectedTabHeaderItem)
                            {
                                //SelectedTabHeaderItem.IsSelected = false;

                                SelectedTabHeaderItem = tbhivm;
                            }
                        }
                    }
                };
            }

            SetActiveTabItem(newTabHeaderItemViewModel);

            RaisePropertyChanged(nameof(TabHeaderItems));
        }

        private void OnTabHeaderItemRequestClose(object sender, EventArgs e)
        {
            if (sender is TabHeaderItemViewModel tabHeaderItemViewModel)
            {
                TabHeaderItemViewModel nextTabHeaderItemViewModel=default;
                if (TabHeaderItems.Any())
                {
                    nextTabHeaderItemViewModel = GetNextTabHeaderItemByInfo(tabHeaderItemViewModel.TabInformation);
                }

                Remove(tabHeaderItemViewModel);

                RequestClose?.Invoke(sender, EventArgs.Empty);

                if (nextTabHeaderItemViewModel is not null)
                {
                    SelectedTabHeaderItem.IsSelected = false;

                    nextTabHeaderItemViewModel.IsSelected = true;
                    SelectedTabHeaderItem = nextTabHeaderItemViewModel;
                }
            }
        }

        private void SetActiveTabItem(TabHeaderItemViewModel tabHeaderItemViewModel)
        {
            if (tabHeaderItemViewModel is not null && !IsEqualsTabItemViewModel(tabHeaderItemViewModel, SelectedTabHeaderItem))
            {
                if (SelectedTabHeaderItem is null)
                {
                    tabHeaderItemViewModel.IsSelected = true;
                    SelectedTabHeaderItem = tabHeaderItemViewModel;
                }

                if (SelectedTabHeaderItem is not null && tabHeaderItemViewModel != SelectedTabHeaderItem)
                {
                    SelectedTabHeaderItem.IsSelected = false;

                    tabHeaderItemViewModel.IsSelected = true;
                    SelectedTabHeaderItem = tabHeaderItemViewModel;
                }
            }
        }

        public void Remove(TabHeaderItemViewModel tabHeaderItemViewModel)
        {
            if (tabHeaderItemViewModel is not null)
            {
                if (IsExistsTabItems(tabHeaderItemViewModel.Name, tabHeaderItemViewModel.Title))
                {
                    TabHeaderItems.Remove(tabHeaderItemViewModel);

                    tabHeaderItemViewModel.RequestClose -= this.OnTabHeaderItemRequestClose;
                }

                if (!TabHeaderItems.Any())
                {
                    SelectedTabHeaderItem = null;

                    _eventAggregator.GetEvent<OnSelectedTabHeaderItemEmptyEvent>().Publish(new());
                }

                RaisePropertyChanged(nameof(TabHeaderItems));
            }
        }

        public void SetTabItem(TabInformation tabInformation)
        {
            var tabHeaderItemViewModel = GetTabHeaderItemByInfo(tabInformation);
            if (tabHeaderItemViewModel is not null)
            {
                SetActiveTabItem(tabHeaderItemViewModel);
            }
        }

        private TabHeaderItemViewModel GetTabHeaderItemByInfo(TabInformation tabInformation)
        {
            var tabHeaderItemViewModel = TabHeaderItems.FirstOrDefault(ti => IsEqualsNameOrTitle(ti.Name, tabInformation.Name) || IsEqualsNameOrTitle(ti.Title, tabInformation.Title));

            return tabHeaderItemViewModel;
        }

        private TabHeaderItemViewModel GetNextTabHeaderItemByInfo(TabInformation tabInformation)
        {
            TabHeaderItemViewModel nextTabHeaderItemViewModel = default;

            var index = TabHeaderItems.ToList().FindIndex(ti => IsEqualsNameOrTitle(ti.Name, tabInformation.Name) || IsEqualsNameOrTitle(ti.Title, tabInformation.Title));

            if ((index + 1) <= (TabHeaderItems.Count - 1))
            {
                nextTabHeaderItemViewModel = TabHeaderItems[index + 1];
            }
            else if ((index - 1) >= 0)
            {
                nextTabHeaderItemViewModel = TabHeaderItems[index - 1];
            }

            //if (index == (TabHeaderItems.Count - 1))
            //{
            //    if ((index - 1) >= 0)
            //    {
            //        nextTabHeaderItemViewModel = TabHeaderItems[index - 1];
            //    }
            //}
            //else if (index < (TabHeaderItems.Count - 1))
            //{
            //    if ((index + 1) <= (TabHeaderItems.Count - 1))
            //    {
            //        nextTabHeaderItemViewModel = TabHeaderItems[index + 1];
            //    }
            //}

            return nextTabHeaderItemViewModel;
        }
        #endregion

        #region Contain Methods
        private bool IsExistsTabItems(string name, string title)
        {
            var isAny = TabHeaderItems.Any(ti => IsEqualsNameOrTitle(ti.Name, name) || IsEqualsNameOrTitle(ti.Title, title));

            return isAny;
        }

        private bool IsEqualsTabItemViewModel(TabHeaderItemViewModel tabHeaderItemViewModel, TabHeaderItemViewModel otherTabHeaderItemViewModel)
        {
            var isEquals = (IsEqualsNameOrTitle(tabHeaderItemViewModel?.Name, otherTabHeaderItemViewModel?.Name) ||
                            IsEqualsNameOrTitle(tabHeaderItemViewModel?.Title, otherTabHeaderItemViewModel?.Title));

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
