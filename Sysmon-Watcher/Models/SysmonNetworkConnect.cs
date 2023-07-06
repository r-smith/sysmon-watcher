using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;

namespace Sysmon_Watcher.Models
{
    internal class SysmonNetworkConnect
    {
        public static ObservableCollection<Column> Columns { get; private set; }

        static SysmonNetworkConnect()
        {
            // Initialize the default GUI columns.
            Columns = new ObservableCollection<Column>
            {
                new Column(bindingPath: "Timestamp", header: "Timestamp", isSelectedByDefault: true),
                new Column(bindingPath: "RuleName", header: "Rule", isSelectedByDefault: false),
                new Column(bindingPath: "UtcTime", header: "UTC Time", isSelectedByDefault: false),
                new Column(bindingPath: "ProcessGuid", header: "Process GUID", isSelectedByDefault: false),
                new Column(bindingPath: "ProcessId", header: "Process ID", isSelectedByDefault: false),
                new Column(bindingPath: "Image", header: "Image", isSelectedByDefault: true),
                new Column(bindingPath: "User", header: "User", isSelectedByDefault: true),
                new Column(bindingPath: "Protocol", header: "Protocol", isSelectedByDefault: false),
                new Column(bindingPath: "Initiated", header: "Initiated", isSelectedByDefault: false),
                new Column(bindingPath: "SourceIsIpv6", header: "Src IPv6?", isSelectedByDefault: false),
                new Column(bindingPath: "SourceIp", header: "Src IP", isSelectedByDefault: true),
                new Column(bindingPath: "SourceHostname", header: "Src Hostname", isSelectedByDefault: false),
                new Column(bindingPath: "SourcePort", header: "Src Port", isSelectedByDefault: false),
                new Column(bindingPath: "SourcePortName", header: "Src Service", isSelectedByDefault: false),
                new Column(bindingPath: "DestinationIsIpv6", header: "Dst IPv6?", isSelectedByDefault: false),
                new Column(bindingPath: "DestinationIp", header: "Dst IP", isSelectedByDefault: true),
                new Column(bindingPath: "DestinationHostname", header: "Dst Hostname", isSelectedByDefault: true),
                new Column(bindingPath: "DestinationPort", header: "Dst Port", isSelectedByDefault: true),
                new Column(bindingPath: "DestinationPortName", header: "Dst Service", isSelectedByDefault: false),
            };
        }

        public DateTime Timestamp { get; set; }
        public string RuleName { get; set; }
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
        public string User { get; set; }
        public string Protocol { get; set; }
        public bool Initiated { get; set; }
        public bool SourceIsIpv6 { get; set; }
        public IPAddress SourceIp { get; set; }
        public string SourceHostname { get; set; }
        public int SourcePort { get; set; }
        public string SourcePortName { get; set; }
        public bool DestinationIsIpv6 { get; set; }
        public IPAddress DestinationIp { get; set; }
        public string DestinationHostname { get; set; }
        public int DestinationPort { get; set; }
        public string DestinationPortName { get; set;}
    }
}
