using Sysmon_Watcher.Models;
using Sysmon_Watcher.UI;
using System;
using System.Windows;

namespace Sysmon_Watcher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                switch (args[1])
                {
                    case "local":
                        new MainWindow(new EventWatcherOptions(args)).Show();
                        break;
                    default:
                        new StartupWindow().Show();
                        break;
                }
            }
            else
            {
                new StartupWindow().Show();
            }
        }
    }
}
