using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Common;
using BeautySalon.Domain.Entities;
using BeautySalon.Domain.Enums;
using FluentValidation;

namespace BeautySalon.Application.Features.Debts;

public sealed class ClientDebtAppService : IClientDebtAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateChargeRequest> _chargeValidator;
    private readonly IValidator<CreatePaymentRequest> _paymentValidator;

    public ClientDebtAppService(
        IUnitOfWork unitOfWork,
        IValidator<CreateChargeRequest> chargeValidator,
        IValidator<CreatePaymentRequest> paymentValidator)
    {
        _unitOfWork = unitOfWork;
        _chargeValidator = chargeValidator;
        _paymentValidator = paymentValidator;
    }

    public async Task<Result<IReadOnlyList<ClientDebtEntryDto>>> GetHistoryAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var entries = await _unitOfWork.ClientDebts.GetByClientAsync(clientId, cancellationToken);
        return Result.Success<IReadOnlyList<ClientDebtEntryDto>>(entries.Select(e => e.ToDto()).ToList());
    }

    public async Task<Result<IReadOnlyList<ClientBalanceDto>>> GetOutstandingBalancesAsync(CancellationToken cancellationToken = default)
    {
        var entries = await _unitOfWork.ClientDebts.GetAllWithClientAsync(cancellationToken);

        var balances = entries
            .GroupBy(e => e.ClientId)
            .Select(g => new ClientBalanceDto(
                g.Key,
                g.First().Client is { } client ? $"{client.Name} {client.LastName}" : string.Empty,
                ComputeBalance(g)))
            .Where(b => b.Balance > 0)
            .OrderByDescending(b => b.Balance)
            .ToList();

        return Result.Success<IReadOnlyList<ClientBalanceDto>>(balances);
    }

    public async Task<Result<ClientDebtEntryDto>> AddChargeAsync(CreateChargeRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _chargeValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<ClientDebtEntryDto>(Error.Validation("Debt.Invalid", validation.ToString(" ")));

        var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
            return Result.Failure<ClientDebtEntryDto>(Error.NotFound("Client.NotFound", "Cliente no encontrado."));

        var entry = new ClientDebtEntry
        {
            ClientId = request.ClientId,
            Type = DebtEntryType.Charge,
            Amount = request.Amount,
            Description = request.Description,
            EntryDate = request.EntryDate,
            ProfessionalId = request.ProfessionalId
        };

        _unitOfWork.ClientDebts.Add(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        entry.Client = client;
        return Result.Success(entry.ToDto());
    }

    public async Task<Result<ClientDebtEntryDto>> AddPaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _paymentValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<ClientDebtEntryDto>(Error.Validation("Debt.Invalid", validation.ToString(" ")));

        var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
            return Result.Failure<ClientDebtEntryDto>(Error.NotFound("Client.NotFound", "Cliente no encontrado."));

        var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(request.PaymentMethodId, cancellationToken);
        if (paymentMethod is null)
            return Result.Failure<ClientDebtEntryDto>(Error.NotFound("PaymentMethod.NotFound", "Método de pago no encontrado."));
        if (!paymentMethod.IsActive)
            return Result.Failure<ClientDebtEntryDto>(Error.Validation("PaymentMethod.Inactive", "El método de pago está desactivado."));

        var existingEntries = await _unitOfWork.ClientDebts.GetByClientAsync(request.ClientId, cancellationToken);
        var currentBalance = ComputeBalance(existingEntries);
        if (request.Amount > currentBalance)
            return Result.Failure<ClientDebtEntryDto>(Error.Validation("Debt.PaymentExceedsBalance", "El abono no puede ser mayor a la deuda pendiente."));

        var entry = new ClientDebtEntry
        {
            ClientId = request.ClientId,
            Type = DebtEntryType.Payment,
            Amount = request.Amount,
            EntryDate = request.EntryDate,
            PaymentMethodId = request.PaymentMethodId,
            ProfessionalId = request.ProfessionalId
        };

        _unitOfWork.ClientDebts.Add(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        entry.Client = client;
        entry.PaymentMethod = paymentMethod;
        return Result.Success(entry.ToDto());
    }

    public async Task<Result> DeleteEntryAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        var entry = await _unitOfWork.ClientDebts.GetByIdAsync(entryId, cancellationToken);
        if (entry is null)
            return Result.Failure(Error.NotFound("Debt.NotFound", "Movimiento no encontrado."));

        _unitOfWork.ClientDebts.Remove(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static decimal ComputeBalance(IEnumerable<ClientDebtEntry> entries) =>
        entries.Where(e => e.Type == DebtEntryType.Charge).Sum(e => e.Amount) -
        entries.Where(e => e.Type == DebtEntryType.Payment).Sum(e => e.Amount);
}
