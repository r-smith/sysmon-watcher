using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Sysmon_Watcher.Models
{
    internal class SysmonRegistryEvent
    {
        public static ObservableCollection<Column> Columns { get; private set; }

        static SysmonRegistryEvent()
        {
            // Initialize the default GUI columns.
            Columns = new ObservableCollection<Column>
            {
                new Column(bindingPath: "Timestamp", header: "Timestamp", isSelectedByDefault: true),
                new Column(bindingPath: "RuleName", header: "Rule", isSelectedByDefault: false),
                new Column(bindingPath: "EventType", header: "Event Type", isSelectedByDefault: true),
                new Column(bindingPath: "UtcTime", header: "UTC Time", isSelectedByDefault: false),
                new Column(bindingPath: "ProcessGuid", header: "Process GUID", isSelectedByDefault: false),
                new Column(bindingPath: "ProcessId", header: "Process ID", isSelectedByDefault: false),
                new Column(bindingPath: "Image", header: "Image", isSelectedByDefault: true),
                new Column(bindingPath: "TargetObject", header: "Target Object", isSelectedByDefault: true),
                new Column(bindingPath: "Details", header: "Details", isSelectedByDefault: true),
                new Column(bindingPath: "User", header: "User", isSelectedByDefault: true),
            };
        }

        public DateTime Timestamp { get; set; }
        public string RuleName { get; set; }
        public string EventType { get; set; }
        public string UtcTime { get; set; }
        public string ProcessGuid { get; set; }
        public int ProcessId { get; set; }
        private string _image;
        public string Image
        {
            get
            {
                if (GlobalData.DisplayShortPaths)
                {
                    try { return Path.GetFileName(_image); }
                    catch { return _image; }
                }
                return _image;
            }
            set => _image = value;
        }
        public string TargetObject { get; set; }
        public string Details { get; set; }
        public string User { get; set; }
    }
}
