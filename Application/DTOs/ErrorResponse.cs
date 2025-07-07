namespace Application.DTOs;

public class ErrorResponse
{
    public List<ErrorInfo> ErrorInfo { get; set; } = new();
}