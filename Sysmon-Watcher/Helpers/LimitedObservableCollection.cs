using System.Collections.ObjectModel;

namespace Sysmon_Watcher.Helpers
{
    public class LimitedObservableCollection<T> : ObservableCollection<T>
    {
        public int Capacity { get; }

        public LimitedObservableCollection(int capacity)
        {
            Capacity = capacity;
        }

        public new void Insert(int index, T item)
        {
            if (Count >= Capacity)
            {
                RemoveAt(Count - 1);
            }
            base.Insert(index, item);
        }
    }
}
