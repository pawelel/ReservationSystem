using Microsoft.Extensions.DependencyInjection;
using ReservationSystem.Web.Demo;

namespace ReservationSystem.Tests;

[Collection("Database")]
public class RaceRunnerTests : IAsyncLifetime
{
    private readonly ReservationWebApplicationFactory _factory;

    public RaceRunnerTests(ReservationWebApplicationFactory factory) => _factory = factory;

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Fifty_simulated_attackers_produce_exactly_one_winner()
    {
        var runner = _factory.Services.GetRequiredService<RaceRunner>();

        var summary = await runner.RunAsync(new RaceParameters(
            DeskId:        1,
            StartAt:       new DateTime(2027, 4, 1, 9, 0, 0),
            EndAt:         new DateTime(2027, 4, 1, 10, 0, 0),
            AttackerCount: 50));

        Assert.Equal(1,  summary.Winners);
        Assert.Equal(49, summary.Conflicts);
        Assert.Equal(0,  summary.OtherErrors);

        Assert.Single(summary.Attackers, a => a.Outcome == "Winner");
        Assert.All(summary.Attackers, a => Assert.True(a.ElapsedMs >= 0));
    }
}
