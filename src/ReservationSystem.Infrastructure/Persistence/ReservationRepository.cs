using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Application.Abstractions;
using ReservationSystem.Application.Common;
using ReservationSystem.Domain.Entities;

namespace ReservationSystem.Infrastructure.Persistence;

internal sealed class ReservationRepository(IDbContextFactory<AppDbContext> dbFactory) : IReservationRepository
{
    private const string DeskNotFoundMessage   = "Desk not found.";
    private const string UserNotFoundMessage   = "User not found.";
    private const string SlotTakenMessage      = "The desk is already reserved for the requested time slot.";

    public async Task<IReadOnlyList<Desk>> GetDesksAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.Desks.AsNoTracking().OrderBy(d => d.Name).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<User>> GetUsersAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.Users.AsNoTracking().OrderBy(u => u.Name).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Reservation>> GetReservationsAsync(int? deskId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.Reservations
            .AsNoTracking()
            .Include(r => r.Desk)
            .Include(r => r.User)
            .Where(r => deskId == null || r.DeskId == deskId)
            .OrderByDescending(r => r.StartAt)
            .ToListAsync(ct);
    }

    public async Task<Reservation?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.Reservations
            .AsNoTracking()
            .Include(r => r.Desk)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    private const int MaxRetries = 3;

    public async Task<(int? NewId, RepositoryError? Error)> CreateReservationAsync(
        int deskId, int userId, DateTime startAt, DateTime endAt, CancellationToken ct = default)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                return await ExecuteCreateProcAsync(deskId, userId, startAt, endAt, ct);
            }
            catch (SqlException ex) when (IsTransient(ex) && attempt < MaxRetries)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10 * attempt), ct);
            }
        }

        throw new InvalidOperationException("Unreachable");
    }

    private async Task<(int? NewId, RepositoryError? Error)> ExecuteCreateProcAsync(
        int deskId, int userId, DateTime startAt, DateTime endAt, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var newIdParam = new SqlParameter("@NewReservationId", SqlDbType.Int)
            { Direction = ParameterDirection.Output };
        var errorParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 500)
            { Direction = ParameterDirection.Output };

        await db.Database.ExecuteSqlRawAsync(
            "EXEC dbo.usp_CreateReservation @DeskId, @UserId, @StartAt, @EndAt, @NewReservationId OUTPUT, @ErrorMessage OUTPUT",
            new object[]
            {
                new SqlParameter("@DeskId",  deskId),
                new SqlParameter("@UserId",  userId),
                new SqlParameter("@StartAt", startAt),
                new SqlParameter("@EndAt",   endAt),
                newIdParam,
                errorParam
            },
            ct);

        if (errorParam.Value is string message)
        {
            RepositoryError error = message switch
            {
                DeskNotFoundMessage => new RepositoryError.DeskNotFound(message),
                UserNotFoundMessage => new RepositoryError.UserNotFound(message),
                SlotTakenMessage    => new RepositoryError.TimeSlotTaken(message),
                _                   => throw new InvalidOperationException("Unknown procedure error returned from usp_CreateReservation.")
            };
            return (null, error);
        }

        return ((int)newIdParam.Value, null);
    }

    public async Task<bool> CancelAsync(int id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var rows = await db.Reservations
            .Where(r => r.Id == id && r.Status == ReservationStatus.Active)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, ReservationStatus.Cancelled), ct);
        return rows > 0;
    }

    private static bool IsTransient(SqlException ex) =>
        ex.Number is 1205 or 1222 or -2 or 4060 or 40501 or 40613;
}
