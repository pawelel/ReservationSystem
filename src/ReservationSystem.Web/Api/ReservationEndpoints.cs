using Microsoft.Extensions.Localization;
using ReservationSystem.Application.Abstractions;
using ReservationSystem.Application.Common;
using ReservationSystem.Application.Dtos;
using ReservationSystem.Web.Resources;

namespace ReservationSystem.Web.Api;

public static class ReservationEndpoints
{
    public static IEndpointRouteBuilder MapReservationApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").DisableAntiforgery();

        group.MapGet("/desks", async (IReservationService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetDesksAsync(ct)));

        group.MapGet("/users", async (IReservationService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetUsersAsync(ct)));

        group.MapGet("/reservations", async (IReservationService svc, int? deskId, CancellationToken ct) =>
            Results.Ok(await svc.GetReservationsAsync(deskId, ct)));

        group.MapPost("/reservations",
            async Task<IResult> (
                CreateReservationRequest request,
                IReservationService svc,
                IUserContext userContext,
                IStringLocalizer<SharedResources> localizer,
                CancellationToken ct) =>
            {
                var actingUserId = userContext.CurrentUserId;
                if (actingUserId is null)
                    return Results.Unauthorized();

                var result = await svc.CreateAsync(request, actingUserId.Value, ct);
                return result.IsSuccess
                    ? Results.Created($"/api/reservations/{result.Value!.Id}", result.Value)
                    : MapFailure(result.ErrorType, result.Error, localizer);
            })
            .DisableAntiforgery();

        group.MapDelete("/reservations/{id:int}",
            async Task<IResult> (
                int id,
                IReservationService svc,
                IUserContext userContext,
                IStringLocalizer<SharedResources> localizer,
                CancellationToken ct) =>
            {
                var cancellingUserId = userContext.CurrentUserId;
                if (cancellingUserId is null)
                    return Results.Unauthorized();

                var result = await svc.CancelAsync(id, cancellingUserId.Value, ct);
                return result.IsSuccess
                    ? Results.NoContent()
                    : MapFailure(result.ErrorType, result.Error, localizer);
            })
            .DisableAntiforgery();

        return app;
    }

    private static IResult MapFailure(ErrorType errorType, string? errorCode, IStringLocalizer localizer)
    {
        var payload = new
        {
            code = errorCode,
            message = errorCode is null ? null : localizer[errorCode].Value
        };

        return errorType switch
        {
            ErrorType.Validation => Results.BadRequest(payload),
            ErrorType.NotFound => Results.NotFound(payload),
            ErrorType.Forbidden => Results.Json(payload, statusCode: StatusCodes.Status403Forbidden),
            _ => Results.Conflict(payload)
        };
    }
}
