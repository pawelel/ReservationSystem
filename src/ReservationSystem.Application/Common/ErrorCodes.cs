namespace ReservationSystem.Application.Common;

public static class ErrorCodes
{
    public const string EndAtNotAfterStart = "Error_EndAtNotAfterStart";
    public const string DeskNotFound = "Error_DeskNotFound";
    public const string UserNotFound = "Error_UserNotFound";
    public const string TimeSlotTaken = "Error_TimeSlotTaken";
    public const string ReservationNotFound = "Error_ReservationNotFound";
    public const string NotOwner = "Error_NotOwner";
    public const string Unexpected = "Error_Unexpected";
}
