
using System.ComponentModel.DataAnnotations;

namespace SorteFederation.GlobalEnums;

public enum OpportunityStagesEnum
{
    [Display(Name = "Prospecting")]
    Prospecting = 1,

    [Display(Name = "Qualification")]
    Qualification,

    [Display(Name = "Proposal")]
    Proposal,

    [Display(Name = "Negotiation")]
    Negotiation,

    [Display(Name = "Closed - Won")]
    ClosedWon,

    [Display(Name = "Closed - Lost")]
    ClosedLost
}
