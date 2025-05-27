using Application.DTOs;

namespace Application.Interfaces;

public interface IWorkspaceService
{
    Task<IEnumerable<WorkspaceDto>> GetUserWorkspacesAsync(string userId, string traceId);
    Task<WorkspaceDto> GetWorkspaceAsync(int id, string userId, string traceId);
    Task<WorkspaceDto> CreateWorkspaceAsync(CreateWorkspaceDto createWorkspaceDto, string userId, string traceId);
    Task<WorkspaceDto> UpdateWorkspaceAsync(int id, UpdateWorkspaceDto updateWorkspaceDto, string userId, string traceId);
    Task DeleteWorkspaceAsync(int id, string userId, string traceId);
}