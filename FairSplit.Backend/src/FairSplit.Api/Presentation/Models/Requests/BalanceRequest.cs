namespace FairSplit.Api.Presentation.Models.Requests;

using System.ComponentModel.DataAnnotations;
using FairSplit.Api.Presentation.Validation;

public sealed class BalanceRequest
{
    [Required]
    [NotEmptyGuid]
    public Guid GroupId { get; set; }

    [Required]
    [NotEmptyGuid]
    public Guid MemberId { get; set; }
}
