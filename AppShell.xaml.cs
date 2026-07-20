using Beauty_Salon.Pages;

namespace Beauty_Salon
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Not tabs - navigated to on demand, so registered as routes rather than ShellContent.
            // Route names deliberately avoid sharing a first path segment with any TabBar route
            // (agenda/clients/catalog/paymentmethods/reports/settings) - MAUI Shell's relative-route
            // resolver misidentifies a route like "clients/form" as a shell-hierarchy path because
            // "clients" also names a real tab, throwing "Relative routing to shell elements is
            // currently not supported" at navigation time. Absolute ("//"/"///") routing doesn't
            // work as a fix either - Routing.RegisterRoute pages only support relative navigation
            // (see the "Absolute routes"/"Relative routes" notes in Microsoft's Shell navigation docs).
            Routing.RegisterRoute("appointment/new", typeof(AppointmentFormPage));
            Routing.RegisterRoute("appointment/reschedule", typeof(ReschedulePage));
            Routing.RegisterRoute("appointment/finish", typeof(FinishAppointmentPage));
            Routing.RegisterRoute("client-detail", typeof(ClientDetailPage));
            Routing.RegisterRoute("client-form", typeof(ClientFormPage));
            Routing.RegisterRoute("category-form", typeof(CategoryFormPage));
            Routing.RegisterRoute("service-form", typeof(ServiceFormPage));
            Routing.RegisterRoute("paymentmethod-form", typeof(PaymentMethodFormPage));
            Routing.RegisterRoute("help", typeof(HelpPage));
        }
    }
}
