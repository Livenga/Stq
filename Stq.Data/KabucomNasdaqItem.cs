namespace Stq.Data {
  using CsvHelper.Configuration.Attributes;


  /// <summary>カブコム証券米国株リスト項目</summary>
  public class KabucomNasdaqItem {
    /// <summary>Ticker</summary>
    [Name("ティッカー")]
    public string Ticker { set; get; } = string.Empty;

    /// <summary>銘柄名</summary>
    [Name("銘柄名")]
    public string Name { set; get; } = string.Empty;

    /// <summary>銘柄名読み</summary>
    [Name("銘柄名カナ(銘柄名カナ略称)")]
    public string NameRuby { set; get; } = string.Empty;

    /// <summary></summary>
    [Name("業種")]
    public string? Industry { set; get; } = null;
  }
}
