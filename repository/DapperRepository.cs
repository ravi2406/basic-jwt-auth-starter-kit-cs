using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

public class DapperRepository<T> : IRepository<T>
{
    private readonly IDbConnection _connection;

    public DapperRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<T> Get(int id)
    {
        string sql = $"SELECT * FROM {typeof(T).Name} WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    public async Task<IEnumerable<T>> GetAll(string query)
    {
        return await _connection.QueryAsync<T>(query);
    }

    public async Task<IEnumerable<T>> GetAll(string query, int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be at least 1", nameof(pageNumber));
        if (pageSize < 1)
            throw new ArgumentException("Page size must be at least 1", nameof(pageSize));

        int offset = (pageNumber - 1) * pageSize;
        string paginatedQuery = $"{query} LIMIT @PageSize OFFSET @Offset";
        return await _connection.QueryAsync<T>(paginatedQuery, new { PageSize = pageSize, Offset = offset });
    }

}
