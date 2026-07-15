namespace BeautySalon.Application.Features.Settings;

public sealed record UpdateWorkingHoursRequest(IReadOnlyList<WorkingHoursInput> Days);
