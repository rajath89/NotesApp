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
    private readonly ICacheService _cacheService;

    public WorkspaceService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<WorkspaceService> logger,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<WorkspaceDto>> GetUserWorkspacesAsync(string userId, string traceId)
    {
        _logger.LogInformation("[{TraceId}] Getting workspaces for user {UserId}", traceId, userId);

        // Try to get from cache first
        string cacheKey = $"{traceId}_user_{userId}_workspaces";
        if (_cacheService.TryGetValue<IEnumerable<WorkspaceDto>>(cacheKey, out var cachedWorkspaces))
        {
            _logger.LogInformation("[{TraceId}] Returning cached workspaces for user {UserId}", traceId, userId);
            return cachedWorkspaces;
        }

        // Get from database
        var workspaces = await _unitOfWork.WorkspaceRepository.GetWorkspacesByUserIdAsync(userId);
        var workspaceDtos = _mapper.Map<IEnumerable<WorkspaceDto>>(workspaces);

        // Store in cache
        _cacheService.Set(cacheKey, workspaceDtos);

        return workspaceDtos;
    }

    public async Task<WorkspaceDto> GetWorkspaceAsync(int id, string userId, string traceId)
    {
        _logger.LogInformation("[{TraceId}] Getting workspace {WorkspaceId} for user {UserId}", traceId, id, userId);

        // Try to get from cache first
        string cacheKey = $"{traceId}_workspace_{id}";
        if (_cacheService.TryGetValue<WorkspaceDto>(cacheKey, out var cachedWorkspace))
        {
            _logger.LogInformation("[{TraceId}] Returning cached workspace {WorkspaceId}", traceId, id);
            return cachedWorkspace;
        }

        var workspace = await _unitOfWork.WorkspaceRepository.GetWorkspaceWithNotesAsync(id);

        if (workspace == null || workspace.UserId != userId)
        {
            _logger.LogWarning("[{TraceId}] Workspace {WorkspaceId} not found or not owned by user {UserId}", traceId,
                id, userId);
            return null;
        }

        var workspaceDto = _mapper.Map<WorkspaceDto>(workspace);

        // Store in cache
        _cacheService.Set(cacheKey, workspaceDto);

        return workspaceDto;
    }

    public async Task<WorkspaceDto> CreateWorkspaceAsync(CreateWorkspaceDto createWorkspaceDto, string userId,
        string traceId)
    {
        _logger.LogInformation("[{TraceId}] Creating workspace for user {UserId}", traceId, userId);

        var workspace = new Workspace
        {
            Name = createWorkspaceDto.Name,
            Description = createWorkspaceDto.Description,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.WorkspaceRepository.AddAsync(workspace);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache
        string cacheKey = $"{traceId}_user_{userId}_workspaces";
        _cacheService.Remove(cacheKey);

        return _mapper.Map<WorkspaceDto>(workspace);
    }

    public async Task<WorkspaceDto> UpdateWorkspaceAsync(int id, UpdateWorkspaceDto updateWorkspaceDto, string userId,
        string traceId)
    {
        _logger.LogInformation("[{TraceId}] Updating workspace {WorkspaceId} for user {UserId}", traceId, id, userId);

        var workspace = await _unitOfWork.WorkspaceRepository.GetByIdAsync(id);

        if (workspace == null || workspace.UserId != userId)
        {
            _logger.LogWarning("[{TraceId}] Workspace {WorkspaceId} not found or not owned by user {UserId}", traceId,
                id, userId);
            return null;
        }

        workspace.Name = updateWorkspaceDto.Name;
        workspace.Description = updateWorkspaceDto.Description;
        workspace.ModifiedAt = DateTime.UtcNow;

        await _unitOfWork.WorkspaceRepository.UpdateAsync(workspace);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate caches
        _cacheService.Remove($"{traceId}_user_{userId}_workspaces");
        _cacheService.Remove($"{traceId}_workspace_{id}");

        return _mapper.Map<WorkspaceDto>(workspace);
    }

    public async Task DeleteWorkspaceAsync(int id, string userId, string traceId)
    {
        _logger.LogInformation("[{TraceId}] Deleting workspace {WorkspaceId} for user {UserId}", traceId, id, userId);

        var workspace = await _unitOfWork.WorkspaceRepository.GetByIdAsync(id);

        if (workspace == null || workspace.UserId != userId)
        {
            _logger.LogWarning("[{TraceId}] Workspace {WorkspaceId} not found or not owned by user {UserId}", traceId,
                id, userId);
            return;
        }

        await _unitOfWork.WorkspaceRepository.DeleteAsync(workspace);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate caches
        _cacheService.Remove($"{traceId}_user_{userId}_workspaces");
        _cacheService.Remove($"{traceId}_workspace_{id}");
    }

}