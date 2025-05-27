using Domain.Entities;

namespace Domain.Interfaces;

public interface INoteRepository : IRepository<Note>
{
    Task<IReadOnlyList<Note>> GetNotesByWorkspaceIdAsync(int workspaceId);
}