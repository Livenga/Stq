namespace Stq.Repos {
  using Dapper;
  using Stq.Db;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;


  /// <summary></summary>
  public interface IMarketRepository {
    /// <summary></summary>
    Task InsertAsync(Stq.Data.Market market);

    /// <summary></summary>
    Task<IEnumerable<Stq.Data.Market>> FindAllAsync();

    /// <summary></summary>
    Task<Stq.Data.Market?> FindByIdAsync(long id);
  }


  /// <summary></summary>
  public class MarketRepository : IMarketRepository {
    private readonly IDb db;


    /// <summary></summary>
    public MarketRepository(IDb db) {
      this.db = db;
    }

    /// <summary></summary>
    public async Task InsertAsync(Stq.Data.Market market) =>
      await db.ExecuteAsync(async (conn, trans) =>
          await conn.ExecuteAsync(
            sql:
              "insert into markets(name, comment, stooq_id, created_at)\n" +
              "values(@Name, @Comment, @StooqId, @CreatedAt);",
            transaction: trans,
            param: market));

    /// <summary></summary>
    public async Task<IEnumerable<Stq.Data.Market>> FindAllAsync() =>
      await db.QueryAsync(async (conn, trans) => await conn.QueryAsync<Stq.Data.Market>(
            sql: "select * from markets order by name asc;",
            transaction: trans));

    /// <summary></summary>
    public async Task<Stq.Data.Market?> FindByIdAsync(long id) =>
      await db.QueryAsync(async (conn, trans) => await conn.QuerySingleOrDefaultAsync<Stq.Data.Market>(
            sql:         "select * from markets where id = @Id;",
            transaction: trans,
            param:       new { Id = id }));
  }
}
