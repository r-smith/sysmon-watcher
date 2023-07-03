using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Sysmon_Watcher.Models
{
    internal class SysmonCreateProcess
    {
        public static ObservableCollection<Column> Columns { get; private set; }

        static SysmonCreateProcess()
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
                new Column(bindingPath: "FileVersion", header: "File Version", isSelectedByDefault: false),
                new Column(bindingPath: "Description", header: "Description", isSelectedByDefault: false),
                new Column(bindingPath: "Product", header: "Product", isSelectedByDefault: false),
                new Column(bindingPath: "Company", header: "Company", isSelectedByDefault: false),
                new Column(bindingPath: "OriginalFileName", header: "Original FileName", isSelectedByDefault: false),
                new Column(bindingPath: "CommandLine", header: "Command Line", isSelectedByDefault: true),
                new Column(bindingPath: "CurrentDirectory", header: "Current Directory", isSelectedByDefault: false),
                new Column(bindingPath: "User", header: "User", isSelectedByDefault: true),
                new Column(bindingPath: "LogonGuid", header: "Logon GUID", isSelectedByDefault: false),
                new Column(bindingPath: "LogonId", header: "Logon ID", isSelectedByDefault: true),
                new Column(bindingPath: "TerminalSessionId", header: "Terminal Session ID", isSelectedByDefault: false),
                new Column(bindingPath: "IntegrityLevel", header: "Integrity Level", isSelectedByDefault: false),
                new Column(bindingPath: "Hashes", header: "Hashes", isSelectedByDefault: false),
                new Column(bindingPath: "ParentProcessGuid", header: "Parent Process GUID", isSelectedByDefault: false),
                new Column(bindingPath: "ParentProcessId", header: "Parent Process ID", isSelectedByDefault: false),
                new Column(bindingPath: "ParentImage", header: "Parent Image", isSelectedByDefault: true),
                new Column(bindingPath: "ParentCommandLine", header: "Parent Command Line", isSelectedByDefault: false),
                new Column(bindingPath: "ParentUser", header: "Parent User", isSelectedByDefault: true),
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
        public string FileVersion { get; set; }
        public string Description { get; set; }
        public string Product { get; set; }
        public string Company { get; set; }
        public string OriginalFileName { get; set; }
        public string CommandLine { get; set; }
        public string CurrentDirectory { get; set; }
        public string User { get; set; }
        public string LogonGuid { get; set; }
        public string LogonId { get; set; }
        public int TerminalSessionId { get; set; }
        public string IntegrityLevel { get; set; }
        public string Hashes { get; set; }
        public string ParentProcessGuid { get; set; }
        public int ParentProcessId { get; set; }
        private string _parentImage;
        public string ParentImage
        {
            get
            {
                if (GlobalData.DisplayShortPaths)
                {
                    try { return Path.GetFileName(_parentImage); }
                    catch { return _parentImage; }
                }
                return _parentImage;
            }
            set => _parentImage = value;
        }
        public string ParentCommandLine { get; set; }
        public string ParentUser { get; set; }
    }
}
