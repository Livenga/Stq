namespace Stq.Db {
  using System;
  using System.Collections.Generic;
  using System.Data;
  using System.Data.SQLite;
  using System.Linq;
  using System.Threading.Tasks;


  /// <summary></summary>
  public class SQLiteClient : IDb {
    /// <summary></summary>
    public string DataSource { set; get; } = string.Empty;


    /// <summary></summary>
    public SQLiteClient() { }


    /// <summary></summary>
    public async Task<T> CreateConnectionAsync<T>() where T : IDbConnection {
      if(typeof(T) != typeof(SQLiteConnection))
        throw new InvalidOperationException();

      var conn = new SQLiteConnection();

      var dictConnectionString = new Dictionary<string, string>();
      dictConnectionString.Add("Data Source", DataSource);

      conn.ConnectionString = string.Join(
          ";",
          dictConnectionString.Select(kvp => $"{kvp.Key}={kvp.Value}").ToArray());
#if DEBUG
      System.Diagnostics.Debug.WriteLine($"D Connection String: {conn.ConnectionString}");
#endif

      await conn.OpenAsync();
      return (T)((object)conn);
    }


    /// <summary></summary>
    public async Task<T> QueryAsync<T>(
        Func<IDbConnection, IDbTransaction, Task<T>> func,
        IsolationLevel il = IsolationLevel.ReadCommitted) {
      using var conn = await CreateConnectionAsync<SQLiteConnection>();
      using var trans = await conn.BeginTransactionAsync(il);

      try {
        return await func(conn, trans);
      } catch {
        await trans.RollbackAsync();
        throw;
      }
    }

    /// <summary></summary>
    public async Task ExecuteAsync(
        Func<IDbConnection, IDbTransaction, Task> action,
        IsolationLevel il = IsolationLevel.ReadCommitted) {
      using var conn = await CreateConnectionAsync<SQLiteConnection>();
      using var trans = await conn.BeginTransactionAsync(il);

      try {
        await action(conn, trans);
        await trans.CommitAsync();
      } catch {
        await trans.RollbackAsync();
        throw;
      }
    }
  }
}
