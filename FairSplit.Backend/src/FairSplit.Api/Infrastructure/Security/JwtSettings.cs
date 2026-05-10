namespace FairSplit.Api.Shared.Security;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = "FairSplit";

    public string Audience { get; set; } = "FairSplit.Mobile";

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 720;
}