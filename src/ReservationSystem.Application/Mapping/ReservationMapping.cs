using ReservationSystem.Application.Dtos;
using ReservationSystem.Domain.Entities;

namespace ReservationSystem.Application.Mapping;

internal static class ReservationMapping
{
    public static DeskDto ToDto(this Desk d) => new(d.Id, d.Name);

    public static UserDto ToDto(this User u) => new(u.Id, u.Name);

    public static ReservationDto ToDto(this Reservation r) => new(
        r.Id,
        r.DeskId,
        r.Desk?.Name ?? string.Empty,
        r.UserId,
        r.User?.Name ?? string.Empty,
        r.StartAt,
        r.EndAt,
        r.Status.ToString(),
        r.CreatedAt);
}
