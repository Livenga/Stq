namespace Stq.A.CalcLinearRegression;

using CsvHelper.Configuration.Attributes;
using Dapper;
using Stq.Db;
using Stq.Repos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


/// <summary></summary>
internal static class Program {
  /// <summary></summary>
  static async Task Main(string[] args) {
    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

    if(args.Length < 2) {
      var name = System.Reflection
        .Assembly
        .GetExecutingAssembly()
        .GetName()?
        .Name ?? "Stq.A.CalcLinearRegression";
      Console.Error.WriteLine($"Usage: {name} [DATASOURCE PATH] [GROUP ID?]");

      return;
    }

    if(! File.Exists(args[0])) {
      Console.Error.WriteLine($"{args[0]}: Not Found.");

      return;
    }

    var db = new SQLiteClient() { DataSource = args[0] };
    var groupRepo = new GroupRepository(db);
    var stockRepo = new StockRepository(db);

    Stq.Data.Group? group = null;
    try {
      group = await groupRepo.FindByIdAsync(System.Convert.ToInt64(args[1], 10));
    } catch(Exception) { }
    if(group == null) {
      Console.Error.WriteLine($"# 登録済みグループ一覧");
      foreach(var g in await groupRepo.FindAllAsync())
        Console.Error.WriteLine($"\t[{g.Id}]\t{g.Name}");

      return;
    }

    Console.Error.WriteLine($"[{group.Id}] {group.Name}\n");

    var addUpValues = new List<AddUpValue>();
    var companies = await db.QueryAsync(async (conn, trans) =>
        await GetCompaniesByGroupIdAsync(conn, trans, group.Id));

    foreach(var company in companies) {
      var stocks = await stockRepo.FindByCompanyIdAsync(company.Id);
      if(stocks.Count() == 0) {
        Console.Error.WriteLine($"# Skip {company.Code} {company.Name}");
        continue;
      }

      var dt = stocks.Max(stock => stock.Date);

      Console.Error.WriteLine($"{company.Code} {company.Name} {stocks.Count()}");

      // 1週間当たり
      var lgWeek = CalcLinearRegression(stocks, dt.AddDays(-7));
      // 1ヶ月当たり
      var lgMonth = CalcLinearRegression(stocks, dt.AddMonths(-1));
      // 3ヶ月当たり
      var lgThreeMonth = CalcLinearRegression(stocks, dt.AddMonths(-3));
      // 半年当たり
      var lgHalfYear = CalcLinearRegression(stocks, dt.AddMonths(-6));
      // 1年当たり
      var lgYear = CalcLinearRegression(stocks, dt.AddYears(-1));

      var auv = new AddUpValue() {
        Ticker              = company.Code,
        Name                = company.Name,
        MarketName          = company.Market.Name,
        WeekSlope           = lgWeek.Slope,
        WeekIntercept       = lgWeek.Intercept,
        MonthSlope          = lgMonth.Slope,
        MonthIntercept      = lgMonth.Intercept,
        ThreeMonthSlope     = lgThreeMonth.Slope,
        ThreeMonthIntercept = lgThreeMonth.Intercept,
        YearSlope           = lgYear.Slope,
        YearIntercept       = lgYear.Intercept };

      addUpValues.Add(auv);

      /*
      //
      var rawStocks = stocks.Select(stock => new RawStock() {
          Date   = stock.Date,
          Open   = stock.Open,
          High   = stock.High,
          Close  = stock.Close,
          Low    = stock.Low,
          Volume = stock.Volume });

      await Stq.IO.Csv.WriteRecordsAsync(
          Path.Combine("charts", $"chart-{company.Code}.csv"), rawStocks);

      var weekChart = rawStocks.GroupBy(
          stock => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
            time:           stock.Date,
            rule:           CalendarWeekRule.FirstFullWeek,
            firstDayOfWeek: DayOfWeek.Sunday),
          stock  => stock)
        .Select(stockPair => new RawStock() {
            Date   = stockPair.First().Date,
            Open   = stockPair.First().Open,
            High   = stockPair.Max(stock => stock.High),
            Close  = stockPair.Last().Close,
            Low    = stockPair.Min(stock => stock.Low),
            Volume = stockPair.Average(stock => stock.Volume) });

      await Stq.IO.Csv.WriteRecordsAsync(
          Path.Combine("charts", $"weekChart-{company.Code}.csv"), weekChart); */
    }

    await Stq.IO.Csv.WriteRecordsAsync($"{group.Name}-addup.csv", addUpValues);
  }


