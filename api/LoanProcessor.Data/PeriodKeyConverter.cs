using System.Text.RegularExpressions;

namespace LoanProcessor.Data;

/// <summary>
/// Converts between the wire/display period key format ("f202004") and the
/// first-of-month <see cref="DateOnly"/> used for storage and range comparisons.
/// </summary>
public static partial class PeriodKeyConverter
{
    public static string ToPeriodKey(DateOnly periodDate) =>
        $"f{periodDate.Year:D4}{periodDate.Month:D2}";

    public static bool TryParse(string periodKey, out DateOnly periodDate)
    {
        var match = PeriodKeyPattern().Match(periodKey);
        if (!match.Success)
        {
            periodDate = default;
            return false;
        }

        var year = int.Parse(match.Groups[1].Value);
        var month = int.Parse(match.Groups[2].Value);
        periodDate = new DateOnly(year, month, 1);
        return true;
    }

    [GeneratedRegex(@"^f(\d{4})(\d{2})$")]
    private static partial Regex PeriodKeyPattern();
}
