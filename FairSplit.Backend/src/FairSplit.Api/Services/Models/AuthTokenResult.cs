namespace FairSplit.Api.Services.Models;

public sealed class AuthTokenResult
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public Guid MemberId { get; set; }

    public string DisplayName { get; set; } = string.Empty;
}