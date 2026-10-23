namespace SorteFederation.Models;

public class UserApplicationChat
{
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid? ParentId { get; set; }
    public virtual ICollection<UserApplicationChat> ApplicationChats { get; set; }

    [Required]
    public string Topic { get; set; }

    public string ChatDetails{ get; set; }

    public Guid UserId { get; set; }
    public virtual User User { get; set; }

    public string CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
     
    
}
