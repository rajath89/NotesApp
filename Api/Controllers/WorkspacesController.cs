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

    public WorkspacesController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    private string GetTraceId() => HttpContext.Items["TraceId"]?.ToString();
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkspaceDto>>> GetWorkspaces()
    {
        var workspaces = await _workspaceService.GetUserWorkspacesAsync(GetUserId(), GetTraceId());
        return Ok(workspaces);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkspaceDto>> GetWorkspace(int id)
    {
        var workspace = await _workspaceService.GetWorkspaceAsync(id, GetUserId(), GetTraceId());

        if (workspace == null)
            return NotFound();

        return Ok(workspace);
    }

    [HttpPost("createWorkspace")]
    public async Task<ActionResult<WorkspaceDto>> CreateWorkspace(CreateWorkspaceDto createWorkspaceDto)
    {
        var workspace = await _workspaceService.CreateWorkspaceAsync(createWorkspaceDto, GetUserId(), GetTraceId());
        return CreatedAtAction(nameof(GetWorkspace), new { id = workspace.Id, version = "1.0" }, workspace);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WorkspaceDto>> UpdateWorkspace(int id, UpdateWorkspaceDto updateWorkspaceDto)
    {
        var workspace = await _workspaceService.UpdateWorkspaceAsync(id, updateWorkspaceDto, GetUserId(), GetTraceId());

        if (workspace == null)
            return NotFound();

        return Ok(workspace);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWorkspace(int id)
    {
        await _workspaceService.DeleteWorkspaceAsync(id, GetUserId(), GetTraceId());
        return NoContent();
    }

}