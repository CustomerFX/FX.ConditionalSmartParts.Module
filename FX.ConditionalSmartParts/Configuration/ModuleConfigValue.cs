using System;
using System.Collections.Generic;
using System.ComponentModel;
using FX.ConditionalSmartParts.Utility;

namespace FX.ConditionalSmartParts.Configuration
{
    [DisplayName("Value")]
    [TypeConverter(typeof(DisplayTypeConverter))]
    internal class ModuleConfigValue
    {
        public ModuleConfigValue()
        {
            SmartParts = new List<string>();
        }

        [DisplayName("Property Value")]
        [Description("The value of the property to conditionally show Smart Parts for")]
        public object Value { get; set; }

        [DisplayName("Smart Parts")]
        [Description("A list of Smart Parts IDs to show for the property value")]
        public List<string> SmartParts { get; set; }
    }
}
