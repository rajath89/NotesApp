using Application.DTOs;

namespace Application.Interfaces;

public interface IWorkspaceService
{
    Task<ServiceResponse<IEnumerable<WorkspaceDto>>> GetUserWorkspacesAsync(string userId, string traceId);
    Task<ServiceResponse<WorkspaceDto>> GetWorkspaceAsync(int id, string userId, string traceId);
    Task<ServiceResponse<WorkspaceDto>> CreateWorkspaceAsync(CreateWorkspaceDto createWorkspaceDto, string userId, string traceId);
    Task<ServiceResponse<WorkspaceDto>> UpdateWorkspaceAsync(int id, UpdateWorkspaceDto updateWorkspaceDto, string userId, string traceId);
    Task<ServiceResponse<bool>> DeleteWorkspaceAsync(int id, string userId, string traceId);
}