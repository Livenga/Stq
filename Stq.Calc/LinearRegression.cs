namespace Stq.Calc {
  using System;
  using System.Linq;


  /// <summary>線型回帰</summary>
  public class LinearRegression {
    /// <summary>傾き</summary>
    public double Slope => slope;

    /// <summary>切片</summary>
    public double Intercept => intercept;


    private readonly double slope;
    private readonly double intercept;


    /// <summary>コンストラクタ</summary>
    /// <param name="slope">傾き</param>
    /// <param name="intercept">切片</param>
    public LinearRegression(
        double slope,
        double intercept) {
      this.slope     = slope;
      this.intercept = intercept;
    }


    /// <summary></summary>
    /// <param name="xs"></param>
    /// <param name="ys"></param>
    public static LinearRegression Calculate(
        double[] xs,
        double[] ys) {
      if(xs.Length != ys.Length)
        // TODO Custom Exception
        throw new Exception();

      var (aveX, aveY) = (xs.Average(), ys.Average());
      var xVariance  = xs.Sum(x => Math.Pow(x - aveX, 2d));
      var covariance = Enumerable.Range(0, xs.Length)
        .Sum(idx => (xs[idx] - aveX) * (ys[idx] - aveY));

      var b = covariance / xVariance;
      var a = aveY - (b * aveX);

      return new LinearRegression(
          slope:     b,
          intercept: a);
    }


    /// <summary></summary>
    public override string ToString() => Intercept >= 0d
      ? $"{Slope:f2}x + {Intercept:f2}"
      : $"{Slope:f2}x - {(-Intercept):f2}";
  }
}
