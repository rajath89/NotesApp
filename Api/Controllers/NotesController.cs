using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

    [ApiController]
    //[ApiVersion("1.0")]
    [Route("NotesApp/api/v1/[controller]")]// "api/v{version:apiVersion}/[controller]"
    [Authorize] 
    public class NotesController : ControllerBase 
{
    private readonly INoteService _noteService;
    private readonly ILogger<NotesController> _logger;

    public NotesController(INoteService noteService, ILogger<NotesController> logger)
    {
        _noteService = noteService;
        _logger = logger;
    }

    private string GetTraceId() => HttpContext.Items["TraceId"]?.ToString();
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet("GetWorkspaceNotes")]
    public async Task<ActionResult<NoteResponse>> GetWorkspaceNotes([FromQuery] int workspaceId)
    {
        var traceId = GetTraceId();
        var userId = GetUserId();
        
        _logger.LogInformation("[{TraceId}] GetWorkspaceNotes request received for workspace {WorkspaceId} and user {UserId}", 
            traceId, workspaceId, userId);
        
        var result = await _noteService.GetWorkspaceNotesAsync(workspaceId, userId, traceId);
        
        if (result.Success)
        {
            _logger.LogInformation("[{TraceId}] GetWorkspaceNotes successful for workspace {WorkspaceId} and user {UserId}", 
                traceId, workspaceId, userId);
            return Ok(new NoteResponse
            {
                Status = 0,
                Notes = result.Data
            });
        }
        
        _logger.LogWarning("[{TraceId}] GetWorkspaceNotes failed for workspace {WorkspaceId} and user {UserId}: {Error}", 
            traceId, workspaceId, userId, result.ErrorMessage);
        
        return Ok(new NoteResponse
        {
            Status = -1,
            ErrorInfo = new ErrorInfo
            {
                Code = result.ErrorCode,
                Description = "Failed to retrieve notes"
            }
        });
    }

    [HttpGet("GetNote")]
    public async Task<ActionResult<NoteResponse>> GetNote([FromQuery] int id)
    {
        var traceId = GetTraceId();
        var userId = GetUserId();
        
        _logger.LogInformation("[{TraceId}] GetNote request received for note {NoteId} and user {UserId}", 
            traceId, id, userId);
        
        var result = await _noteService.GetNoteAsync(id, userId, traceId);
        
        if (result.Success)
        {
            _logger.LogInformation("[{TraceId}] GetNote successful for note {NoteId} and user {UserId}", 
                traceId, id, userId);
            
            return Ok(new NoteResponse
            {
                Status = 0,
                Notes = new List<NoteDto> { result.Data }
            });
        }
        
        _logger.LogWarning("[{TraceId}] GetNote failed for note {NoteId} and user {UserId}: {Error}", 
            traceId, id, userId, result.ErrorMessage);
        
        return Ok(new NoteResponse
        {
            Status = -1,
            ErrorInfo = new ErrorInfo
            {
                Code = result.ErrorCode,
                Description = "Failed to retrieve note"
            }
        });
    }

    [HttpPost("CreateNote")]
    public async Task<ActionResult<NoteOperationResponse>> CreateNote(CreateNoteDto createNoteDto)
    {
        var traceId = GetTraceId();
        var userId = GetUserId();
        
        _logger.LogInformation("[{TraceId}] CreateNote request received for user {UserId}", traceId, userId);
        
        var result = await _noteService.CreateNoteAsync(createNoteDto, userId, traceId);
        
        if (result.Success)
        {
            _logger.LogInformation("[{TraceId}] CreateNote successful for user {UserId}", traceId, userId);
            return Ok(new NoteOperationResponse
            {
                Status = 0
            });
        }
        
        _logger.LogWarning("[{TraceId}] CreateNote failed for user {UserId}: {Error}", 
            traceId, userId, result.ErrorMessage);
        
        return Ok(new NoteOperationResponse
        {
            Status = -1,
            ErrorInfo = new ErrorInfo
            {
                Code = result.ErrorCode,
                Description = "Failed to create note"
            }
        });
    }

    [HttpPut("UpdateNote")]
    public async Task<ActionResult<NoteOperationResponse>> UpdateNote([FromQuery] int id, UpdateNoteDto updateNoteDto)
    {
        var traceId = GetTraceId();
        var userId = GetUserId();
        
        _logger.LogInformation("[{TraceId}] UpdateNote request received for note {NoteId} and user {UserId}", 
            traceId, id, userId);
        
        var result = await _noteService.UpdateNoteAsync(id, updateNoteDto, userId, traceId);
        
        if (result.Success)
        {
            _logger.LogInformation("[{TraceId}] UpdateNote successful for note {NoteId} and user {UserId}", 
                traceId, id, userId);
            
            return Ok(new NoteOperationResponse
            {
                Status = 0
            });
        }
        
        _logger.LogWarning("[{TraceId}] UpdateNote failed for note {NoteId} and user {UserId}: {Error}", 
            traceId, id, userId, result.ErrorMessage);
        
        return Ok(new NoteOperationResponse
        {
            Status = -1,
            ErrorInfo = new ErrorInfo
            {
                Code = result.ErrorCode,
                Description = "Failed to update note"
            }
        });
    }

    [HttpDelete("DeleteNote")]
    public async Task<ActionResult<NoteOperationResponse>> DeleteNote([FromQuery] int id)
    {
        var traceId = GetTraceId();
        var userId = GetUserId();
        
        _logger.LogInformation("[{TraceId}] DeleteNote request received for note {NoteId} and user {UserId}", 
            traceId, id, userId);
        
        var result = await _noteService.DeleteNoteAsync(id, userId, traceId);
        
        if (result.Success)
        {
            _logger.LogInformation("[{TraceId}] DeleteNote successful for note {NoteId} and user {UserId}", 
                traceId, id, userId);
            
            return Ok(new NoteOperationResponse
            {
                Status = 0
            });
        }
        
        _logger.LogWarning("[{TraceId}] DeleteNote failed for note {NoteId} and user {UserId}: {Error}", 
            traceId, id, userId, result.ErrorMessage);
        
        return Ok(new NoteOperationResponse
        {
            Status = -1,
            ErrorInfo = new ErrorInfo
            {
                Code = result.ErrorCode,
                Description = "Failed to delete note"
            }
        });
    }
}