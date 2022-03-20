namespace Stq.Data {
  using System;


  /// <summary></summary>
  public class Market {
    /// <summary></summary>
    public long Id { set; get; } = 0;

    /// <summary></summary>
    public string Name { set; get; } = string.Empty;

    /// <summary></summary>
    public string? Comment { set; get; } = null;

    /// <summary></summary>
    public long? StooqId { set; get; } = null;

    /// <summary></summary>
    public DateTime CreatedAt { set; get; } = DateTime.Now;
  }
}
