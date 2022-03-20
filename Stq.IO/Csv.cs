namespace Stq.IO {
  using CsvHelper;
  using CsvHelper.Configuration;
  using CsvHelper.TypeConversion;
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;


  /// <summary></summary>
  public static class Csv {
    /// <summary></summary>
    public static async Task<IEnumerable<T>> ReadRecordsAsync<T>(Stream stream) {
      using var reader = new StreamReader(stream);

      var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
      csvConfig.LeaveOpen         = false;
      csvConfig.MissingFieldFound = null;
      csvConfig.HeaderValidated   = null;

      using var csv = new CsvReader(
          reader:     reader,
          configuration: csvConfig);

      csv.Context.TypeConverterCache
        .RemoveConverter<DateTime>();
      csv.Context.TypeConverterCache
        .AddConverter<DateTime>(new DateTimeConverter("yyyy-MM-dd"));

      return await csv.GetRecordsAsync<T>().ToArrayAsync();
    }

    /// <summary></summary>
    public static async Task<IEnumerable<T>> ReadRecordsAsync<T>(string path) {
      using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
      return await ReadRecordsAsync<T>(stream);
    }

    /// <summary></summary>
    public static async Task WriteRecordsAsync<T>(
        Stream stream,
        IEnumerable<T> records) {
      using var writer = new StreamWriter(stream);
      using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

      csv.Context.TypeConverterCache.RemoveConverter<DateTime>();
      csv.Context.TypeConverterCache.AddConverter<DateTime>(new DateTimeConverter("yyyy-MM-dd"));

      await csv.WriteRecordsAsync(records);
      await stream.FlushAsync();
    }

    /// <summary></summary>
    public static async Task WriteRecordsAsync<T>(
        string path,
        IEnumerable<T> records) {
      var mode = File.Exists(path) ? FileMode.Truncate : FileMode.CreateNew;

      using var stream = File.Open(path, mode, FileAccess.Write);
      await WriteRecordsAsync<T>(stream, records);
    }
  }


  /// <summary></summary>
  internal class DateTimeConverter : DefaultTypeConverter {
    /// <summary></summary>
    public string Format { private set; get; } = string.Empty;

    /// <summary></summary>
    public DateTimeConverter(string format) {
      Format = format;
    }


    /// <summary></summary>
    public override object ConvertFromString(
        string        text,
        IReaderRow    row,
        MemberMapData memberMapData) =>
      DateTime.ParseExact(
          s:        text,
          format:   Format,
          provider: null);

    /// <summary></summary>
    public override string ConvertToString(
        object        value,
        IWriterRow    row,
        MemberMapData memberMapData) => (value is DateTime dt)
      ? dt.ToString(Format)
      : throw new NotSupportedException();
  }
}
