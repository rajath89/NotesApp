using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class NoteRepository : Repository<Note>, INoteRepository
{
    public NoteRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Note>> GetNotesByWorkspaceIdAsync(int workspaceId)
    {
        return await _dbContext.Notes
            .Where(n => n.WorkspaceId == workspaceId)
            .ToListAsync();
    }
}