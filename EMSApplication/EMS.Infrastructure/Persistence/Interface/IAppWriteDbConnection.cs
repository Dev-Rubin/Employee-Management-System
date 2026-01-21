using System.Data;

namespace EMS.Infrastructure.Persistence.Interface
{
    public interface IAppWriteDbConnection : IAppReadDbConnection
    {
        Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default);
    }
}
