namespace Stq.Data {
  using System;


  /// <summary>グループ</summary>
  public class Group {
    /// <summary>ID</summary>
    public long Id { set; get; } = 0;

    /// <summary>名称</summary>
    public string Name { set; get; } = string.Empty;

    /// <summary>注釈</summary>
    public string? Comment { set; get; } = null;

    /// <summary>登録日時</summary>
    public DateTime CreatedAt { set; get; } = DateTime.Now;
  }


  /// <summary>グループと企業の交差テーブル様相</summary>
  public  class GroupCompanies {
    /// <summary>グループID</summary>
    public long GroupId { set; get; } = 0;

    /// <summary>企業ID</summary>
    public long CompanyId { set; get; } = 0;
  }
}
