using Microsoft.AspNetCore.DataProtection;
using ReservationSystem.Application.Abstractions;
using ReservationSystem.Web.Auth;

namespace ReservationSystem.Web.Api;

public static class SessionEndpoints
{
    public sealed record LoginAsRequest(int UserId);

    public sealed record SessionInfo(int? UserId, string? UserName);

    public static IEndpointRouteBuilder MapSessionApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/session").DisableAntiforgery();

        group.MapGet("/", async (IUserContext userContext, IReservationService svc, CancellationToken ct) =>
        {
            var id = userContext.CurrentUserId;
            if (id is null) return Results.Ok(new SessionInfo(null, null));

            var users = await svc.GetUsersAsync(ct);
            var user = users.FirstOrDefault(u => u.Id == id);
            return Results.Ok(new SessionInfo(user?.Id, user?.Name));
        });

        group.MapPost("/",
            async Task<IResult> (LoginAsRequest request,
                IReservationService svc,
                IDataProtectionProvider dataProtection,
                HttpContext http,
                CancellationToken ct) =>
            {
                var users = await svc.GetUsersAsync(ct);
                if (users.All(u => u.Id != request.UserId))
                    return Results.NotFound(new { error = "User not found." });

                var token = dataProtection
                    .CreateProtector(CookieUserContext.ProtectorPurpose)
                    .Protect(request.UserId.ToString());

                http.Response.Cookies.Append(CookieUserContext.CookieName, token, new CookieOptions
                {
                    HttpOnly    = true,
                    SameSite    = SameSiteMode.Strict,
                    Secure      = http.Request.IsHttps,
                    IsEssential = true,
                    MaxAge      = TimeSpan.FromDays(30)
                });

                return Results.NoContent();
            });

        group.MapDelete("/", (HttpContext http) =>
        {
            http.Response.Cookies.Delete(CookieUserContext.CookieName);
            return Results.NoContent();
        });

        return app;
    }
}
