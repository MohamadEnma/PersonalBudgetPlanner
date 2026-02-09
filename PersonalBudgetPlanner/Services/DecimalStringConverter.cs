using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PersonalBudgetPlanner.Services
{
    public class DecimalStringConverter : IValueConverter
    {
        // Convert from decimal (VM) to string (UI)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d)
                return d.ToString("G", culture);
            return string.Empty;
        }

        // Convert from string (UI) to decimal (VM)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = (value ?? string.Empty).ToString().Trim();
            if (string.IsNullOrEmpty(s))
                return 0m;

            if (decimal.TryParse(s, NumberStyles.Number, culture, out var result))
                return result;

            // If parse fails, return Binding.DoNothing so WPF keeps old value or you can throw
            return Binding.DoNothing;
        }
    }
}
