using System.Runtime.InteropServices;
using System.Text;

namespace Sidwell.Core.Infrastructure.Recalc;

[StructLayout(LayoutKind.Sequential)]
public struct NativeFundamentalSnapshot
{
    public int FiscalYear;
    public int Pad0;

    public double Revenue;
    public double CostOfRevenue;
    public double GrossProfit;
    public double OperatingIncome;
    public double Ebit;
    public double Ebitda;
    public double NetIncome;
    public double Eps;
    public double SharesOutstanding;
    public double SgaExpense;
    public double RdExpense;
    public double InterestExpense;

    public double TotalAssets;
    public double CurrentAssets;
    public double Cash;
    public double Receivables;
    public double Inventory;
    public double PpAndE;
    public double TotalLiabilities;
    public double CurrentLiabilities;
    public double LongTermDebt;
    public double RetainedEarnings;
    public double TotalEquity;

    public double OperatingCashFlow;
    public double Capex;
    public double FreeCashFlow;
    public double Depreciation;

    public double MarketCap;
    public double Price;
    public double DividendPerShare;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct NativeScoreResult
{
    public int AlgoId;
    public int Status;
    public double Score;
    public double RawValue;
    public double Confidence;
    public fixed byte Interpretation[64];

    public string GetInterpretation()
    {
        fixed (byte* p = Interpretation)
        {
            int len = 0;
            while (len < 64 && p[len] != 0)
            {
                len++;
            }

            return len == 0 ? string.Empty : Encoding.UTF8.GetString(p, len);
        }
    }
}

public static partial class SidwellQuantNative
{
    private const string LibraryName = "sidwell_core_algorithms";

    [LibraryImport(LibraryName)]
    public static partial IntPtr sw_engine_create(nuint arenaBytes);

    [LibraryImport(LibraryName)]
    public static partial void sw_engine_destroy(IntPtr engine);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int sw_load_fundamentals_raw(
        IntPtr engine,
        string symbol,
        [In] NativeFundamentalSnapshot[] snapshots,
        nuint count
    );

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int sw_load_prices(
        IntPtr engine,
        string symbol,
        [In] double[] ohlcvFlat,
        nuint rows
    );

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int sw_score_ticker(
        IntPtr engine,
        string symbol,
        [Out] NativeScoreResult[] outScores,
        ref nuint outCount
    );

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int sw_get_composite(
        IntPtr engine,
        string symbol,
        out double outComposite,
        out double outConfidence
    );

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int sw_algo_composite(
        IntPtr engine,
        string symbol,
        double technicalScore,
        out NativeScoreResult outScore
    );

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int sw_technical_score(
        IntPtr engine,
        string symbol,
        int philosophy,
        out double outScore
    );
}
