using Sysmon_Watcher.Models;
using System;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Sysmon_Watcher.Helpers
{
    internal static class WindowsEventWatcher
    {
        public async static Task SubscribeToSysmonEvents(EventWatcherOptions options)
        {
            // Build time range string used for event query.
            string range = "";
            if (options.ShouldReadExistingEvents && !options.ShouldReadAllEvents)
            {
                range = $"[System[TimeCreated[timediff(@SystemTime) &lt;= {options.EventRangeMilliseconds}]]]";
            }

            // Build event log query for retreiving Sysmon events.
            string eventQuery =
                "<QueryList>"
                + "<Query Id='0'>"
                + $"  <Select Path='Microsoft-Windows-Sysmon/Operational'>*{range}</Select>"
                + "</Query>"
                + "</QueryList>";

            EventLogWatcher watcher = null;
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        EventLogQuery logQuery = new EventLogQuery("Microsoft-Windows-Sysmon/Operational", PathType.LogName, eventQuery);
                        if (options.IsRemoteWatcher && !string.IsNullOrWhiteSpace(options.TargetComputer))
                        {
                            EventLogSession eventLogSession = new EventLogSession(options.TargetComputer);
                            logQuery.Session = eventLogSession;
                        }
                        watcher = new EventLogWatcher(eventQuery: logQuery,
                                                      bookmark: null,
                                                      readExistingEvents: options.ShouldReadExistingEvents);
                        watcher.EventRecordWritten += new EventHandler<EventRecordWrittenEventArgs>(EventLogEventRead);
                        watcher.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        if (watcher != null)
                        {
                            watcher.Enabled = false;
                            watcher.Dispose();
                        }
                        throw ex;
                    }
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void EventLogEventRead(object obj, EventRecordWrittenEventArgs arg)
        {
            if (arg.EventRecord == null)
            {
                return;
            }

            try
            {
                switch (arg.EventRecord.Id)
                {
                    case (int)SysmonEvent.NetworkConnection:
                        ProcessSysmonNetworkConnection(arg.EventRecord);
                        break;
                    case (int)SysmonEvent.ProcessCreated:
                        ProcessSysmonProcessCreated(arg.EventRecord);
                        break;
                    case (int)SysmonEvent.FileCreate:
                        ProcessSysmonFileCreated(arg.EventRecord);
                        break;
                    case (int)SysmonEvent.RegistryEvent:
                    case (int)SysmonEvent.RegistryValueSet:
                        ProcessSysmonRegistryEvent(arg.EventRecord);
                        break;
                }
            }
            catch { }
        }

        private static void ProcessSysmonNetworkConnection(EventRecord eventRecord)
        {
            XmlDocument eventData = new XmlDocument();
            eventData.LoadXml(eventRecord.ToXml());
            XmlNamespaceManager ns = new XmlNamespaceManager(eventData.NameTable);
            ns.AddNamespace("ns", "http://schemas.microsoft.com/win/2004/08/events/event");
            XmlNode ruleName = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='RuleName']", ns);

            _ = IPAddress.TryParse(eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='SourceIp']", ns)?.InnerText, out IPAddress sourceIP);
            _ = IPAddress.TryParse(eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='DestinationIp']", ns)?.InnerText, out IPAddress destinationIP);
            _ = int.TryParse(eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='SourcePort']", ns)?.InnerText, out int sourcePort);
            _ = int.TryParse(eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='DestinationPort']", ns)?.InnerText, out int destinationPort);
            _ = int.TryParse(eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='ProcessId']", ns)?.InnerText, out int processId);
            _ = bool.TryParse(eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='Initiated']", ns)?.InnerText, out bool initiated);
            _ = bool.TryParse(eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='SourceIsIpv6']", ns)?.InnerText, out bool sourceIsIpv6);
            _ = bool.TryParse(eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='DestinationIsIpv6']", ns)?.InnerText, out bool destinationIsIpv6);

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                GlobalData.NetworkConnect.Insert(0, new SysmonNetworkConnect
                {
                    Timestamp = eventRecord.TimeCreated.Value,
                    RuleName = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='RuleName']", ns)?.InnerText,
                    UtcTime = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='UtcTime']", ns)?.InnerText,
                    ProcessGuid = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='ProcessGuid']", ns)?.InnerText,
                    ProcessId = processId,
                    Image = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='Image']", ns)?.InnerText,
                    User = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='User']", ns)?.InnerText,
                    Protocol = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='Protocol']", ns)?.InnerText,
                    Initiated = initiated,
                    SourceIsIpv6 = sourceIsIpv6,
                    SourceIp = sourceIP,
                    SourceHostname = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='SourceHostname']", ns)?.InnerText,
                    SourcePort = sourcePort,
                    SourcePortName = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='SourcePortName']", ns)?.InnerText,
                    DestinationIsIpv6 = destinationIsIpv6,
                    DestinationIp = destinationIP,
                    DestinationHostname = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='DestinationHostname']", ns)?.InnerText,
                    DestinationPort = destinationPort,
                    DestinationPortName = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='DestinationPortName']", ns)?.InnerText,
                });
            }));
        }

        private static void ProcessSysmonProcessCreated(EventRecord eventRecord)
        {
            XmlDocument eventData = new XmlDocument();
            eventData.LoadXml(eventRecord.ToXml());
            XmlNamespaceManager ns = new XmlNamespaceManager(eventData.NameTable);
            ns.AddNamespace("ns", "http://schemas.microsoft.com/win/2004/08/events/event");
            XmlNode ruleName = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='RuleName']", ns);

            _ = int.TryParse(eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='ProcessId']", ns)?.InnerText, out int processId);
            _ = int.TryParse(eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='TerminalSessionId']", ns)?.InnerText, out int terminalSessionId);
            _ = int.TryParse(eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='ParentProcessId']", ns)?.InnerText, out int parentProcessId);

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                GlobalData.CreateProcess.Insert(0, new SysmonCreateProcess
                {
                    Timestamp = eventRecord.TimeCreated.Value,
                    RuleName = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='RuleName']", ns)?.InnerText,
                    UtcTime = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='UtcTime']", ns)?.InnerText,
                    ProcessGuid = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='ProcessGuid']", ns)?.InnerText,
                    ProcessId = processId,
                    Image = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='Image']", ns)?.InnerText,
                    FileVersion = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='FileVersion']", ns)?.InnerText,
                    Description = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='Description']", ns)?.InnerText,
                    Product = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='Product']", ns)?.InnerText,
                    Company = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='Company']", ns)?.InnerText,
                    OriginalFileName = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='OriginalFileName']", ns)?.InnerText,
                    CommandLine = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='CommandLine']", ns)?.InnerText,
                    CurrentDirectory = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='CurrentDirectory']", ns)?.InnerText,
                    User = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='User']", ns)?.InnerText,
                    LogonGuid = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='LogonGuid']", ns)?.InnerText,
                    LogonId = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='LogonId']", ns)?.InnerText,
                    TerminalSessionId = terminalSessionId,
                    IntegrityLevel = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='IntegrityLevel']", ns)?.InnerText,
                    Hashes = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='Hashes']", ns)?.InnerText,
                    ParentProcessGuid = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='ParentProcessGuid']", ns)?.InnerText,
                    ParentProcessId = parentProcessId,
                    ParentImage = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='ParentImage']", ns)?.InnerText,
                    ParentCommandLine = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='ParentCommandLine']", ns)?.InnerText,
                    ParentUser = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='ParentUser']", ns)?.InnerText,
                });
            }));
        }

        private static void ProcessSysmonFileCreated(EventRecord eventRecord)
        {
            XmlDocument eventData = new XmlDocument();
            eventData.LoadXml(eventRecord.ToXml());
            XmlNamespaceManager ns = new XmlNamespaceManager(eventData.NameTable);
            ns.AddNamespace("ns", "http://schemas.microsoft.com/win/2004/08/events/event");
            XmlNode ruleName = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='RuleName']", ns);

            _ = int.TryParse(eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='ProcessId']", ns)?.InnerText, out int processId);

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                GlobalData.FileCreate.Insert(0, new SysmonFileCreate
                {
                    Timestamp = eventRecord.TimeCreated.Value,
                    RuleName = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='RuleName']", ns)?.InnerText,
                    UtcTime = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='UtcTime']", ns)?.InnerText,
                    ProcessGuid = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='ProcessGuid']", ns)?.InnerText,
                    ProcessId = processId,
                    Image = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='Image']", ns)?.InnerText,
                    TargetFilename = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='TargetFilename']", ns)?.InnerText,
                    CreationUtcTime = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='CreationUtcTime']", ns)?.InnerText,
                    User = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='User']", ns)?.InnerText,
                });
            }));
        }

        private static void ProcessSysmonRegistryEvent(EventRecord eventRecord)
        {
            XmlDocument eventData = new XmlDocument();
            eventData.LoadXml(eventRecord.ToXml());
            XmlNamespaceManager ns = new XmlNamespaceManager(eventData.NameTable);
            ns.AddNamespace("ns", "http://schemas.microsoft.com/win/2004/08/events/event");
            XmlNode ruleName = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='RuleName']", ns);

            _ = int.TryParse(eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='ProcessId']", ns)?.InnerText, out int processId);

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                GlobalData.RegistryEvent.Insert(0, new SysmonRegistryEvent
                {
                    Timestamp = eventRecord.TimeCreated.Value,
                    RuleName = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='RuleName']", ns)?.InnerText,
                    EventType = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='EventType']", ns)?.InnerText,
                    UtcTime = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='UtcTime']", ns)?.InnerText,
                    ProcessGuid = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='ProcessGuid']", ns)?.InnerText,
                    ProcessId = processId,
                    Image = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='Image']", ns)?.InnerText,
                    TargetObject = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='TargetObject']", ns)?.InnerText,
                    Details = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='Details']", ns)?.InnerText,
                    User = eventData.SelectSingleNode("//ns:Event/ns:EventData/ns:Data[@Name='User']", ns)?.InnerText,
                });
            }));
        }
    }
}
