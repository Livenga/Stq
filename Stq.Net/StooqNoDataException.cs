namespace Stq.Net {
  /// <summary></summary>
  public sealed class StooqNoDataException : StooqException {
    /// <summary></summary>
    public StooqNoDataException(string? code = null) : base(code) { }
  }
}
