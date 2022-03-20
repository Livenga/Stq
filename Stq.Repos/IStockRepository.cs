namespace Stq.Repos {
  using System;
  using System.Collections.Generic;
  using System.Data;
  using System.Threading.Tasks;

  /// <summary></summary>
  public interface IStockRepository {
    /// <summary></summary>
    Task<IEnumerable<Stq.Data.Stock>> FindAllAsync();

    /// <summary></summary>
    Task<IEnumerable<Stq.Data.Stock>> FindByCompanyIdAsync(long companyId);

    /// <summary></summary>
    Task<IEnumerable<Stq.Data.Stock>> FindByCodeAsync(
        string    code,
        DateTime? stratOn = null);

    /// <summary></summary>
    Task<IEnumerable<Stq.Data.Stock>> FindByCodesAsync(IEnumerable<string> codes);

    /// <summary></summary>
    Task InsertAsync(Stq.Data.Stock stock);

    /// <summary></summary>
    Task InsertMultipleAsync(IEnumerable<Stq.Data.Stock> stocks);
  }
}
