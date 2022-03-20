namespace Stq.Repos {
  using Dapper;
  using System;
  using System.Data;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;


  /// <summary></summary>
  public interface IGroupRepository {
    /// <summary></summary>
    Task InsertAsync(
        Stq.Data.Group    group,
        IEnumerable<long> companyIds);

    /// <summary></summary>
    Task<IEnumerable<Stq.Data.Group>> FindAllAsync();

    /// <summary></summary>
    Task<Stq.Data.Group?> FindByIdAsync(long id);

    /// <summary></summary>
    Task<Stq.Data.Group?> FindByNameAsync(string name);

    /// <summary></summary>
    Task<IEnumerable<string>> GetCompanyCodesByGrouNameAsync(string groupName);
  }


  /// <summary></summary>
  public class GroupRepository : IGroupRepository {
    private readonly Stq.Db.IDb db;


    /// <summary></summary>
    public GroupRepository(Stq.Db.IDb db) {
      this.db = db;
    }

    /// <summary></summary>
    public async Task InsertAsync(
        Stq.Data.Group      group,
        IEnumerable<long> companyIds) => await db.ExecuteAsync(
          async (conn, trans) => await InsertAsync(conn, trans, group, companyIds));

    /// <summary></summary>
    private async Task InsertAsync(
        IDbConnection     connection,
        IDbTransaction    transaction,
        Stq.Data.Group    group,
        IEnumerable<long> companyIds) {
      await connection.ExecuteAsync(
          sql:
            "insert into groups(name, comment, created_at)\n" +
            "values(@Name, @Comment, @CreatedAt);",
          transaction: transaction,
          param: group);

      var groupId = await connection.QueryFirstAsync<long>(
          sql: "select last_insert_rowid();",
          transaction: transaction);

      foreach(var item in companyIds 
          .Select(companyId => new Stq.Data.GroupCompanies() {
            GroupId   = groupId,
            CompanyId = companyId })) {
        await connection.ExecuteAsync(
            sql:
              "insert into group_companies(group_id, company_id)\n" +
              "values(@GroupId, @companyId)",
            transaction: transaction,
            param: item);
      }
    }


    /// <summary></summary>
    public async Task<IEnumerable<Stq.Data.Group>> FindAllAsync() =>
      await db.QueryAsync(async (conn, trans) => await conn.QueryAsync<Stq.Data.Group>(
            sql: "select * from groups order by name asc;",
            transaction: trans));

    /// <summary></summary>
    public async Task<Stq.Data.Group?> FindByIdAsync(long id) =>
      await db.QueryAsync(async (conn, trans) => await conn.QuerySingleOrDefaultAsync<Stq.Data.Group>(
            sql: "select * from groups where id = @Id;",
            transaction: trans,
            param: new { Id = id }));

    /// <summary></summary>
    public async Task<Stq.Data.Group?> FindByNameAsync(string name) =>
      await db.QueryAsync(async (conn, trans) =>
          await conn.QuerySingleOrDefaultAsync<Stq.Data.Group>(
            sql: "select * from groups where name = @Name;",
            transaction: trans,
            param: new { Name = name }));

    /// <summary></summary>
    public async Task<IEnumerable<string>> GetCompanyCodesByGrouNameAsync(string groupName) =>
      await db.QueryAsync(async (conn, trans) => await conn.QueryAsync<string>(
            sql:
              "select company_code from group_companies where\n" +
              "group_id = (select id from groups where name = @GroupName)",
            transaction: trans,
            param: new { GroupName = groupName }));
  }
}
