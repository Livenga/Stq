#pragma warning disable CS8625

namespace Stq.Downlaoder.Stock;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Stq.Data;
using Stq.Db;
using Stq.Net;
using Stq.Repos;


/// <summary></summary>
static class Program {
  private readonly static string InvalidCompanyJsonPath = "invalid_company.json";


  /// <summary></summary>
  static async Task Main(string[] args) {
    if(args.Length == 0) {
      var name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "Stq.Downloader.Stock";
      Console.Error.WriteLine($"{name} [DATASOURCE] [MARKET ID]");
      return;
    }

    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

    var db = new SQLiteClient() { DataSource = args[0] };
    var companyRepo = new CompanyRepository(db);
    var stockRepo = new StockRepository(db);

    long? marketId = null;
    if(args.Length > 1) {
      try {
        marketId = System.Convert.ToInt64(args[1], 10);

        var marketRepo = new MarketRepository(db);
        var market = await marketRepo.FindByIdAsync(id: marketId.Value);
        if(market == null) {
          Console.Error.WriteLine($"E {marketId.Value}: 該当するマーケットは存在しません.");
          return;
        }
        Console.Error.WriteLine($"# {market.Id} {market.Name}");
      } catch(Exception except) {
        Console.Error.WriteLine($"E {except.GetType().FullName} {except.Message}");
        Console.Error.WriteLine(except.StackTrace);
        return;
      }
    }

    var companies = ((marketId != null)
        ? await companyRepo.FindByMarketIdAsync(marketId.Value)
        : await companyRepo.FindAllAsync())
      .Where(c => c.IsEnabled);

    if(File.Exists(InvalidCompanyJsonPath)) {
      var invalidCompany = await Stq.IO.Json.ReadAsync<InvalidCompany>(InvalidCompanyJsonPath);

      var skipIndex = companies
        .Select((cmp, idx) => new { Index = idx, Company = cmp })
        .FirstOrDefault(obj => obj.Company.Id == invalidCompany.Company.Id)?
        .Index;

      if(skipIndex != null) {
        var targetCompany = companies.ElementAt(skipIndex.Value);
        Console.Error.WriteLine($"# Skip {skipIndex.Value} {targetCompany.Code} {targetCompany.Name}");
        companies = companies.Skip(skipIndex.Value);
      }
    }


    long cursor = 0;
    var companyCount = companies.Count();
    bool isSuccessful = true;

    using(var stooq = new Stooq(new Uri("socks5://localhost:9050"))) {
      /*
      // 株価のダウンロードを並列化しようとした試み.
      // System.Net.Http.HttpRequestException Connection refused (localhost:9050) が発生し失敗.

      // 比較的軽微な不可でダウンロード処理を並列化する.
      foreach(var pair in companies.Select((c, idx) => new { Company = c, Index = idx })
        .GroupBy(
            obj => obj.Index / 10,
            obj => obj.Company)) {
        Console.Error.WriteLine($"# Crawler No.: {pair.Key} / {companyCount / 10}");

        IEnumerable<Stq.Data.Company> partialCompanies = pair.ToArray();

        var downloadTasks = pair.Select(c => DownloadAsync(
              stooq:           stooq,
              company:         c,
              stockRepository: stockRepo))
          .ToArray();
        var returnCompanies = await Task.WhenAll(downloadTasks);

        try {
          var invalidCompanies = returnCompanies?
            .Where(c => c != null)
            .Cast<Stq.Data.Company>()
            .OrderBy(c => c.Code);

          if(invalidCompanies != null && invalidCompanies.Count() > 0) {
            var _c = invalidCompanies.First();
            Console.Error.WriteLine($"{_c.Code} {_c.Name}");
            break;
          }
        } catch { }
      }
      */

      foreach(var company in companies) {
        try {
          DateTime? d1 = null;
          var d2 = DateTime.Now.AddDays(-1).Date;

          try {
            d1 = (await stockRepo.FindByCompanyIdAsync(company.Id))
              .Max(stock => stock.Date)
              .AddDays(1);
          } catch { }

          Console.Error.WriteLine($"# {++cursor} / {companyCount}\t{company.Id}\t{company.Code} {company.Name}");
          Console.Error.WriteLine($"\t{d1?.ToString("yyyy-MM-dd") ?? string.Empty} -> {d2.ToString("yyyy-MM-dd")}");

          if(d1 != null && d1.Value.Date == DateTime.Now.Date) {
            Console.Error.WriteLine($"\tSkip.");
            continue;
          }

          var downloadStocks = await stooq.DownloadAsync(
              code: company.Code,
              d1:   d1,
              d2:   DateTime.Now.Date);

          if(downloadStocks.Count() > 0) {
            var stocks = downloadStocks.Select(ds => new Stq.Data.Stock() {
                CompanyId = company.Id,
                Date      = ds.Date,
                Open      = ds.Open,
                High      = ds.High,
                Low       = ds.Low,
                Close     = ds.Close,
                Volume    = ds.Volume });

            await stockRepo.InsertMultipleAsync(stocks);
            Console.Error.WriteLine($"\tInsert Stocks ({stocks.Count()})");
          }
        } catch(StooqNoDataException) {
          //
          Console.Error.WriteLine($"\tNo Data.");
        } catch(Exception except) {
#if DEBUG
          Console.Error.WriteLine($"E {except.GetType().FullName} {except.Message}");
          Console.Error.WriteLine(except.StackTrace);
#endif

          var invalidCompany = new InvalidCompany() {
            Company     = company,
                        PublishedAt = DateTime.Now,
          };
          await Stq.IO.Json.WriteAsync(InvalidCompanyJsonPath, invalidCompany);
          isSuccessful = false;

          break;
        }
      }
    }

    // 正常終了の場合, 異常時の情報を削除
    if(isSuccessful && File.Exists(InvalidCompanyJsonPath)) {
      File.Delete(InvalidCompanyJsonPath);
    }
  }

