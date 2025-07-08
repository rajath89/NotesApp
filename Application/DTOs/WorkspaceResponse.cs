namespace Application.DTOs;

public class WorkspaceResponse
{
    public int Status { get; set; }
    public IEnumerable<WorkspaceDto> Workspaces { get; set; }
    public ErrorInfo ErrorInfo { get; set; }
}