using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TimeTracker
{
    [ValueConversion(typeof(long), typeof(string))]
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan ts = TimeSpan.FromSeconds((long)value);

            string outputString = "";
            if (ts.Hours > 0)
            {
                outputString += $"{ts.Hours} hours ";
            }
            if (ts.Minutes > 0)
            {
                outputString += $"{ts.Minutes} minutes ";
            }
            if (ts.Seconds > 0)
            {
                outputString += $"{ts.Seconds} seconds";
            }

            return outputString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
