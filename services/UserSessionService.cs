using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

public class UserSession
{
    public Guid SessionId { get; set; }  // Unique session identifier (can be a GUID)
    public Guid UserId { get; set; }     // User ID associated with the session
    public string Token { get; set; }      // JWT token associated with the session
    public string IpAddress { get; set; }  // IP address from which the session originated
    public string DeviceType { get; set; } // Device type (mobile/desktop)
    public string DeviceFamily { get; set; } // Device family (Android, iOS, etc.)
    public string BrowserName { get; set; } // Browser name (Chrome, Firefox, etc.)
    public string BrowserVersion { get; set; } // Browser version
    public long ExpirationUnixTime { get; set; } // Token expiration date
    public DateTime LoginTime { get; set; } // Timestamp of login
    public bool IsActive {get;set;}
}

public interface IUserSessionService
{
    Task AddSessionAsync(UserSession session);
    Task<UserSession> GetSessionByIdAsync(Guid sessionId);
    Task<IEnumerable<UserSession>> GetSessionsByUserAsync(Guid userId);
    Task RemoveSessionAsync(Guid sessionId);
    Task RemoveAllSessionsForUserAsync(Guid userId);

    Task<bool> IsSessionActiveAsync(Guid sessionId);
    
}


public class UserSessionService : IUserSessionService
{
   IDbConnection _connection;

    public UserSessionService(IDbConnection connection)
    {
        _connection = connection;
    }
    
    public async Task AddSessionAsync(UserSession session)
    {
        const string query = @"
            INSERT INTO UserSessions (SessionId, UserId, Token, IpAddress, DeviceType, DeviceFamily, BrowserName, BrowserVersion, ExpirationUnixTime, LoginTime)
            VALUES (@SessionId, @UserId, @Token, @IpAddress, @DeviceType, @DeviceFamily, @BrowserName, @BrowserVersion, @ExpirationUnixTime, @LoginTime);";
        
        await _connection.ExecuteAsync(query, session);
    }

    public async Task<UserSession> GetSessionByIdAsync(Guid sessionId)
    {
        const string query = @"SELECT * FROM UserSessions WHERE SessionId = @SessionId;";
        
        return await _connection.QueryFirstOrDefaultAsync<UserSession>(query, new { SessionId = sessionId });
    }

    public async Task<IEnumerable<UserSession>> GetSessionsByUserAsync(Guid userId)
    {
        const string query = @"SELECT * FROM UserSessions WHERE UserId = @UserId;";
        
        return await _connection.QueryAsync<UserSession>(query, new { UserId = userId });
    }

    public async Task<bool> IsSessionActiveAsync(Guid sessionId)
    {
        const string query = @"
            SELECT COUNT(1)
            FROM UserSessions 
            WHERE SessionId = @SessionId";

        var count = await _connection.QuerySingleAsync<int>(query, new { SessionId = sessionId });
        
        // If count is 0, the session does not exist (i.e., it's inactive or deleted)
        return count > 0;
    }
    public async Task RemoveSessionAsync(Guid sessionId)
    {
        const string query = @"DELETE FROM UserSessions WHERE SessionId = @SessionId;";
        
        await _connection.ExecuteAsync(query, new { SessionId = sessionId });
    }

    public async Task RemoveAllSessionsForUserAsync(Guid userId)
    {
        const string query = @"DELETE FROM UserSessions WHERE UserId = @UserId;";
        
        await _connection.ExecuteAsync(query, new { UserId = userId });
    }
}
