using System.ComponentModel;

namespace Sysmon_Watcher.Models
{
    internal class Filter
    {
        public Column Column { get; set; }
        public ComparisonType Comparison { get; set; }
        public string Value { get; set; }
        public ActionType Action { get; set; }
        public bool IsCommitted { get; set; } = false;
        public bool IsDeletePending { get; set; } = false;

        public enum ComparisonType
        {
            [Description("is")]
            equals,
            [Description("is not")]
            doesNotEqual,
            [Description("contains")]
            contains,
            [Description("excludes")]
            doesNotContain,
        }

        public enum ActionType
        {
            include,
            exclude,
        }
    }
}
