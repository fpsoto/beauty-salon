using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Common;
using BeautySalon.Domain.Entities;
using FluentValidation;

namespace BeautySalon.Application.Features.Payments;

public sealed class PaymentMethodAppService : IPaymentMethodAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreatePaymentMethodRequest> _createValidator;
    private readonly IValidator<UpdatePaymentMethodRequest> _updateValidator;

    public PaymentMethodAppService(
        IUnitOfWork unitOfWork,
        IValidator<CreatePaymentMethodRequest> createValidator,
        IValidator<UpdatePaymentMethodRequest> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<IReadOnlyList<PaymentMethodDto>>> GetAllAsync(bool onlyActive, CancellationToken cancellationToken = default)
    {
        var methods = onlyActive
            ? await _unitOfWork.PaymentMethods.GetActiveAsync(cancellationToken)
            : await _unitOfWork.PaymentMethods.GetAllAsync(cancellationToken);

        return Result.Success<IReadOnlyList<PaymentMethodDto>>(methods.Select(m => m.ToDto()).ToList());
    }

    public async Task<Result<PaymentMethodDto>> CreateAsync(CreatePaymentMethodRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<PaymentMethodDto>(Error.Validation("PaymentMethod.Invalid", validation.ToString(" ")));

        var method = new PaymentMethod { Name = request.Name, SortOrder = request.SortOrder, IsActive = true };
        _unitOfWork.PaymentMethods.Add(method);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(method.ToDto());
    }

    public async Task<Result<PaymentMethodDto>> UpdateAsync(Guid paymentMethodId, UpdatePaymentMethodRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<PaymentMethodDto>(Error.Validation("PaymentMethod.Invalid", validation.ToString(" ")));

        var method = await _unitOfWork.PaymentMethods.GetByIdAsync(paymentMethodId, cancellationToken);
        if (method is null)
            return Result.Failure<PaymentMethodDto>(Error.NotFound("PaymentMethod.NotFound", "Método de pago no encontrado."));

        method.Name = request.Name;
        method.SortOrder = request.SortOrder;
        method.IsActive = request.IsActive;

        _unitOfWork.PaymentMethods.Update(method);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(method.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid paymentMethodId, CancellationToken cancellationToken = default)
    {
        var method = await _unitOfWork.PaymentMethods.GetByIdAsync(paymentMethodId, cancellationToken);
        if (method is null)
            return Result.Failure(Error.NotFound("PaymentMethod.NotFound", "Método de pago no encontrado."));

        if (await _unitOfWork.PaymentMethods.HasAppointmentHistoryAsync(paymentMethodId, cancellationToken))
            return Result.Failure(Error.Conflict("PaymentMethod.HasHistory", "No se puede eliminar: el método de pago tiene historial de cobros. Desactívelo en su lugar."));

        _unitOfWork.PaymentMethods.Remove(method);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
