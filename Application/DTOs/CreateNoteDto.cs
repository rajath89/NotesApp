namespace Application.DTOs;

public class CreateNoteDto
{
    public string Title { get; set; }
    public string Content { get; set; }
    public int WorkspaceId { get; set; }
}