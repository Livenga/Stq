namespace Stq.IO.Test {
  using Microsoft.VisualBasic;
  using System.IO;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;
  using Xunit;
  using Xunit.Abstractions;


  /// <summary></summary>
  public class CsvTest {
    private readonly ITestOutputHelper outputHelper;
    /// <summary></summary>
    public CsvTest(ITestOutputHelper outputHelper) {
      System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

      this.outputHelper = outputHelper;
    }


    /// <summary></summary>
    [Theory]
    [InlineData("tokyo_se.csv")]
    public async Task GetJPXCompaniesAsyncTest(string fileName) {
      var csvPath = Path.Combine(
          "..", "..", "..", "..", "core_datas", fileName);
      var jpxCompanies = await Stq.IO.Csv.ReadRecordsAsync<Stq.Data.JPXCompany>(csvPath);

      var regex = new Regex("[０-９ａ-ｚＡ-Ｚ（）　]+", RegexOptions.Compiled);

      foreach(var jpx in jpxCompanies) {
        // プロパティの調整
        var name = regex.Replace(
            input:     jpx.Name,
            evaluator: m => Strings.StrConv(m.Value, VbStrConv.Narrow) ?? string.Empty);
        jpx.Name = name;
        foreach(var prop in jpx.GetType().GetProperties()) {
          if(prop.GetValue(jpx) is string _str && _str == "-")
            prop.SetValue(jpx, null);
        }

        outputHelper.WriteLine($"{jpx.Code} {jpx.Name} {jpx.ScaleCode ?? "null"} {jpx.IndustryCode33 ?? "null"}");
      }

      var adjustOutputPath = System.IO.Path.Combine(
          "..", "..", "..", "..", "core_datas", "adjust.csv");
      await Stq.IO.Csv
        .WriteRecordsAsync<Stq.Data.JPXCompany>(adjustOutputPath, jpxCompanies);
    }
  }
}
