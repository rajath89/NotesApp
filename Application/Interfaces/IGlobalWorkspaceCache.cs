using Application.DTOs;

namespace Application.Interfaces;

public interface IGlobalWorkspaceCache
{
    Task<IEnumerable<WorkspaceDto>> GetUserWorkspacesAsync(string userId);
    Task<WorkspaceDto> GetWorkspaceAsync(int workspaceId, string userId);
    Task<IEnumerable<NoteDto>> GetWorkspaceNotesAsync(int workspaceId, string userId);
    Task<NoteDto> GetNoteAsync(int noteId, string userId);
    
    Task SetUserWorkspacesAsync(string userId, IEnumerable<WorkspaceDto> workspaces);
    Task SetWorkspaceAsync(WorkspaceDto workspace);
    Task SetWorkspaceNotesAsync(int workspaceId, IEnumerable<NoteDto> notes);
    Task SetNoteAsync(NoteDto note);
    
    Task InvalidateUserWorkspacesAsync(string userId);
    Task InvalidateWorkspaceAsync(int workspaceId);
    Task InvalidateWorkspaceNotesAsync(int workspaceId);
    Task InvalidateNoteAsync(int noteId);
    Task InvalidateAllUserDataAsync(string userId);
}