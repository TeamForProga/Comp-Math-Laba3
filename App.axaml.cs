using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using laba3.ViewModels;
using laba3.Views;

using laba3.Services;

namespace laba3;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            MainWindowViewModel MainWindowVM = new();

            desktop.MainWindow = new MainWindow
            {
                DataContext = MainWindowVM,
            };

            MainWindowVM.WindowService = new WindowService(desktop.MainWindow);
            desktop.ShutdownRequested += DesktopOnShutdownRequested;
        
            await MainWindowVM.LoadAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private bool _canClose;

    private async void DesktopOnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        if (_canClose) return;
        e.Cancel = true;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop?.MainWindow?.DataContext is MainWindowViewModel mainWVN)
            {
                var state = mainWVN.BuildState();
                await DataStorageService.SaveAsync(state);
            }

            _canClose = true;
            desktop?.Shutdown();
        }
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}