using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly JwtTokenHelper _jwtTokenHelper;
    IUserSessionService _userSessionService;

    public SessionController(JwtTokenHelper jwtTokenHelper, IUserSessionService userSessionService)
    {
          _jwtTokenHelper = jwtTokenHelper;
          _userSessionService = userSessionService;
    }

    [HttpGet("user")]
    public IActionResult GetCurrentUser(){
         var user = new
        {
            Username = User.FindFirst("Username")?.Value,
            UserId = User.FindFirst("UserId")?.Value,
            Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
            IpAddress = User.FindFirst("IpAddress")?.Value,
            Device = User.FindFirst("Device")?.Value,
            DeviceFamily = User.FindFirst("DeviceFamily")?.Value,
            BrowserName = User.FindFirst("BrowserName")?.Value,
            SessionId = User.FindFirst("SessionId")?.Value
        };

        return Ok(user);
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetUserSessions([FromQuery] string userId)
    {
        var sessions = await _userSessionService.GetSessionsByUserAsync(Guid.Parse(userId));
        return Ok(sessions);
    }

    [HttpPost("logout/session")]
    public async Task<IActionResult> LogoutBySession([FromBody] LogoutRequest request)
    {
        // Remove the session from the cache
        await _userSessionService.RemoveSessionAsync(Guid.Parse(request.SessionId));
        return Ok("Logged out successfully.");
    }

    [HttpPost("logout/all")]
    public async Task<IActionResult> LogoutAll([FromBody] LogoutRequest request)
    {
        // Retrieve SessionId from the claims
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null)
        {
            return BadRequest("User ID not found in token claims.");
        }

        var userId = userIdClaim.Value;
        // Remove all sessions for the user
        await _userSessionService.RemoveAllSessionsForUserAsync(Guid.Parse(userId));
        return Ok("Logged out from all sessions.");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {        
        // Retrieve SessionId from the claims
        var sessionIdClaim = User.FindFirst("SessionId");
        if (sessionIdClaim == null)
        {
            return BadRequest("Session ID not found in token claims.");
        }

        var sessionId = sessionIdClaim.Value;

        // Remove the session from storage
        await _userSessionService.RemoveSessionAsync(Guid.Parse(sessionId));

        return Ok("Logged out successfully.");
    }


}

public class LogoutRequest
{
    public string SessionId { get; set; }  // Optional: for logging out from a specific session
    public string UserId { get; set; }     // Optional: for logging out from all sessions
}
