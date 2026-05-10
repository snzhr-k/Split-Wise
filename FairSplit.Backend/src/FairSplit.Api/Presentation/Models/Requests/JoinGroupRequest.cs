namespace FairSplit.Api.Presentation.Models.Requests;

public sealed class JoinGroupRequest
{
    public string? InviteCode { get; set; }

    public string? InviteToken { get; set; }

    public string? DisplayName { get; set; }
}
