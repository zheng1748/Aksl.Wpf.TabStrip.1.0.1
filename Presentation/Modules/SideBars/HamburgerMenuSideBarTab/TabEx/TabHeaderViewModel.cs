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

            //TabHeaderItems = new();

            ActiveTabHeaderItems = new();
            StoreTabHeaderItems = new();
        }
        #endregion

        #region Properties
        public ObservableCollection<TabHeaderItemViewModel> ActiveTabHeaderItems { get; }
        public List<TabHeaderItemViewModel> StoreTabHeaderItems { get; }

      //  public ObservableCollection<TabHeaderItemViewModel> TabHeaderItems { get; set; }

        private TabHeaderItemViewModel _selectedTabHeaderItem;
        public TabHeaderItemViewModel SelectedTabHeaderItem
        {
            get => _selectedTabHeaderItem;
            set => SetProperty<TabHeaderItemViewModel>(ref _selectedTabHeaderItem, value);
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
            if (!IsExistsActivTabHeaderItems(newTabHeaderItemViewModel.Name, newTabHeaderItemViewModel.Title))
            {
                ActiveTabHeaderItems.Add(newTabHeaderItemViewModel);

                newTabHeaderItemViewModel.RequestClose += this.OnTabHeaderItemRequestClose;
            }

           //AddPropertyChanged();
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

            StoreTabHeaderItem(newTabHeaderItemViewModel);

            SetActiveTabHeaderItem(newTabHeaderItemViewModel);

            RaisePropertyChanged(nameof(ActiveTabHeaderItems));
        }

        private void StoreTabHeaderItem(TabHeaderItemViewModel tabHeaderItemViewModel)
        {
            if (!IsExistsStoreTabHeaderItems(tabHeaderItemViewModel.Name, tabHeaderItemViewModel.Title))
            {
                StoreTabHeaderItems.Add(tabHeaderItemViewModel);
            }
        }

        private void OnTabHeaderItemRequestClose(object sender, EventArgs e)
        {
            if (sender is TabHeaderItemViewModel tabHeaderItemViewModel)
            {
                TabHeaderItemViewModel nextTabHeaderItemViewModel=default;
                if (ActiveTabHeaderItems.Any())
                {
                    nextTabHeaderItemViewModel = GetNextActiveTabHeaderItemByInfo(tabHeaderItemViewModel.TabInformation);
                }

                Remove(tabHeaderItemViewModel);

                RequestClose?.Invoke(sender, EventArgs.Empty);

                if (nextTabHeaderItemViewModel is not null)
                {
                    if (SelectedTabHeaderItem is not null)
                    {
                        SelectedTabHeaderItem.IsSelected = false;
                    }

                    SelectedTabHeaderItem = nextTabHeaderItemViewModel;
                   // SelectedTabHeaderItem.IsSelected = true;
                }
            }
        }

        private void SetActiveTabHeaderItem(TabHeaderItemViewModel tabHeaderItemViewModel)
        {
            if (tabHeaderItemViewModel is not null && !IsEqualsTabHeaderItemViewModel(tabHeaderItemViewModel, SelectedTabHeaderItem))
            {
                if (SelectedTabHeaderItem is null)
                {
                    SelectedTabHeaderItem = tabHeaderItemViewModel;
                    //SelectedTabHeaderItem.IsSelected = true;
                }

                if (SelectedTabHeaderItem is not null && tabHeaderItemViewModel != SelectedTabHeaderItem)
                {
                    SelectedTabHeaderItem.IsSelected = false;

                    SelectedTabHeaderItem = tabHeaderItemViewModel;
                  //  SelectedTabHeaderItem.IsSelected = true;

                    //tabHeaderItemViewModel.IsSelected = true;
                    //SelectedTabHeaderItem = tabHeaderItemViewModel;
                }
            }
        }

        public void Remove(TabHeaderItemViewModel tabHeaderItemViewModel)
        {
            if (tabHeaderItemViewModel is not null)
            {
                if (IsExistsActivTabHeaderItems(tabHeaderItemViewModel.Name, tabHeaderItemViewModel.Title))
                {
                    ActiveTabHeaderItems.Remove(tabHeaderItemViewModel);

                    tabHeaderItemViewModel.RequestClose -= this.OnTabHeaderItemRequestClose;
                }

                if (!ActiveTabHeaderItems.Any())
                {
                    SelectedTabHeaderItem = null;

                    _eventAggregator.GetEvent<OnSelectedTabHeaderItemEmptyEvent>().Publish(new());
                }

                RaisePropertyChanged(nameof(ActiveTabHeaderItems));
            }
        }

        public void SetTabHeaderItem(TabInformation tabInformation)
        {
            var activeTabHeaderItem = GetActiveTabHeaderItemByInfo(tabInformation);
            if (activeTabHeaderItem is not null)
            {
                SetActiveTabHeaderItem(activeTabHeaderItem);
            }
            else
            {
                var storeTTabHeaderItem = GetStoreTabHeaderItemViewModel(tabInformation);
                if (storeTTabHeaderItem is not null)
                {
                    AddCore(storeTTabHeaderItem);
                }
            }
        }

        public void RetsetTabItem(TabInformation tabInformation)
        {
            var activeTabHeaderItem = GetActiveTabHeaderItemByInfo(tabInformation);
            if (activeTabHeaderItem is not null)
            {
                SetActiveTabHeaderItem(activeTabHeaderItem);
            }
            else
            {
                var storeTabHeaderItem = GetStoreTabHeaderItemViewModel(tabInformation);
                if (storeTabHeaderItem is not null)
                {
                    ActiveTabHeaderItems.Add(storeTabHeaderItem);
                    storeTabHeaderItem.RequestClose += this.OnTabHeaderItemRequestClose;

                    SetActiveTabHeaderItem(storeTabHeaderItem);
                }
            }
        }

        private TabHeaderItemViewModel GetActiveTabHeaderItemByInfo(TabInformation tabInformation)
        {
            var activeTabHeaderItem = ActiveTabHeaderItems.FirstOrDefault(ti => IsEqualsNameOrTitle(ti.Name, tabInformation.Name) || IsEqualsNameOrTitle(ti.Title, tabInformation.Title));

            return activeTabHeaderItem;
        }

        public TabHeaderItemViewModel GetStoreTabHeaderItemViewModel(TabInformation tabInformation)
        {
            var storeTabHeaderItem = StoreTabHeaderItems.FirstOrDefault(ti => IsEqualsNameOrTitle(ti.Name, tabInformation.Name) || IsEqualsNameOrTitle(ti.Title, tabInformation.Title));

            return storeTabHeaderItem;
        }

        private TabHeaderItemViewModel GetNextActiveTabHeaderItemByInfo(TabInformation tabInformation)
        {
            TabHeaderItemViewModel nextTabHeaderItemViewModel = default;

            var index = ActiveTabHeaderItems.ToList().FindIndex(ti => IsEqualsNameOrTitle(ti.Name, tabInformation.Name) || IsEqualsNameOrTitle(ti.Title, tabInformation.Title));

            if ((index + 1) <= (ActiveTabHeaderItems.Count - 1))
            {
                nextTabHeaderItemViewModel = ActiveTabHeaderItems[index + 1];
            }
            else if ((index - 1) >= 0)
            {
                nextTabHeaderItemViewModel = ActiveTabHeaderItems[index - 1];
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

        public bool IsActiveTabItem(TabInformation tabInformation)
        {
            var isAny = ActiveTabHeaderItems.Any(ti => ti.IsSelected && (IsEqualsNameOrTitle(ti.Name, tabInformation.Name) || IsEqualsNameOrTitle(ti.Title, tabInformation.Title)));

            return isAny;
        }
        #endregion

        #region Contain Methods
        private bool IsExistsActivTabHeaderItems(string name, string title)
        {
            var isAny = ActiveTabHeaderItems.Any(ti => IsEqualsNameOrTitle(ti.Name, name) || IsEqualsNameOrTitle(ti.Title, title));

            return isAny;
        }

        private bool IsExistsStoreTabHeaderItems(string name, string title)
        {
            var isAny = StoreTabHeaderItems.Any(ti => IsEqualsNameOrTitle(ti.Name, name) || IsEqualsNameOrTitle(ti.Title, title));

            return isAny;
        }

        private bool IsEqualsTabHeaderItemViewModel(TabHeaderItemViewModel tabHeaderItemViewModel, TabHeaderItemViewModel otherTabHeaderItemViewModel)
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
