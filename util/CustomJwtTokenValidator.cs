using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

public class CustomJwtTokenValidator
{
    private readonly IUserSessionService _userSessionService;

    public CustomJwtTokenValidator(IUserSessionService userSessionService)
    {
        _userSessionService = userSessionService;
    }

    public async Task OnTokenValidated(TokenValidatedContext context)
    {
        // Retrieve the session ID from the claims
        var sessionIdClaim = context.Principal?.FindFirst("SessionId")?.Value;
        
        if (string.IsNullOrEmpty(sessionIdClaim) || !Guid.TryParse(sessionIdClaim, out var sessionId))
        {
            // No valid session ID found in token, fail the authentication
            context.Fail("Invalid token");
            return;
        }

        // Check if the session ID is still active in the UserSessions table
        var isSessionActive = await _userSessionService.IsSessionActiveAsync(sessionId);

        if (!isSessionActive)
        {
            // If the session is inactive, the token is invalid
            context.Fail("Token is no longer valid: The session has been logged out.");
            return;
        }

        // Additional custom validation logic can be added here
    }
}
