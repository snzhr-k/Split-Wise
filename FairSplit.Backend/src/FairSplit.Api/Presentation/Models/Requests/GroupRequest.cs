namespace FairSplit.Api.Presentation.Models.Requests;

using System.ComponentModel.DataAnnotations;

public sealed class GroupRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
}
