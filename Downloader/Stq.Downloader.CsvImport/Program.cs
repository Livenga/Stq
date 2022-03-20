namespace Stq.Downloader.CsvImport;

using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


/// <summary></summary>
static class Program {
  /// <summary></summary>
  static async Task Main(string[] args) {
    if(args.Length < 3) {
      var _name = Assembly.GetExecutingAssembly().GetName().Name ?? "Stq.Downloader.CsvImport";
      Console.Error.WriteLine($"{_name} [DATASOURCE] [Csv PATH] [Market ID]");
      return;
    }

    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

    var db = new Stq.Db.SQLiteClient() { DataSource = args[0] };
    var simpleCompanies = await Stq.IO.Csv.ReadRecordsAsync<Stq.Net.StooqSimpleCompany>(args[1]);
    var marketId = System.Convert.ToInt64(args[2], 10);

    var marketRepo = new Stq.Repos.MarketRepository(db);
    var companyRepo = new Stq.Repos.CompanyRepository(db);

    var existingCompanies = await companyRepo.FindAllAsync();

    var now = DateTime.Now;
    var companies = simpleCompanies
      .Where(sc => ! existingCompanies.Any(c => c.Code == sc.Code))
      .Select(sc => new Stq.Data.Company() {
          Code      = sc.Code,
          Name      = sc.Name,
          CreatedAt = now,
          MarketId  = marketId });

    Console.Error.WriteLine($"# Target companies count: {companies.Count()}");
    if(companies.Count() > 0)
      await companyRepo.InsertMultipleAsync(companies);
  }

  /// <summary></summary>
  private static async Task InsertCompaniesAsync(
      IDbConnection                 connection,
      IDbTransaction                transaction,
      IEnumerable<Stq.Data.Company> companies) {
    foreach(var company in companies) {
      await connection.ExecuteAsync(
          sql:
            "insert into companies(code, name, created_at)\n" +
            "values(@Code, @Name, @CreatedAt);",
          transaction: transaction,
          param: company);
    }
  }
}
