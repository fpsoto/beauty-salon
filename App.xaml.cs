using Beauty_Salon.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace Beauty_Salon
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Starts on the login page (no Shell chrome); LoginPage swaps the window's
            // root to a DI-resolved AppShell once sign-in succeeds.
            var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
            return new Window(loginPage);
        }
    }
}
