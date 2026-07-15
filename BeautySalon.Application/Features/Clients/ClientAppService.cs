using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Common;
using BeautySalon.Domain.Entities;
using BeautySalon.Domain.Enums;
using BeautySalon.Domain.ValueObjects;
using FluentValidation;

namespace BeautySalon.Application.Features.Clients;

public sealed class ClientAppService : IClientAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateClientRequest> _createValidator;
    private readonly IValidator<UpdateClientRequest> _updateValidator;

    public ClientAppService(
        IUnitOfWork unitOfWork,
        IValidator<CreateClientRequest> createValidator,
        IValidator<UpdateClientRequest> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<IReadOnlyList<ClientDto>>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var clients = await _unitOfWork.Clients.SearchAsync(searchTerm, cancellationToken);
        return Result.Success<IReadOnlyList<ClientDto>>(clients.Select(c => c.ToDto()).ToList());
    }

    public async Task<Result<IReadOnlyList<ClientDto>>> GetFavoritesAsync(CancellationToken cancellationToken = default)
    {
        var clients = await _unitOfWork.Clients.GetFavoritesAsync(cancellationToken);
        return Result.Success<IReadOnlyList<ClientDto>>(clients.Select(c => c.ToDto()).ToList());
    }

    public async Task<Result<ClientDetailDto>> GetDetailAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return Result.Failure<ClientDetailDto>(Error.NotFound("Client.NotFound", "Cliente no encontrado."));

        var appointments = await _unitOfWork.Appointments.GetByClientAsync(clientId, cancellationToken);
        var finished = appointments.Where(a => a.Status == AppointmentStatus.Completed).ToList();
        var today = DateOnly.FromDateTime(DateTime.Now);

        var nextAppointment = appointments
            .Where(a => a.Date >= today && (a.Status == AppointmentStatus.Booked || a.Status == AppointmentStatus.Confirmed))
            .OrderBy(a => a.Date).ThenBy(a => a.StartTime)
            .Select(a => (DateOnly?)a.Date)
            .FirstOrDefault();

        var lastVisit = finished
            .OrderByDescending(a => a.Date)
            .Select(a => (DateOnly?)a.Date)
            .FirstOrDefault();

        var detail = new ClientDetailDto(
            client.ToDto(),
            finished.Count,
            finished.Sum(a => a.ChargedPrice ?? 0m),
            lastVisit,
            nextAppointment,
            appointments.OrderByDescending(a => a.Date).ThenByDescending(a => a.StartTime).Select(a => a.ToSummaryDto()).ToList());

        return Result.Success(detail);
    }

    public async Task<Result<ClientDto>> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<ClientDto>(Error.Validation("Client.Invalid", validation.ToString(" ")));

        var rut = Rut.Create(request.Rut);

        var existing = await _unitOfWork.Clients.GetByRutAsync(rut.Value, cancellationToken);
        if (existing is not null)
            return Result.Failure<ClientDto>(Error.Conflict("Client.DuplicateRut", "Ya existe un cliente con ese RUT."));

        var client = new Client
        {
            Name = request.Name,
            LastName = request.LastName,
            Rut = rut,
            Phone = request.Phone,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address,
            Notes = request.Notes,
            IsActive = true
        };

        _unitOfWork.Clients.Add(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(client.ToDto());
    }

    public async Task<Result<ClientDto>> UpdateAsync(Guid clientId, UpdateClientRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<ClientDto>(Error.Validation("Client.Invalid", validation.ToString(" ")));

        var client = await _unitOfWork.Clients.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return Result.Failure<ClientDto>(Error.NotFound("Client.NotFound", "Cliente no encontrado."));

        var rut = Rut.Create(request.Rut);
        if (rut != client.Rut)
        {
            var existing = await _unitOfWork.Clients.GetByRutAsync(rut.Value, cancellationToken);
            if (existing is not null && existing.Id != clientId)
                return Result.Failure<ClientDto>(Error.Conflict("Client.DuplicateRut", "Ya existe un cliente con ese RUT."));
        }

        client.Name = request.Name;
        client.LastName = request.LastName;
        client.Rut = rut;
        client.Phone = request.Phone;
        client.Email = request.Email;
        client.DateOfBirth = request.DateOfBirth;
        client.Address = request.Address;
        client.Notes = request.Notes;

        _unitOfWork.Clients.Update(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(client.ToDto());
    }

    public async Task<Result> SetFavoriteAsync(Guid clientId, bool isFavorite, CancellationToken cancellationToken = default)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return Result.Failure(Error.NotFound("Client.NotFound", "Cliente no encontrado."));

        client.IsFavorite = isFavorite;
        _unitOfWork.Clients.Update(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> SetActiveAsync(Guid clientId, bool isActive, CancellationToken cancellationToken = default)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return Result.Failure(Error.NotFound("Client.NotFound", "Cliente no encontrado."));

        client.IsActive = isActive;
        _unitOfWork.Clients.Update(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return Result.Failure(Error.NotFound("Client.NotFound", "Cliente no encontrado."));

        _unitOfWork.Clients.Remove(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
