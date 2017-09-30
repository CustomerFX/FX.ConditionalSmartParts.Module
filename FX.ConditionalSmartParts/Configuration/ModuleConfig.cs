using System;
using System.Collections.Generic;
using System.ComponentModel;
using FX.ConditionalSmartParts.Utility;

namespace FX.ConditionalSmartParts.Configuration
{
    [DisplayName("Conditional Smart Part Module")]
    [TypeConverter(typeof(DisplayTypeConverter))]
    internal class ModuleConfig
    {
        public ModuleConfig()
        {
            Active = true;
            ConfigEntities = new List<ModuleConfigEntity>();
        }

        [DisplayName("Active")]
        [Description("Set whether this module is active or not for all entities. Set to false to turn off this module.")]
        public bool Active { get; set; }

        [DisplayName("Configured Entity")]
        [Description("A list of entities & configurations")]
        public List<ModuleConfigEntity> ConfigEntities { get; set; }
    }
}
