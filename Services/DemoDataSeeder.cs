using BeautySalon.Application.Common;
using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Application.Features.Catalog;
using BeautySalon.Application.Features.Clients;
using BeautySalon.Application.Features.Payments;
using BeautySalon.Application.Features.Products;
using BeautySalon.Application.Features.Sales;
using BeautySalon.Application.Features.Schedule;

namespace Beauty_Salon.Services;

public sealed class DemoDataSeeder : IDemoDataSeeder
{
    private static readonly (string Name, string LastName, string Comuna)[] ClientSeeds =
    [
        ("Javiera", "Muñoz Rojas", "Providencia"),
        ("Camila", "Fernández Soto", "Ñuñoa"),
        ("Valentina", "Castro Díaz", "Las Condes"),
        ("Fernanda", "Silva Contreras", "La Reina"),
        ("Antonia", "Reyes Morales", "Macul"),
        ("Catalina", "Vargas Pinto", "Providencia"),
        ("Constanza", "Espinoza Lagos", "Vitacura"),
        ("Josefina", "Herrera Bravo", "San Miguel"),
        ("Martina", "Gómez Fuentes", "La Florida"),
        ("Isidora", "Torres Navarro", "Ñuñoa"),
        ("Trinidad", "Salazar Mendoza", "Peñalolén"),
        ("Florencia", "Riquelme Aguilar", "Providencia"),
        ("Rocío", "Bravo Sepúlveda", "Maipú"),
        ("Millaray", "Contreras Ortiz", "Ñuñoa"),
        ("Belén", "Sánchez Toro", "San Bernardo")
    ];

    private static readonly string[] CancellationReasons =
    [
        "Cliente reagendó",
        "Cliente canceló por enfermedad",
        "Cliente no pudo asistir",
        "Imprevisto de última hora",
        "Cliente pidió cambiar de día"
    ];

    private static readonly string[] AppointmentNotes =
    [
        "Primera vez en el salón",
        "Pidió el mismo resultado que la sesión anterior",
        "Piel sensible, usar productos suaves",
        "Cliente frecuente, muy puntual",
        "Vino recomendada por una amiga",
        "Prefiere que la atiendan en la tarde"
    ];

    // The real Resplandecer service catalog (mirrors BeautySalon.Persistence/Seed/DatabaseSeeder.cs's
    // SeedCatalogAsync exactly) - demo appointments must only ever use these, never whatever
    // placeholder/leftover services might already exist on a device whose database predates
    // that real catalog (DatabaseSeeder only seeds the catalog once, on a first-ever run).
    private static readonly (string CategoryName, string ColorHex, (string ServiceName, int? DurationMinutes)[] Services)[] RealCatalog =
    [
        ("Estética", "#8E44AD",
        [
            ("Pestañas: Lifting de pestañas (Protocolo Coreano)", null),
            ("Pestañas: Lifting de pestañas Tradicional", null),
            ("Pestañas: Tratamiento Nutritivo para Pestañas", null),
            ("Cejas: Brow Lamination + Perfilado", null),
            ("Cejas: Perfilado + Henna", null),
            ("Cejas: Brow Lamination + Perfilado + Henna", null),
            ("Faciales: Spa Facial", null),
            ("Faciales: Limpieza Facial Básica", null),
            ("Faciales: Limpieza Facial Profunda", null),
            ("Espalda: Spa y Limpieza de Espalda", null),
            ("Espalda: Spa y Limpieza Profunda de Espalda", null)
        ]),
        ("Depilación con Cera", "#E91E63",
        [
            ("Rostro: Cejas", null),
            ("Rostro: Bozo", null),
            ("Rostro: Mentón", null),
            ("Rostro: Rostro Completo", null),
            ("Cuerpo: Axilas", null),
            ("Cuerpo: Brazos Completos", null),
            ("Cuerpo: Media Pierna", null),
            ("Cuerpo: Piernas Completas", null),
            ("Zona Íntima: Rebaje Corto (Simple)", null),
            ("Zona Íntima: Rebaje Completo", null)
        ]),
        ("Depilación Semidefinitiva", "#F39C12",
        [
            ("Zona Cuerpo", null),
            ("Zona Íntima", null)
        ]),
        ("Masajes", "#16A085",
        [
            ("Masaje Mixto: Express", 40),
            ("Masaje Mixto: Normal", 60),
            ("Masaje Mixto: Premium", 80)
        ])
    ];

