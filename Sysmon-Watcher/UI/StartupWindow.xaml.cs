using Sysmon_Watcher.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sysmon_Watcher.UI
{
    public partial class StartupWindow : Window
    {
        public StartupWindow()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            if (TargetRemoteOption.IsChecked == true)
            {
                WatchRemoteComputer();
            }
            else
            {
                WatchLocalComputer();
            }
            StartButton.IsEnabled = true;
        }

        private EventWatcherOptions BuildWatcherOptions()
        {
            EventWatcherOptions options = new EventWatcherOptions();

            // Local or remote.
            if (TargetRemoteOption.IsChecked == true)
            {
                options.IsRemoteWatcher = true;
                options.TargetComputer = TargetComputer.Text;
            }
            else
            {
                options.IsRemoteWatcher = false;
            }

            // Event range.
            if (RangeNoneOption.IsChecked == true)
            {
                options.ShouldReadExistingEvents = false;
            }
            else if (RangeAllOption.IsChecked == true)
            {
                options.ShouldReadExistingEvents = true;
                options.ShouldReadAllEvents = true;
            }
            else if (RangeSomeOption.IsChecked == true)
            {
                options.ShouldReadExistingEvents = true;
                options.ShouldReadAllEvents = false;
                if (int.TryParse(RangeValue.Text, out int rangeValue) && rangeValue > 0)
                {
                    options.EventRangeMilliseconds = RangeValueToMilliseconds(rangeValue);
                }
                else
                {
                    options.EventRangeMilliseconds = 3600000;
                }
            }

            return options;
        }

        private long RangeValueToMilliseconds(int rangeValue)
        {
            const long _defaultValue = 3600000;

            if (RangeUnit.SelectedItem != null && RangeUnit.SelectedItem is ComboBoxItem item && item.Content is string unit)
            {
                switch (unit)
                {
                    case "minute":
                        return (long)TimeSpan.FromMinutes(rangeValue).TotalMilliseconds;
                    case "hour":
                        return (long)TimeSpan.FromHours(rangeValue).TotalMilliseconds;
                    case "day":
                        return (long)TimeSpan.FromDays(rangeValue).TotalMilliseconds;
                }
            }

            return _defaultValue;
        }

        private void WatchRemoteComputer()
        {
            new MainWindow(BuildWatcherOptions()).Show();
            Close();
        }

        private void WatchLocalComputer()
        {
            EventWatcherOptions options = BuildWatcherOptions();

            if (!IsRunningAsAdmin())
            {
                try
                {
                    // Restart as admin. Supply the argument "local" to have the application go straight to MainWindow after relaunching.
                    ProcessStartInfo startInfo = new ProcessStartInfo(Assembly.GetEntryAssembly().CodeBase)
                    {
                        Verb = "runas",
                        UseShellExecute = true,
                        Arguments = options.ToCommandLineArgs,
                    };
                    Process.Start(startInfo);
                    Application.Current.Shutdown();
                    return;
                }
                catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
                {
                    // The user canceled elevation. Do nothing.
                }
                catch (Exception ex)
                {
                    // Something went wrong when relaunching the application as admin.
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                new MainWindow(options).Show();
                Close();
            }
        }

        private bool IsRunningAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void TargetComputer_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (TargetComputer.IsEnabled)
            {
                TargetComputer.Focus();
                TargetComputer.SelectAll();
            }
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
