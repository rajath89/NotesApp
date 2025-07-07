using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
        
    [Required]
    public string Password { get; set; } = string.Empty;
        
    [Required]
    public string TraceId { get; set; } = string.Empty;
}