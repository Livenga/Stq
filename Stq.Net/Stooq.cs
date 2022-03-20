namespace Stq.Net {
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;


  /// <summary></summary>
  public class Stooq : IDisposable {
    /// <summary></summary>
    public string? Cookie { set; get; } = null;

    private readonly HttpClient client;

    /// <summary></summary>
    public Stooq(Uri? proxyUri = null) {
      HttpClientHandler? handler = null;

      if(proxyUri != null) {
        handler = new HttpClientHandler();
        handler.UseCookies = true;
        handler.UseProxy   = true;
        handler.Proxy      = new WebProxy(proxyUri);
      }

      client = (handler != null)
        ? new HttpClient(handler)
        : new HttpClient();
    }


    /// <summary></summary>
    public async Task<IEnumerable<Stq.Data.StooqStopValue>> DownloadAsync(
        string    code,
        DateTime? d1 = null,
        DateTime? d2 = null) {
      var queries = new Dictionary<string, string>();
      queries.Add("s", code);

      if(d1 != null || d2 != null) {
        if((d1 != null && d2 != null) && d1 > d2) {
          // 入れ替え
          var dt = d1;
          d1 = d2;
          d2 = dt;
        }

        if(d1 != null)
          queries.Add("d1", d1.Value.ToString("yyyyMMdd"));
        if(d2 != null)
          queries.Add("d2", d2.Value.ToString("yyyyMMdd"));
      }

      var query = string.Join(
          "&",
          queries
          .Select(kvp => $"{kvp.Key}={kvp.Value}")
          .ToArray());

      var uri = new Uri($"https://stooq.com/q/d/l/?{query}");
#if DEBUG
      Debug.WriteLine($"D Downloading... {uri}");
#endif

      var buffer = await SendAsync(HttpMethod.Get, uri);

      switch(buffer.Length) {
        case 7:
          throw new StooqNoDataException(code);
        case 29:
          throw new StooqLimitException(code);
      }

      using(var stream = new MemoryStream(buffer)) {
        stream.Position = 0;

        try {
          return await Stq.IO.Csv.ReadRecordsAsync<Stq.Data.StooqStopValue>(stream);
        } catch {
#if DEBUG
          var message = System.Text.Encoding.UTF8.GetString(buffer);
          Console.Error.WriteLine($"E {message} ({message.Length})");
#endif
          throw;
        }
      }
    }


    /// <summary></summary>
    public async Task<IEnumerable<StooqSimpleCompany>> DownloadCompaniesAsync(StooqMarket market) {
      var parser = new AngleSharp.Html.Parser.HtmlParser();
      var regex = new Regex("(?<Current>[0-9]{1,6}) - (?<Next>[0-9]{1,6}) from (?<Last>[0-9]{1,6})", RegexOptions.Compiled);

      int currentPageNumber = 1;
      int? lastPageNumber = null;

      var companies = new List<StooqSimpleCompany>();

      do {
        var uri = new Uri($"https://stooq.com/t/tr/?m={(short)market}&l={currentPageNumber}");
#if DEBUG
        Console.Error.WriteLine($"\t{uri} Downloading... {currentPageNumber} / {lastPageNumber ?? -1}");
#endif

        var buffer = await SendAsync(
            method:     HttpMethod.Get,
            requestUri: uri);

        AngleSharp.Html.Dom.IHtmlDocument doc;
        using(var stream = new MemoryStream(buffer)) {
          stream.Position = 0;
          doc = await parser.ParseDocumentAsync(stream, default);
        }

        if(lastPageNumber == null) {
          var strNav = doc.QuerySelectorAll("td#f13").FirstOrDefault(td => regex.IsMatch(td.TextContent))?.TextContent;
          if(strNav != null) {
            var companiesCount  = System.Convert
              .ToInt32(regex.Match(strNav).Groups["Last"].Value, 10);

            lastPageNumber = (companiesCount % 50) > 0
              ? (companiesCount / 50) + 1
              : companiesCount / 50;
          }
        }

        var trs = doc.QuerySelectorAll("table#fth1.fth1 > tbody > tr")
          .Where(tr => tr is AngleSharp.Html.Dom.IHtmlTableRowElement)
          .Cast<AngleSharp.Html.Dom.IHtmlTableRowElement>();
        foreach(var tr in trs) {
          (var code, var name) = (tr.Cells[3].TextContent, tr.Cells[4].TextContent);
          companies.Add(new StooqSimpleCompany() { Code = code, Name = name });
        }

        if(trs.Count() == 0)
          break;
      } while(lastPageNumber != null && currentPageNumber++ < lastPageNumber);

      return companies;
    }

    /// <summary></summary>
    private async Task<byte[]> SendAsync(
        HttpMethod method,
        Uri requestUri,
        IDictionary<string, object?>? headers = null) {
      var req = new HttpRequestMessage();
      req.Method     = method;
      req.RequestUri = requestUri;

      if(Cookie != null)
        req.Headers.Add("Cookie", Cookie);

      // NOTE 引数 headers に Cookie が含まれている場合,
      // 引数の値を優先する.
      if(headers != null && headers.Count > 0) {
        foreach(var header in headers.Where(kvp => kvp.Value != null)) {
          req.Headers.Add(
              name:  header.Key,
              value: header.Value?.ToString());
        }
      }

      using var resp = await client.SendAsync(request: req);
      if(! resp.IsSuccessStatusCode)
        // TODO Custom Exception
        throw new Exception();

      return await resp.Content.ReadAsByteArrayAsync();
    }


    /// <summary></summary>
    public void Dispose() {
      client.Dispose();
    }
  }
}
