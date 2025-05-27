namespace Application.DTOs;

public class WorkspaceDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public List<NoteDto> Notes { get; set; } = new List<NoteDto>();
}