namespace SorteFederation.Models;

public class UserGroup: BaseEntity
{
    public Guid UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }

    public Guid GroupId { get; set; }
    [ForeignKey(nameof(GroupId))]
    public virtual Group Group { get; set; }
     
}