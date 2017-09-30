using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace FX.ConditionalSmartParts.Utility
{
    internal class DisplayTypeConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var displayName = value.GetType().GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault() as DisplayNameAttribute;
            return displayName == null
                ? base.ConvertTo(context, culture, value, destinationType)
                : displayName.DisplayName;
        }
    }
}
