namespace ReservationSystem.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int DeskId { get; set; }
    public Desk Desk { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
    public DateTime CreatedAt { get; set; }
}
