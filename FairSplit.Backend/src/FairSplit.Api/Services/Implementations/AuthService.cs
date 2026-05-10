using FairSplit.Api.Services.Interfaces;
using FairSplit.Api.Services.Models;
using FairSplit.Api.Shared.Security;

namespace FairSplit.Api.Services.Implementations;

public sealed class AuthService(IConfiguration configuration) : IAuthService
{
    public AuthTokenResult CreateDevToken(Guid memberId, string displayName)
    {
        var settings = configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings were not configured.");

        if (string.IsNullOrWhiteSpace(settings.SigningKey))
        {
            throw new InvalidOperationException("JWT signing key was not configured.");
        }

        var nowUtc = DateTimeOffset.UtcNow;
        var (token, expiresAtUtc) = JwtTokenFactory.CreateAccessToken(
            memberId,
            displayName,
            settings,
            nowUtc);

        return new AuthTokenResult
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc,
            MemberId = memberId,
            DisplayName = displayName
        };
    }
}