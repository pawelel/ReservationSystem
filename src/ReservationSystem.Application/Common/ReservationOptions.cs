using System.ComponentModel.DataAnnotations;

namespace ReservationSystem.Application.Common;

public sealed class ReservationOptions
{
    public const string SectionName = "Reservation";

    [Range(2, 5000)]
    public int MaxRaceAttackers { get; set; } = 200;
}
