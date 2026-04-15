namespace ReservationSystem.Application.Common;

public abstract record RepositoryError(string Message)
{
    public sealed record DeskNotFound(string Message) : RepositoryError(Message);
    public sealed record UserNotFound(string Message) : RepositoryError(Message);
    public sealed record TimeSlotTaken(string Message) : RepositoryError(Message);
    public sealed record Unexpected(string Message) : RepositoryError(Message);
}
