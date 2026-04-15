namespace ReservationSystem.Application.Dtos;

public class CreateReservationRequest
{
    public int DeskId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}
