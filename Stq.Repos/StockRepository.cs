namespace Stq.Repos {
  using Dapper;
  using Stq.Db;
  using System;
  using System.Collections.Generic;
  using System.Data;
  using System.Linq;
  using System.Threading.Tasks;


  /// <summary></summary>
  public class StockRepository : IStockRepository {
    private readonly IDb db;

    /// <summary></summary>
    public StockRepository(IDb db) {
      this.db = db;
    }


    /// <summary></summary>
    public async Task<IEnumerable<Stq.Data.Stock>> FindAllAsync() =>
      await db.QueryAsync(async (conn, trans) => await conn.QueryAsync<Stq.Data.Stock>(
            sql: "select * from stocks;",
            transaction: trans));


    /// <summary></summary>
    public async Task<IEnumerable<Stq.Data.Stock>> FindByCompanyIdAsync(long companyId) =>
      await db.QueryAsync(async (conn, trans) =>
          await conn.QueryAsync<Stq.Data.Stock>(
            sql:
              "select * from stocks\n" +
              "where company_id = @CompanyId\n" +
              "order by date asc;",
            transaction: trans,
            param: new { CompanyId = companyId }));

    /// <summary></summary>
    public async Task<IEnumerable<Stq.Data.Stock>> FindByCodeAsync(
        string    code,
        DateTime? startOn = null) =>
      await db.QueryAsync(async (conn, trans) =>
          await conn.QueryAsync<Stq.Data.Stock>(
            sql:         startOn == null
              ? "select s.* from stocks as s\n" +
                "inner join companies as c on s.company_id = c.id\n" +
                "where c.code = @Code order by s.date asc;"
              : "select s.* from stocks as s\n" +
                "inner join companies as c on s.company_id = c.id\n" +
                "where c.code = @Code and s.date >= @StartOn order by s.date asc;",
            transaction: trans,
            param:       new { Code = code, StartOn = startOn }));

    /// <summary></summary>
    public async Task<IEnumerable<Stq.Data.Stock>> FindByCodesAsync(IEnumerable<string> codes) =>
      await db.QueryAsync(async (conn, trans) => await conn.QueryAsync<Stq.Data.Stock>(
            sql:
              "select s.* from stocks as s\n" +
              "inner join companies as c on s.company_id = c.id\n" +
              "where c.code in @Codes;",
            transaction: trans,
            param: new { Codes = codes.ToArray() }));

    /// <summary></summary>
    public async Task InsertAsync(Stq.Data.Stock stock) =>
      await db.ExecuteAsync(async (conn, trans) => await InsertAsync(conn, trans, stock));

    /// <summary></summary>
    public async Task InsertMultipleAsync(IEnumerable<Stq.Data.Stock> stocks) =>
      await db.ExecuteAsync(async (conn, trans) => await InsertMultipleAsync(conn, trans, stocks));


    /// <summary></summary>
    private async Task InsertMultipleAsync(
        IDbConnection               connection,
        IDbTransaction              transaction,
        IEnumerable<Stq.Data.Stock> stocks) {
      foreach(var stock in stocks) {
        await InsertAsync(connection, transaction, stock);
      }
    }

    /// <summary></summary>
    private async Task InsertAsync(
        IDbConnection  connection,
        IDbTransaction transaction,
        Stq.Data.Stock stock) => await connection.ExecuteAsync(
          sql:
            "insert into stocks(company_id, date, open, high, low, close, volume)\n" +
            "values(@CompanyId, @Date, @Open, @High, @Low, @Close, @Volume);",
          transaction: transaction,
          param: stock);
  }
}
