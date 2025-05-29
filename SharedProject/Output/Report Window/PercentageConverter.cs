using System.Globalization;
using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Output
{
    internal class PercentageConverter : MultiValueConverter<int, int, string>
    {
        protected override string Convert(int total, int percentValue, object parameter, CultureInfo culture)
        {
            if (total == 0)
            {
                return "---";
            }

            double d = (double)percentValue / total;
            int decimals = 2;

            if (parameter != null && int.TryParse(parameter.ToString(), out int parsedDecimals))
            {
                decimals = parsedDecimals;
            }

            return (d * 100).ToString($"F{decimals}", culture) + " %";
        }
    }

}
