using System.Globalization;

namespace ProcureFlow.Web.Helpers;

public static class CurrencyFormatter
{
    private static readonly CultureInfo CroatianCulture = CultureInfo.GetCultureInfo("hr-HR");

    public static string ToEuro(decimal amount)
    {
        return $"{amount.ToString("N2", CroatianCulture)} €";
    }
}
