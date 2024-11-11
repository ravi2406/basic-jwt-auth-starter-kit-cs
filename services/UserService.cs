using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

public class User
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}

public interface IUserService
{
    Task AddUserAsync(User user);
    Task<User> GetUserByIdAsync(Guid userId);
    Task<User> GetUserByUsernameAsync(string username);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(Guid userId);
}

public class UserService : IUserService
{
    IDbConnection _connection;

    public UserService(IDbConnection connection)
    {
        _connection = connection;
    }

    
    public async Task AddUserAsync(User user)
    {
        const string query = @"
            INSERT INTO Users (UserId, Username, PasswordHash, Email, FullName, CreatedAt)
            VALUES (@UserId, @Username, @PasswordHash, @Email, @FullName, @CreatedAt);";

        user.UserId = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        
        await _connection.ExecuteAsync(query, user);
    }

    public async Task<User> GetUserByIdAsync(Guid userId)
    {
        const string query = @"SELECT * FROM Users WHERE UserId = @UserId;";
        
        return await _connection.QueryFirstOrDefaultAsync<User>(query, new { UserId = userId });
    }

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        const string query = @"SELECT * FROM Users WHERE Username = @Username;";
        
        return await _connection.QueryFirstOrDefaultAsync<User>(query, new { Username = username });
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        const string query = @"SELECT * FROM Users;";
        
        return await _connection.QueryAsync<User>(query);
    }

    public async Task UpdateUserAsync(User user)
    {
        const string query = @"
            UPDATE Users
            SET Username = @Username,
                PasswordHash = @PasswordHash,
                Email = @Email,
                FullName = @FullName,
                LastLogin = @LastLogin
            WHERE UserId = @UserId;";
        
        await _connection.ExecuteAsync(query, user);
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        const string query = @"DELETE FROM Users WHERE UserId = @UserId;";
        
        await _connection.ExecuteAsync(query, new { UserId = userId });
    }
}
