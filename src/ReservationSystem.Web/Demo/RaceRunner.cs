using System.Diagnostics;
using Microsoft.Extensions.Options;
using ReservationSystem.Application.Abstractions;
using ReservationSystem.Application.Common;
using ReservationSystem.Application.Dtos;

namespace ReservationSystem.Web.Demo;

public sealed record RaceParameters(int DeskId, DateTime StartAt, DateTime EndAt, int AttackerCount);

public sealed record RaceAttacker(
    int UserId,
    string UserName,
    string Outcome,
    int? ReservationId,
    string? ErrorCode,
    long ElapsedMs);

public sealed record RaceSummary(
    int AttackerCount,
    int Winners,
    int Conflicts,
    int OtherErrors,
    IReadOnlyList<RaceAttacker> Attackers);

public sealed class RaceRunner(IServiceScopeFactory scopeFactory, IOptions<ReservationOptions> options)
{
    public const int MinAttackers = 2;

    public int MaxAttackers => options.Value.MaxRaceAttackers;

    public async Task<RaceSummary> RunAsync(RaceParameters parameters, CancellationToken ct = default)
    {
        if (parameters.AttackerCount < MinAttackers || parameters.AttackerCount > MaxAttackers)
            throw new ArgumentOutOfRangeException(nameof(parameters.AttackerCount));

        await using var primary = scopeFactory.CreateAsyncScope();
        var svc = primary.ServiceProvider.GetRequiredService<IReservationService>();
        var users = await svc.GetUsersAsync(ct);

        var gate = new TaskCompletionSource();

        var tasks = Enumerable.Range(0, parameters.AttackerCount).Select(async i =>
        {
            var user = users[Random.Shared.Next(users.Count)];
            await gate.Task;

            var stopwatch = Stopwatch.StartNew();
            await using var scope = scopeFactory.CreateAsyncScope();
            var scopedSvc = scope.ServiceProvider.GetRequiredService<IReservationService>();

            var result = await scopedSvc.CreateAsync(
                new CreateReservationRequest
                {
                    DeskId = parameters.DeskId,
                    StartAt = parameters.StartAt,
                    EndAt = parameters.EndAt
                },
                user.Id,
                ct);

            stopwatch.Stop();

            return new RaceAttacker(
                user.Id,
                user.Name,
                OutcomeFor(result),
                result.IsSuccess ? result.Value!.Id : null,
                result.IsSuccess ? null : result.Error,
                stopwatch.ElapsedMilliseconds);
        }).ToArray();

        gate.SetResult();
        var attackers = await Task.WhenAll(tasks);

        return new RaceSummary(
            parameters.AttackerCount,
            attackers.Count(a => a.Outcome == "Winner"),
            attackers.Count(a => a.Outcome == "Conflict"),
            attackers.Count(a => a.Outcome == "Error"),
            attackers);
    }

    private static string OutcomeFor(Result<ReservationDto> result) =>
        result.IsSuccess
            ? "Winner"
            : result.ErrorType == ErrorType.Conflict ? "Conflict" : "Error";
}
