namespace FairSplit.Api.Presentation.Models.Requests;

using System.ComponentModel.DataAnnotations;
using FairSplit.Api.Presentation.Validation;

public sealed class DevTokenRequest
{
    [Required]
    [NotEmptyGuid]
    public Guid MemberId { get; set; }

    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;
}