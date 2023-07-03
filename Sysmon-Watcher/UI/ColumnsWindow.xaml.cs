using Sysmon_Watcher.Helpers;
using Sysmon_Watcher.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sysmon_Watcher.UI
{
    public partial class ColumnsWindow : Window
    {
        private static ColumnsWindow instance;
        public event EventHandler ColumnsWindowSaveClicked;

        private ColumnsWindow()
        {
            InitializeComponent();

            Util.ColumnsIsSelectedClone(SysmonNetworkConnect.Columns);
            Util.ColumnsIsSelectedClone(SysmonCreateProcess.Columns);
            Util.ColumnsIsSelectedClone(SysmonFileCreate.Columns);
            Util.ColumnsIsSelectedClone(SysmonRegistryEvent.Columns);
        }

        public static ColumnsWindow GetInstance(string tabName)
        {
            if (instance == null)
            {
                instance = new ColumnsWindow();
            }
            instance.ComboBox_SelectByName(tabName);
            instance.Activate();
            return instance;
        }

        private void ComboBox_SelectByName(string collectionName)
        {
            foreach (ComboBoxItem item in ComboBoxCollection.Items)
            {
                if (item.Content?.ToString() == collectionName)
                {
                    ComboBoxCollection.SelectedItem = item;
                    return;
                }
            }
        }

        private void ComboBoxCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxCollection.SelectedItem == null)
            {
                return;
            }

            UpdateItemsControlSource(((ComboBoxItem)ComboBoxCollection.SelectedItem).Content?.ToString());
        }

        private void UpdateItemsControlSource(string collectionName)
        {
            switch (collectionName)
            {
                case "Network":
                    ItemsControl.ItemsSource = SysmonNetworkConnect.Columns;
                    break;
                case "Processes":
                    ItemsControl.ItemsSource = SysmonCreateProcess.Columns;
                    break;
                case "Files":
                    ItemsControl.ItemsSource = SysmonFileCreate.Columns;
                    break;
                case "Registry":
                    ItemsControl.ItemsSource = SysmonRegistryEvent.Columns;
                    break;
                default:
                    ItemsControl.ItemsSource = null;
                    break;
            }
        }

        private void UpdateBindings()
        {
            // Update binding on all checkboxes.
            //foreach (CheckBox checkBox in Util.FindVisualChildren<CheckBox>(MainGrid))
            //{
            //    checkBox.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateSource();
            //}
            Util.ColumnsIsSelectedCommit(SysmonNetworkConnect.Columns);
            Util.ColumnsIsSelectedCommit(SysmonCreateProcess.Columns);
            Util.ColumnsIsSelectedCommit(SysmonFileCreate.Columns);
            Util.ColumnsIsSelectedCommit(SysmonRegistryEvent.Columns);
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            UpdateBindings();
            ColumnsWindowSaveClicked?.Invoke(this, EventArgs.Empty);
            Close();
        }
        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            UpdateBindings();
            ColumnsWindowSaveClicked?.Invoke(this, EventArgs.Empty);
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
