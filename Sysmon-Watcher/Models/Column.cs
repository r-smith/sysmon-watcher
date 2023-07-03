namespace Sysmon_Watcher.Models
{
    public class Column
    {
        public string BindingPath { get; private set; }
        public string Header { get; private set; }
        public bool IsSelected { get; set; }
        public bool IsSelectedClone { get; set; }
        public bool IsSelectedByDefault { get; private set; }

        public Column(string bindingPath, string header, bool isSelectedByDefault)
        {
            BindingPath = bindingPath;
            Header = header;
            IsSelected = isSelectedByDefault;
            IsSelectedByDefault = isSelectedByDefault;
        }
    }
}
