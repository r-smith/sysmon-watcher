using Sysmon_Watcher.Helpers;

namespace Sysmon_Watcher.Models
{
    internal static class GlobalData
    {
        public static LimitedObservableCollection<SysmonNetworkConnect> NetworkConnect { get; } = new LimitedObservableCollection<SysmonNetworkConnect>(1_000);
        public static LimitedObservableCollection<SysmonCreateProcess> CreateProcess { get; } = new LimitedObservableCollection<SysmonCreateProcess>(1_000);
        public static LimitedObservableCollection<SysmonFileCreate> FileCreate { get; } = new LimitedObservableCollection<SysmonFileCreate>(1_000);
        public static LimitedObservableCollection<SysmonRegistryEvent> RegistryEvent { get; } = new LimitedObservableCollection<SysmonRegistryEvent>(1_000);

        public static bool DisplayShortPaths = true;
    }
}
