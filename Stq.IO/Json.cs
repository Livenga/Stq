namespace Stq.IO {
  using System;
  using System.IO;
  using System.Text.Json;
  using System.Threading.Tasks;


  /// <summary></summary>
  public static class Json {
    private static readonly JsonSerializerOptions opts = new JsonSerializerOptions() {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = true,
    };


    /// <summary></summary>
    public static async Task<T> ReadAsync<T>(Stream stream) {
      var objs = await JsonSerializer.DeserializeAsync<T>(
          utf8Json: stream,
          options:  opts);

      return objs ?? throw new NullReferenceException();
    }

    /// <summary></summary>
    public static async Task<T> ReadAsync<T>(string path) {
      using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
      return await ReadAsync<T>(stream);
    }


    /// <summary></summary>
    public static async Task WriteAsync<T>(
        Stream stream,
        T value) {
      await JsonSerializer.SerializeAsync(
          utf8Json: stream,
          options:  opts,
          value:    value);
    }

    /// <summary></summary>
    public static async Task WriteAsync<T>(
        string path,
        T      value) {
      var mode = File.Exists(path) ? FileMode.Truncate : FileMode.CreateNew;

      using var stream = File.Open(path, mode, FileAccess.Write);
      await WriteAsync<T>(stream, value);
    }
  }
}
