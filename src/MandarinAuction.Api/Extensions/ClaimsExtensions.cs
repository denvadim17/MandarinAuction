using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MandarinAuction.Api.Extensions;

public static class ClaimsExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }
}
