namespace FairSplit.Api.Presentation.Models.Requests;

using System.ComponentModel.DataAnnotations;
using FairSplit.Api.Presentation.Validation;

public sealed class MemberRequest
{
    [Required]
    [NotEmptyGuid]
    public Guid GroupId { get; set; }

    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;
}
