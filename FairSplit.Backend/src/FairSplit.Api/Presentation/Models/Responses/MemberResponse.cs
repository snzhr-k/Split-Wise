namespace FairSplit.Api.Presentation.Models.Responses;

public sealed class MemberResponse
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
