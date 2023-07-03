using Sysmon_Watcher.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows;

namespace Sysmon_Watcher.Helpers
{
    internal static class Util
    {
        public static string GetPropertyValue(object obj, string propertyName)
        {
            Type classType = obj.GetType();
            PropertyInfo propertyInfo = classType.GetProperty(propertyName);

            if (propertyInfo != null)
            {
                object propertyValue = propertyInfo.GetValue(obj, null);

                if (propertyValue != null)
                {
                    return propertyValue.ToString();
                }
            }
            return null;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield return (T)Enumerable.Empty<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject ithChild = VisualTreeHelper.GetChild(depObj, i);
                if (ithChild == null) continue;
                if (ithChild is T t) yield return t;
                foreach (T childOfChild in FindVisualChildren<T>(ithChild)) yield return childOfChild;
            }
        }

        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            if (parentObject is T parent)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }

        public static void ColumnsIsSelectedClone(ObservableCollection<Column> columns)
        {
            foreach (Column column in columns)
            {
                column.IsSelectedClone = column.IsSelected;
            }
        }

        public static void ColumnsIsSelectedCommit(ObservableCollection<Column> columns)
        {
            foreach (Column column in columns)
            {
                column.IsSelected = column.IsSelectedClone;
            }
        }
    }
}
