using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Converters/FriendStatusToColorConverter.cs
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace gatchapon.Converters
{
    public class FriendStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status)
                {
                    case "Friends": return Color.FromArgb("#A58668"); // Or a grey color if you want to make it look unclickable
                    case "Requested": return Color.FromArgb("#FFC107"); // Orange/Yellow
                    case "Add Friend": return Color.FromArgb("#4CAF50"); // Green
                }
            }
            return Color.FromArgb("#CCCCCC"); // Default grey
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
