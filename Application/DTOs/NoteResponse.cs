namespace Application.DTOs;

public class NoteResponse
{
    public int Status { get; set; }
    public IEnumerable<NoteDto> Notes { get; set; }
    public ErrorInfo ErrorInfo { get; set; }
}