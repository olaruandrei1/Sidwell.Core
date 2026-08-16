namespace Sidwell.Core.Analytics.Extensions;

public static class DoubleExtensions
{
    public static double Round2(this double value)
    {
        if (double.IsNaN(value))
            return double.NaN;

        if (double.IsInfinity(value))
            return 0.0;

        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
