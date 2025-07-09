using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class NoteService : INoteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<NoteService> _logger;
    private readonly IGlobalWorkspaceCache _globalCache;

    public NoteService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<NoteService> logger,
        IGlobalWorkspaceCache globalCache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _globalCache = globalCache;
    }

    public async Task<ServiceResponse<IEnumerable<NoteDto>>> GetWorkspaceNotesAsync(int workspaceId, string userId, string traceId)
    {
        try
        {
            _logger.LogInformation("[{TraceId}] Getting notes for workspace {WorkspaceId}", traceId, workspaceId);

            // Verify workspace belongs to user first
            var workspace = await _unitOfWork.WorkspaceRepository.GetByIdAsync(workspaceId);
            if (workspace == null || workspace.UserId != userId)
            {
                _logger.LogWarning("[{TraceId}] Workspace {WorkspaceId} not found or not owned by user {UserId}", 
                    traceId, workspaceId, userId);
                return ServiceResponse<IEnumerable<NoteDto>>.FailureResponse("Workspace not found or access denied");
            }

            // Try cache first
            var cachedNotes = await _globalCache.GetWorkspaceNotesAsync(workspaceId, userId);
            if (cachedNotes != null)
            {
                _logger.LogInformation("[{TraceId}] Returning cached notes for workspace {WorkspaceId}", 
                    traceId, workspaceId);
                return ServiceResponse<IEnumerable<NoteDto>>.SuccessResponse(cachedNotes);
            }

            // Fetch from database
            var notes = await _unitOfWork.NoteRepository.GetNotesByWorkspaceIdAsync(workspaceId);
            var noteDtos = _mapper.Map<IEnumerable<NoteDto>>(notes);

            // Update cache
            await _globalCache.SetWorkspaceNotesAsync(workspaceId, noteDtos);

            _logger.LogInformation("[{TraceId}] Successfully retrieved {Count} notes for workspace {WorkspaceId}", 
                traceId, noteDtos.Count(), workspaceId);

            return ServiceResponse<IEnumerable<NoteDto>>.SuccessResponse(noteDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Error getting notes for workspace {WorkspaceId}", traceId, workspaceId);
            return ServiceResponse<IEnumerable<NoteDto>>.FailureResponse("Failed to retrieve notes");
        }
    }

    public async Task<ServiceResponse<NoteDto>> GetNoteAsync(int id, string userId, string traceId)
    {
        try
        {
            _logger.LogInformation("[{TraceId}] Getting note {NoteId}", traceId, id);

            // Try cache first
            var cachedNote = await _globalCache.GetNoteAsync(id, userId);
            if (cachedNote != null)
            {
                _logger.LogInformation("[{TraceId}] Returning cached note {NoteId}", traceId, id);
                return ServiceResponse<NoteDto>.SuccessResponse(cachedNote);
            }

            // Fetch from database
            var note = await _unitOfWork.NoteRepository.GetByIdAsync(id);
            if (note == null)
            {
                _logger.LogWarning("[{TraceId}] Note {NoteId} not found", traceId, id);
                return ServiceResponse<NoteDto>.FailureResponse("Note not found");
            }

            // Verify the note belongs to a workspace owned by the user
            var workspace = await _unitOfWork.WorkspaceRepository.GetByIdAsync(note.WorkspaceId);
            if (workspace == null || workspace.UserId != userId)
            {
                _logger.LogWarning("[{TraceId}] Note {NoteId} belongs to workspace not owned by user {UserId}", 
                    traceId, id, userId);
                return ServiceResponse<NoteDto>.FailureResponse("Note not found or access denied");
            }

            var noteDto = _mapper.Map<NoteDto>(note);

            // Update cache
            await _globalCache.SetNoteAsync(noteDto);

            _logger.LogInformation("[{TraceId}] Successfully retrieved note {NoteId}", traceId, id);
            return ServiceResponse<NoteDto>.SuccessResponse(noteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Error getting note {NoteId}", traceId, id);
            return ServiceResponse<NoteDto>.FailureResponse("Failed to retrieve note");
        }
    }

    public async Task<ServiceResponse<NoteDto>> CreateNoteAsync(CreateNoteDto createNoteDto, string userId, string traceId)
    {
        try
        {
            _logger.LogInformation("[{TraceId}] Creating note in workspace {WorkspaceId}", 
                traceId, createNoteDto.WorkspaceId);

            // Verify workspace belongs to user
            var workspace = await _unitOfWork.WorkspaceRepository.GetByIdAsync(createNoteDto.WorkspaceId);
            if (workspace == null || workspace.UserId != userId)
            {
                _logger.LogWarning("[{TraceId}] Workspace {WorkspaceId} not found or not owned by user {UserId}", 
                    traceId, createNoteDto.WorkspaceId, userId);
                return ServiceResponse<NoteDto>.FailureResponse("Invalid workspace ID or workspace not owned by user");
            }

            var note = new Note
            {
                Title = createNoteDto.Title,
                Content = createNoteDto.Content,
                WorkspaceId = createNoteDto.WorkspaceId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.NoteRepository.AddAsync(note);
            await _unitOfWork.SaveChangesAsync();

            var noteDto = _mapper.Map<NoteDto>(note);

            // Update cache with new note
            await _globalCache.SetNoteAsync(noteDto);

            // Invalidate workspace notes list (needs to include new note)
            await _globalCache.InvalidateWorkspaceNotesAsync(createNoteDto.WorkspaceId);

            // Also invalidate workspace cache if it includes note count
            await _globalCache.InvalidateWorkspaceAsync(createNoteDto.WorkspaceId);

            _logger.LogInformation("[{TraceId}] Successfully created note {NoteId} in workspace {WorkspaceId}", 
                traceId, note.Id, createNoteDto.WorkspaceId);

            return ServiceResponse<NoteDto>.SuccessResponse(noteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Error creating note in workspace {WorkspaceId}", 
                traceId, createNoteDto.WorkspaceId);
            return ServiceResponse<NoteDto>.FailureResponse("Failed to create note");
        }
    }

    public async Task<ServiceResponse<NoteDto>> UpdateNoteAsync(int id, UpdateNoteDto updateNoteDto, string userId, string traceId)
    {
        try
        {
            _logger.LogInformation("[{TraceId}] Updating note {NoteId}", traceId, id);

            var note = await _unitOfWork.NoteRepository.GetByIdAsync(id);
            if (note == null)
            {
                _logger.LogWarning("[{TraceId}] Note {NoteId} not found", traceId, id);
                return ServiceResponse<NoteDto>.FailureResponse("Note not found");
            }

            // Verify the note belongs to a workspace owned by the user
            var workspace = await _unitOfWork.WorkspaceRepository.GetByIdAsync(note.WorkspaceId);
            if (workspace == null || workspace.UserId != userId)
            {
                _logger.LogWarning("[{TraceId}] Note {NoteId} belongs to workspace not owned by user {UserId}", 
                    traceId, id, userId);
                return ServiceResponse<NoteDto>.FailureResponse("Note not found or access denied");
            }

            note.Title = updateNoteDto.Title ?? note.Title;
            note.Content = updateNoteDto.Content ?? note.Content;
            note.ModifiedAt = DateTime.UtcNow;

            await _unitOfWork.NoteRepository.UpdateAsync(note);
            await _unitOfWork.SaveChangesAsync();

            var noteDto = _mapper.Map<NoteDto>(note);

            // Update cache with modified note
            await _globalCache.SetNoteAsync(noteDto);

            // Invalidate workspace notes list (titles might have changed)
            await _globalCache.InvalidateWorkspaceNotesAsync(note.WorkspaceId);

            _logger.LogInformation("[{TraceId}] Successfully updated note {NoteId}", traceId, id);
            return ServiceResponse<NoteDto>.SuccessResponse(noteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Error updating note {NoteId}", traceId, id);
            return ServiceResponse<NoteDto>.FailureResponse("Failed to update note");
        }
    }

    public async Task<ServiceResponse<bool>> DeleteNoteAsync(int id, string userId, string traceId)
    {
        try
        {
            _logger.LogInformation("[{TraceId}] Deleting note {NoteId}", traceId, id);

            var note = await _unitOfWork.NoteRepository.GetByIdAsync(id);
            if (note == null)
            {
                _logger.LogWarning("[{TraceId}] Note {NoteId} not found", traceId, id);
                return ServiceResponse<bool>.FailureResponse("Note not found");
            }

            // Verify the note belongs to a workspace owned by the user
            var workspace = await _unitOfWork.WorkspaceRepository.GetByIdAsync(note.WorkspaceId);
            if (workspace == null || workspace.UserId != userId)
            {
                _logger.LogWarning("[{TraceId}] Note {NoteId} belongs to workspace not owned by user {UserId}", 
                    traceId, id, userId);
                return ServiceResponse<bool>.FailureResponse("Note not found or access denied");
            }

            int workspaceId = note.WorkspaceId;

            await _unitOfWork.NoteRepository.DeleteAsync(note);
            await _unitOfWork.SaveChangesAsync();

            // Remove from cache
            await _globalCache.InvalidateNoteAsync(id);
            await _globalCache.InvalidateWorkspaceNotesAsync(workspaceId);

            // Also invalidate workspace cache if it includes note count
            await _globalCache.InvalidateWorkspaceAsync(workspaceId);

            _logger.LogInformation("[{TraceId}] Successfully deleted note {NoteId}", traceId, id);
            return ServiceResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Error deleting note {NoteId}", traceId, id);
            return ServiceResponse<bool>.FailureResponse("Failed to delete note");
        }
    }
}