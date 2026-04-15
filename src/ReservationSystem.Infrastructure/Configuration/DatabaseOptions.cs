using System.ComponentModel.DataAnnotations;

namespace ReservationSystem.Infrastructure.Configuration;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required(AllowEmptyStrings = false, ErrorMessage = "Connection string is required (Database:ConnectionString).")]
    public string ConnectionString { get; set; } = string.Empty;
}
