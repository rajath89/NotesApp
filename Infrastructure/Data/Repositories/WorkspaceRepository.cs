using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class WorkspaceRepository : Repository<Workspace>, IWorkspaceRepository
{
    public WorkspaceRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Workspace>> GetWorkspacesByUserIdAsync(string userId)
    {
        return await _dbContext.Workspaces
            .Where(w => w.UserId == userId)
            .ToListAsync();
    }

    public async Task<Workspace> GetWorkspaceWithNotesAsync(int workspaceId)
    {
        return await _dbContext.Workspaces
            .Include(w => w.Notes)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);
    }
}