using System.Collections.ObjectModel;

namespace Sysmon_Watcher.Models
{
    internal static class FilterData
    {
        public static ObservableCollection<Filter> Filters { get; } = new ObservableCollection<Filter>();
    }
}
