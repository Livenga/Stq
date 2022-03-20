namespace Stq.Repos.Data {
  using System;
  using System.Collections.Generic;

  /// <summary></summary>
  public class Company : Stq.Data.Company {
    /// <summary></summary>
    public Stq.Data.Market Market { set; get; } = default;
  }
}
