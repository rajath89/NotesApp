namespace Domain.Entities;

public class Note
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public int WorkspaceId { get; set; }
    public Workspace Workspace { get; set; }
}