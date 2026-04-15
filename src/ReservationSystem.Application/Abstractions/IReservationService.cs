using ReservationSystem.Application.Common;
using ReservationSystem.Application.Dtos;

namespace ReservationSystem.Application.Abstractions;

public interface IReservationService
{
    Task<IReadOnlyList<DeskDto>> GetDesksAsync(CancellationToken ct = default);

    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct = default);

    Task<IReadOnlyList<ReservationDto>> GetReservationsAsync(int? deskId, CancellationToken ct = default);

    Task<Result<ReservationDto>> CreateAsync(CreateReservationRequest request, int actingUserId, CancellationToken ct = default);

    Task<Result> CancelAsync(int id, int cancellingUserId, CancellationToken ct = default);
}
