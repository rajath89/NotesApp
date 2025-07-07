using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
    [Route("NotesApp/public/api/v1/[controller]")]
    [AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    private string GetTraceId() => HttpContext.Items["TraceId"]?.ToString() ?? "untraced";

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest loginRequest)
    {
        var traceId = GetTraceId();
        
        try
        {
            _logger.LogInformation("[{TraceId}] Login request received for user {Email}", traceId, loginRequest.Email);

            // Use the traceId from the request if provided, otherwise use the one from middleware
            var requestTraceId = !string.IsNullOrEmpty(loginRequest.TraceId) ? loginRequest.TraceId : traceId;

            var result = await _authService.LoginAsync(loginRequest, requestTraceId);

            if (result.Completion.Code == "SUCCESS" && !string.IsNullOrEmpty(result.Token))
            {
                // Set the SSOTOKEN cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                Response.Cookies.Append("SSOTOKEN", result.Token, cookieOptions);
                
                _logger.LogInformation("[{TraceId}] Login successful for user {Email}, SSOTOKEN cookie set", 
                    requestTraceId, loginRequest.Email);

                return Ok(new AuthResponse { Status = 0 });
            }
            else
            {
                _logger.LogWarning("[{TraceId}] Login failed for user {Email}", requestTraceId, loginRequest.Email);
                
                return BadRequest(new AuthResponse 
                { 
                    Status = -1,
                    ErrorInfo = new ErrorInfo
                    {
                        Code = "LOGINAUTHFAILED",
                        Description = "Invalid credentials"
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Error processing login request for user {Email}", traceId, loginRequest.Email);
            
            return StatusCode(500, new AuthResponse
            {
                Status = -1,
                ErrorInfo = new ErrorInfo
                {
                    Code = "PREAUTHSYSTEMDOWN",
                    Description = "Auth Api Failed"
                }
            });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest registerRequest)
    {
        var traceId = GetTraceId();
        
        try
        {
            _logger.LogInformation("[{TraceId}] Registration request received for user {Email}", traceId, registerRequest.Email);

            // Use the traceId from the request if provided, otherwise use the one from middleware
            var requestTraceId = !string.IsNullOrEmpty(registerRequest.TraceId) ? registerRequest.TraceId : traceId;

            var result = await _authService.RegisterAsync(registerRequest, requestTraceId);

            if (result.Completion.Code == "SUCCESS")
            {
                _logger.LogInformation("[{TraceId}] Registration successful for user {Email}", requestTraceId, registerRequest.Email);
                return Ok(new AuthResponse { Status = 0 });
            }
            else
            {
                _logger.LogWarning("[{TraceId}] Registration failed for user {Email}", requestTraceId, registerRequest.Email);
                
                return BadRequest(new AuthResponse 
                { 
                    Status = -1,
                    ErrorInfo = new ErrorInfo
                    {
                        Code = "REGISTRATIONFAILED",
                        Description = "Registration failed"
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Error processing registration request for user {Email}", traceId, registerRequest.Email);
            
            return StatusCode(500, new AuthResponse
            {
                Status = -1,
                ErrorInfo = new ErrorInfo
                {
                    Code = "PREAUTHSYSTEMDOWN",
                    Description = "Auth Api Failed"
                }
            });
        }
    }

    [HttpPost("logout")]
    public ActionResult<AuthResponse> Logout()
    {
        var traceId = GetTraceId();
        
        try
        {
            _logger.LogInformation("[{TraceId}] Logout request received", traceId);

            // Clear the SSOTOKEN cookie
            Response.Cookies.Delete("SSOTOKEN");
            
            _logger.LogInformation("[{TraceId}] Logout successful, SSOTOKEN cookie cleared", traceId);

            return Ok(new AuthResponse { Status = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Error processing logout request", traceId);
            
            return StatusCode(500, new AuthResponse
            {
                Status = -1,
                ErrorInfo = new ErrorInfo
                {
                    Code = "PREAUTHSYSTEMDOWN",
                    Description = "Auth Api Failed"
                }
            });
        }
    }
}