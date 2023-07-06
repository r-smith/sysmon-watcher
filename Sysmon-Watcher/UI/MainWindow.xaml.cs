using Sysmon_Watcher.Models;
using Sysmon_Watcher.Helpers;
using Sysmon_Watcher.UI;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace Sysmon_Watcher
{
    public partial class MainWindow : Window
    {
        private readonly ICollectionView NetworkConnectView;
        private readonly ICollectionView CreateProcessView;
        private readonly ICollectionView FileCreateView;
        private readonly ICollectionView RegistryEventView;
        private readonly EventWatcherOptions EventWatcherOptions;

        public MainWindow(EventWatcherOptions options)
        {
            InitializeComponent();

            RefreshMaximizeRestoreButton();

            if (options.IsRemoteWatcher && !string.IsNullOrWhiteSpace(options.TargetComputer))
            {
                Title = $"Sysmon Watcher ({options.TargetComputer})";
            }
            else
            {
                Title = "Sysmon Watcher (Local)";
            }

            NetworkConnectView = CollectionViewSource.GetDefaultView(GlobalData.NetworkConnect);
            NetworkConnectView.Filter = ApplyFilter;
            NetworkConnectGrid.ItemsSource = NetworkConnectView;
            CreateProcessView = CollectionViewSource.GetDefaultView(GlobalData.CreateProcess);
            CreateProcessView.Filter = ApplyFilter;
            CreateProcessGrid.ItemsSource = CreateProcessView;
            FileCreateView = CollectionViewSource.GetDefaultView(GlobalData.FileCreate);
            FileCreateView.Filter = ApplyFilter;
            FileCreateGrid.ItemsSource = FileCreateView;
            RegistryEventView = CollectionViewSource.GetDefaultView(GlobalData.RegistryEvent);
            RegistryEventView.Filter = ApplyFilter;
            RegistryEventGrid.ItemsSource = RegistryEventView;

            UpdateColumns();
            EventWatcherOptions = options;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await WindowsEventWatcher.SubscribeToSysmonEvents(EventWatcherOptions);
            }
            catch (Exception ex)
            {
                MessageWindow window = new MessageWindow($"Failed subscribing to event log. {ex.Message}")
                {
                    Owner = this
                };
                window.ShowDialog();
            }
        }

        private bool ApplyFilter(object entry)
        {
            // Match until proven otherwise.
            bool isMatch = true;
            foreach (Filter filter in FilterData.Filters)
            {
                // Get the value for the provided column name.
                string columnValue = Util.GetPropertyValue(entry, filter.Column.BindingPath);
                if (columnValue == null)
                {
                    // Consider it a match if the column doesn't exist.
                    continue;
                }

                // Perform the selected comparison.
                bool comparisonResult = false;
                switch (filter.Comparison)
                {
                    case Filter.ComparisonType.equals:
                        comparisonResult = string.Equals(columnValue, filter.Value, StringComparison.OrdinalIgnoreCase);
                        break;
                    case Filter.ComparisonType.doesNotEqual:
                        comparisonResult = !string.Equals(columnValue, filter.Value, StringComparison.OrdinalIgnoreCase);
                        break;
                    case Filter.ComparisonType.contains:
                        comparisonResult = columnValue.ToLower().Contains(filter.Value.ToLower());
                        break;
                    case Filter.ComparisonType.doesNotContain:
                        comparisonResult = !columnValue.ToLower().Contains(filter.Value.ToLower());
                        break;
                }

                if (comparisonResult == true && filter.Action == Filter.ActionType.exclude)
                {
                    return false;
                }
                if (comparisonResult == false && filter.Action == Filter.ActionType.include)
                {
                    return false;
                }
            }
            return isMatch;
        }

        private void UpdateColumns()
        {
            SetDataGridColumns(NetworkConnectGrid, SysmonNetworkConnect.Columns);
            //NetworkConnectGrid.Items.SortDescriptions.Clear();
            //NetworkConnectGrid.Items.SortDescriptions.Add(new SortDescription("Image", ListSortDirection.Ascending));
            //NetworkConnectView.SortDescriptions.Clear();
            //NetworkConnectView.SortDescriptions.Add(new SortDescription("Image", ListSortDirection.Ascending));
            //NetworkConnectGrid.Columns[0].SortDirection = ListSortDirection.Descending;
            SetDataGridColumns(CreateProcessGrid, SysmonCreateProcess.Columns);
            SetDataGridColumns(FileCreateGrid, SysmonFileCreate.Columns);
            SetDataGridColumns(RegistryEventGrid, SysmonRegistryEvent.Columns);
        }

        private void SetDataGridColumns(DataGrid dataGrid, ObservableCollection<Column> columns)
        {
            dataGrid.Columns.Clear();
            foreach (Column column in columns)
            {
                if (!column.IsSelected)
                {
                    continue;
                }
                DataGridTextColumn c = new DataGridTextColumn()
                {
                    Header = column.Header,
                    Binding = new Binding(column.BindingPath),
                };
                dataGrid.Columns.Add(c);
            }
        }

        private void Columns_Click(object sender, RoutedEventArgs e)
        {
            string header = string.Empty;
            if (TabControl.SelectedItem is TabItem tabItem)
            {
                header = tabItem?.Header?.ToString();
            }
            ColumnsWindow window = ColumnsWindow.GetInstance(header);
            window.Owner = this;
            window.ColumnsWindowSaveClicked += ColumnsWindow_SaveClicked;
            window.Show();
        }

        private void ColumnsWindow_SaveClicked(object sender, EventArgs e)
        {
            UpdateColumns();
        }

        private void RefreshCollectionViews()
        {
            NetworkConnectView.Refresh();
            CreateProcessView.Refresh();
            FileCreateView.Refresh();
            RegistryEventView.Refresh();
        }

        private void Filters_Click(object sender, RoutedEventArgs e)
        {
            FiltersWindow window = FiltersWindow.GetInstance();
            window.Owner = this;
            window.FiltersWindowSaveClicked += FiltersWindow_SaveClicked;
            window.Show();
        }

        private void FiltersWindow_SaveClicked(object sender, EventArgs e)
        {
            RefreshCollectionViews();
        }

        private void ToggleShortPaths_Click(object sender, RoutedEventArgs e)
        {
            GlobalData.DisplayShortPaths = !GlobalData.DisplayShortPaths;
            UpdateColumns();
            if (GlobalData.DisplayShortPaths)
            {
                ToggleShortPathsHeader.Text = "Short paths";
            }
            else
            {
                ToggleShortPathsHeader.Text = "Full paths";
            }
        }

        private void DataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = Util.FindVisualParent<DataGridCell>(e.OriginalSource as DependencyObject);
            if (cell != null)
            {
                DataGridRow row = Util.FindVisualParent<DataGridRow>(cell);
                if (row != null)
                {
                    DataGrid grid = Util.FindVisualParent<DataGrid>(row);
                    grid.SelectedItem = row.Item;
                    cell.Focus();

                    // Create a new ContextMenu for the selected DataGrid cell.
                    ContextMenu contextMenu = new ContextMenu();
                    BuildContextMenu(contextMenu, cell);
                    grid.ContextMenu = contextMenu;
                }
            }
            else if (Util.FindVisualParent<DataGridColumnHeader>(e.OriginalSource as DependencyObject) is DataGridColumnHeader header && header != null)
            {
                // Create a new ContextMenu for the DataGrid column header.
                ContextMenu contextMenu = new ContextMenu();
                MenuItem menuColumns = new MenuItem()
                {
                    Header = "Edit columns",
                };
                menuColumns.Click += (s, eventArgs) => Columns_Click(s, eventArgs);
                contextMenu.Items.Add(menuColumns);
                header.ContextMenu = contextMenu;
            }
            else if (Util.FindVisualParent<ScrollBar>(e.OriginalSource as DependencyObject) != null)
            {
                // Scroll bar clicked. Do nothing - Allows scroll bar's context menu to display.
            }
            else
            {
                e.Handled = true;
            }
        }

        private void BuildContextMenu(ContextMenu contextMenu, DataGridCell cell)
        {
            string cellValue = ((TextBlock)cell.Content).Text;

            MenuItem menuCopy = new MenuItem()
            {
                Header = "Copy value"
            };
            menuCopy.Click += MenuCopy_Click;
            contextMenu.Items.Add(menuCopy);

            contextMenu.Items.Add(new Separator());

            MenuItem menuInclude = new MenuItem
            {
                Header = $"Include '{cellValue}'",
            };
            menuInclude.Click += MenuInclude_Click;
            contextMenu.Items.Add(menuInclude);

            MenuItem menuExclude = new MenuItem
            {
                Header = $"Exclude '{cellValue}'",
            };
            menuExclude.Click += MenuExclude_Click;
            contextMenu.Items.Add(menuExclude);
        }

        private void MenuCopy_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            DataGrid dataGrid = (DataGrid)contextMenu.PlacementTarget;

            DataGridCellInfo cellInfo = dataGrid.CurrentCell;
            object cellValue = cellInfo.Item.GetType().GetProperty(cellInfo.Column.SortMemberPath)?.GetValue(cellInfo.Item);

            Clipboard.SetText(cellValue.ToString());
        }

        private void MenuInclude_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            DataGrid dataGrid = (DataGrid)contextMenu.PlacementTarget;

            DataGridCellInfo cellInfo = dataGrid.CurrentCell;
            object cellValue = cellInfo.Item.GetType().GetProperty(cellInfo.Column.SortMemberPath)?.GetValue(cellInfo.Item);

            DataGridColumn column = cellInfo.Column;
            string headerName = column.Header.ToString();
            string headerBindingPath = column.SortMemberPath;

            FilterData.Filters.Add(new Filter
            {
                Column = new Column(headerBindingPath, headerName, false),
                Comparison = Filter.ComparisonType.equals,
                Value = cellValue.ToString(),
                Action = Filter.ActionType.include,
                IsCommitted = true,
            });
            RefreshCollectionViews();
        }

        private void MenuExclude_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            DataGrid dataGrid = (DataGrid)contextMenu.PlacementTarget;

            DataGridCellInfo cellInfo = dataGrid.CurrentCell;
            object cellValue = cellInfo.Item.GetType().GetProperty(cellInfo.Column.SortMemberPath)?.GetValue(cellInfo.Item);

            DataGridColumn column = cellInfo.Column;
            string headerName = column.Header.ToString();
            string headerBindingPath = column.SortMemberPath;

            FilterData.Filters.Add(new Filter
            {
                Column = new Column(headerBindingPath, headerName, false),
                Comparison = Filter.ComparisonType.equals,
                Value = cellValue.ToString(),
                Action = Filter.ActionType.exclude,
                IsCommitted = true,
            });
            RefreshCollectionViews();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            RefreshMaximizeRestoreButton();
        }

        private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RefreshMaximizeRestoreButton()
        {
            if (WindowState == WindowState.Maximized)
            {
                maximizeButton.Visibility = Visibility.Collapsed;
                restoreButton.Visibility = Visibility.Visible;
            }
            else
            {
                maximizeButton.Visibility = Visibility.Visible;
                restoreButton.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HookProc);
        }

        protected virtual IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_GETMINMAXINFO)
            {
                // We need to tell the system what our size should be when maximized. Otherwise it will cover the whole screen,
                // including the task bar.
                MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

                // Adjust the maximized size and position to fit the work area of the correct monitor
                IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

                if (monitor != IntPtr.Zero)
                {
                    MONITORINFO monitorInfo = new MONITORINFO
                    {
                        cbSize = Marshal.SizeOf(typeof(MONITORINFO))
                    };
                    GetMonitorInfo(monitor, ref monitorInfo);
                    RECT rcWorkArea = monitorInfo.rcWork;
                    RECT rcMonitorArea = monitorInfo.rcMonitor;
                    mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                    mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                    mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                    mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
                }

                Marshal.StructureToPtr(mmi, lParam, true);
            }
            return IntPtr.Zero;
        }

        private const int WM_GETMINMAXINFO = 0x0024;
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }
    }
}