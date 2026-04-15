using ReservationSystem.Application.Abstractions;
using ReservationSystem.Application.Common;
using ReservationSystem.Application.Dtos;
using ReservationSystem.Application.Mapping;

namespace ReservationSystem.Application.Services;

internal sealed class ReservationService(IReservationRepository repository) : IReservationService
{
    public async Task<IReadOnlyList<DeskDto>> GetDesksAsync(CancellationToken ct = default)
    {
        var desks = await repository.GetDesksAsync(ct);
        return desks.Select(d => d.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct = default)
    {
        var users = await repository.GetUsersAsync(ct);
        return users.Select(u => u.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<ReservationDto>> GetReservationsAsync(int? deskId, CancellationToken ct = default)
    {
        var reservations = await repository.GetReservationsAsync(deskId, ct);
        return reservations.Select(r => r.ToDto()).ToList();
    }

    public async Task<Result<ReservationDto>> CreateAsync(CreateReservationRequest request, int actingUserId, CancellationToken ct = default)
    {
        if (request.EndAt <= request.StartAt)
            return Result<ReservationDto>.Validation(ErrorCodes.EndAtNotAfterStart);

        var (newId, error) = await repository.CreateReservationAsync(
            request.DeskId, actingUserId, request.StartAt, request.EndAt, ct);

        if (error is not null)
        {
            return error switch
            {
                RepositoryError.DeskNotFound   => Result<ReservationDto>.NotFound(ErrorCodes.DeskNotFound),
                RepositoryError.UserNotFound   => Result<ReservationDto>.NotFound(ErrorCodes.UserNotFound),
                RepositoryError.TimeSlotTaken  => Result<ReservationDto>.Conflict(ErrorCodes.TimeSlotTaken),
                _                              => Result<ReservationDto>.Conflict(ErrorCodes.Unexpected)
            };
        }

        var entity = await repository.GetByIdAsync(newId!.Value, ct);
        return Result<ReservationDto>.Success(entity!.ToDto());
    }

    public async Task<Result> CancelAsync(int id, int cancellingUserId, CancellationToken ct = default)
    {
        var reservation = await repository.GetByIdAsync(id, ct);
        if (reservation is null)
            return Result.NotFound(ErrorCodes.ReservationNotFound);

        if (reservation.UserId != cancellingUserId)
            return Result.Forbidden(ErrorCodes.NotOwner);

        var cancelled = await repository.CancelAsync(id, ct);
        return cancelled
            ? Result.Success()
            : Result.NotFound(ErrorCodes.ReservationNotFound);
    }
}
