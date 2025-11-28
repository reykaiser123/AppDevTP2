using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gatchapon.Converters
{
    internal class BoolToClaimColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If the task is completed, return green; otherwise, return original color
            return (bool)value ? Color.FromArgb("#4CAF50") : Color.FromArgb("#8B7E74");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
