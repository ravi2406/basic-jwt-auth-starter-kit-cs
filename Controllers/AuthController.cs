using Microsoft.AspNetCore.Mvc;
using UAParser.FormFactor;
using Utilities;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtTokenHelper _jwtTokenHelper;
    IUserSessionService _userSessionService;
    IUserService _userService;

    public AuthController(JwtTokenHelper jwtTokenHelper, IUserSessionService userSessionService, IUserService userService)
    {
          _jwtTokenHelper = jwtTokenHelper;
          _userSessionService = userSessionService;
          _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
                // Retrieve the User-Agent string from the request headers
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        // Use UAParser to parse the User-Agent string
        var uaParser = Parser.GetDefault();
        var clientInfo = uaParser.Parse(userAgent);

        // Extract browser information
        var browserName = clientInfo.UA.Family;        // Browser name (e.g., "Chrome", "Firefox")
        var browserVersion = $"{clientInfo.UA.Major}.{clientInfo.UA.Minor}"; // Browser version
        var os = clientInfo.OS.Family;                 // OS name (e.g., "Windows", "iOS")
        var osVersion = $"{clientInfo.OS.Major}.{clientInfo.OS.Minor}"; // OS version

        // Check if device is mobile or desktop
        var deviceFamily = clientInfo.Device.Family;
        var deviceType = clientInfo.Device.FormFactor;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        
        // Basic username/password check (replace with actual user validation logic)
        var user = await _userService.GetUserByUsernameAsync(request.Username);
        if (user != default(User) && AppHelper.HashPassword(request.Password) == user.PasswordHash) // replace this with db based authentication
        {
            var userId = user.UserId; // Replace with actual user retrieval logic
            var sessionId = Guid.NewGuid(); // Unique session ID

            var tokenData = new
            {
                Username = request.Username,
                UserId = userId,
                Roles = new[] { "User", "Admin" }, // Example roles , can come from database            
                IpAddress = ipAddress,
                Device = deviceType,
                DeviceFamily = deviceFamily,
                BrowserName = browserName,
                SessionId = sessionId
            };

            string encryptedToken = _jwtTokenHelper.GenerateToken(tokenData);

            var session = new UserSession
            {
                SessionId = sessionId,
                UserId = userId,
                Token = encryptedToken,
                IpAddress = ipAddress,
                DeviceType = deviceType.ToString(),
                DeviceFamily = deviceFamily,
                BrowserName = browserName,
                BrowserVersion = browserVersion,
                ExpirationUnixTime = _jwtTokenHelper.GetExpirationTime(encryptedToken),
                LoginTime = DateTime.UtcNow,
                IsActive = true
            };

            // Store the session
            await _userSessionService.AddSessionAsync(session);

            return Ok(new { Token = encryptedToken });
        }

        return Unauthorized();
    }

     [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Username and password are required.");
        }

        // Hash the password
        string passwordHash = AppHelper.HashPassword(request.Password);

        // Create the User object
        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            Email = request.Email,
            FullName = request.FullName
        };

        // Add user to the database
        await _userService.AddUserAsync(user);

        return Ok("User registered successfully.");
    }
   
}



public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}


public class RegisterRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
}