    /// <summary></summary>
    internal static async Task<Stq.Data.Company?> DownloadAsync(
        Stq.Net.Stooq    stooq,
        Stq.Data.Company company,
        IStockRepository stockRepository) {
      DateTime? d1 = null;
      DateTime d2 = DateTime.Now.Date;

        try {
          var stocks = await stockRepository.FindByCompanyIdAsync(companyId: company.Id);

          if(stocks.Count() > 0)
            d1 = stocks.Max(stock => stock.Date).Date.AddDays(1).Date;

          if(d1 != null && d1.Value.Date > d2.Date) {
            Console.Error.WriteLine($"# Skip {company.Code} {company.Name}");
            return null;
          }

          Console.Error.WriteLine($"\tDownloading... {d1?.ToString("yyyy-MM-dd") ?? "null"} => {d2.ToString("yyyy-MM-dd")}");
          // TODO ダウンロード実行
          var downloadedStocks = await stooq.DownloadAsync(
              code: company.Code,
              d1:   d1,
              d2:   d2);

          await stockRepository.InsertMultipleAsync(
              downloadedStocks.Select(ds => new Stq.Data.Stock() {
                CompanyId = company.Id,
                Date      = ds.Date,
                Open      = ds.Open,
                High      = ds.High,
                Low       = ds.Low,
                Close     = ds.Low,
                Volume    = ds.Volume }));
        } catch(StooqNoDataException) {
        } catch(Exception except) {
          Console.Error.WriteLine($"E {except.GetType().FullName} {except.Message}");
          Console.Error.WriteLine(except.StackTrace);

          return company;
        }

        return null;
    }
}


/// <summary></summary>
internal class InvalidCompany {
  /// <summary></summary>
  public Company Company { set; get; } = default;

  /// <summary></summary>
  public DateTime PublishedAt { set; get; } = DateTime.Now;
}
