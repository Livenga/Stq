namespace Stq.Repos {
  using System.Collections.Generic;
  using System.Threading.Tasks;


  /// <summary></summary>
  public interface ICompanyRepository {
    /// <summary></summary>
    Task<IEnumerable<Data.Company>> FindAllAsync();

    /// <summary></summary>
    Task<IEnumerable<Data.Company>> FindByMarketIdAsync(long marketId);

    /// <summary></summary>
    Task InsertAsync(Stq.Data.Company company);

    /// <summary></summary>
    Task InsertMultipleAsync(IEnumerable<Stq.Data.Company> companies);
  }
}
