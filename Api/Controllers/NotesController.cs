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

        public NotesController(INoteService noteService)
        {
            _noteService = noteService;
        }

        private string GetTraceId() => HttpContext.Items["TraceId"]?.ToString() ?? "untraced";
        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet("workspace/{workspaceId}")]
        public async Task<ActionResult<IEnumerable<NoteDto>>> GetWorkspaceNotes(int workspaceId)
        {
            var notes = await _noteService.GetWorkspaceNotesAsync(workspaceId, GetUserId(), GetTraceId());
            return Ok(notes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NoteDto>> GetNote(int id)
        {
            var note = await _noteService.GetNoteAsync(id, GetUserId(), GetTraceId());
            
            if (note == null)
                return NotFound();
            
            return Ok(note);
        }

        [HttpPost]
        public async Task<ActionResult<NoteDto>> CreateNote(CreateNoteDto createNoteDto)
        {
            var note = await _noteService.CreateNoteAsync(createNoteDto, GetUserId(), GetTraceId());
            
            if (note == null)
                return BadRequest("Invalid workspace ID or workspace not owned by user");
            
            return CreatedAtAction(nameof(GetNote), new { id = note.Id, version = "1.0" }, note);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<NoteDto>> UpdateNote(int id, UpdateNoteDto updateNoteDto)
        {
            var note = await _noteService.UpdateNoteAsync(id, updateNoteDto, GetUserId(), GetTraceId());
            
            if (note == null)
                return NotFound();
            
            return Ok(note);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNote(int id)
        {
            await _noteService.DeleteNoteAsync(id, GetUserId(), GetTraceId());
            return NoContent();
        }
}