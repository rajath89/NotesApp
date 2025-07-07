using Application.DTOs;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<AuthApiResponse> LoginAsync(LoginRequest loginRequest, string traceId);
    Task<AuthApiResponse> RegisterAsync(RegisterRequest registerRequest, string traceId);
}