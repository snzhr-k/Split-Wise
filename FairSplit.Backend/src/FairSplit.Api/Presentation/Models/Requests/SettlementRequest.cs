namespace FairSplit.Api.Presentation.Models.Requests;

using System.ComponentModel.DataAnnotations;
using FairSplit.Api.Presentation.Validation;

public sealed class SettlementRequest
{
    [Required]
    [NotEmptyGuid]
    public Guid GroupId { get; set; }

    [Required]
    [NotEmptyGuid]
    public Guid FromMemberId { get; set; }

    [Required]
    [NotEmptyGuid]
    public Guid ToMemberId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
}
