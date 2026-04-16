using System.Net;
using System.Net.Http.Json;
using ReservationSystem.Application.Dtos;

namespace ReservationSystem.Tests;

[Collection("Database")]
public class ConcurrencyTests : IAsyncLifetime
{
    private const int UserAnna = 1;
    private const int UserJan = 2;
    private const int UserPiotr = 3;
    private const int UserMaria = 4;
    private const int UserTomasz = 5;

    private readonly ReservationWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public ConcurrencyTests(ReservationWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
        await LoginAs(_client, UserAnna);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task LoginAs(HttpClient client, int userId)
    {
        var response = await client.PostAsJsonAsync("/api/session", new { userId });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Fifty_parallel_requests_for_the_same_slot_create_exactly_one_reservation()
    {
        const int parallelRequests = 50;
        var request = new CreateReservationRequest
        {
            DeskId = 1,
            StartAt = new DateTime(2026, 5, 1, 9, 0, 0),
            EndAt = new DateTime(2026, 5, 1, 10, 0, 0)
        };

        var gate = new TaskCompletionSource();

        var tasks = Enumerable.Range(0, parallelRequests).Select(async _ =>
        {
            await gate.Task;
            return await _client.PostAsJsonAsync("/api/reservations", request);
        }).ToArray();

        gate.SetResult();
        var responses = await Task.WhenAll(tasks);

        var created = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflict = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);
        var breakdown = string.Join(", ", responses.GroupBy(r => (int)r.StatusCode).Select(g => $"{g.Key}:{g.Count()}"));

        Assert.True(created == 1 && conflict == parallelRequests - 1,
            $"Expected 1×201 + {parallelRequests - 1}×409. Got {breakdown}.");

        var listed = await _client.GetFromJsonAsync<List<ReservationDto>>("/api/reservations?deskId=1");
        Assert.NotNull(listed);
        Assert.Single(listed!, r => r.Status == "Active");
    }

    [Fact]
    public async Task Cancelled_reservation_frees_the_slot()
    {
        var slot = new CreateReservationRequest
        {
            DeskId = 2,
            StartAt = new DateTime(2026, 6, 1, 9, 0, 0),
            EndAt = new DateTime(2026, 6, 1, 10, 0, 0)
        };

        await LoginAs(_client, UserAnna);
        var first = await _client.PostAsJsonAsync("/api/reservations", slot);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        var firstBody = await first.Content.ReadFromJsonAsync<ReservationDto>();

        await LoginAs(_client, UserJan);
        var second = await _client.PostAsJsonAsync("/api/reservations", slot);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        await LoginAs(_client, UserAnna);
        var cancel = await _client.DeleteAsync($"/api/reservations/{firstBody!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, cancel.StatusCode);

        await LoginAs(_client, UserJan);
        var third = await _client.PostAsJsonAsync("/api/reservations", slot);
        Assert.Equal(HttpStatusCode.Created, third.StatusCode);
    }

    [Fact]
    public async Task Adjacent_slots_do_not_conflict()
    {
        var morning = new CreateReservationRequest
        {
            DeskId = 3,
            StartAt = new DateTime(2026, 7, 1, 9, 0, 0),
            EndAt = new DateTime(2026, 7, 1, 11, 0, 0)
        };
        var afternoon = new CreateReservationRequest
        {
            DeskId = 3,
            StartAt = morning.EndAt,
            EndAt = morning.EndAt.AddHours(2)
        };

        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/reservations", morning)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/reservations", afternoon)).StatusCode);
    }

    [Fact]
    public async Task Partial_overlap_is_rejected()
    {
        var taken = new CreateReservationRequest
        {
            DeskId = 4,
            StartAt = new DateTime(2026, 8, 1, 9, 0, 0),
            EndAt = new DateTime(2026, 8, 1, 11, 0, 0)
        };
        var overlapping = new CreateReservationRequest
        {
            DeskId = taken.DeskId,
            StartAt = new DateTime(2026, 8, 1, 10, 0, 0),
            EndAt = new DateTime(2026, 8, 1, 12, 0, 0)
        };

        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/reservations", taken)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _client.PostAsJsonAsync("/api/reservations", overlapping)).StatusCode);
    }

    [Fact]
    public async Task Invalid_time_range_returns_400()
    {
        var invalid = new CreateReservationRequest
        {
            DeskId = 1,
            StartAt = new DateTime(2026, 9, 1, 10, 0, 0),
            EndAt = new DateTime(2026, 9, 1, 9, 0, 0)
        };

        var response = await _client.PostAsJsonAsync("/api/reservations", invalid);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_desk_returns_404()
    {
        var request = new CreateReservationRequest
        {
            DeskId = 9999,
            StartAt = new DateTime(2026, 10, 1, 9, 0, 0),
            EndAt = new DateTime(2026, 10, 1, 10, 0, 0)
        };

        var response = await _client.PostAsJsonAsync("/api/reservations", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Second_cancel_of_same_reservation_returns_404()
    {
        var request = new CreateReservationRequest
        {
            DeskId = 5,
            StartAt = new DateTime(2026, 11, 1, 9, 0, 0),
            EndAt = new DateTime(2026, 11, 1, 10, 0, 0)
        };

        var create = await _client.PostAsJsonAsync("/api/reservations", request);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<ReservationDto>();

        var first = await _client.DeleteAsync($"/api/reservations/{created!.Id}");
        var second = await _client.DeleteAsync($"/api/reservations/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
    }

    [Fact]
    public async Task Same_time_on_different_desks_both_succeed()
    {
        var slot = (Start: new DateTime(2026, 12, 1, 9, 0, 0),
                    End: new DateTime(2026, 12, 1, 10, 0, 0));

        var a = new CreateReservationRequest { DeskId = 1, StartAt = slot.Start, EndAt = slot.End };
        var b = new CreateReservationRequest { DeskId = 2, StartAt = slot.Start, EndAt = slot.End };

        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/reservations", a)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/reservations", b)).StatusCode);
    }

    [Fact]
    public async Task Only_owner_can_cancel_reservation()
    {
        var request = new CreateReservationRequest
        {
            DeskId = 3,
            StartAt = new DateTime(2027, 1, 1, 9, 0, 0),
            EndAt = new DateTime(2027, 1, 1, 10, 0, 0)
        };

        await LoginAs(_client, UserPiotr);
        var create = await _client.PostAsJsonAsync("/api/reservations", request);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var owned = await create.Content.ReadFromJsonAsync<ReservationDto>();

        await LoginAs(_client, UserMaria);
        var intruder = await _client.DeleteAsync($"/api/reservations/{owned!.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, intruder.StatusCode);

        await LoginAs(_client, UserPiotr);
        var owner = await _client.DeleteAsync($"/api/reservations/{owned.Id}");
        Assert.Equal(HttpStatusCode.NoContent, owner.StatusCode);
    }

    [Fact]
    public async Task Anonymous_cancel_is_unauthorized()
    {
        var request = new CreateReservationRequest
        {
            DeskId = 4,
            StartAt = new DateTime(2027, 2, 1, 9, 0, 0),
            EndAt = new DateTime(2027, 2, 1, 10, 0, 0)
        };
        var created = await _client.PostAsJsonAsync("/api/reservations", request);
        var reservation = await created.Content.ReadFromJsonAsync<ReservationDto>();

        using var anon = _factory.CreateClient();
        var response = await anon.DeleteAsync($"/api/reservations/{reservation!.Id}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
