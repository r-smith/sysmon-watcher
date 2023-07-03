using System.ComponentModel;
using System.Reflection;

namespace Sysmon_Watcher.Helpers
{
    internal class EnumWithDescription<TEnum>
    {
        public TEnum Value { get; }
        public string Description { get; }

        public EnumWithDescription(TEnum value)
        {
            Value = value;
            Description = GetEnumDescription(value);
        }

        private string GetEnumDescription(TEnum value)
        {
            DescriptionAttribute descriptionAttribute = typeof(TEnum).
                GetField(value.ToString()).
                GetCustomAttribute<DescriptionAttribute>();
            return descriptionAttribute?.Description ?? value.ToString();
        }
    }
}
