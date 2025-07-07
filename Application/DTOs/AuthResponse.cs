using System.Text.Json.Serialization;

namespace Application.DTOs;

public class AuthResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("errorInfo")]
    public ErrorInfo? ErrorInfo { get; set; }
}