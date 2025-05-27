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
        private readonly ICacheService _cacheService;

        public NoteService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<NoteService> logger,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<NoteDto>> GetWorkspaceNotesAsync(int workspaceId, string userId, string traceId)
        {
            _logger.LogInformation("[{TraceId}] Getting notes for workspace {WorkspaceId}", traceId, workspaceId);

            // Verify workspace belongs to user
            var workspace = await _unitOfWork.WorkspaceRepository.GetByIdAsync(workspaceId);
            if (workspace == null || workspace.UserId != userId)
            {
                _logger.LogWarning("[{TraceId}] Workspace {WorkspaceId} not found or not owned by user {UserId}", traceId, workspaceId, userId);
                return Enumerable.Empty<NoteDto>();
            }

            // Try to get from cache first
            string cacheKey = $"{traceId}_workspace_{workspaceId}_notes";
            if (_cacheService.TryGetValue<IEnumerable<NoteDto>>(cacheKey, out var cachedNotes))
            {
                _logger.LogInformation("[{TraceId}] Returning cached notes for workspace {WorkspaceId}", traceId, workspaceId);
                return cachedNotes;
            }

            var notes = await _unitOfWork.NoteRepository.GetNotesByWorkspaceIdAsync(workspaceId);
            var noteDtos = _mapper.Map<IEnumerable<NoteDto>>(notes);

            // Store in cache
            _cacheService.Set(cacheKey, noteDtos);

            return noteDtos;
        }

        public async Task<NoteDto> GetNoteAsync(int id, string userId, string traceId)
        {
            _logger.LogInformation("[{TraceId}] Getting note {NoteId}", traceId, id);

            // Try to get from cache first
            string cacheKey = $"{traceId}_note_{id}";
            if (_cacheService.TryGetValue<NoteDto>(cacheKey, out var cachedNote))
            {
                _logger.LogInformation("[{TraceId}] Returning cached note {NoteId}", traceId, id);
                return cachedNote;
            }

            var note = await _unitOfWork.NoteRepository.GetByIdAsync(id);

            if (note == null)
            {
                _logger.LogWarning("[{TraceId}] Note {NoteId} not found", traceId, id);
                return null;
            }

            // Verify the note belongs to a workspace owned by the user
            var workspace = await _unitOfWork.WorkspaceRepository.GetByIdAsync(note.WorkspaceId);
            if (workspace == null || workspace.UserId != userId)
            {
                _logger.LogWarning("[{TraceId}] Note {NoteId} belongs to workspace not owned by user {UserId}", traceId, id, userId);
                return null;
            }

            var noteDto = _mapper.Map<NoteDto>(note);

            // Store in cache
            _cacheService.Set(cacheKey, noteDto);

            return noteDto;
        }

        public async Task<NoteDto> CreateNoteAsync(CreateNoteDto createNoteDto, string userId, string traceId)
        {
            _logger.LogInformation("[{TraceId}] Creating note in workspace {WorkspaceId}", traceId, createNoteDto.WorkspaceId);

            // Verify workspace belongs to user
            var workspace = await _unitOfWork.WorkspaceRepository.GetByIdAsync(createNoteDto.WorkspaceId);
            if (workspace == null || workspace.UserId != userId)
            {
                _logger.LogWarning("[{TraceId}] Workspace {WorkspaceId} not found or not owned by user {UserId}", 
                    traceId, createNoteDto.WorkspaceId, userId);
                return null;
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

            // Invalidate caches
            _cacheService.Remove($"{traceId}_workspace_{createNoteDto.WorkspaceId}_notes");
            _cacheService.Remove($"{traceId}_workspace_{createNoteDto.WorkspaceId}");

            return _mapper.Map<NoteDto>(note);
        }

        public async Task<NoteDto> UpdateNoteAsync(int id, UpdateNoteDto updateNoteDto, string userId, string traceId)
        {
            _logger.LogInformation("[{TraceId}] Updating note {NoteId}", traceId, id);

            var note = await _unitOfWork.NoteRepository.GetByIdAsync(id);

            if (note == null)
            {
                _logger.LogWarning("[{TraceId}] Note {NoteId} not found", traceId, id);
                return null;
            }

            // Verify the note belongs to a workspace owned by the user
            var workspace = await _unitOfWork.WorkspaceRepository.GetByIdAsync(note.WorkspaceId);
            if (workspace == null || workspace.UserId != userId)
            {
                _logger.LogWarning("[{TraceId}] Note {NoteId} belongs to workspace not owned by user {UserId}", traceId, id, userId);
                return null;
            }

            note.Title = updateNoteDto.Title;
            note.Content = updateNoteDto.Content;
            note.ModifiedAt = DateTime.UtcNow;

            await _unitOfWork.NoteRepository.UpdateAsync(note);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate caches
            _cacheService.Remove($"{traceId}_note_{id}");
            _cacheService.Remove($"{traceId}_workspace_{note.WorkspaceId}_notes");
            _cacheService.Remove($"{traceId}_workspace_{note.WorkspaceId}");

            return _mapper.Map<NoteDto>(note);
        }

        public async Task DeleteNoteAsync(int id, string userId, string traceId)
        {
            _logger.LogInformation("[{TraceId}] Deleting note {NoteId}", traceId, id);

            var note = await _unitOfWork.NoteRepository.GetByIdAsync(id);

            if (note == null)
            {
                _logger.LogWarning("[{TraceId}] Note {NoteId} not found", traceId, id);
                return;
            }

            // Verify the note belongs to a workspace owned by the user
            var workspace = await _unitOfWork.WorkspaceRepository.GetByIdAsync(note.WorkspaceId);
            if (workspace == null || workspace.UserId != userId)
            {
                _logger.LogWarning("[{TraceId}] Note {NoteId} belongs to workspace not owned by user {UserId}", traceId, id, userId);
                return;
            }

            int workspaceId = note.WorkspaceId;

            await _unitOfWork.NoteRepository.DeleteAsync(note);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate caches
            _cacheService.Remove($"{traceId}_note_{id}");
            _cacheService.Remove($"{traceId}_workspace_{workspaceId}_notes");
            _cacheService.Remove($"{traceId}_workspace_{workspaceId}");
        }
}