using System.Reflection;

namespace ReservationSystem.Infrastructure.Persistence.Scripts;

internal static class SqlScript
{
    private static readonly Assembly Asm = typeof(SqlScript).Assembly;

    public static string Load(string name)
    {
        var resourceName = $"ReservationSystem.Infrastructure.Persistence.Scripts.{name}";
        using var stream = Asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded SQL script '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
