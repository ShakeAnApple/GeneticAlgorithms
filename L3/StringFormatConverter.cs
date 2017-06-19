using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Data;
using System.Globalization;

namespace L3
{
    public class StringFormatConverter : MarkupExtension, IValueConverter
    {
        public string FormatString { get; set; }

        public StringFormatConverter()
        {
        }

        public StringFormatConverter(string formatString)
        {
            this.FormatString = formatString;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueStr = value == null ? string.Empty : value.ToString();

            try
            {
                return string.Format(this.FormatString, valueStr);
            }
            catch (FormatException ex)
            {
                var str = string.Format("Invalid format string '{0}' for value '{1}'", this.FormatString, valueStr);
                Debug.Print(ex.ToString());
                Debug.Print(str);
                return str;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
