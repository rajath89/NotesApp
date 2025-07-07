using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class AuthApiLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
        
    [Required]
    public string Password { get; set; } = string.Empty;
        
    [Required]
    public string TraceId { get; set; } = string.Empty;
        
    [Required]
    public string UseCase { get; set; } = string.Empty;
}