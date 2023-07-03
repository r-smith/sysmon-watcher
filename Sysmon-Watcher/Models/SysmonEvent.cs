namespace Sysmon_Watcher.Models
{
    public enum SysmonEvent
    {
        // Sysmon Event IDs: https://learn.microsoft.com/en-us/sysinternals/downloads/sysmon

        ProcessCreated = 1,
        ProcessChangedFileCreationTime = 2,
        NetworkConnection = 3,
        ServiceStateChange = 4,
        ProcessTerminated = 5,
        DriverLoaded = 6,
        ImageLoaded = 7,
        CreateRemoteThread = 8,
        RawAccessRead = 9,
        ProcessAccess = 10,
        FileCreate = 11,
        RegistryEvent = 12,
        RegistryValueSet = 13,
        RegistryValueRename = 14,
        FileCreateStreamHash = 15,
        ServiceConfigurationChange = 16,
        PipeCreated = 17,
        PipeConnected = 18,
        WmiEventFilter = 19,
        WmiEventConsumer = 20,
        WmiEventConsumerToFilter = 21,
        DnsQuery = 22,
        FileDelete = 23,
        ClipboardChange = 24,
        ProcessTampering = 25,
        FileDeleteDetected = 26,
        FileBlockExecutable = 27,
        FileBlockShredding = 28,
        Error = 255,
    }
}
