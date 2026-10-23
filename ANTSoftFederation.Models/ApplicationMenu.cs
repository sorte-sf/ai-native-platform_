namespace SorteFederation.Models;

public class ApplicationMenu
{
    [Key]
    public required Guid Id { get; set; } = Guid.CreateVersion7();

    public required string Title { get; set; }

    public string SubTitle { get; set; }

    public string Icon { get; set; }
     
    public string Link { get; set; }


    [Required]
    public ApplicationMenuType Type { get; set; }

    public int? Order { get; set; }

    public string[]? Roles { get; set; } = null;

    public Guid ApplicationId { get; set; }
    [ForeignKey(nameof(ApplicationId))]
    public virtual ProjectApplication Application { get; set; }

    public Guid? ParentMenuId { get; set; }
    [ForeignKey(nameof(ParentMenuId))]
    public virtual ApplicationMenu ParentMenu { get; set; }

    public virtual ICollection<ApplicationMenu> Children { get; set; }

    public string CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;
    public string UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public RecordState RecordState { get; set; } = RecordState.Active;
    
    
}

