using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    public class TabHubViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        #endregion

        #region Constructors
        public TabHubViewModel()
        {
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();

            TabHeaderViewModel = new();
            TabContentViewModel = new();

            RegisterPropertyChanged();
        }
        #endregion

        #region Properties
        public TabHeaderViewModel TabHeaderViewModel { get; set; }

        public TabContentViewModel TabContentViewModel { get; set; }
        #endregion

        #region RegisterPropertyChanged Method
        private void RegisterPropertyChanged()
        {
            TabHeaderViewModel.PropertyChanged += (sender, e) =>
            {
                if (sender is TabHeaderViewModel thvm)
                {
                    if (e.PropertyName == nameof(TabHeaderViewModel.SelectedTabHeaderItem))
                    {
                        if (thvm.SelectedTabHeaderItem is not null &&  thvm.SelectedTabHeaderItem.IsSelected)
                        {
                            TabContentViewModel.SetTabItemOnSelected(thvm.SelectedTabHeaderItem.TabInformation);
                        }
                    }
                }
            };
        }
        #endregion

        #region Methods
        public void Add(TabInformation tabInformation)
        {
            TabHeaderViewModel.Add(tabInformation);

            TabHeaderViewModel.RequestClose += (sender, e) =>
            {
                if (sender is TabHeaderItemViewModel tabHeaderItemViewModel)
                {
                    TabContentViewModel.SetTabItemOnClose(tabHeaderItemViewModel.TabInformation);
                }
            };

            TabContentViewModel.Add(tabInformation);
        }

        public System.Windows.DependencyObject GetViewElementByType(Type viewType)
        {
            var viewElement = TabContentViewModel.GetViewElementByType(viewType);

            return viewElement;
        }

        public void SetTabItem(TabInformation tabInformation)
        {
            TabHeaderViewModel.SetTabHeaderItem(tabInformation);

            TabContentViewModel.SetTabContentItem(tabInformation);
        }

        public void RetsetTabItem(TabInformation tabInformation)
        {
            TabHeaderViewModel.RetsetTabItem(tabInformation);

            TabContentViewModel.RetsetTabItem(tabInformation);
        }

        public bool IsActiveTabItem(TabInformation tabInformation)
        {
            var isExists = TabHeaderViewModel.IsActiveTabItem(tabInformation);

            return isExists;
        }
        #endregion

        #region Contain Methods
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
