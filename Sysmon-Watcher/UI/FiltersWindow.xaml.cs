using Sysmon_Watcher.Helpers;
using Sysmon_Watcher.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Sysmon_Watcher.UI
{
    public partial class FiltersWindow : Window
    {
        private static FiltersWindow instance;
        public event EventHandler FiltersWindowSaveClicked;
        private readonly ICollectionView FiltersView;

        private FiltersWindow()
        {
            InitializeComponent();

            // Initialize the Filters view and bind to the DataGrid.
            FiltersView = CollectionViewSource.GetDefaultView(FilterData.Filters);
            FiltersView.Filter = HidePendingDelete;
            FiltersGrid.ItemsSource = FiltersView;

            // Bind Column ComboBox to all combined columns.
            FilterColumn.ItemsSource = GetAllColumns();
            FilterColumn.DisplayMemberPath = "Header";
            FilterColumn.SelectedIndex = 0;

            // Bind Action ComboBox to enum.
            FilterAction.ItemsSource = Enum.GetValues(typeof(Filter.ActionType)).Cast<Filter.ActionType>();
            FilterAction.SelectedIndex = 0;

            // Bind Comparison ComboBox to enum. Cast to EnumWithDescription.
            // The enum Description attribute provides the display string.
            FilterComparison.ItemsSource = Enum.GetValues(typeof(Filter.ComparisonType))
                .Cast<Filter.ComparisonType>()
                .Select(e => new EnumWithDescription<Filter.ComparisonType>(e));
            FilterComparison.DisplayMemberPath = nameof(EnumWithDescription<Filter.ComparisonType>.Description);
            FilterComparison.SelectedIndex = 0;
        }

        public static FiltersWindow GetInstance()
        {
            if (instance == null)
            {
                instance = new FiltersWindow();
            }
            instance.Activate();
            return instance;
        }

        private IOrderedEnumerable<Column> GetAllColumns()
        {
            IEnumerable<Column> allColumns =
                SysmonNetworkConnect.Columns
                .Concat(SysmonCreateProcess.Columns)
                .Concat(SysmonFileCreate.Columns)
                .Concat(SysmonRegistryEvent.Columns);
            return allColumns
                .GroupBy(c => c.Header)
                .Select(g => g.First())
                .OrderBy(c => c.Header);
        }

        private bool HidePendingDelete(object entry)
        {
            // If a filter entry has the IsDeletePending flag set, then hide it from the collection view.
            if (entry != null && entry is Filter filter && filter.IsDeletePending)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void AddFilter()
        {
            // Add an uncommitted filter to the filters collection.
            FilterData.Filters.Add(new Filter
            {
                Column = (Column)FilterColumn.SelectedItem,
                Comparison = ((EnumWithDescription<Filter.ComparisonType>)FilterComparison.SelectedItem).Value,
                Value = FilterValue.Text,
                Action = (Filter.ActionType)FilterAction.SelectedItem,
            });
        }

        private void RemoveFilter()
        {
            // Remove a filter from the filters collection.
            // If the entry isn't committed, delete it entirely.
            // Otherwise, set IsDeletePending on the entry.
            if (FiltersGrid.SelectedItem != null && FiltersGrid.SelectedItem is Filter selectedFilter)
            {
                if (selectedFilter.IsCommitted == false)
                {
                    FilterData.Filters.Remove(selectedFilter);
                }
                else
                {
                    selectedFilter.IsDeletePending = true;
                }
            }
            FiltersView.Refresh();
        }

        private void SaveFilters()
        {
            // Save and commit filters in the filter collection.
            // If IsDeletePending, delete the filter entry.
            // Otherwise, set the filter entry as committed.
            for (int i = FilterData.Filters.Count - 1; i >= 0; i--)
            {
                if (FilterData.Filters[i].IsDeletePending)
                {
                    FilterData.Filters.RemoveAt(i);
                    continue;
                }
                else
                {
                    FilterData.Filters[i].IsCommitted = true;
                }
            }
            FiltersView.Refresh();
        }

        private void CleanupFilters()
        {
            // If committed, clear the IsDeletePending flag.
            // If not committed, delete the filter entry.
            for (int i = FilterData.Filters.Count - 1; i >= 0; i--)
            {
                if (FilterData.Filters[i].IsCommitted)
                {
                    FilterData.Filters[i].IsDeletePending = false;
                }
                else
                {
                    FilterData.Filters.RemoveAt(i);
                }
            }
            FiltersView.Refresh();
        }

        private void ClearFilters()
        {
            // Clear the filters collection.
            // If the entry isn't committed, delete it entirely.
            // Otherwise, set IsDeletePending on the entry.
            for (int i = FilterData.Filters.Count - 1; i >= 0; i--)
            {
                if (FilterData.Filters[i].IsCommitted == false)
                {
                    FilterData.Filters.RemoveAt(i);
                    continue;
                }
                else
                {
                    FilterData.Filters[i].IsDeletePending = true;
                }
            }
            FiltersView.Refresh();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (FilterValue.Text.Length == 0)
            {
                FilterValue.Focus();
                return;
            }
            AddFilter();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            RemoveFilter();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearFilters();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            SaveFilters();
            FiltersWindowSaveClicked?.Invoke(this, EventArgs.Empty);
            Close();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            SaveFilters();
            FiltersWindowSaveClicked?.Invoke(this, EventArgs.Empty);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CleanupFilters();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            instance = null;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Normal)
            {
                WindowState = WindowState.Normal;
            }
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