    private const decimal PlaceholderServicePrice = 15_000m;
    private const int DefaultServiceDurationMinutes = 60;

    private static readonly (string Name, string? Description, decimal Price)[] ProductSeeds =
    [
        ("Shampoo Reparador", "Para cabello dañado o tratado químicamente", 12000m),
        ("Acondicionador Nutritivo", "Hidratación profunda post-lavado", 11000m),
        ("Ampolla Capilar x1", "Tratamiento intensivo de reparación", 3500m),
        ("Sérum Facial Hidratante", "Ácido hialurónico, uso diario", 15000m),
        ("Crema Corporal Hidratante", "Textura ligera, absorción rápida", 9000m),
        ("Aceite de Argán", "Multiuso: puntas, cutículas y piel", 14000m),
        ("Guatero de Semillas", "Calor terapéutico, apto microondas", 9500m),
        ("Parches Detox", "Caja de 10 unidades, aplicar en la planta de los pies", 7000m),
        ("Té Detox", "Mezcla de hierbas, caja de 20 sobres", 6500m)
    ];

    private static readonly TimeOnly[] DaySlots =
    [
        new TimeOnly(9, 30),
        new TimeOnly(11, 0),
        new TimeOnly(13, 0),
        new TimeOnly(15, 0),
        new TimeOnly(16, 30)
    ];

    private readonly IClientAppService _clientAppService;
    private readonly ICatalogAppService _catalogAppService;
    private readonly IPaymentMethodAppService _paymentMethodAppService;
    private readonly IProductAppService _productAppService;
    private readonly IProductSaleAppService _productSaleAppService;
    private readonly IAppointmentAppService _appointmentAppService;
    private readonly ICurrentUserContext _currentUserContext;

    private Guid ProfessionalId => _currentUserContext.UserId ?? WellKnownIds.AdminUserId;

    public DemoDataSeeder(
        IClientAppService clientAppService,
        ICatalogAppService catalogAppService,
        IPaymentMethodAppService paymentMethodAppService,
        IProductAppService productAppService,
        IProductSaleAppService productSaleAppService,
        IAppointmentAppService appointmentAppService,
        ICurrentUserContext currentUserContext)
    {
        _clientAppService = clientAppService;
        _catalogAppService = catalogAppService;
        _paymentMethodAppService = paymentMethodAppService;
        _productAppService = productAppService;
        _productSaleAppService = productSaleAppService;
        _appointmentAppService = appointmentAppService;
        _currentUserContext = currentUserContext;
    }

    public async Task<string> SeedAsync()
    {
        var existingClients = await _clientAppService.SearchAsync(string.Empty);
        if (existingClients.IsSuccess && existingClients.Value.Count > 0)
            return "Ya existen clientes en la base de datos - no se sembraron datos de demo para evitar duplicados.";

        var random = new Random();

        var clients = await SeedClientsAsync(random);
        var products = await SeedProductsAsync();
        var services = await EnsureRealCatalogAsync();

        var paymentMethodsResult = await _paymentMethodAppService.GetAllAsync(true);
        var paymentMethods = paymentMethodsResult.IsSuccess ? paymentMethodsResult.Value : [];

        var (appointmentsCreated, appointmentsSkipped) = await SeedAppointmentsAsync(random, clients, services, paymentMethods);
        var (salesCreated, salesSkipped) = await SeedProductSalesAsync(random, clients, products, paymentMethods);

        return "Datos de demo creados:\n" +
               $"- {clients.Count} clientes\n" +
               $"- {products.Count} productos\n" +
               $"- {appointmentsCreated} citas ({appointmentsSkipped} omitidas)\n" +
               $"- {salesCreated} ventas de productos ({salesSkipped} omitidas)";
    }

