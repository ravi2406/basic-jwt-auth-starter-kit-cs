using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

public class SimpleTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly EncryptionHelper _encryptionHelper;

    public SimpleTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IConfiguration configuration)
        : base(options, logger, encoder, clock)
    {
        var encryptionKey = configuration["EncryptionKey"]; // Ensure same key as in AuthController
        _encryptionHelper = new EncryptionHelper(encryptionKey);
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

        var authHeader = Request.Headers["Authorization"].ToString();
        if (!authHeader.StartsWith("Bearer "))
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));

        var encryptedToken = authHeader.Substring("Bearer ".Length).Trim();
        try
        {
            var tokenData = _encryptionHelper.Decrypt<dynamic>(encryptedToken); // Decrypt the token
            if(tokenData.ExpirationDate > DateTime.UtcNow)
                return Task.FromResult(AuthenticateResult.Fail("Invalid token")); 

            // Create claims from decrypted token data
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, tokenData.Username.ToString()),
                new Claim(ClaimTypes.NameIdentifier, tokenData.UserId.ToString())
            };
            foreach (var role in tokenData.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
        }
    }
}
