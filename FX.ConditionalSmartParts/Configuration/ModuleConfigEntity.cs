using System;
using System.Collections.Generic;
using System.ComponentModel;
using FX.ConditionalSmartParts.Utility;

namespace FX.ConditionalSmartParts.Configuration
{
    [DisplayName("Conditional Smart Part Entity")]
    [TypeConverter(typeof(DisplayTypeConverter))]
    internal class ModuleConfigEntity
    {
        public ModuleConfigEntity()
        {
            ConfigValues = new List<ModuleConfigValue>();
        }

        [DisplayName("Entity")]
        [Description("The entity you wish to use to conditionally show Smart Parts for")]
        public string Entity { get; set; }

        [DisplayName("Entity Property")]
        [Description("The property on the current entity you wish to use to conditionally show Smart Parts for")]
        public string EntityProperty { get; set; }

        [DisplayName("Field Values")]
        [Description("The values of the property to conditionally show Smart Parts for")]
        public List<ModuleConfigValue> ConfigValues { get; set; }
    }
}
