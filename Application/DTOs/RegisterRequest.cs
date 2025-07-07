using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
        
    [Required]
    public string Username { get; set; } = string.Empty;
        
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
        
    [Required]
    public string TraceId { get; set; } = string.Empty;
        
    [Required]
    public string UseCase { get; set; } = string.Empty;
        
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}