namespace Stq.Repos {
  using Dapper;
  using Stq.Db;
  using System.Collections.Generic;
  using System.Data;
  using System.Threading.Tasks;


  /// <summary></summary>
  public class CompanyRepository : ICompanyRepository {
    private readonly IDb db;

    /// <summary></summary>
    public CompanyRepository(IDb db) {
      this.db = db;
    }

    /// <summary></summary>
    public async Task<IEnumerable<Data.Company>> FindAllAsync() =>
      await db.QueryAsync(async (conn, trans) => await conn.QueryAsync<Data.Company>(
            sql:
              "select * from companies as c\n" +
              "inner join markets as m on c.market_id = m.id\n" +
              "order by c.code asc;",
            transaction: trans,
            splitOn:     "id,id",
            types:       new [] { typeof(Data.Company), typeof(Stq.Data.Market) },
            map:         Map ));


    /// <summary></summary>
    public async Task<IEnumerable<Data.Company>> FindByMarketIdAsync(long marketId) =>
      await db.QueryAsync(async (conn, trans) => await conn.QueryAsync<Data.Company>(
            sql:
              "select * from companies as c\n" +
              "inner join markets as m on c.market_id = m.id\n" +
              "where c.market_id = @MarketId\n" +
              "order by c.code asc;",
            transaction: trans,
            splitOn: "id,id",
            types: new [] { typeof(Data.Company), typeof(Stq.Data.Market) },
            map: Map,
            param: new { MarketId = marketId }));

    /// <summary></summary>
    public async Task InsertAsync(Stq.Data.Company company) =>
      await db.ExecuteAsync(async (conn, trans) =>
          await InsertAsync(conn, trans, company));

    /// <summary></summary>
    public async Task InsertMultipleAsync(IEnumerable<Stq.Data.Company> companies) =>
      await db.ExecuteAsync(async (conn, trans) =>
          await InsertMultipleAsync(conn, trans, companies));


    /// <summary></summary>
    private async Task InsertAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        Stq.Data.Company company) =>
      await connection.ExecuteAsync(
          sql:
            "insert into companies(code, name, created_at, market_id, is_enabled)\n" +
            "values(@Code, @Name, @CreatedAt, @MarketId, @IsEnabled);",
          transaction: transaction,
          param:       company);


    /// <summary></summary>
    private async Task InsertMultipleAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IEnumerable<Stq.Data.Company> companies) {
      foreach(var company in companies) {
        await InsertAsync(connection, transaction, company);
      }
    }

    /// <summary></summary>
    private Data.Company Map(object[] objs) {
      var c = (Data.Company)objs[0];
      c.Market = (Stq.Data.Market)objs[1];
      return c;
    }
  }


  /// <summary></summary>
  internal class CompanyEqualityComparer : IEqualityComparer<Stq.Data.Company> {
    /// <summary></summary>
    public bool Equals(
        Stq.Data.Company? x,
        Stq.Data.Company? y) {
      if(x == null || y == null)
        return false;

      return x.Id == y.Id;
    }

    /// <summary></summary>
    public int GetHashCode(Stq.Data.Company obj) => obj.Code.GetHashCode();
  }
}
