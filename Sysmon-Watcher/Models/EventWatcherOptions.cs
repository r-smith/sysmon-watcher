namespace Sysmon_Watcher.Models
{
    public class EventWatcherOptions
    {
        public bool IsRemoteWatcher { get; set; } = false;
        public string TargetComputer { get; set; } = "";
        public bool ShouldReadExistingEvents { get; set; } = true;
        public bool ShouldReadAllEvents { get; set; } = false;
        public long EventRangeMilliseconds { get; set; } = 3600000;
        public string ToCommandLineArgs
        {
            get
            {
                if (!ShouldReadExistingEvents)
                {
                    return "local none";
                }
                else
                {
                    if (ShouldReadAllEvents)
                    {
                        return "local all";
                    }
                    else
                    {
                        return $"local range {EventRangeMilliseconds}";
                    }
                }
            }
        }

        // Default constructor.
        public EventWatcherOptions()
        {

        }

        // Optional constructor to build using command line arguments.
        public EventWatcherOptions(string[] args)
        {
            IsRemoteWatcher = false;
            ShouldReadExistingEvents = true;
            ShouldReadAllEvents = false;
            EventRangeMilliseconds = 3600000;

            if (args.Length > 2)
            {
                switch (args[2])
                {
                    case "none":
                        ShouldReadExistingEvents = false;
                        break;
                    case "all":
                        ShouldReadAllEvents = true;
                        break;
                    case "range":
                        if (args.Length > 3 && long.TryParse(args[3], out long range) && range > 0)
                        {
                            EventRangeMilliseconds = range;
                        }
                        break;
                }
            }
        }
    }
}
