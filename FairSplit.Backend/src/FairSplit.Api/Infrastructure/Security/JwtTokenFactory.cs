using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FairSplit.Api.Shared.Security;

public static class JwtTokenFactory
{
    public static (string Token, DateTimeOffset ExpiresAtUtc) CreateAccessToken(
        Guid memberId,
        string displayName,
        JwtSettings settings,
        DateTimeOffset nowUtc)
    {
        var expiresAtUtc = nowUtc.AddMinutes(settings.AccessTokenMinutes);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, memberId.ToString()),
            new Claim(ClaimTypes.Name, displayName),
            new Claim("display_name", displayName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: nowUtc.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}