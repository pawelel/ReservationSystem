namespace ReservationSystem.Application.Dtos;

public record ReservationDto(
    int Id,
    int DeskId,
    string DeskName,
    int UserId,
    string UserName,
    DateTime StartAt,
    DateTime EndAt,
    string Status,
    DateTime CreatedAt);
