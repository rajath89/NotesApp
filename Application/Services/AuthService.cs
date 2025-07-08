using System.Text;
using System.Text.Json;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(HttpClient httpClient, ILogger<AuthService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<AuthApiResponse> LoginAsync(LoginRequest loginRequest, string traceId)
    {
        try
        {
            _logger.LogInformation("[{TraceId}] Attempting login for user {Email}", traceId, loginRequest.Email);

            var authApiUrl = _configuration["AuthApi:BaseUrl"];
            var applicationId = _configuration["AuthApi:ApplicationId"];
            var securityToken = _configuration["AuthApi:SecurityToken"];

            var request = new HttpRequestMessage(HttpMethod.Post, $"{authApiUrl}/api/v1/Auth/Login");

            // Add headers
            request.Headers.Add("ApplicationId", applicationId);
            request.Headers.Add("SecurityToken", securityToken);
            request.Headers.Add("TraceId", traceId);
            request.Headers.Add("User-Agent", "NotesApp/1.0");

            // Add content
            var jsonContent = JsonSerializer.Serialize(GetAuthApiLoginRequest(traceId, loginRequest),
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var authResponse = JsonSerializer.Deserialize<AuthApiResponse>(responseContent,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                _logger.LogInformation("[{TraceId}] Login successful for user {Email}", traceId, loginRequest.Email);
                return authResponse ?? new AuthApiResponse();
            }
            else
            {
                _logger.LogWarning(
                    "[{TraceId}] Login failed for user {Email}. Status: {StatusCode}, Response: {Response}",
                    traceId, loginRequest.Email, response.StatusCode, responseContent);

                // Try to parse error response
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent,
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });

                    return new AuthApiResponse
                    {
                        Completion = new Completion
                        {
                            Code = "ERROR",
                            Value = ((int)response.StatusCode).ToString()
                        }
                    };
                }
                catch
                {
                    return new AuthApiResponse
                    {
                        Completion = new Completion
                        {
                            Code = "ERROR",
                            Value = "500"
                        }
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Error during login for user {Email}", traceId, loginRequest.Email);
            return new AuthApiResponse
            {
                Completion = new Completion
                {
                    Code = "ERROR",
                    Value = "500"
                }
            };
        }
    }

    private AuthApiLoginRequest GetAuthApiLoginRequest(string traceId, LoginRequest loginRequest)
    {
        return new AuthApiLoginRequest
        {
            Email = loginRequest.Email,
            Password = loginRequest.Password,
            TraceId = traceId,
            UseCase = "user_login"
        };
    }

    private AuthApiRegisterRequest GetAuthApiRegisterRequest(string traceId, RegisterRequest registerRequest)
    {
        return new AuthApiRegisterRequest
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password,
            FirstName = registerRequest.FirstName,
            LastName = registerRequest.LastName,
            Username = registerRequest.Username,
            TraceId = traceId,
            UseCase = "user_register"
        };
    }

    public async Task<AuthApiResponse> RegisterAsync(RegisterRequest registerRequest, string traceId)
    {
        try
        {
            _logger.LogInformation("[{TraceId}] Attempting registration for user {Email}", traceId,
                registerRequest.Email);

            var authApiUrl = _configuration["AuthApi:BaseUrl"];
            var applicationId = _configuration["AuthApi:ApplicationId"];
            var securityToken = _configuration["AuthApi:SecurityToken"];

            var request = new HttpRequestMessage(HttpMethod.Post, $"{authApiUrl}/api/v1/Auth/Register");

            // Add headers
            request.Headers.Add("ApplicationId", applicationId);
            request.Headers.Add("SecurityToken", securityToken);
            request.Headers.Add("TraceId", traceId);

            // Add content
            var jsonContent = JsonSerializer.Serialize(GetAuthApiRegisterRequest(traceId, registerRequest),
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var authResponse = JsonSerializer.Deserialize<AuthApiResponse>(responseContent,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                if (authResponse?.Completion?.Code == "SUCCESS" && authResponse.TokenType == "UID" &&
                    !string.IsNullOrEmpty(authResponse.Token))
                {
                    _logger.LogInformation("[{TraceId}] User {Email} registered successfully with UID: {UserId}",
                        traceId, registerRequest.Email, authResponse.Token);

                    //UpdateApplicationUser(registerRequest, authResponse.Token);
                }
                else
                {
                    _logger.LogWarning(
                        "[{TraceId}] Registration response for user {Email} did not contain expected UID",
                        traceId, registerRequest.Email);

                    return new AuthApiResponse
                    {
                        Completion = new Completion
                        {
                            Code = "ERROR",
                            Value = "500"
                        }
                    };
                }

                _logger.LogInformation("[{TraceId}] Registration successful for user {Email}", traceId,
                    registerRequest.Email);
                return authResponse ?? new AuthApiResponse();
            }
            else
            {
                _logger.LogWarning(
                    "[{TraceId}] Registration failed for user {Email}. Status: {StatusCode}, Response: {Response}",
                    traceId, registerRequest.Email, response.StatusCode, responseContent);

                return new AuthApiResponse
                {
                    Completion = new Completion
                    {
                        Code = "ERROR",
                        Value = ((int)response.StatusCode).ToString()
                    }
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Error during registration for user {Email}", traceId,
                registerRequest.Email);
            return new AuthApiResponse
            {
                Completion = new Completion
                {
                    Code = "ERROR",
                    Value = "500"
                }
            };
        }
    }
}