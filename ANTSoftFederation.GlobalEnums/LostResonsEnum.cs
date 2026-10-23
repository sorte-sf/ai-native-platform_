
using System.ComponentModel.DataAnnotations;

namespace SorteFederation.GlobalEnums;


public enum LostResonsEnum
{
    None = 1,

    [Display(Name = "Pricing Too High")]
    PricingTooHigh,

    [Display(Name = "Competitor Won")]
    CompetitorWon,

    [Display(Name = "Lack of Budget")]
    LackOfBudget,

    [Display(Name = "Lack of Interest")]
    LackOfInterest,

    [Display(Name = "Timeline Issues")]
    TimelineIssues,

    [Display(Name = "Product Limitations")]
    ProductLimitations,

    [Display(Name = "Other")]
    Other
}
