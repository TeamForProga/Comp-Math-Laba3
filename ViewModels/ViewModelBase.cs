using CommunityToolkit.Mvvm.ComponentModel;
using laba3.Services;


namespace laba3.ViewModels;

public abstract class ViewModelBase : ObservableObject, ISupportWindowService
{
    public IWindowService? WindowService { get; set; }
}
