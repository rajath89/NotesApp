using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
//[ApiVersion("1.0")]
[Route("NotesApp/api/v1/[controller]")] //"api/v{version:apiVersion}/[controller]"
[Authorize]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;
    private readonly ILogger<WorkspacesController> _logger;

    public WorkspacesController(IWorkspaceService workspaceService, ILogger<WorkspacesController> logger)
    {
        _workspaceService = workspaceService;
        _logger = logger;
    }

    private string GetTraceId() => HttpContext.Items["TraceId"]?.ToString();
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public async Task<ActionResult<WorkspaceResponse>> GetWorkspaces()
    {
        var traceId = GetTraceId();
        var userId = GetUserId();
        
        _logger.LogInformation("[{TraceId}] GetWorkspaces request received for user {UserId}", traceId, userId);
        
        var result = await _workspaceService.GetUserWorkspacesAsync(userId, traceId);
        
        if (result.Success)
        {
            _logger.LogInformation("[{TraceId}] GetWorkspaces successful for user {UserId}", traceId, userId);
            return Ok(new WorkspaceResponse
            {
                Status = 0,
                Workspaces = result.Data
            });
        }
        
        _logger.LogWarning("[{TraceId}] GetWorkspaces failed for user {UserId}: {Error}", 
            traceId, userId, result.ErrorMessage);
        
        return Ok(new WorkspaceResponse
        {
            Status = -1,
            ErrorInfo = new ErrorInfo
            {
                Code = result.ErrorCode,
                Description = "Failed to retrieve workspaces"
            }
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkspaceResponse>> GetWorkspace(int id)
    {
        var traceId = GetTraceId();
        var userId = GetUserId();
        
        _logger.LogInformation("[{TraceId}] GetWorkspace request received for workspace {WorkspaceId} and user {UserId}", 
            traceId, id, userId);
        
        var result = await _workspaceService.GetWorkspaceAsync(id, userId, traceId);
        
        if (result.Success)
        {
            _logger.LogInformation("[{TraceId}] GetWorkspace successful for workspace {WorkspaceId} and user {UserId}", 
                traceId, id, userId);
            
            return Ok(new WorkspaceResponse
            {
                Status = 0,
                Workspaces = new List<WorkspaceDto> { result.Data }
            });
        }
        
        _logger.LogWarning("[{TraceId}] GetWorkspace failed for workspace {WorkspaceId} and user {UserId}: {Error}", 
            traceId, id, userId, result.ErrorMessage);
        
        return Ok(new WorkspaceResponse
        {
            Status = -1,
            ErrorInfo = new ErrorInfo
            {
                Code = result.ErrorCode,
                Description = "Failed to retrieve workspaces"
            }
        });
    }

    [HttpPost]
    public async Task<ActionResult<WorkspaceOperationResponse>> CreateWorkspace(CreateWorkspaceDto createWorkspaceDto)
    {
        var traceId = GetTraceId();
        var userId = GetUserId();
        
        _logger.LogInformation("[{TraceId}] CreateWorkspace request received for user {UserId}", traceId, userId);
        
        var result = await _workspaceService.CreateWorkspaceAsync(createWorkspaceDto, userId, traceId);
        
        if (result.Success)
        {
            _logger.LogInformation("[{TraceId}] CreateWorkspace successful for user {UserId}", traceId, userId);
            return Ok(new WorkspaceOperationResponse
            {
                Status = 0
            });
        }
        
        _logger.LogWarning("[{TraceId}] CreateWorkspace failed for user {UserId}: {Error}", 
            traceId, userId, result.ErrorMessage);
        
        return Ok(new WorkspaceOperationResponse
        {
            Status = -1,
            ErrorInfo = new ErrorInfo
            {
                Code = result.ErrorCode,
                Description = "Failed to create workspaces"
            }
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WorkspaceOperationResponse>> UpdateWorkspace(int id, UpdateWorkspaceDto updateWorkspaceDto)
    {
        var traceId = GetTraceId();
        var userId = GetUserId();
        
        _logger.LogInformation("[{TraceId}] UpdateWorkspace request received for workspace {WorkspaceId} and user {UserId}", 
            traceId, id, userId);
        
        var result = await _workspaceService.UpdateWorkspaceAsync(id, updateWorkspaceDto, userId, traceId);
        
        if (result.Success)
        {
            _logger.LogInformation("[{TraceId}] UpdateWorkspace successful for workspace {WorkspaceId} and user {UserId}", 
                traceId, id, userId);
            
            return Ok(new WorkspaceOperationResponse
            {
                Status = 0
            });
        }
        
        _logger.LogWarning("[{TraceId}] UpdateWorkspace failed for workspace {WorkspaceId} and user {UserId}: {Error}", 
            traceId, id, userId, result.ErrorMessage);
        
        return Ok(new WorkspaceOperationResponse
        {
            Status = -1,
            ErrorInfo = new ErrorInfo
            {
                Code = result.ErrorCode,
                Description = "Failed to update workspace"
            }
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<WorkspaceOperationResponse>> DeleteWorkspace(int id)
    {
        var traceId = GetTraceId();
        var userId = GetUserId();
        
        _logger.LogInformation("[{TraceId}] DeleteWorkspace request received for workspace {WorkspaceId} and user {UserId}", 
            traceId, id, userId);
        
        var result = await _workspaceService.DeleteWorkspaceAsync(id, userId, traceId);
        
        if (result.Success)
        {
            _logger.LogInformation("[{TraceId}] DeleteWorkspace successful for workspace {WorkspaceId} and user {UserId}", 
                traceId, id, userId);
            
            return Ok(new WorkspaceOperationResponse
            {
                Status = 0
            });
        }
        
        _logger.LogWarning("[{TraceId}] DeleteWorkspace failed for workspace {WorkspaceId} and user {UserId}: {Error}", 
            traceId, id, userId, result.ErrorMessage);
        
        return Ok(new WorkspaceOperationResponse
        {
            Status = -1,
            ErrorInfo = new ErrorInfo
            {
                Code = result.ErrorCode,
                Description = "Failed to update workspace" // Note: As per your requirement, delete failure shows "update workspace"
            }
        });
    }
}