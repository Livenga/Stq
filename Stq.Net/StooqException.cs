namespace Stq.Net {
  using System;

  /// <summary></summary>
  public class StooqException : Exception {
    /// <summary></summary>
    public string? Code => code;

    private readonly string? code;

    /// <summary></summary>
    public StooqException(string? code) {
      this.code = code;
    }
  }
}
