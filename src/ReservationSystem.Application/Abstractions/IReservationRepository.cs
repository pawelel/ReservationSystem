using ReservationSystem.Application.Common;
using ReservationSystem.Domain.Entities;

namespace ReservationSystem.Application.Abstractions;

public interface IReservationRepository
{
    Task<IReadOnlyList<Desk>> GetDesksAsync(CancellationToken ct = default);

    Task<IReadOnlyList<User>> GetUsersAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Reservation>> GetReservationsAsync(int? deskId, CancellationToken ct = default);

    Task<Reservation?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<(int? NewId, RepositoryError? Error)> CreateReservationAsync(
        int deskId, int userId, DateTime startAt, DateTime endAt, CancellationToken ct = default);

    Task<bool> CancelAsync(int id, CancellationToken ct = default);
}
