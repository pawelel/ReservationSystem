namespace ReservationSystem.Application.Abstractions;

public interface IUserContext
{
    int? CurrentUserId { get; }
}
