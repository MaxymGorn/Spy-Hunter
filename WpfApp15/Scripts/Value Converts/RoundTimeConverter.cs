using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WpfApp15.Scripts.Value_Converts
{
    class RoundTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan time;
            if (!TimeSpan.TryParse(value.ToString(), out time))
            {

            }
            return time.Days+":"+time.Hours+":"+time.Minutes+":"+time.Seconds+":"+time.Milliseconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
