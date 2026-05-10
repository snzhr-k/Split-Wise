namespace FairSplit.Api.Presentation.Models.Responses;

public sealed class AuthTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public Guid MemberId { get; set; }

    public string DisplayName { get; set; } = string.Empty;
}