    private async Task<List<ClientDto>> SeedClientsAsync(Random random)
    {
        var created = new List<ClientDto>();

        for (var i = 0; i < ClientSeeds.Length; i++)
        {
            var (name, lastName, comuna) = ClientSeeds[i];
            var rut = GenerateRut(12_300_000 + i * 137);
            var phone = $"+569{random.Next(10_000_000, 99_999_999)}";
            var hasEmail = random.Next(2) == 0;
            var email = hasEmail ? $"{name.ToLowerInvariant()}.{lastName.Split(' ')[0].ToLowerInvariant()}@gmail.com" : null;
            var hasBirthDate = random.Next(2) == 0;
            var birthDate = hasBirthDate
                ? DateOnly.FromDateTime(DateTime.Today.AddYears(-random.Next(20, 46)).AddDays(-random.Next(365)))
                : (DateOnly?)null;

            var result = await _clientAppService.CreateAsync(new CreateClientRequest(
                name, lastName, rut, phone, email, birthDate, comuna, null));

            if (result.IsSuccess)
                created.Add(result.Value);
        }

        // A handful of favorites, for a screen that doesn't look empty - a random subset each
        // time, not always the same first few clients.
        foreach (var client in created.OrderBy(_ => random.Next()).Take(random.Next(2, 5)))
            await _clientAppService.SetFavoriteAsync(client.Id, true);

        return created;
    }

    private async Task<List<ProductDto>> SeedProductsAsync()
    {
        var created = new List<ProductDto>();

        foreach (var (name, description, price) in ProductSeeds)
        {
            var result = await _productAppService.CreateAsync(new CreateProductRequest(name, description, price));
            if (result.IsSuccess)
                created.Add(result.Value);
        }

        return created;
    }

    private async Task<List<SalonServiceDto>> EnsureRealCatalogAsync()
    {
        var categoriesResult = await _catalogAppService.GetCategoriesAsync(false);
        var existingCategories = (categoriesResult.IsSuccess ? categoriesResult.Value : []).ToList();

        var servicesResult = await _catalogAppService.GetServicesAsync(null, false);
        var existingServices = (servicesResult.IsSuccess ? servicesResult.Value : []).ToList();

        var realServices = new List<SalonServiceDto>();

        foreach (var (categoryName, colorHex, catalogServices) in RealCatalog)
        {
            var category = existingCategories.FirstOrDefault(c => string.Equals(c.Name, categoryName, StringComparison.OrdinalIgnoreCase));
            if (category is null)
            {
                var createCategoryResult = await _catalogAppService.CreateCategoryAsync(new CreateServiceCategoryRequest(categoryName, colorHex));
                if (createCategoryResult.IsFailure)
                    continue;

                category = createCategoryResult.Value;
                existingCategories.Add(category);
            }

            foreach (var (serviceName, durationMinutes) in catalogServices)
            {
                var existingService = existingServices.FirstOrDefault(s => string.Equals(s.Name, serviceName, StringComparison.OrdinalIgnoreCase));
                if (existingService is not null)
                {
                    realServices.Add(existingService);
                    continue;
                }

                var createServiceResult = await _catalogAppService.CreateServiceAsync(new CreateSalonServiceRequest(
                    serviceName, category.Id, PlaceholderServicePrice, durationMinutes ?? DefaultServiceDurationMinutes, null, null));

                if (createServiceResult.IsSuccess)
                {
                    existingServices.Add(createServiceResult.Value);
                    realServices.Add(createServiceResult.Value);
                }
            }
        }

        return realServices;
    }

