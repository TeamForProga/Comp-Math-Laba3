using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.ApplicationLifetimes;

namespace laba3.Services;

public interface ISupportWindowService {
    IWindowService? WindowService { get; set; }
}

public interface IWindowService
{
    WindowNotificationManager? NotificationManager { get; }
    string? WindowTitle { get; }
    Task<bool?> ShowDialogAsync<TWindow>(ISupportWindowService viewModel, string? _windowTitle = "Window") where TWindow : Window, new();
    void Close(bool? result = null);
}

public class WindowService : IWindowService
{
    private Window? currentWindow;

    public WindowNotificationManager? NotificationManager { get; private set; }

    public string? WindowTitle { get; private set; }

    public WindowService(Window? _currentWindow, string? _windowTitle = "Window") {
        currentWindow = _currentWindow;
        WindowTitle = _windowTitle;
        NotificationManager = new WindowNotificationManager(currentWindow);
    }

    public async Task<bool?> ShowDialogAsync<TWindow>(ISupportWindowService viewModel, string? _windowTitle = "Window") where TWindow : Window, new() {        
        if(currentWindow == null) return null;

        TWindow dialogWindow = new();

        var dialogWinService = new WindowService(dialogWindow, _windowTitle);

        viewModel.WindowService = dialogWinService;
        dialogWindow.DataContext = viewModel;

        return await dialogWindow.ShowDialog<bool?>(currentWindow);
    }

    public void Close(bool? result = null)
    {
        currentWindow?.Close(result);
        currentWindow = null;
        NotificationManager = null;
    }
}