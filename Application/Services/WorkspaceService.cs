using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<WorkspaceService> _logger;
    private readonly IGlobalWorkspaceCache _globalCache;

    public WorkspaceService(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ILogger<WorkspaceService> logger, 
        IGlobalWorkspaceCache globalCache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _globalCache = globalCache;
    }

    public async Task<ServiceResponse<IEnumerable<WorkspaceDto>>> GetUserWorkspacesAsync(string userId, string traceId)
    {
        try
        {
            _logger.LogInformation("[{TraceId}] Getting workspaces for user {UserId}", traceId, userId);

            // Try cache first
            var cachedWorkspaces = await _globalCache.GetUserWorkspacesAsync(userId);
            if (cachedWorkspaces != null)
            {
                _logger.LogInformation("[{TraceId}] Returning cached workspaces for user {UserId}", traceId, userId);
                return ServiceResponse<IEnumerable<WorkspaceDto>>.SuccessResponse(cachedWorkspaces);
            }

            // Fetch from database
            var workspaces = await _unitOfWork.WorkspaceRepository.GetWorkspacesByUserIdAsync(userId);
            var workspaceDtos = _mapper.Map<IEnumerable<WorkspaceDto>>(workspaces);

            // Update cache
            await _globalCache.SetUserWorkspacesAsync(userId, workspaceDtos);

            _logger.LogInformation("[{TraceId}] Successfully retrieved {Count} workspaces for user {UserId}", 
                traceId, workspaceDtos.Count(), userId);

            return ServiceResponse<IEnumerable<WorkspaceDto>>.SuccessResponse(workspaceDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Error getting workspaces for user {UserId}", traceId, userId);
            return ServiceResponse<IEnumerable<WorkspaceDto>>.FailureResponse("Failed to retrieve workspaces");
        }
    }

    public async Task<ServiceResponse<WorkspaceDto>> GetWorkspaceAsync(int id, string userId, string traceId)
    {
        try
        {
            _logger.LogInformation("[{TraceId}] Getting workspace {WorkspaceId} for user {UserId}", traceId, id, userId);

            // Try cache first
            var cachedWorkspace = await _globalCache.GetWorkspaceAsync(id, userId);
            if (cachedWorkspace != null)
            {
                _logger.LogInformation("[{TraceId}] Returning cached workspace {WorkspaceId}", traceId, id);
                return ServiceResponse<WorkspaceDto>.SuccessResponse(cachedWorkspace);
            }

            // Fetch from database
            var workspace = await _unitOfWork.WorkspaceRepository.GetWorkspaceWithNotesAsync(id);
            if (workspace == null)
            {
                _logger.LogWarning("[{TraceId}] Workspace {WorkspaceId} not found", traceId, id);
                return ServiceResponse<WorkspaceDto>.FailureResponse("Workspace not found");
            }

            if (workspace.UserId != userId)
            {
                _logger.LogWarning("[{TraceId}] Workspace {WorkspaceId} not owned by user {UserId}", traceId, id, userId);
                return ServiceResponse<WorkspaceDto>.FailureResponse("Workspace not found or access denied");
            }

            var workspaceDto = _mapper.Map<WorkspaceDto>(workspace);
            
            // Update cache
            await _globalCache.SetWorkspaceAsync(workspaceDto);

            _logger.LogInformation("[{TraceId}] Successfully retrieved workspace {WorkspaceId} for user {UserId}", 
                traceId, id, userId);

            return ServiceResponse<WorkspaceDto>.SuccessResponse(workspaceDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Error getting workspace {WorkspaceId} for user {UserId}", 
                traceId, id, userId);
            return ServiceResponse<WorkspaceDto>.FailureResponse("Failed to retrieve workspace");
        }
    }

    public async Task<ServiceResponse<WorkspaceDto>> CreateWorkspaceAsync(CreateWorkspaceDto createWorkspaceDto, string userId, string traceId)
    {
        try
        {
            _logger.LogInformation("[{TraceId}] Creating workspace for user {UserId}", traceId, userId);

            var workspace = new Workspace
            {
                Name = createWorkspaceDto.Name,
                Description = createWorkspaceDto.Description,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Notes = new List<Note>()
            };

            await _unitOfWork.WorkspaceRepository.AddAsync(workspace);
            await _unitOfWork.SaveChangesAsync();

            var workspaceDto = _mapper.Map<WorkspaceDto>(workspace);

            // Update cache with new workspace
            await _globalCache.SetWorkspaceAsync(workspaceDto);

            // Invalidate user workspaces list (needs to include new workspace)
            await _globalCache.InvalidateUserWorkspacesAsync(userId);

            _logger.LogInformation("[{TraceId}] Successfully created workspace {WorkspaceId} for user {UserId}", 
                traceId, workspace.Id, userId);

            return ServiceResponse<WorkspaceDto>.SuccessResponse(workspaceDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Error creating workspace for user {UserId}", traceId, userId);
            return ServiceResponse<WorkspaceDto>.FailureResponse("Failed to create workspace");
        }
    }

    public async Task<ServiceResponse<WorkspaceDto>> UpdateWorkspaceAsync(int id, UpdateWorkspaceDto updateWorkspaceDto, string userId, string traceId)
    {
        try
        {
            _logger.LogInformation("[{TraceId}] Updating workspace {WorkspaceId} for user {UserId}", traceId, id, userId);

            var workspace = await _unitOfWork.WorkspaceRepository.GetByIdAsync(id);
            if (workspace == null)
            {
                _logger.LogWarning("[{TraceId}] Workspace {WorkspaceId} not found", traceId, id);
                return ServiceResponse<WorkspaceDto>.FailureResponse("Workspace not found");
            }

            if (workspace.UserId != userId)
            {
                _logger.LogWarning("[{TraceId}] Workspace {WorkspaceId} not owned by user {UserId}", traceId, id, userId);
                return ServiceResponse<WorkspaceDto>.FailureResponse("Workspace not found or access denied");
            }

            workspace.Name = updateWorkspaceDto.Name ?? workspace.Name;
            workspace.Description = updateWorkspaceDto.Description ?? workspace.Description;
            workspace.ModifiedAt = DateTime.UtcNow;

            await _unitOfWork.WorkspaceRepository.UpdateAsync(workspace);
            await _unitOfWork.SaveChangesAsync();

            var workspaceDto = _mapper.Map<WorkspaceDto>(workspace);

            // Update cache with modified workspace
            await _globalCache.SetWorkspaceAsync(workspaceDto);

            // Invalidate user workspaces list (workspace names might have changed)
            await _globalCache.InvalidateUserWorkspacesAsync(userId);

            _logger.LogInformation("[{TraceId}] Successfully updated workspace {WorkspaceId} for user {UserId}", 
                traceId, id, userId);

            return ServiceResponse<WorkspaceDto>.SuccessResponse(workspaceDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Error updating workspace {WorkspaceId} for user {UserId}", 
                traceId, id, userId);
            return ServiceResponse<WorkspaceDto>.FailureResponse("Failed to update workspace");
        }
    }

    public async Task<ServiceResponse<bool>> DeleteWorkspaceAsync(int id, string userId, string traceId)
    {
        try
        {
            _logger.LogInformation("[{TraceId}] Deleting workspace {WorkspaceId} for user {UserId}", traceId, id, userId);

            var workspace = await _unitOfWork.WorkspaceRepository.GetByIdAsync(id);
            if (workspace == null)
            {
                _logger.LogWarning("[{TraceId}] Workspace {WorkspaceId} not found", traceId, id);
                return ServiceResponse<bool>.FailureResponse("Workspace not found");
            }

            if (workspace.UserId != userId)
            {
                _logger.LogWarning("[{TraceId}] Workspace {WorkspaceId} not owned by user {UserId}", traceId, id, userId);
                return ServiceResponse<bool>.FailureResponse("Workspace not found or access denied");
            }

            await _unitOfWork.WorkspaceRepository.DeleteAsync(workspace);
            await _unitOfWork.SaveChangesAsync();

            // Remove from cache
            await _globalCache.InvalidateWorkspaceAsync(id);
            await _globalCache.InvalidateUserWorkspacesAsync(userId);

            // Also invalidate workspace notes since the workspace is being deleted
            await _globalCache.InvalidateWorkspaceNotesAsync(id);

            _logger.LogInformation("[{TraceId}] Successfully deleted workspace {WorkspaceId} for user {UserId}", 
                traceId, id, userId);

            return ServiceResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Error deleting workspace {WorkspaceId} for user {UserId}", 
                traceId, id, userId);
            return ServiceResponse<bool>.FailureResponse("Failed to delete workspace");
        }
    }
}