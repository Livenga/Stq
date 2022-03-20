namespace Stq.Downloader.Company;

using Stq.Net;
using System;
using System.Threading.Tasks;


/// <summary></summary>
static class Program {
  /// <summary></summary>
  static async Task Main(string[] args) {
    if(args.Length == 0) {
      Console.Error.WriteLine($"# Stooq Markets");
      foreach(var _market in Enum.GetValues<StooqMarket>()) {
        Console.Error.WriteLine($"\t{(ushort)_market}\t{_market}");
      }
      return;
    }

    StooqMarket market;
    try {
      market = Enum.Parse<StooqMarket>(args[0]);
    } catch(Exception except) {
      Console.Error.WriteLine($"!!!Error!!! {except.GetType().FullName} : {except.Message}");
      return;
    }

    using var stooq = new Stooq();
    stooq.Cookie = null;

    Console.Error.WriteLine($"# Target Market {market}");

    var pathName = $"{market}-{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv";
    var stooqSimpleCompanies = await stooq.DownloadCompaniesAsync(market);
    await Stq.IO.Csv.WriteRecordsAsync(
        path:    pathName,
        records: stooqSimpleCompanies);
  }
}
