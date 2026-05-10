namespace FairSplit.Api.Presentation.Models.Requests;

using System.ComponentModel.DataAnnotations;
using FairSplit.Api.Presentation.Validation;

public sealed class ExpenseRequest
{
    [Required]
    [NotEmptyGuid]
    public Guid PayerMemberId { get; set; }

    [Required]
    [Range(0.01, 10000000)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(50)]
    public string SplitType { get; set; } = "equal";

    [Required]
    [MinLength(1, ErrorMessage = "At least one participant is required.")]
    public IReadOnlyCollection<ExpenseParticipantShareRequest> Participants { get; set; } = [];
}

public sealed class ExpenseParticipantShareRequest
{
    [Required]
    [NotEmptyGuid]
    public Guid MemberId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ShareAmount { get; set; }
}
