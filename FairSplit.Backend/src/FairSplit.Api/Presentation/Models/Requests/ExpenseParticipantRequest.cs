namespace FairSplit.Api.Presentation.Models.Requests;

using System.ComponentModel.DataAnnotations;
using FairSplit.Api.Presentation.Validation;

public sealed class ExpenseParticipantRequest
{
    [Required]
    [NotEmptyGuid]
    public Guid ExpenseId { get; set; }

    [Required]
    [NotEmptyGuid]
    public Guid MemberId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ShareAmount { get; set; }
}
