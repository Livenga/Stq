namespace Stq.Data {
  using System;


  /// <summary></summary>
  public class Company {
    /// <summary></summary>
    public long Id { set; get; } = 0;

    /// <summary></summary>
    public string Code { set; get; } = string.Empty;

    /// <summary></summary>
    public string Name { set; get; } = string.Empty;

    /// <summary></summary>
    public DateTime CreatedAt { set; get; } = DateTime.Now;

    /// <summary></summary>
    public long? MarketId { set; get; } = null;

    /// <summary></summary>
    public bool IsEnabled { set; get; } = true;
  }
}
