using Microsoft.AspNetCore.DataProtection;
using ReservationSystem.Application.Abstractions;

namespace ReservationSystem.Web.Auth;

internal sealed class CookieUserContext(IHttpContextAccessor accessor, IDataProtectionProvider dataProtection) : IUserContext
{
    internal const string CookieName = "reservation-as";
    internal const string ProtectorPurpose = "ReservationSystem.LoginAs.v1";

    private readonly IDataProtector _protector = dataProtection.CreateProtector(ProtectorPurpose);

    public int? CurrentUserId
    {
        get
        {
            var cookie = accessor.HttpContext?.Request.Cookies[CookieName];
            if (string.IsNullOrEmpty(cookie)) return null;

            try
            {
                return int.TryParse(_protector.Unprotect(cookie), out var id) ? id : null;
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                return null;
            }
        }
    }
}
