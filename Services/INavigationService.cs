namespace Beauty_Salon.Services;

// Thin wrapper over Shell.Current.GoToAsync so ViewModels never touch the static
// Shell directly - keeps them testable and decoupled from the navigation host.
public interface INavigationService
{
    Task GoToAsync(string route);
    Task GoToAsync(string route, IDictionary<string, object> parameters);
    Task GoBackAsync();
}
