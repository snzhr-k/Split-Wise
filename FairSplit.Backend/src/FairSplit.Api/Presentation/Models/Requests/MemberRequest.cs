namespace FairSplit.Api.Presentation.Models.Requests;

public sealed class MemberRequest
{
    public Guid GroupId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
