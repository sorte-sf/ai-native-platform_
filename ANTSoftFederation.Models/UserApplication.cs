namespace SorteFederation.Models;

public class UserApplication : BaseEntity
{
    public Guid ANTFedUserId { get; set; }
    [ForeignKey(nameof(ANTFedUserId))]
    public virtual User ANTFedUser { get; set; }

    public Guid CompanyApplicationId { get; set; }
    [ForeignKey(nameof(CompanyApplicationId))]
    public virtual CompanyApplication CompanyApplication { get; set; }
}
