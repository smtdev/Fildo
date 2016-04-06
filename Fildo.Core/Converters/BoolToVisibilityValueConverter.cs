namespace Fildo.Core.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Globalization;
    using MvvmCross.Platform.Converters;
    using MvvmCross.Platform.UI;

    public class BoolToVisibilityValueConverter : MvxValueConverter<bool, MvxVisibility>
    {
        protected override MvxVisibility Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value)
            {
                return MvxVisibility.Visible;
            }
            else
            {
                return MvxVisibility.Collapsed;
            }
        }
    }
}
