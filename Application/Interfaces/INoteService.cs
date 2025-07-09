using Application.DTOs;

namespace Application.Interfaces;

public interface INoteService
{
    Task<ServiceResponse<IEnumerable<NoteDto>>> GetWorkspaceNotesAsync(int workspaceId, string userId, string traceId);
    Task<ServiceResponse<NoteDto>> GetNoteAsync(int id, string userId, string traceId);
    Task<ServiceResponse<NoteDto>> CreateNoteAsync(CreateNoteDto createNoteDto, string userId, string traceId);
    Task<ServiceResponse<NoteDto>> UpdateNoteAsync(int id, UpdateNoteDto updateNoteDto, string userId, string traceId);
    Task<ServiceResponse<bool>> DeleteNoteAsync(int id, string userId, string traceId);
}