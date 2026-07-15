using Beauty_Salon.Pages;

namespace Beauty_Salon
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Not tabs - navigated to on demand, so registered as routes rather than ShellContent.
            Routing.RegisterRoute("appointment/new", typeof(AppointmentFormPage));
            Routing.RegisterRoute("appointment/reschedule", typeof(ReschedulePage));
            Routing.RegisterRoute("appointment/finish", typeof(FinishAppointmentPage));
            Routing.RegisterRoute("clients/detail", typeof(ClientDetailPage));
            Routing.RegisterRoute("clients/form", typeof(ClientFormPage));
            Routing.RegisterRoute("catalog/category-form", typeof(CategoryFormPage));
            Routing.RegisterRoute("catalog/service-form", typeof(ServiceFormPage));
            Routing.RegisterRoute("paymentmethods/form", typeof(PaymentMethodFormPage));
        }
    }
}
