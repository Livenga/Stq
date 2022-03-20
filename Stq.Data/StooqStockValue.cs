namespace Stq.Data {
  using CsvHelper.Configuration.Attributes;
  using System;


  /// <summary></summary>
  public class StooqStopValue {
    /// <summary></summary>
    [Name("Date")]
    public DateTime Date { set; get; } = DateTime.Now.Date;

    /// <summary></summary>
    [Name("Open")]
    public double Open { set; get; } = double.NaN;

    /// <summary></summary>
    [Name("High")]
    public double High { set; get; } = double.NaN;

    /// <summary></summary>
    [Name("Low")]
    public double Low { set; get; } = double.NaN;

    /// <summary></summary>
    [Name("Close")]
    public double Close { set; get; } = double.NaN;

    /// <summary></summary>
    [Name("Volume")]
    public double? Volume { set; get; } = null;
  }
}
