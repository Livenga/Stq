namespace Stq.Data {
  using CsvHelper.Configuration.Attributes;


  /// <summary></summary>
  public class JPXCompany {
    /// <summary></summary>
    [Name("日付")]
    public string Date { set; get; } = string.Empty;

    [Name("コード")]
    public string Code { set; get; } = string.Empty;

    [Name("銘柄名")]
    public string Name { set; get; } = string.Empty;

    [Name("市場・商品区分")]
    public string? Market { set; get; } = null;

    [Name("33業種コード")]
    public string? IndustryCode33 { set; get; } = null;

    [Name("33業種区分")]
    public string? IndustryDivision33 { set; get; } = null;

    [Name("17業種コード")]
    public string? IndustryCode17 { set; get; } = null;

    [Name("17業種区分")]
    public string? IndustryDivision17 { set; get; } = null;

    [Name("規模コード")]
    public string? ScaleCode { set; get; } = null;

    [Name("規模区分")]
    public string? ScaleDivision { set; get; } = null;
  }
}
