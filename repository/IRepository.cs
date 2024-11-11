using System.Collections.Generic;
using System.Threading.Tasks;

public interface IRepository<T>
{
    Task<T> Get(int id);
    Task<IEnumerable<T>> GetAll(string query);
    Task<IEnumerable<T>> GetAll(string query, int pageNumber, int pageSize);
}
