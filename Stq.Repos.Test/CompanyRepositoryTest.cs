namespace Stq.Repos.Test {
  using Stq.Db;
  using Stq.Repos;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using Xunit;
  using Xunit.Abstractions;


  /// <summary></summary>
  public class CompanyRepositoryTest {
    private readonly ITestOutputHelper outputHelper;
    private readonly IDb db;
    private readonly ICompanyRepository companyRepository;

    /// <summary></summary>
    public CompanyRepositoryTest(ITestOutputHelper outputHelper) {
      this.outputHelper = outputHelper;

      db = new SQLiteClient() {
        DataSource = Path.Combine(
            "..", "..", "..", "..", "dataset.db"),
      };

      companyRepository = new CompanyRepository(db);
    }

    [Fact]
    public async Task FindAllAsyncTest() {
      var companies = await companyRepository.FindAllAsync();

      foreach(var company in companies) {
        outputHelper.WriteLine($"{company.Code} {company.Name}");
      }
    }

    [Theory]
    [InlineData("JPXCompanies.utf8.csv", 8)]
    public async Task InsertMultipleAsyncTest(
        string csvName,
        long   marketId) {
      var path = System.IO.Path.Combine(
          "..", "..", "..", "..", "core_datas", csvName);

      var marketRepository = new Stq.Repos.MarketRepository(db);
      var market = (await marketRepository.FindAllAsync())
        .FirstOrDefault(market => market.StooqId == marketId);
      if(market == null) {
        throw new NullReferenceException();
      }

      var csvCompanies = await Stq.IO.Csv.ReadRecordsAsync<Stq.Data.JPXCompany>(path);
      var n = DateTime.Now;
      var companies = csvCompanies.Select(c => new Stq.Data.Company() {
          Code               = c.Code,
          Name               = c.Name,
          MarketId           = marketId,
          CreatedAt          = n });

      await companyRepository.InsertMultipleAsync(companies);
    }
  }
}
