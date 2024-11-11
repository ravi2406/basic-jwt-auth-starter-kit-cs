using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

public class JwtTokenHelper
{
    private readonly string _secretKey;
    private readonly int _expiryDurationInSeconds;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenHelper(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"];
        _issuer = configuration["jwt:Issuer"];
        _audience = configuration["jwt:Audience"];
        _expiryDurationInSeconds = int.Parse(configuration["Jwt:ExpiryDurationInSeconds"]);
    }

    public string GenerateToken(object tokenData)
    {
        // Create claims from token data using reflection
        var claims = new List<Claim>();

        foreach (var property in tokenData.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propertyName = property.Name;
            var propertyValue = property.GetValue(tokenData);

            if (propertyValue is IEnumerable<string> roles)
            {
                // Add roles as separate claims
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            else if (propertyValue is DateTime dateTimeValue)
            {
                // Convert DateTime properties to strings
                claims.Add(new Claim(propertyName, dateTimeValue.ToString("o"))); // Use "o" format for UTC
            }
            else
            {
                claims.Add(new Claim(propertyName, propertyValue?.ToString() ?? string.Empty));
            }
        }

        // Add expiration claim
        claims.Add(new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddSeconds(_expiryDurationInSeconds).ToUnixTimeSeconds().ToString()));

        // Generate JWT token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(_expiryDurationInSeconds),
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenDescriptor);
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _issuer,
            ValidAudience = _audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, parameters, out _);
            return principal;
        }
        catch
        {
            return null; // Token is invalid
        }
    }
    public long GetExpirationTime(string token)
    {
        // Initialize the JWT handler
        var tokenHandler = new JwtSecurityTokenHandler();

        // Ensure the token is in a valid JWT format
        if (!tokenHandler.CanReadToken(token))
        {
            return -1;
        }

        // Read the token to extract claims
        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Get the expiration claim (exp) from the JWT payload
        var expClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Exp);
        if (expClaim == null)
        {
            return -1; // Expiration claim not found
        }

        // Convert expiration claim to DateTime
        if (long.TryParse(expClaim.Value, out var expSeconds))
        {
           return expSeconds;
        }

        return -1; // Invalid expiration format
    }

     public static string GetClaimValue(string token, string claimName)
    {
        // Initialize the JWT handler
        var tokenHandler = new JwtSecurityTokenHandler();

        // Ensure the token is in a valid JWT format
        if (!tokenHandler.CanReadToken(token))
        {
            return null;
        }

        // Read the token to extract claims
        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Retrieve the specified claim
        var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == claimName);

        // Return the claim's value if found, otherwise return null
        return claim?.Value;
    }

}
