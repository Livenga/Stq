namespace Stq.Net.Test {
  using Stq.Net;
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Xunit;
  using Xunit.Abstractions;


  /// <summary></summary>
  public class StooqTest {
    private readonly ITestOutputHelper outputHelper;


    /// <summary></summary>
    public StooqTest(ITestOutputHelper outputHelper) {
      this.outputHelper = outputHelper;
    }


    [Theory]
    [InlineData("8002.JP")]
    public async Task DownloadAsyncTest(string code) {
      using var stooq = new Stooq();
      var stocks = await stooq.DownloadAsync(code: code);

      outputHelper.WriteLine($"# {code}");
      foreach(var stock in stocks) {
        outputHelper.WriteLine($"\t{stock.Close} {stock.Volume} {stock.Date.ToString("yyyy-MM-dd")}");
      }
    }

    [Theory]
    [InlineData(StooqMarket.NASDAQ)]
    public async Task DownloadCompaniesAsyncTest(StooqMarket market) {
      using var stooq = new Stooq(new Uri("socks5://localhost:9050"));
      stooq.Cookie = "cookie_uu=220304000; PHPSESSID=2qe0ep9ov0uk09d6mup6sjoc22; FCCDCF=[null,null,null,[\"CPVUkp5PVUkp5EsABBPLCFCoAP_AAH_AAB5YHQpD7T7FbSFCyP55fLsAMAhXRkCEAqQAAASABmABQAKQIAQCkkAQFASgBAACAAAgICZBAQIMCAgACUABQABAAAEEAAAABAAIIAAAgAEAAAAIAAACAIAAAAAIAAAAEAAAmwgAAIIACAAABAAAAAAAAAAAAAAAAgdCgPsLsVtIUJI_Gk8uwAgCFdGQIQCoAAAAIAGYAAAApAgBAKQQBAABKAAAAIAACAgJgEBAggACAABQAFAAEAAAAAAAAAAAAggAACAAQAAAAgAAAIAgAAAAAgAAAAAAACBCAAAggAIAAAAAAAAAAAAAAAAAAACAAA.f-gAAAAAAAA\",\"1~2072.70.89.93.108.122.149.2202.162.167.196.2253.241.2299.259.2357.311.317.323.2373.338.358.415.440.449.2506.2526.482.486.494.495.2568.2571.2575.540.574.2677.817.864.981.1051.1095.1097.1127.1201.1205.1211.1276.1301.1365.1415.1449.1570.1577.1651.1716.1765.1870.1878.1889\",\"350A064B-0D15-4075-8CDB-A9FEDC9A73AA\"],null,null,[]]; FCNEC=[[\"AKsRol-uW_awWWj3L2_HhyRvHYeOYVzM2gjj1TRZiojTWC6v94cDbeG3nGHTZQ5hu_tD9f-AjmsFvb0ocqnRnN0c552K-IB6HD5nAu4fFiGwZk3wG7r0G8OP6Wjmvw3mF9FO3LkSmdQu65zfKiYA8Fog6AXGGt_UOg==\"],null,[]]; privacy=1646384197";
      var companies = await stooq.DownloadCompaniesAsync(market);

      await Stq.IO.Csv.WriteRecordsAsync("dump.csv", companies);
    }
  }
}
