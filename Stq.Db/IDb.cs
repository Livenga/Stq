namespace Stq.Db {
  using System;
  using System.Data;
  using System.Threading.Tasks;


  /// <summary></summary>
  public interface IDb {
    /// <summary></summary>
    Task<T> CreateConnectionAsync<T>() where T : IDbConnection;

    /// <summary></summary>
    Task<T> QueryAsync<T>(
        Func<IDbConnection, IDbTransaction, Task<T>> func,
        IsolationLevel il = IsolationLevel.ReadCommitted);

    /// <summary></summary>
    Task ExecuteAsync(
        Func<IDbConnection, IDbTransaction, Task> action,
        IsolationLevel il = IsolationLevel.ReadCommitted);
  }
}
