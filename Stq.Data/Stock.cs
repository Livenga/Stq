namespace Stq.Data {
  using System;


  /// <summary></summary>
  public class Stock {
    /// <summary></summary>
    public long CompanyId { set; get; } = 0;

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
}
