#pragma warning disable CS8625

namespace Stq.Graphics.CreateChart;

using Dapper;
using ScottPlot;
using Stq.Repos;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


/// <summary></summary>
static class Program {
  /// <summary></summary>
  static async Task Main(string[] args) {
    if(args.Length == 0)
      return;

    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

    var codes = new string[args.Length - 1];
    Array.Copy(
        sourceArray:      args,
        sourceIndex:      1,
        destinationArray: codes,
        destinationIndex: 0,
        length:           args.Length - 1);
    if(codes.Length == 0)
      return;

    var n = DateTime.Now;
    var db = new Stq.Db.SQLiteClient() { DataSource = args[0] };
    var stockRepository = new StockRepository(db);

    var startOn = new DateTime(
        year:  (DateTime.Now.Year - 3),
        month: 1,
        day:   1);
    var stocks = await db.QueryAsync(
        async (conn, trans) => await StockFindByCodesAsync(conn, trans, codes, startOn));

    foreach(var groupedStocks in stocks.GroupBy(
          keySelector:     stock => stock.Company,
          elementSelector: stock => stock,
          comparer:        new CompanyEqualityComparer())) {
      var plt = new ScottPlot.Plot(4096, 2160);
      var c = groupedStocks.Key;
      Console.Error.WriteLine($"# {c.Code} {c.Name}");

      var ohlcStocks = groupedStocks
        .Select(s => new OHLC(
              open:      s.Open,
              high:      s.High,
              low:       s.Low,
              close:     s.Close,
              timeStart: s.Date,
              timeSpan:  TimeSpan.FromDays(1),
              volume:    s.Volume ?? 0d))
        .ToArray();

      var financePlot = plt.AddCandlesticks(ohlcStocks);
      plt.XAxis.DateTimeFormat(true);

      // 移動平均 5-25
      var sma5  = financePlot.GetSMA(5);
      var sma25 = financePlot.GetSMA(25);

      var pltSma5  = plt.AddScatterLines(
          xs:        sma5.xs,
          ys:        sma5.ys,
          color:     System.Drawing.Color.Blue,
          lineWidth: 2);
      pltSma5.Label = "移動平均 5";

      var pltSma25 = plt.AddScatterLines(
          xs:        sma25.xs,
          ys:        sma25.ys,
          color:     System.Drawing.Color.Navy,
          lineWidth: 2);
      pltSma25.Label = "移動平均 25";

      // 回帰直線
      var xs = ohlcStocks.Select(s => s.DateTime.ToOADate()).ToArray();
      var ys = ohlcStocks.Select(s => s.Close).ToArray();
      var lg = new ScottPlot.Statistics.LinearRegressionLine(xs: xs, ys: ys);
      var adjustLg = Stq.Calc.LinearRegression.Calculate(
          xs: Enumerable.Range(0, xs.Length).Select(idx => (double)idx).ToArray(),
          ys: ys);

      var lgLine = plt.AddLine(
          slope:     lg.slope,
          offset:    lg.offset,
          xLimits:   (xs[0], xs[xs.Length - 1]),
          lineWidth: 2);
      lgLine.Label = (lg.offset >= 0)
        ? $"回帰直線 {adjustLg.Slope:f2}x +{adjustLg.Intercept:f2}"
        : $"回帰直線 {adjustLg.Slope:f2}x {adjustLg.Intercept:f2}";

      var legend = plt.Legend();
      legend.IsVisible = true;
      legend.Location  = Alignment.UpperLeft;
      legend.FontSize  = 24f;

      var path = Path.Combine("charts", $"{c.Code}.png");
      plt.SaveFig(path);
    }
  }


  internal static async Task<IEnumerable<Stock>> StockFindByCodesAsync(
      IDbConnection       connection,
      IDbTransaction      transaction,
      IEnumerable<string> codes,
      DateTime?           startOn = null) {
        string sql;
        if(startOn == null) {
          sql = "select * from stocks as s\n" +
            "inner join companies as c on s.company_id = c.id\n" +
            "where c.code in @Codes;";
        } else {
          sql = "select * from stocks as s\n" +
            "inner join companies as c on s.company_id = c.id\n" +
            "where c.code in @Codes and s.Date >= @StartOn;";
        }
    return await connection.QueryAsync<Stock>(
        sql: sql,
        transaction: transaction,
        splitOn: "company_id,id",
        map: Map,
        types: new [] { typeof(Stock), typeof(Stq.Data.Company) },
        param: new {
          Codes   = codes.ToArray(),
          StartOn = startOn });
  }

  internal static Stock Map(object[] objs) {
    var s = (Stock)objs[0];
    s.Company = (Stq.Data.Company)objs[1];
    return s;
  }
}


/// <summary></summary>
internal class Stock : Stq.Data.Stock {
  /// <summary></summary>
  public Stq.Data.Company Company { set; get; } = default;
}

internal class CompanyEqualityComparer : IEqualityComparer<Stq.Data.Company> {
  /// <summary></summary>
  public bool Equals(
      Stq.Data.Company? x,
      Stq.Data.Company? y) => (x == null || y == null)
    ? false
    : x.Id == y.Id;

  /// <summary></summary>
  public int GetHashCode(Stq.Data.Company obj) =>
    obj.Id.GetHashCode();
}
