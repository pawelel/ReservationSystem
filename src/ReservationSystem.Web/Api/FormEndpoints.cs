using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using ReservationSystem.Application.Abstractions;
using ReservationSystem.Web.Auth;

namespace ReservationSystem.Web.Api;

public static class FormEndpoints
{
    public static IEndpointRouteBuilder MapFormEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/login", async (
                [FromForm] int userId,
                IReservationService svc,
                IDataProtectionProvider dataProtection,
                HttpContext http,
                CancellationToken ct) =>
            {
                var users = await svc.GetUsersAsync(ct);
                if (users.Any(u => u.Id == userId))
                {
                    var token = dataProtection
                        .CreateProtector(CookieUserContext.ProtectorPurpose)
                        .Protect(userId.ToString());

                    http.Response.Cookies.Append(CookieUserContext.CookieName, token, new CookieOptions
                    {
                        HttpOnly    = true,
                        SameSite    = SameSiteMode.Strict,
                        Secure      = http.Request.IsHttps,
                        IsEssential = true,
                        MaxAge      = TimeSpan.FromDays(30)
                    });
                }

                return Results.Redirect("/");
            })
            .DisableAntiforgery();

        app.MapPost("/logout", (HttpContext http) =>
        {
            http.Response.Cookies.Delete(CookieUserContext.CookieName);
            return Results.Redirect("/");
        })
        .DisableAntiforgery();

        app.MapPost("/culture", ([FromForm] string culture, [FromForm] string? returnUrl, HttpContext http) =>
        {
            http.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

            return Results.Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
        })
        .DisableAntiforgery();

        app.MapPost("/reservations/cancel", async (
                [FromForm] int id,
                IReservationService svc,
                IUserContext userContext,
                CancellationToken ct) =>
            {
                var cancellingUserId = userContext.CurrentUserId;
                if (cancellingUserId is not null)
                    await svc.CancelAsync(id, cancellingUserId.Value, ct);

                return Results.Redirect("/");
            })
            .DisableAntiforgery();

        return app;
    }
}
