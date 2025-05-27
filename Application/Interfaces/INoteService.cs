using Application.DTOs;

namespace Application.Interfaces;

public interface INoteService
{
    Task<IEnumerable<NoteDto>> GetWorkspaceNotesAsync(int workspaceId, string userId, string traceId);
    Task<NoteDto> GetNoteAsync(int id, string userId, string traceId);
    Task<NoteDto> CreateNoteAsync(CreateNoteDto createNoteDto, string userId, string traceId);
    Task<NoteDto> UpdateNoteAsync(int id, UpdateNoteDto updateNoteDto, string userId, string traceId);
    Task DeleteNoteAsync(int id, string userId, string traceId);
}