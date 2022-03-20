namespace Stq.Net {
  /// <summary></summary>
  public sealed class StooqLimitException : StooqException {
    /// <summary></summary>
    public StooqLimitException(string? code = null) : base(code) { }
  }
}
