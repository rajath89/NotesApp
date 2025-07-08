using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class GlobalWorkspaceCache : IGlobalWorkspaceCache
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<GlobalWorkspaceCache> _logger;
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(30);

    public GlobalWorkspaceCache(ICacheService cacheService, ILogger<GlobalWorkspaceCache> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    // Cache Key Generation Methods
    private string GetUserWorkspacesKey(string userId) => $"user:{userId}:workspaces";
    private string GetWorkspaceKey(int workspaceId) => $"workspace:{workspaceId}";
    private string GetWorkspaceNotesKey(int workspaceId) => $"workspace:{workspaceId}:notes";
    private string GetNoteKey(int noteId) => $"note:{noteId}";

    // Get Operations
    public async Task<IEnumerable<WorkspaceDto>> GetUserWorkspacesAsync(string userId)
    {
        var cacheKey = GetUserWorkspacesKey(userId);
        if (_cacheService.TryGetValue<IEnumerable<WorkspaceDto>>(cacheKey, out var cachedWorkspaces))
        {
            _logger.LogDebug("Cache hit for user workspaces: {UserId}", userId);
            return cachedWorkspaces;
        }

        _logger.LogDebug("Cache miss for user workspaces: {UserId}", userId);
        return null;
    }

    public async Task<WorkspaceDto> GetWorkspaceAsync(int workspaceId, string userId)
    {
        var cacheKey = GetWorkspaceKey(workspaceId);
        if (_cacheService.TryGetValue<WorkspaceDto>(cacheKey, out var cachedWorkspace))
        {
            // if (cachedWorkspace.UserId == userId)
            // {
            //     _logger.LogDebug("Cache hit for workspace: {WorkspaceId}", workspaceId);
            //     return cachedWorkspace;
            // }
            // else
            // {
            //     _logger.LogWarning("Cache hit but workspace {WorkspaceId} doesn't belong to user {UserId}",
            //         workspaceId, userId);
            //     return null;
            // }
            return cachedWorkspace;
        }

        _logger.LogDebug("Cache miss for workspace: {WorkspaceId}", workspaceId);
        return null;
    }

    public async Task<IEnumerable<NoteDto>> GetWorkspaceNotesAsync(int workspaceId, string userId)
    {
        var cacheKey = GetWorkspaceNotesKey(workspaceId);
        if (_cacheService.TryGetValue<IEnumerable<NoteDto>>(cacheKey, out var cachedNotes))
        {
            _logger.LogDebug("Cache hit for workspace notes: {WorkspaceId}", workspaceId);
            return cachedNotes;
        }

        _logger.LogDebug("Cache miss for workspace notes: {WorkspaceId}", workspaceId);
        return null;
    }

    public async Task<NoteDto> GetNoteAsync(int noteId, string userId)
    {
        var cacheKey = GetNoteKey(noteId);
        if (_cacheService.TryGetValue<NoteDto>(cacheKey, out var cachedNote))
        {
            _logger.LogDebug("Cache hit for note: {NoteId}", noteId);
            return cachedNote;
        }

        _logger.LogDebug("Cache miss for note: {NoteId}", noteId);
        return null;
    }

    // Set Operations
    public async Task SetUserWorkspacesAsync(string userId, IEnumerable<WorkspaceDto> workspaces)
    {
        var cacheKey = GetUserWorkspacesKey(userId);
        _cacheService.Set(cacheKey, workspaces, _defaultTtl);
        _logger.LogDebug("Cached user workspaces for user: {UserId}", userId);
    }

    public async Task SetWorkspaceAsync(WorkspaceDto workspace)
    {
        var cacheKey = GetWorkspaceKey(workspace.Id);
        _cacheService.Set(cacheKey, workspace, _defaultTtl);
        _logger.LogDebug("Cached workspace: {WorkspaceId}", workspace.Id);
    }

    public async Task SetWorkspaceNotesAsync(int workspaceId, IEnumerable<NoteDto> notes)
    {
        var cacheKey = GetWorkspaceNotesKey(workspaceId);
        _cacheService.Set(cacheKey, notes, _defaultTtl);
        _logger.LogDebug("Cached workspace notes: {WorkspaceId}", workspaceId);
    }

    public async Task SetNoteAsync(NoteDto note)
    {
        var cacheKey = GetNoteKey(note.Id);
        _cacheService.Set(cacheKey, note, _defaultTtl);
        _logger.LogDebug("Cached note: {NoteId}", note.Id);
    }

    // Invalidation Operations
    public async Task InvalidateUserWorkspacesAsync(string userId)
    {
        var cacheKey = GetUserWorkspacesKey(userId);
        _cacheService.Remove(cacheKey);
        _logger.LogDebug("Invalidated user workspaces cache for user: {UserId}", userId);
    }

    public async Task InvalidateWorkspaceAsync(int workspaceId)
    {
        var cacheKey = GetWorkspaceKey(workspaceId);
        _cacheService.Remove(cacheKey);
        _logger.LogDebug("Invalidated workspace cache: {WorkspaceId}", workspaceId);
    }

    public async Task InvalidateWorkspaceNotesAsync(int workspaceId)
    {
        var cacheKey = GetWorkspaceNotesKey(workspaceId);
        _cacheService.Remove(cacheKey);
        _logger.LogDebug("Invalidated workspace notes cache: {WorkspaceId}", workspaceId);
    }

    public async Task InvalidateNoteAsync(int noteId)
    {
        var cacheKey = GetNoteKey(noteId);
        _cacheService.Remove(cacheKey);
        _logger.LogDebug("Invalidated note cache: {NoteId}", noteId);
    }

    public async Task InvalidateAllUserDataAsync(string userId)
    {
        _cacheService.RemoveByPattern($"user:{userId}:");
        _logger.LogDebug("Invalidated all cache data for user: {UserId}", userId);
    }
}