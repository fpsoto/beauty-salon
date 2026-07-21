using Beauty_Salon.Pages;
using Beauty_Salon.Services;
using BeautySalon.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Beauty_Salon
{
    public partial class App : Application
    {
        // How long a signed-in session survives across app restarts before requiring
        // login again - a deliberate UX choice for a single-owner device, not a security
        // boundary (there's no sensitive multi-tenant data at stake).
        private static readonly TimeSpan MaxSessionAge = TimeSpan.FromDays(30);

        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var persistedSession = _serviceProvider.GetRequiredService<IPersistedSessionStore>().TryRestore(MaxSessionAge);
            if (persistedSession is { } session)
            {
                _serviceProvider.GetRequiredService<ISessionService>().SignIn(session.UserId, session.Username);
                var shell = _serviceProvider.GetRequiredService<AppShell>();
                return new Window(shell);
            }

            // Starts on the login page (no Shell chrome); LoginPage swaps the window's
            // root to a DI-resolved AppShell once sign-in succeeds.
            var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
            return new Window(loginPage);
        }
    }
}
