using System.Text.Json.Serialization;

namespace Application.DTOs;

public class AuthErrorInfo
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}