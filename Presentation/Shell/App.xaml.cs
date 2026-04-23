using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Prism.Events;

using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;

using Aksl.Modules.Home;
using Aksl.Modules.Shell;
using Aksl.Modules.Shell.ViewModels;
using Aksl.Modules.Shell.Views;

using Aksl.Modules.HamburgerMenuSideBarTab;
using Aksl.Modules.HamburgerMenuNavigationSideBarTab;
using Aksl.Modules.HamburgerMenuTreeSideBarTab;

using Aksl.Modules.Account;

using Aksl.Modules.Pipeline;
using Aksl.Modules.Thermometer;
using Aksl.Modules.CoolingTower;
using Aksl.Modules.AirCompresser;
using Aksl.Modules.Others;

namespace Aksl.Wpf.Unity
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.Register(typeof(ShellView).ToString(), () => Container.Resolve<ShellViewModel>());
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            #region Initialize
            var services = new ServiceCollection();
            services.AddOptions();

            string basePath = Directory.GetCurrentDirectory();
            string configPath = Path.Combine(basePath, "Configuration");
            string appSettingsPath = Path.Combine(configPath, "appsettings.json");
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().SetBasePath(basePath)
                                                                                   .AddJsonFile(path: appSettingsPath, optional: true, reloadOnChange: false);

            var configuration = configurationBuilder.Build();
            #endregion

            #region Logging
            services.AddLogging(builder =>
            {
                var loggingSection = configuration.GetSection("Logging");
                var includeScopes = loggingSection.GetValue<bool>("IncludeScopes");

                builder.AddConfiguration(loggingSection);

                //加入一个ConsoleLoggerProvider
                //builder.AddConsole(consoleLoggerOptions =>
                //{
                //    consoleLoggerOptions.IncludeScopes = includeScopes;
                //});

                //加入一个DebugLoggerProvider
                builder.AddDebug();
            });
            #endregion

            var serviceProvider = services.BuildServiceProvider();

            containerRegistry.RegisterInstance<IServiceProvider>(serviceProvider);

            containerRegistry.RegisterDialogWindow<Dialogs.Views.FixedSizeDialogWindow>(name: nameof(Dialogs.Views.FixedSizeDialogWindow));
            containerRegistry.RegisterDialog<Dialogs.Views.ConfirmView, Dialogs.ViewModels.ConfirmViewModel>();

            containerRegistry.RegisterSingleton(typeof(Dialogs.Services.IDialogViewService), typeof(Dialogs.Services.DialogViewService));

            RegisterMenuFactoryAsync(containerRegistry).Await();

            RegisterBuildWorkspaceViewEventAsync().Await();
        }

        protected async Task RegisterMenuFactoryAsync(IContainerRegistry containerRegistry)
        {
            try
            {
                MenuService menuService = new(new List<string> {"pack://application:,,,/Aksl.Wpf.Shell;Component/Data/AllMenus.xml",
                                                                "pack://application:,,,/Aksl.Wpf.Shell;Component/Data/Industry.xml",
                                                                "pack://application:,,,/Aksl.Wpf.Shell;Component/Data/Pipelines.xml",
                                                                "pack://application:,,,/Aksl.Wpf.Shell;Component/Data/Thermometers.xml",
                                                                "pack://application:,,,/Aksl.Wpf.Shell;Component/Data/CoolingTowers.xml",
                                                                "pack://application:,,,/Aksl.Wpf.Shell;Component/Data/AirCompressers.xml",
                                                                "pack://application:,,,/Aksl.Wpf.Shell;Component/Data/Others.xml"
                                                                });

                await menuService.CreateMenusAsync();

                containerRegistry.RegisterInstance<IMenuService>(menuService);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        protected Task RegisterBuildWorkspaceViewEventAsync()
        {
            try
            {
                var eventAggregator = Container.Resolve<IEventAggregator>();

                _ = eventAggregator.GetEvent<OnBuildHamburgerMenuSideBarTabWorkspaceViewEvent>();
                _ = eventAggregator.GetEvent<OnBuildHamburgerMenuNavigationSideBarTabWorkspaceViewEvent>();
                _ = eventAggregator.GetEvent<OnBuildHamburgerMenuTreeSideBarTabWorkspaceViewEvent>();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }

            return Task.CompletedTask;
        }
  

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            _ = moduleCatalog.AddModule(nameof(HomeModule), typeof(HomeModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);

            _ = moduleCatalog.AddModule(nameof(AccountModule), typeof(AccountModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);

            _ = moduleCatalog.AddModule(nameof(PipelineModule), typeof(PipelineModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            _ = moduleCatalog.AddModule(nameof(ThermometerModule), typeof(ThermometerModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            _ = moduleCatalog.AddModule(nameof(CoolingTowerModule), typeof(CoolingTowerModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            _ = moduleCatalog.AddModule(nameof(AirCompresserModule), typeof(AirCompresserModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);

            _ = moduleCatalog.AddModule(nameof(OthersModule), typeof(OthersModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);

            _ = moduleCatalog.AddModule(typeof(HamburgerMenuSideBarTabModule).Name, typeof(HamburgerMenuSideBarTabModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            _ = moduleCatalog.AddModule(typeof(HamburgerMenuNavigationSideBarTabModule).Name, typeof(HamburgerMenuNavigationSideBarTabModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            _ = moduleCatalog.AddModule(typeof(HamburgerMenuTreeSideBarTabModule).Name, typeof(HamburgerMenuTreeSideBarTabModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);

            _ = moduleCatalog.AddModule(typeof(ShellModule).Name, typeof(ShellModule).AssemblyQualifiedName, InitializationMode.WhenAvailable, typeof(HamburgerMenuSideBarTabModule).Name);
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<ShellView>();
        }

        protected override  void InitializeShell(Window shell)
        {
            base.InitializeShell(shell);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
    }
}
