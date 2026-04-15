using ReservationSystem.Web.Demo;

namespace ReservationSystem.Web.Api;

public static class DemoEndpoints
{
    public sealed record RaceRequestDto(int DeskId, DateTime StartAt, DateTime EndAt, int AttackerCount);

    public static IEndpointRouteBuilder MapDemoApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/demo").DisableAntiforgery();

        group.MapPost("/race", async Task<IResult> (
                RaceRequestDto request,
                RaceRunner runner,
                CancellationToken ct) =>
            {
                if (request.AttackerCount < RaceRunner.MinAttackers || request.AttackerCount > runner.MaxAttackers)
                    return Results.BadRequest(new { error = "Error_AttackerCountOutOfRange" });

                if (request.EndAt <= request.StartAt)
                    return Results.BadRequest(new { error = "Error_EndAtNotAfterStart" });

                var summary = await runner.RunAsync(
                    new RaceParameters(request.DeskId, request.StartAt, request.EndAt, request.AttackerCount),
                    ct);

                return Results.Ok(summary);
            });

        return app;
    }
}
