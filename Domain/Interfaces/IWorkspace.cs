using Domain.Entities;

namespace Domain.Interfaces;

public interface IWorkspaceRepository : IRepository<Workspace>
{
    Task<IReadOnlyList<Workspace>> GetWorkspacesByUserIdAsync(string userId);
    Task<Workspace> GetWorkspaceWithNotesAsync(int workspaceId);
}