  /// <summary></summary>
  private static Stq.Calc.LinearRegression CalcLinearRegression(
      IEnumerable<Stq.Data.Stock> stocks,
      DateTime startOn) {
    var _stocks = stocks.Where(stock => stock.Date >= startOn);

    var xs = Enumerable.Range(0, _stocks.Count()).Select(idx => (double)idx).ToArray();
    var ys = _stocks.Select(stock => stock.Close).ToArray();

    return Calc.LinearRegression.Calculate(
        xs: xs,
        ys: ys);
  }

  /// <summary></summary>
  private static async Task<IEnumerable<Stq.Repos.Data.Company>> GetCompaniesByGroupIdAsync(
      IDbConnection  connection,
      IDbTransaction transaction,
      long           groupId) =>
    await connection.QueryAsync<Stq.Repos.Data.Company>(
        sql:
          "select c.*, m.* from companies as c\n" +
          "inner join markets as m on c.market_id = m.id\n" +
          "where c.id in (\n" +
          " select company_id from group_companies where group_id = @GroupId)\n" +
          "order by code",
        splitOn: "id,id",
        transaction: transaction,
        map: CompanyMap,
        types: new [] { typeof(Stq.Repos.Data.Company), typeof(Stq.Data.Market) },
        param: new { GroupId = groupId });

  private static Stq.Repos.Data.Company CompanyMap(object[] objs) {
    var c = (Stq.Repos.Data.Company)objs[0];
    c.Market = (Stq.Data.Market)objs[1];
    return c;
  }
}

/// <summary></summary>
internal class AddUpValue {
  /// <summary></summary>
  [Name("銘柄コード")]
  public string Ticker { set; get; } = string.Empty;

  /// <summary></summary>
  [Name("銘柄名")]
  public string Name { set; get; } = string.Empty;

  /// <summary></summary>
  [Name("証券所名")]
  public string MarketName { set; get; } = string.Empty;

  /// <summary></summary>
  [Name("週傾き")]
  public double WeekSlope { set; get; } = double.NaN;

  /// <summary></summary>
  [Name("週切片")]
  public double WeekIntercept { set; get; } = double.NaN;

  /// <summary></summary>
  [Name("月傾き")]
  public double MonthSlope { set; get; } = double.NaN;

  /// <summary></summary>
  [Name("月切片")]
  public double MonthIntercept { set; get; } = double.NaN;

  /// <summary></summary>
  [Name("3ヶ月傾き")]
  public double ThreeMonthSlope { set; get; } = double.NaN;

  /// <summary></summary>
  [Name("3ヶ月切片")]
  public double ThreeMonthIntercept { set; get; } = double.NaN;

  /// <summary></summary>
  [Name("年傾き")]
  public double YearSlope { set; get; } = double.NaN;

  /// <summary></summary>
  [Name("年切片")]
  public double YearIntercept { set; get; } = double.NaN;
}

/// <summary></summary>
internal class RawStock {
    /// <summary></summary>
    public DateTime Date { set; get; } = DateTime.Now.Date;

    /// <summary></summary>
    public double Open { set; get; } = double.NaN;

    /// <summary></summary>
    public double High { set; get; } = double.NaN;

    /// <summary></summary>
    public double Low { set; get; } = double.NaN;

    /// <summary></summary>
    public double Close { set; get; } = double.NaN;

    /// <summary></summary>
    public double? Volume { set; get; } = null;
}
