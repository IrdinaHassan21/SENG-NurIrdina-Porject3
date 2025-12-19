using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CatCollectorAPI.Data;

[ApiController]
[Route("api/debug")]
public class DebugConfigController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public DebugConfigController(IConfiguration config, IWebHostEnvironment env)
    {
        _config = config;
        _env = env;
    }

    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        // Only expose this endpoint in Development to avoid leaking operational details in production
        if (!_env.IsDevelopment()) return NotFound();

        var key = _config["Jwt:Key"] ?? string.Empty;
        var len = System.Text.Encoding.UTF8.GetByteCount(key);
        var isValid = len >= 32;

        return Ok(new { jwtKeyLength = len, jwtKeyPresent = !string.IsNullOrEmpty(key), jwtKeyValid = isValid, environment = _env.EnvironmentName });
    }

    [HttpGet("whoami")]
    public IActionResult WhoAmI()
    {
        if (!_env.IsDevelopment()) return NotFound();

        var authenticated = HttpContext.User?.Identity?.IsAuthenticated ?? false;
        var claims = HttpContext.User?.Claims.Select(c => new { c.Type, c.Value }) ?? Enumerable.Empty<object>();
        return Ok(new { authenticated, claims });
    }

    [HttpGet("headers")]
    [AllowAnonymous]
    public IActionResult Headers()
    {
        // Expose headers in Development only
        if (!_env.IsDevelopment()) return NotFound();

        var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
        // Also include some auth diagnostic helpers
        var authHeader = Request.Headers.ContainsKey("Authorization") ? Request.Headers["Authorization"].ToString() : null;
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        return Ok(new { headers, authHeader, remoteIp });
    }
}
