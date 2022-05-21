namespace TuringSmartScreenTool.ViewModels.Pages
{
    public record NavigationContext();

    public interface INavigationAware
    {
        void OnNavigatedFrom(NavigationContext navigationContext);
        void OnNavigatedTo(NavigationContext navigationContext);
    }
}