    private async Task<(int Created, int Skipped)> SeedAppointmentsAsync(
        Random random, List<ClientDto> clients, IReadOnlyList<SalonServiceDto> services, IReadOnlyList<PaymentMethodDto> paymentMethods)
    {
        if (clients.Count == 0 || services.Count == 0 || paymentMethods.Count == 0)
            return (0, 0);

        var created = 0;
        var skipped = 0;
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Roughly two weeks: 9 days back (completed/cancelled/no-show history) through
        // 4 days ahead (upcoming, still Booked) - skips weekends since the seeded working
        // hours only cover Mon-Fri.
        for (var offset = -9; offset <= 4; offset++)
        {
            var date = today.AddDays(offset);
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;

            var slotsForDay = random.Next(2, DaySlots.Length + 1);

            for (var slot = 0; slot < slotsForDay; slot++)
            {
                var client = clients[random.Next(clients.Count)];
                var service = services[random.Next(services.Count)];

                // A little jitter so times don't all land robotically on the exact slot -
                // still safely spaced given the slots themselves are 90+ minutes apart.
                var startTime = DaySlots[slot].AddMinutes(random.Next(0, 4) * 5);
                var appointmentNotes = random.Next(5) == 0 ? AppointmentNotes[random.Next(AppointmentNotes.Length)] : null;

                var createResult = await _appointmentAppService.CreateAsync(new CreateAppointmentRequest(
                    client.Id, ProfessionalId, date, startTime, [service.Id], appointmentNotes));

                if (createResult.IsFailure)
                {
                    skipped++;
                    continue;
                }

                created++;
                var appointmentId = createResult.Value.Id;

                if (date < today)
                {
                    var roll = random.Next(100);
                    if (roll < 75)
                    {
                        var paymentMethod = paymentMethods[random.Next(paymentMethods.Count)];
                        var tip = random.Next(3) == 0 ? Math.Round(service.SuggestedPrice * 0.1m, 0) : (decimal?)null;
                        var finishNotes = random.Next(4) == 0 ? AppointmentNotes[random.Next(AppointmentNotes.Length)] : null;
                        await _appointmentAppService.FinishAsync(new FinishAppointmentRequest(
                            appointmentId, service.SuggestedPrice, null, tip, paymentMethod.Id, finishNotes));
                    }
                    else if (roll < 88)
                    {
                        var reason = CancellationReasons[random.Next(CancellationReasons.Length)];
                        await _appointmentAppService.CancelAsync(appointmentId, reason);
                    }
                    else
                    {
                        await _appointmentAppService.MarkNoShowAsync(appointmentId);
                    }
                }
                else if (date == today && random.Next(2) == 0)
                {
                    await _appointmentAppService.ConfirmAsync(appointmentId);
                }
            }
        }

        return (created, skipped);
    }

    private async Task<(int Created, int Skipped)> SeedProductSalesAsync(
        Random random, List<ClientDto> clients, List<ProductDto> products, IReadOnlyList<PaymentMethodDto> paymentMethods)
    {
        if (products.Count == 0 || paymentMethods.Count == 0)
            return (0, 0);

        var created = 0;
        var skipped = 0;
        var today = DateOnly.FromDateTime(DateTime.Today);

        for (var i = 0; i < 10; i++)
        {
            var product = products[random.Next(products.Count)];
            var paymentMethod = paymentMethods[random.Next(paymentMethods.Count)];
            var quantity = random.Next(1, 4);
            var isWalkIn = clients.Count == 0 || random.Next(2) == 0;
            var clientId = isWalkIn ? (Guid?)null : clients[random.Next(clients.Count)].Id;
            var saleDate = today.AddDays(-random.Next(0, 10));

            var result = await _productSaleAppService.CreateAsync(new CreateProductSaleRequest(
                product.Id, quantity, product.SalePrice, clientId, paymentMethod.Id, saleDate, ProfessionalId));

            if (result.IsSuccess)
                created++;
            else
                skipped++;
        }

        return (created, skipped);
    }

    // Mirrors BeautySalon.Domain.ValueObjects.Rut's private check-digit algorithm (modulo 11) -
    // duplicated here only because it's private to that type; not worth exposing a public
    // "generate a valid RUT" API on the domain type just for this temporary demo seeder.
    private static string GenerateRut(int body)
    {
        var bodyStr = body.ToString();
        var sum = 0;
        var factor = 2;

        for (var i = bodyStr.Length - 1; i >= 0; i--)
        {
            sum += (bodyStr[i] - '0') * factor;
            factor = factor == 7 ? 2 : factor + 1;
        }

        var remainder = 11 - (sum % 11);
        var checkDigit = remainder switch
        {
            11 => "0",
            10 => "K",
            _ => remainder.ToString()
        };

        return $"{bodyStr}-{checkDigit}";
    }
}
