namespace Stq.Calc.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;


/// <summary></summary>
public class LinearRegressionTest { 
  private readonly ITestOutputHelper outputHelper;

  /// <summary></summary>
  public LinearRegressionTest(ITestOutputHelper outputHelper) {
    this.outputHelper = outputHelper;
  }


  [Theory]
  [InlineData(15,
      480d, 650d, 760d, 560d, 480d, 680d, 710d, 390d, 980d, 1100d, 880d, 490d, 760d, 580d, 925d,
      120d, 190d, 210d, 200d, 190d, 230d, 175d, 165d, 200d, 320d, 195d, 170d, 270d, 150d, 260d)]
  [InlineData(5,
      5d, 7d, 4d, 5d, 8d,
      5d, 6d, 4d, 4d, 6d)]
  public void Test1(
      int             valueCount,
      params double[] values) {
    if(valueCount != (values.Length / 2)) {
      throw new ArgumentException(
          message:   "",
          paramName: "");
    }

    double[] xs = new double[valueCount], ys = new double[valueCount];
    Array.Copy(values, 0, xs, 0, valueCount);
    Array.Copy(values, valueCount, ys, 0, valueCount);

    (double aveX, double aveY) = (xs.Average(), ys.Average());

    var covariance = Enumerable
      .Range(0, xs.Length)
      .Sum(idx => (xs.ElementAt(idx) - aveX) * (ys.ElementAt(idx) - aveY));

    var xVariance = xs.Sum(x => Math.Pow(x - aveX, 2d));

    var b = covariance / xVariance;
    var a = aveY - (b * aveX);

    outputHelper.WriteLine($"共分散: {covariance}");
    outputHelper.WriteLine($"x 分散: {xVariance}");
    outputHelper.WriteLine($"回帰係数: {b}");
    outputHelper.WriteLine($"切片:     {a}");
    outputHelper.WriteLine($"\t{b:f3}x + {a:f3}");


    var lg = Stq.Calc.LinearRegression.Calculate(xs: xs, ys: ys);
    outputHelper.WriteLine($"\t{lg.Slope:f3}x + {lg.Intercept:f3}");
  }
}
